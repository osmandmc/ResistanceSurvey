using System.Threading.Tasks;
using RS.EF;
using RS.MVC.Applications;
using RS.MVC.Features.Review;
using RS.MVC.Models;

namespace RS.MVC.Features.Cases
{
    /// <summary>A new case with its first protest, without an article (Vakalar → "Yeni vaka").</summary>
    public record CreateCaseCommand(CaseInput Case, ProtestInput Protest, string UserName);

    /// <summary>Same rules and same save path as creating a case from an article, minus the article.</summary>
    public class CreateCaseHandler(RSDBContext db, IResistanceApplication resistances, ListValueResolver lists)
    {
        public async Task<CaseCommandResult> Handle(CreateCaseCommand command)
        {
            var errors = ReviewValidation.ForNewCase(command.Case, command.Protest);
            if (errors.Count > 0)
                return CaseCommandResult.Invalid(errors);

            await using var transaction = await db.Database.BeginTransactionAsync();
            var reasonIds = await lists.ReasonIdsAsync(command.Case.ReasonIds, command.Case.NewReasonNames);
            var corporationIds = await lists.CorporationIdsAsync(command.Case.CorporationIds, command.Case.NewCorporationNames);
            var id = resistances.Create(CaseMapping.ToResistanceModel(command.Case, command.Protest, null, command.UserName, reasonIds, corporationIds));
            await transaction.CommitAsync();
            return CaseCommandResult.Ok(id);
        }
    }
}
