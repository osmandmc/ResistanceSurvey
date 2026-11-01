using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.COMMON.Constants;
using static RS.COMMON.Constants.Enums;
using RS.COMMON.Entities;
using RS.COMMON.Entities.LookupEntity;
using RS.EF;
using RS.MVC.Features.Cases;

namespace RS.MVC.Features.Analysis
{
    /// <summary>
    /// Analiz (R38, part 2): the numbers behind the status charts, for the cases the case list's filters select. Protest
    /// counts take only the protests in the selected year and month; a case's year is the year of its first protest in
    /// that period. The data is small (thousands of cases), so the rows are loaded once and counted in memory.
    /// </summary>
    public record CaseAnalysisQuery(CaseFilter Filter);

    /// <summary>One line or bar group of a chart: a value per month or year, in the order of the chart's axis.</summary>
    public record AnalysisSeries(string Name, IReadOnlyList<int> Values);

    public record AnalysisCount(string Name, int Count);

    /// <summary>Case results in a group (Tam, Yarım, Sıfır Kazanım, Bilinmiyor).</summary>
    public record AnalysisResults(string Group, int Full, int Partial, int Zero, int Unknown);

    /// <summary>Per year: how many cases have no known result or no union recorded, and protests without a place.</summary>
    public record AnalysisCompleteness(int Year, int Cases, int UnknownResult, int NoUnion, int Protests, int NoLocation);

    public record AnalysisTotals(int Cases, int Protests, int Provinces, int KnownResults);

    /// <summary>
    /// The map (R36): per province, its protests (a protest in several provinces counts in each) and its cases (each
    /// once). CityId is the plate code, as in the map's shapes.
    /// </summary>
    public record AnalysisProvince(int CityId, string Name, int Protests, int Cases);

    public record CaseAnalysis(
        AnalysisTotals Totals,
        IReadOnlyList<string> Months,
        IReadOnlyList<AnalysisSeries> ProtestsByCategory,
        IReadOnlyList<int> ActiveCases,
        IReadOnlyList<int> Years,
        IReadOnlyList<AnalysisSeries> ReasonsByYear,
        IReadOnlyList<AnalysisSeries> ProtestTypesByYear,
        IReadOnlyList<AnalysisSeries> EmploymentByYear,
        IReadOnlyList<AnalysisCount> Confederations,
        IReadOnlyList<AnalysisResults> ResultsByEmployment,
        IReadOnlyList<AnalysisResults> ResultsByUnion,
        IReadOnlyList<AnalysisCount> Durations,
        IReadOnlyList<AnalysisCompleteness> Completeness,
        IReadOnlyList<AnalysisProvince> Provinces);

    public class CaseAnalysisHandler(RSDBContext db)
    {
        /// <summary>Reasons, protest types and employment types beyond this many are summed as “Diğer”.</summary>
        public const int TopN = 8;

        /// <summary>
        /// Years with fewer protests than this before the first busier year are left off the axes: a handful of entries
        /// dated 2010–2018 (and one in the year 202) would otherwise stretch every chart.
        /// </summary>
        public const int MinProtestsForFirstYear = 10;

        public const string Other = "Diğer";
        public const string NoUnion = "Sendika kaydı yok";
        public const string UnionWithoutConfederation = "Konfederasyonu bilinmeyen sendika";

        public sealed class CaseRow
        {
            public int Id { get; set; }
            public int CategoryId { get; set; }
            public ResistanceResult Result { get; set; }
            public List<int> Reasons { get; set; }
            public List<int> EmploymentTypes { get; set; }
            public List<int> Organizations { get; set; }
            /// <summary>The old single union field (set on a few hundred older cases).</summary>
            public int? LegacyUnionId { get; set; }
        }

        public sealed class ProtestRow
        {
            public int CaseId { get; set; }
            public DateTime Start { get; set; }
            public DateTime? End { get; set; }
            public List<int> Types { get; set; }
            public List<int> Cities { get; set; }
        }

