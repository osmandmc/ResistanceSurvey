using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.COMMON.Entities;
using RS.EF;
using RS.MVC.Features.Activity;

namespace RS.MVC.Features.Review
{
    /// <summary>A workplace (R37) as the form offers it: where it is and whom it belongs to (to suggest the case's above).</summary>
    public record WorkplaceOption(int Id, string Name, string CityName, string DistrictName, int? OwnerCompanyId, string OwnerName, bool Approved);

    /// <summary>
    /// Text: part of the name. CompanyId, without text: the places known for that company, used in its cases (as employer
    /// or as the organization above) or belonging to it, most used first; this is how the form suggests a place.
    /// </summary>
    public record SearchWorkplacesQuery(string Text, int? CompanyId = null);

    public class SearchWorkplacesHandler(RSDBContext db)
    {
        public const int MaxResults = 20;

        public async Task<IReadOnlyList<WorkplaceOption>> Handle(SearchWorkplacesQuery query)
        {
            var places = db.Workplace.Where(w => !w.Deleted);
            var text = query.Text?.Trim() ?? "";
            if (text != "")
                places = places.Where(w => w.Name.Contains(text)).OrderBy(w => w.Name);
            else if (query.CompanyId is { } companyId)
                places = places
                    .Where(w => w.OwnerCompanyId == companyId
                        || db.Resistance.Any(r => !r.Deleted && r.WorkplaceId == w.Id && (r.CompanyId == companyId || r.MainCompanyId == companyId)))
                    .OrderByDescending(w => db.Resistance.Count(r => !r.Deleted && r.WorkplaceId == w.Id && (r.CompanyId == companyId || r.MainCompanyId == companyId)))
                    .ThenBy(w => w.Name);
            else
                return [];
            return await places.Take(MaxResults)
                .Select(w => new WorkplaceOption(w.Id, w.Name, w.City != null ? w.City.Name : null, w.District != null ? w.District.Name : null,
                    w.OwnerCompanyId, w.OwnerCompany != null ? w.OwnerCompany.Name : null, w.Approved))
                .ToListAsync();
        }
    }

    /// <summary>“Yeni işyeri”: a place typed in a form, waiting for approval like other typed values (Approved: from Tanımlar).</summary>
    public record CreateWorkplaceCommand(string Name, int? CityId, int? DistrictId, int? OwnerCompanyId, bool Approved = false);

    /// <summary>Existed: a place with that name in that province was already there and is returned instead of a duplicate.</summary>
    public record CreateWorkplaceResult(WorkplaceOption Workplace, bool Existed, string Error = null);

    public class CreateWorkplaceHandler(RSDBContext db)
    {
        public async Task<CreateWorkplaceResult> Handle(CreateWorkplaceCommand command)
        {
            var name = NameMatching.Clean(command.Name);
            if (name == "")
                return new CreateWorkplaceResult(null, false, "Lütfen işyerinin adını giriniz.");
            if (command.CityId is { } cityId && !await db.City.AnyAsync(c => c.Id == cityId))
                return new CreateWorkplaceResult(null, false, "Bilinmeyen il.");
            if (command.DistrictId is { } districtId && !await db.District.AnyAsync(d => d.Id == districtId && d.CityId == command.CityId))
                return new CreateWorkplaceResult(null, false, "İlçe bu ile ait değil.");
            if (command.OwnerCompanyId is { } ownerId && !await db.Company.AnyAsync(c => c.Id == ownerId && !c.Deleted))
                return new CreateWorkplaceResult(null, false, "Bilinmeyen şirket ya da kurum.");

            // The same name in the same province is the same place (case, punctuation and Turkish letters ignored).
            var key = NameMatching.Key(name);
            var candidates = await db.Workplace.Where(w => !w.Deleted && w.CityId == command.CityId).Select(w => new { w.Id, w.Name }).ToListAsync();
            var existing = candidates.FirstOrDefault(w => NameMatching.Key(w.Name) == key);
            if (existing != null)
                return new CreateWorkplaceResult(await OptionAsync(existing.Id), Existed: true);

            var workplace = new Workplace
            {
                Name = name,
                CityId = command.CityId,
                DistrictId = command.DistrictId,
                OwnerCompanyId = command.OwnerCompanyId,
                Approved = command.Approved,
            };
            db.Workplace.Add(workplace);
            await db.SaveChangesAsync();
            db.LogActivity(Actions.WorkplaceCreate, $"İşyeri {ActivityLog.Quoted(name)}", subject: "workplaces", subjectId: workplace.Id);
            await db.SaveChangesAsync();
            return new CreateWorkplaceResult(await OptionAsync(workplace.Id), Existed: false);
        }

        private Task<WorkplaceOption> OptionAsync(int id) =>
            db.Workplace.Where(w => w.Id == id)
                .Select(w => new WorkplaceOption(w.Id, w.Name, w.City != null ? w.City.Name : null, w.District != null ? w.District.Name : null,
                    w.OwnerCompanyId, w.OwnerCompany != null ? w.OwnerCompany.Name : null, w.Approved))
                .FirstAsync();
    }
}
