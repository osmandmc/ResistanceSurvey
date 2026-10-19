using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.COMMON.Entities;
using RS.EF;
using RS.MVC.Applications;
using RS.MVC.Features.Articles;
using RS.MVC.Models;
using static RS.COMMON.Constants.Enums;

namespace RS.MVC.Features.Review
{
    public enum ReviewStatus
    {
        Ok,
        Invalid,
        AlreadyHandled,
        NotFound,
    }

    public record ReviewSaveResult(
        ReviewStatus Status,
        int? ResistanceId = null,
        bool OutcomeSent = false,
        IReadOnlyDictionary<string, string> Errors = null)
    {
        public static ReviewSaveResult Invalid(IReadOnlyDictionary<string, string> errors) => new(ReviewStatus.Invalid, Errors: errors);

        public static ReviewSaveResult CaseNotFound() =>
            Invalid(new Dictionary<string, string> { ["resistanceId"] = "Vaka bulunamadı." });
    }

    /// <summary>
    /// The steps every way of attaching an article to a case shares: it must not be handled yet and must exist in
    /// the pipeline; one transaction saves the thin News row (Linked, with its ArticleId) together with the case
    /// writes; then Digested is sent (re-sent by the inbox if that fails).
    /// </summary>
    public class ArticleCaseLinker(RSDBContext db, IPipelineClient pipeline, ArticleNewsService articleNews, OutcomeSender outcomes)
    {
        public Task<bool> CaseExistsAsync(int resistanceId) =>
            db.Resistance.AnyAsync(r => r.Id == resistanceId && !r.Deleted);

        /// <summary><paramref name="writeCase"/> saves the case part and returns the resistance id.</summary>
        public async Task<ReviewSaveResult> LinkAsync(string articleId, Func<News, Task<int>> writeCase)
        {
            if (await articleNews.IsHandledAsync(articleId))
                return new ReviewSaveResult(ReviewStatus.AlreadyHandled);

            var article = await pipeline.GetArticleAsync(articleId);
            if (article == null)
                return new ReviewSaveResult(ReviewStatus.NotFound);

            int resistanceId;
            try
            {
                await using var transaction = await db.Database.BeginTransactionAsync();
                var news = await articleNews.FindOrAddAsync(article);
                news.Status = Status.Linked;
                await db.SaveChangesAsync();
                resistanceId = await writeCase(news);
                await transaction.CommitAsync();
            }
            catch (DbUpdateException)
            {
                // A colleague saved it a moment earlier (unique ArticleId); anything else is a real error.
                db.ChangeTracker.Clear();
                if (await articleNews.IsHandledAsync(articleId))
                    return new ReviewSaveResult(ReviewStatus.AlreadyHandled);
                throw;
            }

            var sent = await outcomes.TrySendAsync(articleId, new PipelineOutcomeRequest(PipelineOutcomeKind.Digested, resistanceId));
            return new ReviewSaveResult(ReviewStatus.Ok, resistanceId, sent);
        }

        /// <summary>Attaches the article's News row to the case, once.</summary>
        public async Task AttachAsync(int resistanceId, int newsId)
        {
            if (!await db.ResistanceNews.AnyAsync(rn => rn.ResistanceId == resistanceId && rn.NewsId == newsId))
            {
                db.ResistanceNews.Add(new ResistanceNews { ResistanceId = resistanceId, NewsId = newsId });
                await db.SaveChangesAsync();
            }
        }
    }
}