        /// <summary>The case rows (public so a test can check they translate to SQL Server).</summary>
        public static IQueryable<CaseRow> Cases(IQueryable<Resistance> cases) => cases.Select(r => new CaseRow
        {
            Id = r.Id,
            CategoryId = r.CategoryId,
            Result = r.ResistanceResult,
            Reasons = r.ResistanceResistanceReasons.Select(x => x.ResistanceReasonId).ToList(),
            EmploymentTypes = r.ResistanceEmploymentTypes.Select(x => x.EmploymentTypeId).ToList(),
            Organizations = r.ResistanceCorporations.Select(x => x.CorporationId).ToList(),
            LegacyUnionId = r.TradeUnionId,
        });

        /// <summary>Every union and confederation → its confederation (a confederation is its own; null: none known).</summary>
        public static IQueryable<KeyValuePair<int, int?>> Unions(RSDBContext db) =>
            db.Corporation.Where(c => c is TradeUnion || c is Confederation)
                .Select(c => new KeyValuePair<int, int?>(c.Id, c is Confederation ? c.Id : ((TradeUnion)c).ConfederationId));

        /// <summary>The protests of the selected cases in the selected period.</summary>
        public static IQueryable<ProtestRow> Protests(IQueryable<Resistance> cases, CaseFilter filter)
        {
            var protests = cases.SelectMany(r => r.Protestos).Where(p => !p.Deleted);
            if (filter.Year is { } year)
                protests = protests.Where(p => p.StartDate.Year == year);
            if (filter.Month is { } month)
                protests = protests.Where(p => p.StartDate.Month == month);
            return protests.Select(p => new ProtestRow
            {
                CaseId = p.ResistanceId,
                Start = p.StartDate,
                End = p.EndDate,
                Types = p.ProtestoProtestoTypes.Select(t => t.ProtestoTypeId).ToList(),
                Cities = p.Locations.Select(l => l.CityId).ToList(),
            });
        }

        public async Task<CaseAnalysis> Handle(CaseAnalysisQuery query)
        {
            var filter = query.Filter ?? new CaseFilter();
            var selected = filter.Apply(db.Resistance);
            var cases = await Cases(selected).AsSplitQuery().ToListAsync();
            var protests = await Protests(selected, filter).AsSplitQuery().ToListAsync();

            var categories = await db.Category.ToDictionaryAsync(c => c.Id, c => c.Name);
            var reasons = await db.ResistanceReason.ToDictionaryAsync(c => c.Id, c => c.Name);
            var protestTypes = await db.ProtestoType.ToDictionaryAsync(c => c.Id, c => c.Name);
            var employmentTypes = await db.EmploymentType.ToDictionaryAsync(c => c.Id, c => c.Name);
            var confederations = await db.Set<Confederation>().ToDictionaryAsync(c => c.Id, c => c.Name);
            var unions = (await Unions(db).ToListAsync()).ToDictionary(u => u.Key, u => u.Value);
            var cities = await db.City.ToDictionaryAsync(c => c.Id, c => c.Name);
            return Build(cases, protests, categories, reasons, protestTypes, employmentTypes, confederations, unions, cities);
        }

