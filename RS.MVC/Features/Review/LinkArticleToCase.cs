using System.Threading.Tasks;
using RS.COMMON.Entities;
using RS.MVC.Models;

namespace RS.MVC.Features.Review
{
    /// <summary>Only attach an inbox article to an existing resistance (no new protest).</summary>
    public record LinkArticleToCaseCommand(string ArticleId, int? ResistanceId, string UserName = null, AiUse Ai = null);

    public class LinkArticleToCaseHandler(ArticleCaseLinker linker, AiSuggestionRecorder ai)
    {
        public async Task<ReviewSaveResult> Handle(LinkArticleToCaseCommand command)
        {
            var errors = ReviewValidation.ForLinkOnly(command.ResistanceId);
            if (errors.Count > 0)
                return ReviewSaveResult.Invalid(errors);
            var resistanceId = command.ResistanceId!.Value;
            if (!await linker.CaseExistsAsync(resistanceId))
                return ReviewSaveResult.CaseNotFound();

            return await linker.LinkAsync(command.ArticleId, async news =>
            {
                await linker.AttachAsync(resistanceId, news.Id);
                await ai.SaveAsync(news, command.Ai, AiDecision.LinkOnly, resistanceId, null, command.UserName);
                return resistanceId;
            });
        }
    }
}
