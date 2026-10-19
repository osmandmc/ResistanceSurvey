using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.EF;

namespace RS.MVC.Features.Review
{
    public record Option(int Id, string Name);

    public record CorporationOption(int Id, string Name, bool IsTradeUnion);

    public record DistrictOption(int Id, string Name, int CityId);

    /// <summary>Every list the case form needs, in one answer (the old form made about 16 calls).</summary>
    public record CaseFormOptions(
        IReadOnlyList<Option> Categories,
        IReadOnlyList<Option> Reasons,
        IReadOnlyList<Option> EmployeeCountRanges,
        IReadOnlyList<CorporationOption> Corporations,
        IReadOnlyList<Option> TradeUnions,
        IReadOnlyList<Option> TradeUnionAuthorities,
        IReadOnlyList<Option> EmploymentTypes,
        IReadOnlyList<Option> CompanyTypes,
        IReadOnlyList<Option> CompanyScales,
        IReadOnlyList<Option> CompanyWorklines,
        IReadOnlyList<Option> ProtestTypes,
        IReadOnlyList<Option> ProtestPlaces,
        IReadOnlyList<Option> Genders,
        IReadOnlyList<Option> ProtestEmployeeCountRanges,
        IReadOnlyList<Option> Cities,
        IReadOnlyList<DistrictOption> Districts,
        IReadOnlyList<Option> InterventionTypes);

    public record GetCaseFormOptionsQuery;

    public class GetCaseFormOptionsHandler(RSDBContext db)
    {
        /// <summary>Corporation type meaning "union": selecting one shows the union-authority field.</summary>
        public const int TradeUnionCorporationTypeId = 1;

        public async Task<CaseFormOptions> Handle(GetCaseFormOptionsQuery _) => new(
            Categories: await db.Category.OrderBy(e => e.Name).Select(e => new Option(e.Id, e.Name)).ToListAsync(),
            Reasons: await db.ResistanceReason.OrderBy(e => e.Name).Select(e => new Option(e.Id, e.Name)).ToListAsync(),
            EmployeeCountRanges: await db.EmployeeCount.Select(e => new Option(e.Id, e.Name)).ToListAsync(),
            Corporations: await db.Corporation.OrderBy(e => e.Name)
                .Select(e => new CorporationOption(e.Id, e.Name, e.CorporationTypeId == TradeUnionCorporationTypeId)).ToListAsync(),
            TradeUnions: await db.TradeUnion.OrderBy(e => e.Name).Select(e => new Option(e.Id, e.Name)).ToListAsync(),
            TradeUnionAuthorities: await db.TradeUnionAuthority.OrderBy(e => e.Name).Select(e => new Option(e.Id, e.Name)).ToListAsync(),
            EmploymentTypes: await db.EmploymentType.OrderBy(e => e.Name).Select(e => new Option(e.Id, e.Name)).ToListAsync(),
            CompanyTypes: await db.CompanyType.OrderBy(e => e.Name).Select(e => new Option(e.Id, e.Name)).ToListAsync(),
            CompanyScales: await db.CompanyScale.OrderBy(e => e.Name).Select(e => new Option(e.Id, e.Name)).ToListAsync(),
            CompanyWorklines: await db.CompanyWorkLine.OrderBy(e => e.Name).Select(e => new Option(e.Id, e.Name)).ToListAsync(),
            ProtestTypes: await db.ProtestoType.OrderBy(e => e.Name).Select(e => new Option(e.Id, e.Name)).ToListAsync(),
            ProtestPlaces: await db.ProtestoPlace.OrderBy(e => e.Name).Select(e => new Option(e.Id, e.Name)).ToListAsync(),
            Genders: await db.Gender.OrderBy(e => e.Name).Select(e => new Option(e.Id, e.Name)).ToListAsync(),
            ProtestEmployeeCountRanges: await db.ProtestoEmployeeCount.Select(e => new Option(e.Id, e.Name)).ToListAsync(),
            Cities: await db.City.OrderBy(e => e.Name).Select(e => new Option(e.Id, e.Name)).ToListAsync(),
            Districts: await db.District.OrderBy(e => e.Name).Select(e => new DistrictOption(e.Id, e.Name, e.CityId)).ToListAsync(),
            InterventionTypes: await db.InterventionType.OrderBy(e => e.Name).Select(e => new Option(e.Id, e.Name)).ToListAsync());
    }
}
