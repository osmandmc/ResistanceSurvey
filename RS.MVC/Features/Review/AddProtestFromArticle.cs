using Microsoft.EntityFrameworkCore;
using RS.COMMON.Entities;
using RS.EF;
using RS.MVC.Features.Cases;
using RS.MVC.Features.Activity;
using System.Threading.Tasks;
using RS.MVC.Applications;
using RS.MVC.Models;

namespace RS.MVC.Features.Review
{
    /// <summary>A new protest on an existing resistance, from an inbox article.</summary>
    public record AddProtestFromArticleCommand(string ArticleId, int? ResistanceId, ProtestInput Protest, string UserName, AiUse Ai = null);

    public class AddProtestFromArticleHandler(ArticleCaseLinker linker, CaseWriter writer, RSDBContext db, AiSuggestionRecorder ai)
    {
        public async Task<ReviewSaveResult> Handle(AddProtestFromArticleCommand command)
        {
            var errors = ReviewValidation.ForAddProtest(command.ResistanceId, command.Protest);
            if (errors.Count > 0)
                return ReviewSaveResult.Invalid(errors);
            var resistanceId = command.ResistanceId!.Value;
            if (!await linker.CaseExistsAsync(resistanceId))
                return ReviewSaveResult.CaseNotFound();

            return await linker.LinkAsync(command.ArticleId, async news =>
            {
                var resistance = await db.Resistance.FirstAsync(r => r.Id == resistanceId);
                await writer.AddProtestAsync(resistance, command.Protest, command.UserName);
                await linker.AttachAsync(resistanceId, news.Id);
                await ai.SaveAsync(news, command.Ai, AiDecision.AddProtest, resistanceId, new { command.Protest }, command.UserName);
                db.LogActivity(Actions.ArticleAddProtest, news.Header, resistanceId, command.ArticleId);
                await db.SaveChangesAsync();
                return resistanceId;
            });
        }
    }
}
