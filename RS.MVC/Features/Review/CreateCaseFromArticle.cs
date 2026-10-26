using System.Threading.Tasks;
using RS.MVC.Applications;
using RS.MVC.Models;

using RS.MVC.Features.Cases;
using RS.COMMON.Entities;
using RS.EF;
using RS.MVC.Features.Activity;

namespace RS.MVC.Features.Review
{
    /// <summary>A new resistance with its first protest, from an inbox article.</summary>
    public record CreateCaseFromArticleCommand(string ArticleId, CaseInput Case, ProtestInput Protest, string UserName, AiUse Ai = null);

    public class CreateCaseFromArticleHandler(ArticleCaseLinker linker, CaseWriter writer, ListValueResolver lists, AiSuggestionRecorder ai, RSDBContext db)
    {
        public async Task<ReviewSaveResult> Handle(CreateCaseFromArticleCommand command)
        {
            var errors = ReviewValidation.ForNewCase(command.Case, command.Protest);
            if (errors.Count > 0)
                return ReviewSaveResult.Invalid(errors);

            return await linker.LinkAsync(command.ArticleId, async news =>
            {
                // Typed values become list entries inside the same transaction as the case.
                var reasonIds = await lists.ReasonIdsAsync(command.Case.ReasonIds, command.Case.NewReasonNames);
                var corporationIds = await lists.CorporationIdsAsync(command.Case.CorporationIds, command.Case.NewCorporationNames);
                var resistanceId = await writer.CreateCaseAsync(command.Case, command.Protest, command.UserName, reasonIds, corporationIds, news.Id);
                await ai.SaveAsync(news, command.Ai, AiDecision.NewCase, resistanceId, new { command.Case, command.Protest }, command.UserName);
                db.LogActivity(Actions.ArticleNewCase, news.Header, resistanceId, command.ArticleId);
                await db.SaveChangesAsync();
                return resistanceId;
            });
        }
    }
}
