using System.Threading.Tasks;
using RS.MVC.Applications;
using RS.MVC.Models;

using RS.MVC.Features.Cases;

namespace RS.MVC.Features.Review
{
    /// <summary>A new resistance with its first protest, from an inbox article.</summary>
    public record CreateCaseFromArticleCommand(string ArticleId, CaseInput Case, ProtestInput Protest, string UserName);

    public class CreateCaseFromArticleHandler(ArticleCaseLinker linker, CaseWriter writer, ListValueResolver lists)
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
                return await writer.CreateCaseAsync(command.Case, command.Protest, command.UserName, reasonIds, corporationIds, news.Id);
            });
        }
    }
}
