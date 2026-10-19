using System.Threading.Tasks;
using RS.MVC.Applications;
using RS.MVC.Models;

namespace RS.MVC.Features.Review
{
    /// <summary>A new protest on an existing resistance, from an inbox article.</summary>
    public record AddProtestFromArticleCommand(string ArticleId, int? ResistanceId, ProtestInput Protest, string UserName);

    public class AddProtestFromArticleHandler(ArticleCaseLinker linker, IResistanceApplication resistances)
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
                resistances.AddProtesto(CaseMapping.ToProtestoAddModel(resistanceId, command.Protest, command.UserName));
                await linker.AttachAsync(resistanceId, news.Id);
                return resistanceId;
            });
        }
    }
}