        /// <summary>The counting, without the database.</summary>
        public static CaseAnalysis Build(
            IReadOnlyList<CaseRow> cases, IReadOnlyList<ProtestRow> protests, IReadOnlyDictionary<int, string> categories,
            IReadOnlyDictionary<int, string> reasons, IReadOnlyDictionary<int, string> protestTypes,
            IReadOnlyDictionary<int, string> employmentTypes, IReadOnlyDictionary<int, string> confederations,
            IReadOnlyDictionary<int, int?> unions, IReadOnlyDictionary<int, string> cities)
        {
            IEnumerable<int?> UnionsOf(CaseRow c) =>
                c.Organizations.Append(c.LegacyUnionId ?? 0).Distinct().Where(unions.ContainsKey).Select(id => unions[id]);
            bool HasUnion(CaseRow c) => UnionsOf(c).Any();

            var byId = cases.ToDictionary(c => c.Id);
            protests = protests.Where(p => byId.ContainsKey(p.CaseId)).ToList();

            // The axes: from the first year with enough protests to the last protest.
            var perYear = protests.GroupBy(p => p.Start.Year).ToDictionary(g => g.Key, g => g.Count());
            var firstYear = perYear.Where(y => y.Value >= MinProtestsForFirstYear).Select(y => (int?)y.Key).Min()
                ?? perYear.Keys.Select(y => (int?)y).Min();
            var inAxis = protests.Where(p => firstYear != null && p.Start.Year >= firstYear).ToList();
            var months = MonthsBetween(inAxis);
            var monthIndex = months.Select((m, i) => (m, i)).ToDictionary(x => x.m, x => x.i);
            var years = inAxis.Select(p => p.Start.Year).Distinct().Order().ToList();

            // A case's year: its first protest in the period.
            var caseYear = inAxis.GroupBy(p => p.CaseId).ToDictionary(g => g.Key, g => g.Min(p => p.Start).Year);
            var axisCases = cases.Where(c => caseYear.ContainsKey(c.Id)).ToList();

            // Protests per month by category, and the cases with a protest that month.
            var categoryOrder = new[] { KnownListIds.WorkplaceCategory, KnownListIds.GeneralCategory, KnownListIds.SolidarityCategory };
            var protestsByCategory = categoryOrder
                .Select(id => new AnalysisSeries(
                    categories.GetValueOrDefault(id) ?? id.ToString(),
                    CountPer(months, monthIndex, inAxis.Where(p => byId[p.CaseId].CategoryId == id), p => MonthKey(p.Start))))
                .ToList();
            var activeCases = months.Select(m => inAxis.Where(p => MonthKey(p.Start) == m).Select(p => p.CaseId).Distinct().Count()).ToList();

            var reasonsByYear = TopSeries(years, axisCases.SelectMany(c => c.Reasons.Distinct().Select(r => (Key: r, caseYear[c.Id]))), reasons);
            var typesByYear = TopSeries(years, inAxis.SelectMany(p => p.Types.Distinct().Select(t => (Key: t, p.Start.Year))), protestTypes);
            var employmentByYear = TopSeries(years, axisCases.SelectMany(c => c.EmploymentTypes.Distinct().Select(e => (Key: e, caseYear[c.Id]))), employmentTypes);

            var confederationCounts = axisCases
                .SelectMany(c => ConfederationsOf(UnionsOf(c).ToList(), confederations))
                .GroupBy(n => n)
                .Select(g => new AnalysisCount(g.Key, g.Count()))
                .OrderByDescending(c => c.Count).ThenBy(c => c.Name, StringComparer.Create(TurkishCulture, false))
                .ToList();

            var resultsByEmployment = axisCases
                .SelectMany(c => c.EmploymentTypes.Distinct().Select(e => (Group: employmentTypes.GetValueOrDefault(e) ?? e.ToString(), c.Result)))
                .GroupBy(x => x.Group)
                .Select(g => Results(g.Key, g.Select(x => x.Result)))
                .OrderByDescending(r => r.Full + r.Partial + r.Zero + r.Unknown)
                .Take(TopN)
                .ToList();
            var resultsByUnion = new[] { true, false }
                .Select(union => Results(union ? "Sendika kaydı var" : NoUnion, axisCases.Where(c => HasUnion(c) == union).Select(c => c.Result)))
                .ToList();

            var durations = Durations(inAxis);
            var completeness = years.Select(y =>
            {
                var yearCases = axisCases.Where(c => caseYear[c.Id] == y).ToList();
                var yearProtests = inAxis.Where(p => p.Start.Year == y).ToList();
                return new AnalysisCompleteness(y, yearCases.Count, yearCases.Count(c => c.Result == ResistanceResult.Unknown),
                    yearCases.Count(c => !HasUnion(c)), yearProtests.Count, yearProtests.Count(p => p.Cities.Count == 0));
            }).ToList();

            var totals = new AnalysisTotals(
                axisCases.Count, inAxis.Count, inAxis.SelectMany(p => p.Cities).Distinct().Count(),
                axisCases.Count(c => c.Result != ResistanceResult.Unknown));

            var provinces = inAxis
                .SelectMany(p => p.Cities.Distinct().Select(city => (City: city, p.CaseId)))
                .GroupBy(x => x.City)
                .Select(g => new AnalysisProvince(g.Key, cities.GetValueOrDefault(g.Key) ?? g.Key.ToString(), g.Count(), g.Select(x => x.CaseId).Distinct().Count()))
                .OrderByDescending(p => p.Protests)
                .ToList();

            return new CaseAnalysis(totals, months, protestsByCategory, activeCases, years, reasonsByYear, typesByYear,
                employmentByYear, confederationCounts, resultsByEmployment, resultsByUnion, durations, completeness, provinces);
        }

