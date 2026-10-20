using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.EF;
using RS.MVC.Models;

namespace RS.MVC.Features.Cases
{
    /// <summary>One case for the detail page: its fields in the form's shape, its protests and its articles.</summary>
    public record GetCaseQuery(int Id);

    /// <summary>The case part, as the case form edits it (ids), plus the names the pickers show.</summary>
    public record CaseDetail(
        int Id,
        CaseInput Case,
        string CompanyName,
        string MainCompanyName,
        string Note,
        IReadOnlyList<ProtestDetail> Protests,
        IReadOnlyList<CaseArticle> Articles,
        string Creator,
        DateTime? CreateDate,
        string Updater,
        DateTime? UpdateDate);

    /// <summary>One protest in the form's shape, plus a short summary for the list.</summary>
    public record ProtestDetail(int Id, ProtestInput Protest, IReadOnlyList<string> TypeNames, IReadOnlyList<string> CityNames);

    /// <summary>An article linked to the case (a News row). ArticleId is set for articles that came through the pipeline.</summary>
    public record CaseArticle(int NewsId, string Title, string Link, DateTime Date, string ArticleId);

    public class GetCaseHandler(RSDBContext db)
    {
        public async Task<CaseDetail> Handle(GetCaseQuery query)
        {
            var r = await db.Resistance
                .AsNoTracking()
                .Where(x => x.Id == query.Id && !x.Deleted)
                .Select(x => new
                {
                    x.Id, x.Name, x.Description, x.CategoryId, x.ResistanceTargetType, x.CompanyId, x.MainCompanyId,
                    x.CorporationId, x.DevelopRight, x.EmployeeCountNumber, x.EmployeeCountId, x.TradeUnionAuthorityId,
                    x.TradeUnionId, x.FiredEmployeeCountByProtesto, x.ResistanceResult, x.Note,
                    x.Creator, x.CreateDate, x.Updater, x.UpdateDate,
                    CompanyName = x.Company != null ? x.Company.Name : null,
                    MainCompanyName = x.MainCompany != null ? x.MainCompany.Name : null,
                    ReasonIds = x.ResistanceResistanceReasons.Select(y => y.ResistanceReasonId).ToList(),
                    CorporationIds = x.ResistanceCorporations.Select(y => y.CorporationId).ToList(),
                    EmploymentTypeIds = x.ResistanceEmploymentTypes.Select(y => y.EmploymentTypeId).ToList(),
                })
                .FirstOrDefaultAsync();
            if (r == null)
                return null;

            var protests = await db.Protesto
                .AsNoTracking()
                .Where(p => p.ResistanceId == query.Id && !p.Deleted)
                .OrderBy(p => p.StartDate)
                .Select(p => new
                {
                    p.Id, p.StartDate, p.EndDate, p.GenderId, p.CustodyCount, p.EmployeeCountNumber, p.ProtestoEmployeeCountId,
                    p.SimpleProtestoDescription, p.StrikeDuration, p.Note,
                    TypeIds = p.ProtestoProtestoTypes.Select(t => t.ProtestoTypeId).ToList(),
                    TypeNames = p.ProtestoProtestoTypes.Select(t => t.ProtestoType.Name).ToList(),
                    PlaceIds = p.ProtestoProtestoPlaces.Select(t => t.ProtestoPlaceId).ToList(),
                    InterventionTypeIds = p.ProtestoInterventionTypes.Select(t => t.InterventionTypeId).ToList(),
                    Locations = p.Locations.Select(l => new { l.CityId, l.DistrictId, l.Place, CityName = l.City.Name }).ToList(),
                })
                .ToListAsync();

            var articles = await db.ResistanceNews
                .AsNoTracking()
                .Where(rn => rn.ResistanceId == query.Id)
                .OrderBy(rn => rn.News.Date)
                .Select(rn => new CaseArticle(rn.NewsId, rn.News.Header, rn.News.Link, rn.News.Date, rn.News.ArticleId))
                .ToListAsync();

            var caseInput = new CaseInput
            {
                Name = r.Name,
                Description = r.Description,
                CategoryId = r.CategoryId,
                TargetType = r.ResistanceTargetType,
                CompanyId = r.CompanyId,
                IsOutsource = r.MainCompanyId != null,
                MainCompanyId = r.MainCompanyId,
                TargetCorporationId = r.CorporationId,
                ReasonIds = r.ReasonIds,
                DevelopRight = r.DevelopRight,
                EmployeeCount = r.EmployeeCountNumber,
                EmployeeCountRangeId = r.EmployeeCountId,
                CorporationIds = r.CorporationIds,
                TradeUnionAuthorityId = r.TradeUnionAuthorityId,
                TradeUnionId = r.TradeUnionId,
                EmploymentTypeIds = r.EmploymentTypeIds,
                FiredEmployeeCount = r.FiredEmployeeCountByProtesto,
                Result = r.ResistanceResult,
            };

            var protestDetails = protests.Select(p => new ProtestDetail(
                p.Id,
                new ProtestInput
                {
                    ProtestTypeIds = p.TypeIds,
                    StrikeDurationSeconds = p.StrikeDuration == 0 ? null : p.StrikeDuration,
                    SimpleDescription = p.SimpleProtestoDescription,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    PlaceIds = p.PlaceIds,
                    GenderId = p.GenderId,
                    Locations = p.Locations.Select(l => new LocationInput { CityId = l.CityId, DistrictId = l.DistrictId, Place = l.Place }).ToList(),
                    EmployeeCount = p.EmployeeCountNumber,
                    EmployeeCountRangeId = p.ProtestoEmployeeCountId,
                    InterventionTypeIds = p.InterventionTypeIds,
                    CustodyCount = p.CustodyCount,
                    Note = p.Note,
                },
                p.TypeNames,
                p.Locations.Select(l => l.CityName).Distinct().ToList())).ToList();

            return new CaseDetail(
                r.Id, caseInput, r.CompanyName, r.MainCompanyName, r.Note, protestDetails, articles,
                r.Creator, r.CreateDate, r.Updater, r.UpdateDate);
        }
    }
}
