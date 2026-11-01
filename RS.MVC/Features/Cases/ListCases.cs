using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.EF;

namespace RS.MVC.Features.Cases
{
    /// <summary>Vakalar: a page of cases, most recent protest first.</summary>
    public record ListCasesQuery(CaseFilter Filter, int Page = 1, int PageSize = ListCasesHandler.DefaultPageSize);

    /// <summary>
    /// Target: the company, or the institution for cases against one. WorkplaceName: the case's place (R37). Ongoing: the latest protest has no end date
    /// (same rule as the review screen's case search).
    /// </summary>
    public record CaseListItem(
        int Id,
        string Name,
        string Target,
        string WorkplaceName,
        string MainCompanyName,
        string CategoryName,
        int ProtestCount,
        DateTime? LastProtestDate,
        bool Ongoing,
        IReadOnlyList<string> Cities,
        int ArticleCount);

    public record CaseListPage(IReadOnlyList<CaseListItem> Items, int Total, int Page, int PageSize);

    public class ListCasesHandler(RSDBContext db)
    {
        public const int DefaultPageSize = 25;
        public const int MaxPageSize = 100;

        public async Task<CaseListPage> Handle(ListCasesQuery query)
        {
            var page = Math.Max(1, query.Page);
            var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);
            var cases = (query.Filter ?? new CaseFilter()).Apply(db.Resistance);

            var total = await cases.CountAsync();
            var rows = await Build(cases).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            var items = rows.Select(r => new CaseListItem(
                r.Id, r.Name, r.Target ?? (r.Organizers is { Count: > 0 } o ? string.Join(", ", o.Take(3)) + (o.Count > 3 ? " …" : "") : null), r.WorkplaceName, r.MainCompanyName, r.CategoryName, r.ProtestCount, r.LastProtestDate, r.Ongoing, r.Cities, r.ArticleCount)).ToList();
            return new CaseListPage(items, total, page, pageSize);
        }

        /// <summary>The page query (public so a test can check it translates to SQL Server).</summary>
        public static IQueryable<Row> Build(IQueryable<COMMON.Entities.Resistance> cases) =>
            cases
                .Select(r => new Row
                {
                    Id = r.Id,
                    Name = r.Name,
                    Target = r.Company != null ? r.Company.Name : null,
                    Organizers = r.ResistanceCorporations.Where(x => x.Role == COMMON.Entities.OrganizationRole.Organizing).Select(x => x.Corporation.Name).ToList(),
                    WorkplaceName = r.Workplace != null && !r.Workplace.Deleted ? r.Workplace.Name : null,
                    MainCompanyName = r.MainCompany != null ? r.MainCompany.Name : null,
                    CategoryName = r.Category != null ? r.Category.Name : null,
                    ProtestCount = r.Protestos.Count(p => !p.Deleted),
                    LastProtestDate = r.Protestos.Where(p => !p.Deleted).Max(p => (DateTime?)p.StartDate),
                    Ongoing = r.Protestos.Where(p => !p.Deleted).OrderByDescending(p => p.StartDate).Select(p => p.EndDate == null).FirstOrDefault(),
                    Cities = r.Protestos.Where(p => !p.Deleted).SelectMany(p => p.Locations).Select(l => l.City.Name).Distinct().ToList(),
                    ArticleCount = r.ResistanceNews.Count(),
                })
                .OrderByDescending(r => r.LastProtestDate)
                .ThenByDescending(r => r.Id);

        public class Row
        {
            public int Id { get; set; }
            public string Name { get; set; }
            /// <summary>The workplace; Genel and Dayanışma cases without one show their organizers instead (R31).</summary>
            public string Target { get; set; }
            public List<string> Organizers { get; set; }
            /// <summary>R37: the place, when the case names one (empty for a direct employer = the employer itself).</summary>
            public string WorkplaceName { get; set; }
            public string MainCompanyName { get; set; }
            public string CategoryName { get; set; }
            public int ProtestCount { get; set; }
            public DateTime? LastProtestDate { get; set; }
            public bool Ongoing { get; set; }
            public List<string> Cities { get; set; }
            public int ArticleCount { get; set; }
        }
    }
}
