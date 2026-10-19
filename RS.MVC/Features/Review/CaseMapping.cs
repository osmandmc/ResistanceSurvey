using System.Collections.Generic;
using System.Linq;
using RS.COMMON.DTO;
using RS.COMMON.Entities;
using RS.COMMON.Entities.LookupEntity;
using RS.MVC.Models;

namespace RS.MVC.Features.Review
{
    /// <summary>
    /// Maps the review form (ids only) onto the models the existing save code takes (ResistanceApplication.Create
    /// and AddProtesto), until that code moves into handlers too.
    /// </summary>
    public static class CaseMapping
    {
        /// <summary><paramref name="reasonIds"/> and <paramref name="corporationIds"/>: the case's lists after typed values were resolved.</summary>
        public static ResistanceModel ToResistanceModel(
            CaseInput c, ProtestInput p, News news, string userName, IReadOnlyList<int> reasonIds, IReadOnlyList<int> corporationIds)
        {
            var againstCompany = c.TargetType == ResistanceTargetType.Company;
            return new ResistanceModel
            {
                UserName = userName,
                Name = c.Name,
                ResistanceDescription = c.Description,
                CategoryId = c.CategoryId!.Value,
                TargetType = c.TargetType!.Value,
                CompanyId = againstCompany && c.CompanyId is { } companyId ? new Company { Id = companyId } : null,
                MainCompanyId = againstCompany && c.IsOutsource && c.MainCompanyId is { } mainId ? new Company { Id = mainId } : null,
                CorporationId = againstCompany ? null : c.TargetCorporationId,
                ResistanceReasonIds = reasonIds.Select(id => new ResistanceReason { Id = id }).ToList(),
                DevelopRight = c.DevelopRight!.Value,
                EmployeeCount = c.EmployeeCount,
                EmployeeCountId = c.EmployeeCountRangeId,
                CorporationIds = corporationIds.Select(id => new Corporation { Id = id }).ToList(),
                TradeUnionAuthorityId = c.TradeUnionAuthorityId,
                TradeUnionId = c.TradeUnionId,
                EmploymentTypeIds = c.EmploymentTypeIds.Select(id => new EmploymentType { Id = id }).ToList(),
                FiredEmployeeCountByProtesto = c.FiredEmployeeCount ?? 0,
                ResistanceResult = c.Result!.Value,
                ResistanceNews = [news],
                Protesto = new ProtestoModel
                {
                    ProtestoTypeIds = p.ProtestTypeIds.Select(id => new ProtestoType { Id = id }).ToList(),
                    StrikeDuration = p.StrikeDurationSeconds ?? 0,
                    SimpleProtestoDescription = p.SimpleDescription,
                    ProtestoStartDate = p.StartDate!.Value,
                    ProtestoEndDate = p.EndDate,
                    ProtestoPlaceIds = p.PlaceIds.Select(id => new ProtestoPlace { Id = id }).ToList(),
                    GenderId = p.GenderId!.Value,
                    Locations = ToLocations(p),
                    EmployeeCountInProtesto = p.EmployeeCount,
                    EmployeeCountInProtestoId = p.EmployeeCountRangeId,
                    InterventionTypeIds = p.InterventionTypeIds.Select(id => new InterventionType { Id = id }).ToList(),
                    CustodyCount = p.CustodyCount,
                    Note = p.Note,
                },
            };
        }

        public static ProtestoAddModel ToProtestoAddModel(int resistanceId, ProtestInput p, string userName) => new()
        {
            ResistanceId = resistanceId,
            UserName = userName,
            ProtestoTypeIds = p.ProtestTypeIds,
            StrikeDuration = p.StrikeDurationSeconds ?? 0,
            SimpleProtestoDescription = p.SimpleDescription,
            ProtestoStartDate = p.StartDate!.Value,
            ProtestoEndDate = p.EndDate,
            ProtestoPlaceIds = p.PlaceIds,
            GenderId = p.GenderId!.Value,
            Locations = ToLocations(p),
            EmployeeCountInProtesto = p.EmployeeCount,
            EmployeeCountInProtestoId = p.EmployeeCountRangeId,
            InterventionTypeIds = p.InterventionTypeIds,
            CustodyCount = p.CustodyCount,
            Note = p.Note,
        };

        private static List<ProtestoLocationModel> ToLocations(ProtestInput p) =>
            (p.Locations ?? []).Select(l => new ProtestoLocationModel { CityId = l.CityId, DistrictId = l.DistrictId, Place = l.Place }).ToList();
    }
}
