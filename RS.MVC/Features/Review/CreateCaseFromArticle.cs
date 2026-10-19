using System.Threading.Tasks;
using RS.MVC.Applications;
using RS.MVC.Models;

namespace RS.MVC.Features.Review
{
    /// <summary>A new resistance with its first protest, from an inbox article.</summary>
    public record CreateCaseFromArticleCommand(string ArticleId, CaseInput Case, ProtestInput Protest, string UserName);

    public class CreateCaseFromArticleHandler(ArticleCaseLinker linker, IResistanceApplication resistances)
    {
        public async Task<ReviewSaveResult> Handle(CreateCaseFromArticleCommand command)
        {
            var errors = ReviewValidation.ForNewCase(command.Case, command.Protest);
            if (errors.Count > 0)
                return ReviewSaveResult.Invalid(errors);

            return await linker.LinkAsync(command.ArticleId, news =>
                Task.FromResult(resistances.Create(CaseMapping.ToResistanceModel(command.Case, command.Protest, news, command.UserName))));
        }
    }
}
