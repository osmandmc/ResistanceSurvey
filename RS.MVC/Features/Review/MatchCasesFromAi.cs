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
    // R20: which existing case the article probably belongs to, and whether its protest looks already recorded, from the
    // AI's checked answer (a pipeline draft or a pasted answer). Points are simple and shown as reasons. Measured on the
    // evaluation set (2026-10-26): the right case was among the three likely ones for 14 of 18 follow-up articles.

    public record MatchCasesFromAiCommand(string ArticleId, string Answer);

    /// <param name="CompanyId">The case's company (null for event cases), so the case search can show its cases.</param>
    /// <param name="Reasons">Why, as shown: "aynı şirket", "devam ediyor", "son eylem 3 gün önce" …</param>
    /// <param name="Recorded">A protest of this case that looks like the one the article tells of (null: none).</param>
    public record AiCaseMatch(ResistanceOption Case, int? CompanyId, int Points, IReadOnlyList<string> Reasons, AiRecordedProtest Recorded);

    public record AiRecordedProtest(int ProtestId, DateTime StartDate, IReadOnlyList<string> Cities, IReadOnlyList<string> Types);

    /// <param name="Likely">At most three cases with a recent or ongoing protest: probably this article's case.</param>
    /// <param name="Older">The company's other, older cases: context, not a match.</param>
    public record AiCaseMatches(IReadOnlyList<AiCaseMatch> Likely, IReadOnlyList<AiCaseMatch> Older)
    {
        public static readonly AiCaseMatches None = new([], []);
    }

    public class MatchCasesFromAiHandler(RSDBContext db, CheckAiAnswerHandler check, IPipelineClient pipeline, SearchCasesHandler searchCases)
    {
        public sealed class CandidateProtest
        {
            public int Id { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public List<int> Cities { get; set; }
            public List<string> CityNames { get; set; }
            public List<int> Types { get; set; }
            public List<string> TypeNames { get; set; }
        }

        public sealed class Candidate
        {
            public int Id { get; set; }
            public int? CompanyId { get; set; }
            public int? MainCompanyId { get; set; }
            public List<CandidateProtest> Protests { get; set; }
        }

        /// <summary>
        /// The cases to score (public so a test can check it translates to SQL Server). A registered company decides;
        /// without one (a miting, an anma …) the organizations in the same city, but only cases with a recent protest:
        /// DİSK in İstanbul has many cases. Null when the answer gives neither.
        /// </summary>
        public IQueryable<Candidate> Candidates(List<int> companyIds, IReadOnlyList<int> corporationIds, List<int> cityIds, DateTime asOf)
        {
            var cases = db.Resistance.Where(r => !r.Deleted);
            if (companyIds.Count > 0)
                cases = cases.Where(r => (r.CompanyId != null && companyIds.Contains(r.CompanyId.Value))
                    || (r.MainCompanyId != null && companyIds.Contains(r.MainCompanyId.Value)));
            else if (corporationIds.Count > 0 && cityIds.Count > 0)
            {
                var recentSince = asOf.AddDays(-RecentDays);
                var corporations = corporationIds.ToList();
                cases = cases.Where(r => r.ResistanceCorporations.Any(rc => corporations.Contains(rc.CorporationId))
                    && r.Protestos.Any(pr => !pr.Deleted && pr.StartDate >= recentSince && pr.Locations.Any(l => cityIds.Contains(l.CityId))));
            }
            else
                return null;

            return cases
                .Select(r => new Candidate
                {
                    Id = r.Id,
                    CompanyId = r.CompanyId,
                    MainCompanyId = r.MainCompanyId,
                    Protests = r.Protestos.Where(pr => !pr.Deleted).Select(pr => new CandidateProtest
                    {
                        Id = pr.Id,
                        StartDate = pr.StartDate,
                        EndDate = pr.EndDate,
                        Cities = pr.Locations.Select(l => l.CityId).ToList(),
                        CityNames = pr.Locations.Select(l => l.City.Name).ToList(),
                        Types = pr.ProtestoProtestoTypes.Select(t => t.ProtestoTypeId).ToList(),
                        TypeNames = pr.ProtestoProtestoTypes.Select(t => t.ProtestoType.Name).ToList(),
                    }).ToList(),
                })
                .OrderBy(r => r.Id)
                .Take(200);
        }

        public const int MaxLikely = 3;
        public const int MaxOlder = 3;

        /// <summary>A protest this recent (or still ongoing) makes a case "likely".</summary>
        public const int RecentDays = 60;

        /// <summary>A protest within this many days of the article's protest, same city and type, looks already recorded.</summary>
        public const int SameProtestDays = 2;

        public async Task<AiCaseMatches> Handle(MatchCasesFromAiCommand command)
        {
            var answer = await check.Handle(new CheckAiAnswerCommand(command.Answer));
            if (answer.Error != null || !answer.Relevant || answer.Case == null)
                return AiCaseMatches.None;
            var article = await pipeline.GetArticleAsync(command.ArticleId);
            var asOf = (article?.PublishedAt ?? DateTimeOffset.UtcNow).Date;

            var c = answer.Case;
            var p = answer.Protest;
            var companyIds = new[] { c.CompanyId, c.MainCompanyId }.Where(id => id != null).Select(id => id.Value).ToList();
            var cityIds = (p?.Locations ?? []).Select(l => l.CityId).Distinct().ToList();
            var typeIds = (p?.ProtestTypeIds ?? []).ToList();
            var start = p?.StartDate is { } s && DateTime.TryParse(s, out var d) ? d.Date : asOf;

            var query = Candidates(companyIds, c.CorporationIds, cityIds, asOf);
            if (query == null)
                return AiCaseMatches.None;
            var candidates = await query.ToListAsync();

            var companyOf = candidates.ToDictionary(r => r.Id, r => r.CompanyId);
            var scored = new List<(int Id, int Points, List<string> Reasons, bool Likely, DateTime Last, AiRecordedProtest Recorded)>();
            foreach (var r in candidates)
            {
                var protests = r.Protests.Where(pr => pr.StartDate.Date <= asOf.AddDays(SameProtestDays)).ToList();
                if (protests.Count == 0)
                    continue;
                int points = 0;
                var reasons = new List<string>();
                if (c.CompanyId is { } companyId && (r.CompanyId == companyId || r.MainCompanyId == companyId))
                {
                    points += 5;
                    reasons.Add("aynı şirket");
                }
                else if (c.MainCompanyId is { } mainId && (r.MainCompanyId == mainId || r.CompanyId == mainId))
                {
                    points += 4;
                    reasons.Add("aynı ana şirket");
                }
                else
                {
                    points += 3;
                    reasons.Add("aynı örgüt, aynı il");
                }
                if (companyIds.Count > 0 && cityIds.Count > 0 && protests.Any(pr => pr.Cities.Any(cityIds.Contains)))
                {
                    points += 1;
                    reasons.Add("aynı il");
                }
                var last = protests.Max(pr => pr.StartDate.Date);
                var ongoing = protests.Any(pr => pr.StartDate.Date >= asOf.AddDays(-180) && (pr.EndDate == null || pr.EndDate.Value.Date >= asOf));
                var days = (asOf - last).Days;
                if (ongoing)
                {
                    points += 2;
                    reasons.Add("devam ediyor");
                }
                if (days <= RecentDays)
                {
                    points += 2;
                    reasons.Add(days <= 0 ? "son eylem bugün" : $"son eylem {days} gün önce");
                }
                var same = protests
                    .Where(pr => Math.Abs((pr.StartDate.Date - start).TotalDays) <= SameProtestDays
                        && (cityIds.Count == 0 || pr.Cities.Any(cityIds.Contains))
                        && (typeIds.Count == 0 || pr.Types.Any(typeIds.Contains)))
                    .OrderBy(pr => Math.Abs((pr.StartDate.Date - start).TotalDays))
                    .FirstOrDefault();
                var recorded = same == null ? null : new AiRecordedProtest(same.Id, same.StartDate, same.CityNames.Distinct().ToList(), same.TypeNames);
                scored.Add((r.Id, points, reasons, ongoing || days <= RecentDays, last, recorded));
            }

            var likely = scored.Where(x => x.Likely).OrderByDescending(x => x.Points).ThenByDescending(x => x.Last).Take(MaxLikely).ToList();
            var older = companyIds.Count == 0 ? [] : scored.Where(x => !x.Likely).OrderByDescending(x => x.Last).Take(MaxOlder).ToList();
            var ids = likely.Concat(older).Select(x => x.Id).ToList();
            if (ids.Count == 0)
                return AiCaseMatches.None;
            var options = (await searchCases.Rows(new SearchCasesQuery("", CaseIds: ids))).ToDictionary(x => x.Id, SearchCasesHandler.ToOption);
            AiCaseMatch Match((int Id, int Points, List<string> Reasons, bool Likely, DateTime Last, AiRecordedProtest Recorded) x) =>
                new(options[x.Id], companyOf[x.Id], x.Points, x.Reasons, x.Recorded);
            return new AiCaseMatches(
                likely.Where(x => options.ContainsKey(x.Id)).Select(Match).ToList(),
                older.Where(x => options.ContainsKey(x.Id)).Select(Match).ToList());
        }
    }
}
