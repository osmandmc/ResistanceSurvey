using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.EF;
using RS.MVC.Applications;
using RS.MVC.Models;

namespace RS.MVC.Features.Review
{
    /// <summary>"Olası vakalar": cases of the known companies named in the article.</summary>
    public record SuggestCasesForArticleQuery(string ArticleId);

    /// <summary>MatchedCompany: the company found in the article; InTitle: found in the title, not only the text.</summary>
    public record CaseSuggestion(ResistanceOption Case, int MatchedCompanyId, string MatchedCompany, bool InTitle);

    public class SuggestCasesForArticleHandler(RSDBContext db, IPipelineClient pipeline, SearchCasesHandler searchCases)
    {
        public const int MaxSuggestions = 5;

        /// <summary>At most this many companies found in one article are used (title matches first).</summary>
        public const int MaxCompanies = 10;

        /// <summary>
        /// Ranked: a company named in the title first, then ongoing cases, then the most recent protest.
        /// Empty when the article is unknown or names no known company.
        /// </summary>
        public async Task<IReadOnlyList<CaseSuggestion>> Handle(SuggestCasesForArticleQuery query)
        {
            var article = await pipeline.GetArticleAsync(query.ArticleId);
            if (article == null)
                return [];

            var companies = await db.Company.Where(c => !c.Deleted).Select(c => new { c.Id, c.Name }).ToListAsync();
            var matches = CompanyNameMatcher
                .Find(companies.Select(c => (c.Id, c.Name)), article.Title, article.Body)
                .Take(MaxCompanies)
                .ToList();
            if (matches.Count == 0)
                return [];

            var byCompany = matches.ToDictionary(m => m.CompanyId);
            var rows = await searchCases.Rows(new SearchCasesQuery("", CompanyIds: byCompany.Keys.ToList()));

            return rows
                .Select(row => (row, match: MatchFor(row, byCompany)))
                .Where(x => x.match != null)
                .OrderByDescending(x => x.match.InTitle)
                .ThenByDescending(x => x.row.Ongoing)
                .ThenByDescending(x => x.row.LastProtestDate ?? DateTime.MinValue)
                .Take(MaxSuggestions)
                .Select(x => new CaseSuggestion(SearchCasesHandler.ToOption(x.row), x.match.CompanyId, x.match.CompanyName, x.match.InTitle))
                .ToList();
        }

        // The case's own company first, else its main company (a subcontractor's case).
        private static CompanyNameMatcher.Match MatchFor(SearchCasesHandler.CaseRow row, Dictionary<int, CompanyNameMatcher.Match> byCompany)
        {
            if (row.CompanyId is { } companyId && byCompany.TryGetValue(companyId, out var own))
                return own;
            if (row.MainCompanyId is { } mainId && byCompany.TryGetValue(mainId, out var main))
                return main;
            return null;
        }
    }
}
