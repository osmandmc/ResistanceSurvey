using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.COMMON.Entities;
using RS.EF;
using RS.MVC.Features.Activity;

namespace RS.MVC.Features.Review
{
    /// <summary>"Şirket Ekle" in the case form.</summary>
    /// <param name="TypeId">Sahiplik (CompanyType).</param>
    public record CreateCompanyCommand(string Name, int? TypeId, int? ScaleId, int? WorklineId,
        RS.COMMON.Entities.InstitutionKind? InstitutionKind = null, int? ServiceBranchId = null);

    /// <summary>Existed: a company with that name was already there and is returned instead of a duplicate.</summary>
    public record CreateCompanyResult(Option Company, bool Existed, string Error = null);

    public class CreateCompanyHandler(RSDBContext db)
    {
        public async Task<CreateCompanyResult> Handle(CreateCompanyCommand command)
        {
            var name = NameMatching.Clean(command.Name);
            if (name == "")
                return new CreateCompanyResult(null, false, "Lütfen şirket adını giriniz.");

            var key = NameMatching.Key(name);
            var candidates = await db.Company.Where(c => !c.Deleted).Select(c => new { c.Id, c.Name }).ToListAsync();
            var existing = candidates.FirstOrDefault(c => NameMatching.Key(c.Name) == key);
            if (existing != null)
                return new CreateCompanyResult(new Option(existing.Id, existing.Name), Existed: true);

            var company = new Company
            {
                Name = name,
                CompanyTypeId = command.TypeId,
                CompanyScaleId = command.ScaleId,
                CompanyWorkLineId = command.WorklineId,
                InstitutionKind = command.InstitutionKind,
                ServiceBranchId = command.ServiceBranchId,
            };
            db.Company.Add(company);
            await db.SaveChangesAsync();
            db.LogActivity(Actions.CompanyCreate, $"Şirket {ActivityLog.Quoted(name)}", subject: "companies", subjectId: company.Id);
            await db.SaveChangesAsync();
            return new CreateCompanyResult(new Option(company.Id, company.Name), Existed: false);
        }
    }
}
