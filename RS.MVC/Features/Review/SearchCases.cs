using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.EF;
using RS.MVC.Models;

namespace RS.MVC.Features.Review
{
    /// <summary>
    /// Existing resistances, for adding a protest or linking an article. The company leads, as researchers search
    /// today: a company lists all its cases (also as main company of a subcontractor). Free text matches the case
    /// name, its description and the company; city and "latest protest since" narrow it down.
    /// </summary>
    public record SearchCasesQuery(
        string Text,
        int? CompanyId = null,
        int? CityId = null,
        DateTime? LastProtestSince = null,
        IReadOnlyCollection<int> CompanyIds = null);

    public class SearchCasesHandler(RSDBContext db)
    {
        public const int MaxResults = 20;

        /// <summary>Ongoing cases first, then the most recent protest; deleted cases are left out.</summary>
        public async Task<IReadOnlyList<ResistanceOption>> Handle(SearchCasesQuery query) =>
            (await Rows(query)).Select(ToOption).ToList();

        /// <summary>The raw rows, with the company ids (for callers that rank by company).</summary>
        public async Task<IReadOnlyList<CaseRow>> Rows(SearchCasesQuery query) => await Build(query).ToListAsync();

        public static ResistanceOption ToOption(CaseRow r) =>
            new(r.Id, r.Name, r.CompanyName, r.LastProtestDate, r.ProtestCount, r.Ongoing, r.Cities);

        /// <summary>The database query (public so a test can check it translates to SQL Server).</summary>
        public IQueryable<CaseRow> Build(SearchCasesQuery query)
        {
            var text = query.Text?.Trim() ?? "";
            var cases = db.Resistance.Where(r => !r.Deleted);

            if (query.CompanyId is { } companyId)
                cases = cases.Where(r => r.CompanyId == companyId || r.MainCompanyId == companyId);
            if (query.CompanyIds is { Count: > 0 } companyIds)
                cases = cases.Where(r => (r.CompanyId != null && companyIds.Contains(r.CompanyId.Value))
                    || (r.MainCompanyId != null && companyIds.Contains(r.MainCompanyId.Value)));
            if (text != "")
                cases = cases.Where(r => r.Name.Contains(text) || r.Description.Contains(text)
                    || (r.Company != null && r.Company.Name.Contains(text)));
            if (query.CityId is { } cityId)
                cases = cases.Where(r => r.Protestos.Any(p => !p.Deleted && p.Locations.Any(l => l.CityId == cityId)));

            // Projected with an object initializer, so EF can filter and sort on the computed values in SQL.
            var rows = cases.Select(r => new CaseRow
            {
                Id = r.Id,
                CompanyId = r.CompanyId,
                MainCompanyId = r.MainCompanyId,
                Name = r.Name,
                CompanyName = r.Company != null ? r.Company.Name : null,
                LastProtestDate = r.Protestos.Where(p => !p.Deleted).Max(p => (DateTime?)p.StartDate),
                ProtestCount = r.Protestos.Count(p => !p.Deleted),
                Ongoing = r.Protestos.Where(p => !p.Deleted).OrderByDescending(p => p.StartDate).Select(p => p.EndDate == null).FirstOrDefault(),
                Cities = r.Protestos.Where(p => !p.Deleted).SelectMany(p => p.Locations).Select(l => l.City.Name).Distinct().ToList(),
            });

            if (query.LastProtestSince is { } since)
                rows = rows.Where(r => r.LastProtestDate >= since);

            return rows
                .OrderByDescending(r => r.Ongoing)
                .ThenByDescending(r => r.LastProtestDate)
                .Take(MaxResults);
        }

        public class CaseRow
        {
            public int Id { get; set; }
            public int? CompanyId { get; set; }
            public int? MainCompanyId { get; set; }
            public string Name { get; set; }
            public string CompanyName { get; set; }
            public DateTime? LastProtestDate { get; set; }
            public int ProtestCount { get; set; }
            public bool Ongoing { get; set; }
            public List<string> Cities { get; set; }
        }
    }
}
