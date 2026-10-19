using System.Threading.Tasks;

namespace RS.MVC.Features.Review
{
    /// <summary>Only attach an inbox article to an existing resistance (no new protest).</summary>
    public record LinkArticleToCaseCommand(string ArticleId, int? ResistanceId);

    public class LinkArticleToCaseHandler(ArticleCaseLinker linker)
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
                return resistanceId;
            });
        }
    }
}