        private static readonly System.Globalization.CultureInfo TurkishCulture = new("tr-TR");

        private static string MonthKey(DateTime date) => $"{date.Year:0000}-{date.Month:00}";

        private static List<string> MonthsBetween(IReadOnlyCollection<ProtestRow> protests)
        {
            if (protests.Count == 0)
                return [];
            var first = protests.Min(p => p.Start);
            var last = protests.Max(p => p.Start);
            var months = new List<string>();
            for (var m = new DateTime(first.Year, first.Month, 1); m <= last; m = m.AddMonths(1))
                months.Add(MonthKey(m));
            return months;
        }

        private static List<int> CountPer(List<string> axis, Dictionary<string, int> index, IEnumerable<ProtestRow> rows, Func<ProtestRow, string> key)
        {
            var values = new int[axis.Count];
            foreach (var row in rows)
                if (index.TryGetValue(key(row), out var i))
                    values[i]++;
            return values.ToList();
        }

        /// <summary>The TopN keys by total as series over the years, the rest summed as “Diğer”.</summary>
        private static List<AnalysisSeries> TopSeries(List<int> years, IEnumerable<(int Key, int Year)> items, IReadOnlyDictionary<int, string> names)
        {
            var list = items.ToList();
            var top = list.GroupBy(x => x.Key).OrderByDescending(g => g.Count()).Take(TopN).Select(g => g.Key).ToHashSet();
            var series = list.Where(x => top.Contains(x.Key))
                .GroupBy(x => x.Key)
                .OrderByDescending(g => g.Count())
                .Select(g => new AnalysisSeries(names.GetValueOrDefault(g.Key) ?? g.Key.ToString(), PerYear(years, g.Select(x => x.Year))))
                .ToList();
            var rest = list.Where(x => !top.Contains(x.Key)).Select(x => x.Year).ToList();
            if (rest.Count > 0)
                series.Add(new AnalysisSeries(Other, PerYear(years, rest)));
            return series;
        }

        private static List<int> PerYear(List<int> years, IEnumerable<int> values)
        {
            var counts = values.GroupBy(y => y).ToDictionary(g => g.Key, g => g.Count());
            return years.Select(y => counts.GetValueOrDefault(y)).ToList();
        }

        /// <summary>The confederations of a case's unions (each union's confederation id), each once; or why there is none.</summary>
        private static IEnumerable<string> ConfederationsOf(List<int?> unionConfederations, IReadOnlyDictionary<int, string> confederations)
        {
            if (unionConfederations.Count == 0)
                return [NoUnion];
            var named = unionConfederations.Where(id => id != null)
                .Select(id => confederations.GetValueOrDefault(id.Value) ?? UnionWithoutConfederation)
                .Distinct().ToList();
            return named.Count > 0 ? named : [UnionWithoutConfederation];
        }

        private static AnalysisResults Results(string group, IEnumerable<ResistanceResult> results)
        {
            var list = results.ToList();
            return new AnalysisResults(group, list.Count(r => r == ResistanceResult.FullGain), list.Count(r => r == ResistanceResult.PartialGain),
                list.Count(r => r == ResistanceResult.ZeroGain), list.Count(r => r == ResistanceResult.Unknown));
        }

        /// <summary>How long cases lasted: from the first protest's start to the last protest's end (or start).</summary>
        private static List<AnalysisCount> Durations(IEnumerable<ProtestRow> protests)
        {
            var buckets = new (string Name, int MaxDays)[]
            {
                ("Tek gün", 0), ("2–7 gün", 6), ("8–30 gün", 29), ("1–3 ay", 90), ("3–12 ay", 365), ("1 yıldan uzun", int.MaxValue),
            };
            var days = protests.GroupBy(p => p.CaseId)
                .Select(g => (g.Max(p => p.End ?? p.Start).Date - g.Min(p => p.Start).Date).Days)
                .ToList();
            return buckets.Select((b, i) => new AnalysisCount(b.Name,
                days.Count(d => d <= b.MaxDays && (i == 0 || d > buckets[i - 1].MaxDays)))).ToList();
        }
    }
}
