using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.EF;
using RS.MVC.Applications;
using RS.MVC.Features.Articles;
using RS.MVC.Models;
using static RS.COMMON.Constants.Enums;

namespace RS.MVC.Features.Inbox
{
    /// <summary>Mark an inbox article as not relevant, with an optional reason.</summary>
    public record DismissArticleCommand(string ArticleId, string Reason);

    public enum DismissResult
    {
        Dismissed,

        /// <summary>Saved in the app, but the pipeline could not be told; the inbox re-sends it later.</summary>
        DismissedOutcomePending,

        /// <summary>Someone already linked or dismissed this article.</summary>
        AlreadyHandled,

        NotFound,
    }

    /// <summary>Saves the thin News row as Passive (the app's decision), then sends Dismissed to the pipeline.</summary>
    public class DismissArticleHandler(RSDBContext db, IPipelineClient pipeline, ArticleNewsService articleNews, OutcomeSender outcomes)
    {
        public async Task<DismissResult> Handle(DismissArticleCommand command)
        {
            if (await articleNews.IsHandledAsync(command.ArticleId))
                return DismissResult.AlreadyHandled;

            var article = await pipeline.GetArticleAsync(command.ArticleId);
            if (article == null)
                return DismissResult.NotFound;

            // An article imported the old way and already linked to a case counts as handled.
            var news = await articleNews.FindOrAddAsync(article);
            if (news.Status == Status.Linked)
            {
                await db.SaveChangesAsync();
                return DismissResult.AlreadyHandled;
            }
            news.Status = Status.Passive;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // A colleague saved it a moment earlier (unique ArticleId); anything else is a real error.
                db.ChangeTracker.Clear();
                if (await articleNews.IsHandledAsync(command.ArticleId))
                    return DismissResult.AlreadyHandled;
                throw;
            }

            var reason = string.IsNullOrWhiteSpace(command.Reason) ? null : command.Reason.Trim();
            return await outcomes.TrySendAsync(command.ArticleId, new PipelineOutcomeRequest(PipelineOutcomeKind.Dismissed, Reason: reason))
                ? DismissResult.Dismissed
                : DismissResult.DismissedOutcomePending;
        }
    }
}
