using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.COMMON.Entities;
using RS.EF;
using RS.MVC.Applications;
using RS.MVC.Features.Definitions;
using RS.MVC.Features.Inbox;
using RS.MVC.Features.Review;
using RS.MVC.Models;

namespace RS.MVC.Features.Dashboard
{
    /// <summary>
    /// One field over the period. Scalars: Kept (saved as suggested), Changed (saved another value), Removed (suggested,
    /// saved empty), Added (AI left it empty, researchers filled it). Lists count per item (Changed stays 0) and list
    /// the items in Items.
    /// </summary>
    public record AiReportField(string Field, string Label, bool IsList, int Kept, int Changed, int Removed, int Added, IReadOnlyList<AiReportItem> Items);

    public record AiReportItem(string Name, int Kept, int Removed, int Added);

    /// <summary>The answers of one origin (e.g. "pasted", "pipeline · gemini-flash-lite-latest") and prompt version.</summary>
    public record AiReportGroup(string Origin, string PromptVersion, int Count);

    /// <summary>
    /// What researchers did with the articles the pipeline's AI judged (R19d). Columns: linked to a case, then dismissed
    /// per reason ("NotLabour", "AlreadyRecorded", "OutOfScope", "Other", "Unknown" for older dismissals without a kind).
    /// </summary>
    public record AiReportRelevance(IReadOnlyList<AiReportVerdict> Rows, IReadOnlyList<string> Versions, bool Truncated, string Error);

    /// <summary>Verdict: "Relevant", "Uncertain", "NotRelevant" or "NotAssessed".</summary>
    public record AiReportVerdict(string Verdict, int Linked, IReadOnlyDictionary<string, int> Dismissed);

    /// <summary>
    /// Decisions: answers compared (new case or added protest); Applied: of those, how many were put into the form with
    /// "Forma uygula". Others: answers used for "Sadece bağla" or a dismissal (nothing to compare field by field).
    /// </summary>
    public record AiReport(
        DateOnly From,
        DateOnly To,
        int Decisions,
        int Applied,
        int Others,
        IReadOnlyList<AiReportGroup> Groups,
        IReadOnlyList<AiReportField> Fields,
        AiReportRelevance Relevance);

    /// <summary>Days: the period, by decision date (answers) and arrival date (pipeline verdicts). Origin and PromptVersion narrow the answers.</summary>
    public record AiReportQuery(int Days = AiReportHandler.DefaultDays, string Origin = null, string PromptVersion = null, string RelevanceVersion = null);

    /// <summary>
    /// "AI raporu" (R19d): per field, how often researchers keep the AI's values, from the answers kept with each
    /// decision (R19c); and the pipeline's relevance verdicts against what researchers decided. Decides which fields the
    /// drafts should fill and whether a model or prompt change helped.
    /// </summary>
    public class AiReportHandler(RSDBContext db, IPipelineClient pipeline)
    {
        public const int DefaultDays = 90;
        public const int MaxDays = 365;
        public const int MaxPages = 30;

        private static readonly JsonSerializerOptions Json = JsonSerializerOptions.Web;

        public static readonly string[] DismissColumns = ["NotLabour", "AlreadyRecorded", "OutOfScope", "Other", "Unknown"];
        public static readonly string[] Verdicts = ["Relevant", "Uncertain", "NotRelevant", "NotAssessed"];

        internal record Suggested(AiCaseValues Case, AiProtestValues Protest);

        /// <summary>What was saved with an AI answer (AiSuggestion.Saved): the case for a new case, and the protest.</summary>
        internal record Saved(CaseInput Case, ProtestInput Protest);

        public async Task<AiReport> Handle(AiReportQuery query)
        {
            var days = Math.Clamp(query.Days, 1, MaxDays);
            var to = DateOnly.FromDateTime(DateTime.UtcNow);
            var from = to.AddDays(-(days - 1));
            var since = from.ToDateTime(TimeOnly.MinValue);

            var rows = await db.AiSuggestion.AsNoTracking().Where(s => s.CreatedAt >= since)
                .Select(s => new { s.Origin, s.PromptVersion, s.Decision, s.Applied, s.Suggested, s.Saved })
                .ToListAsync();
            var groups = rows.GroupBy(r => (r.Origin, r.PromptVersion))
                .Select(g => new AiReportGroup(g.Key.Origin, g.Key.PromptVersion, g.Count()))
                .OrderByDescending(g => g.Count).ToList();
            var chosen = rows.Where(r => (query.Origin == null || r.Origin == query.Origin)
                && (query.PromptVersion == null || r.PromptVersion == query.PromptVersion)).ToList();

            var compared = chosen
                .Where(r => r.Decision is AiDecision.NewCase or AiDecision.AddProtest && r.Suggested != null && r.Saved != null)
                .Select(r => (r.Decision, r.Applied, Suggested: Read<Suggested>(r.Suggested), Saved: Read<Saved>(r.Saved)))
                .Where(r => r.Suggested != null && r.Saved != null)
                .ToList();

            var names = await Names.LoadAsync(db);
            var fields = new FieldTally(names);
            foreach (var (decision, _, suggested, saved) in compared)
            {
                // Adding a protest keeps the case as it was: only the protest part is compared.
                if (decision == AiDecision.NewCase && suggested.Case != null && saved.Case != null)
                    fields.AddCase(suggested.Case, saved.Case);
                if (suggested.Protest != null && saved.Protest != null)
                    fields.AddProtest(suggested.Protest, saved.Protest);
            }

            return new AiReport(from, to, compared.Count, compared.Count(c => c.Applied), chosen.Count - compared.Count, groups,
                fields.Result(), await RelevanceAsync(from, to, query.RelevanceVersion));
        }

        internal static T Read<T>(string json) where T : class
        {
            try
            {
                return JsonSerializer.Deserialize<T>(json, Json);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>The pipeline's handled articles of the period: its relevance verdict against the app's decision.</summary>
        private async Task<AiReportRelevance> RelevanceAsync(DateOnly from, DateOnly to, string version)
        {
            var counts = Verdicts.ToDictionary(v => v, _ => (Linked: 0, Dismissed: DismissColumns.ToDictionary(c => c, _ => 0)));
            var versions = new HashSet<string>();
            string next = null;
            var pages = 0;
            try
            {
                do
                {
                    var page = await pipeline.ListArticlesAsync(new PipelineArticleQuery(from, to, Status: "all", Outcome: "handled", PageSize: 100, Next: next));
                    foreach (var article in page.Items)
                    {
                        var stage = article.Stages != null && article.Stages.TryGetValue("Relevance", out var s) ? s : null;
                        var stageVersion = VersionName(stage?.Version);
                        if (stageVersion != null)
                            versions.Add(stageVersion);
                        if (version != null && stageVersion != version)
                            continue;
                        var verdict = stage?.Answer is { } answer && counts.ContainsKey(answer) ? answer : "NotAssessed";
                        var row = counts[verdict];
                        if (article.Outcome?.Kind == "Dismissed")
                            row.Dismissed[DismissColumn(article.Outcome.Reason)]++;
                        else if (article.Outcome != null)
                            row.Linked++;
                        counts[verdict] = row;
                    }
                    next = page.Next;
                } while (next != null && ++pages < MaxPages);
            }
            catch (HttpRequestException)
            {
                return new AiReportRelevance([], [], false, "Haber işleme hattına ulaşılamadı.");
            }

            var rows = Verdicts.Select(v => new AiReportVerdict(v, counts[v].Linked, counts[v].Dismissed)).ToList();
            return new AiReportRelevance(rows, versions.Order(StringComparer.Ordinal).ToList(), next != null, null);
        }

        /// <summary>"relevance-v3.1 · gemini-flash-lite-latest" → "relevance-v3.1".</summary>
        public static string VersionName(string version) =>
            string.IsNullOrWhiteSpace(version) ? null : version.Split('·')[0].Trim();

        /// <summary>The kind the dismissal's reason starts with (DismissArticleHandler.KindLabels), or Unknown.</summary>
        public static string DismissColumn(string reason)
        {
            if (!string.IsNullOrWhiteSpace(reason))
            {
                foreach (var (kind, label) in DismissArticleHandler.KindLabels)
                {
                    if (reason.StartsWith(label, StringComparison.Ordinal))
                        return kind.ToString();
                }
            }
            return "Unknown";
        }

        /// <summary>Names of the list values, for the items of list fields.</summary>
        internal sealed class Names
        {
            public Dictionary<int, string> Companies { get; private init; }
            public Dictionary<string, Dictionary<int, string>> Lists { get; private init; }

            public static async Task<Names> LoadAsync(RSDBContext db) => new()
            {
                Companies = await db.Company.ToDictionaryAsync(c => c.Id, c => c.Name),
                Lists = new()
                {
                    ["reasons"] = await db.ResistanceReason.ToDictionaryAsync(x => x.Id, x => x.Name),
                    ["corporations"] = await db.Corporation.ToDictionaryAsync(x => x.Id, x => x.Name),
                    ["employmentTypes"] = await db.EmploymentType.ToDictionaryAsync(x => x.Id, x => x.Name),
                    ["protestTypes"] = await db.ProtestoType.ToDictionaryAsync(x => x.Id, x => x.Name),
                    ["places"] = await db.ProtestoPlace.ToDictionaryAsync(x => x.Id, x => x.Name),
                    ["interventionTypes"] = await db.InterventionType.ToDictionaryAsync(x => x.Id, x => x.Name),
                    ["cities"] = await db.City.ToDictionaryAsync(x => x.Id, x => x.Name),
                },
            };

            public string Of(string list, int id) => Lists[list].TryGetValue(id, out var name) ? name : $"#{id}";
        }

        /// <summary>
        /// Counts per field; the fields in the order of the form. With a parent, everything counted is counted there too
        /// (one article's tally inside a total).
        /// </summary>
        internal sealed class FieldTally(Names names, FieldTally parent = null)
        {
            private readonly Names _names = names ?? parent!._names;
            private readonly Dictionary<string, Counter> _fields = new();
            private readonly List<(string Field, string Label, bool IsList)> _order =
            [
                ("case.category", "Kategori", false),
                ("case.company", "Şirket", false),
                ("case.isOutsource", "Taşeron", false),
                ("case.mainCompany", "Ana şirket", false),
                ("case.reasons", "Vaka nedenleri", true),
                ("case.corporations", "Destekleyen kurumlar", true),
                ("case.employmentTypes", "İstihdam biçimleri", true),
                ("case.employeeCount", "Çalışan sayısı (vaka)", false),
                ("case.firedEmployeeCount", "İşten çıkarılan", false),
                ("protest.types", "Eylem türleri", true),
                ("protest.startDate", "Başlangıç tarihi", false),
                ("protest.endDate", "Bitiş tarihi", false),
                ("protest.places", "Eylem mekânları", true),
                ("protest.cities", "İller", true),
                ("protest.employeeCount", "Katılımcı sayısı", false),
                ("protest.employeeCountRange", "Katılımcı aralığı", false),
                ("protest.interventionTypes", "Müdahale türleri", true),
                ("protest.custodyCount", "Gözaltı sayısı", false),
                ("protest.strikeHours", "Grev süresi (saat)", false),
            ];

            private sealed class Counter
            {
                public int Kept, Changed, Removed, Added;
                public readonly Dictionary<string, int[]> Items = new(); // name → kept, removed, added
            }

            private Counter Field(string field) => _fields.TryGetValue(field, out var c) ? c : _fields[field] = new Counter();

            private void Scalar<T>(string field, T suggested, T saved)
            {
                parent?.Scalar(field, suggested, saved);
                var hasSuggested = suggested is not null && !Equals(suggested, "");
                var hasSaved = saved is not null && !Equals(saved, "");
                var counter = Field(field);
                if (hasSuggested && hasSaved)
                {
                    if (Equals(suggested, saved)) counter.Kept++;
                    else counter.Changed++;
                }
                else if (hasSuggested) counter.Removed++;
                else if (hasSaved) counter.Added++;
            }

            private void List(string field, IEnumerable<string> suggested, IEnumerable<string> saved)
            {
                var s = (suggested ?? []).Where(x => x != null).ToHashSet();
                var v = (saved ?? []).Where(x => x != null).ToHashSet();
                parent?.List(field, s, v);
                var counter = Field(field);
                foreach (var item in s.Union(v))
                {
                    var tally = counter.Items.TryGetValue(item, out var t) ? t : counter.Items[item] = new int[3];
                    if (s.Contains(item) && v.Contains(item)) { tally[0]++; counter.Kept++; }
                    else if (s.Contains(item)) { tally[1]++; counter.Removed++; }
                    else { tally[2]++; counter.Added++; }
                }
            }

            // List items by name, so an id and the same typed-in name count as one.
            private IEnumerable<string> Ids(string list, IEnumerable<int> ids) => (ids ?? []).Select(id => _names.Of(list, id));

            // Compared as Tanımlar' "Olası tekrarlar" does: case, Turkish letters and legal-form words ignored.
            private string CompanyName(int? id, string newName) =>
                id is { } i ? (_names.Companies.TryGetValue(i, out var n) ? Key(n) : $"#{i}")
                : string.IsNullOrWhiteSpace(newName) ? null : Key(newName);

            private static string Key(string name) => DuplicateKey.For(DefinitionKind.Companies, name);

            private static int? Positive(int? value) => value is > 0 ? value : null;

            public void AddCase(AiCaseValues s, CaseInput v)
            {
                Scalar("case.category", s.CategoryId, v.CategoryId);
                Scalar("case.company", CompanyName(s.CompanyId, s.NewCompanyName), CompanyName(v.CompanyId, null));
                Scalar("case.isOutsource", s.IsOutsource ? "evet" : null, v.IsOutsource ? "evet" : null);
                Scalar("case.mainCompany", CompanyName(s.MainCompanyId, s.NewMainCompanyName), CompanyName(v.MainCompanyId, null));
                List("case.reasons", Ids("reasons", s.ReasonIds), Ids("reasons", v.ReasonIds).Concat(v.NewReasonNames ?? []));
                List("case.corporations", Ids("corporations", s.CorporationIds).Concat(s.NewCorporationNames ?? []),
                    Ids("corporations", v.CorporationIds).Concat(v.NewCorporationNames ?? []));
                List("case.employmentTypes", Ids("employmentTypes", s.EmploymentTypeIds), Ids("employmentTypes", v.EmploymentTypeIds));
                Scalar("case.employeeCount", Positive(s.EmployeeCount), Positive(v.EmployeeCount));
                Scalar("case.firedEmployeeCount", Positive(s.FiredEmployeeCount), Positive(v.FiredEmployeeCount));
            }

            public void AddProtest(AiProtestValues s, ProtestInput v)
            {
                List("protest.types", Ids("protestTypes", s.ProtestTypeIds), Ids("protestTypes", v.ProtestTypeIds));
                Scalar("protest.startDate", Day(s.StartDate), v.StartDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                Scalar("protest.endDate", Day(s.EndDate), v.EndDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                List("protest.places", Ids("places", s.PlaceIds), Ids("places", v.PlaceIds));
                List("protest.cities", Ids("cities", (s.Locations ?? []).Select(l => l.CityId)),
                    Ids("cities", (v.Locations ?? []).Where(l => l.CityId != null).Select(l => l.CityId!.Value)));
                Scalar("protest.employeeCount", Positive(s.EmployeeCount), Positive(v.EmployeeCount));
                Scalar("protest.employeeCountRange", s.EmployeeCountRangeId, v.EmployeeCountRangeId);
                List("protest.interventionTypes", Ids("interventionTypes", s.InterventionTypeIds), Ids("interventionTypes", v.InterventionTypeIds));
                Scalar("protest.custodyCount", Positive(s.CustodyCount), Positive(v.CustodyCount));
                Scalar("protest.strikeHours", Positive(s.StrikeHours), v.StrikeDurationSeconds is > 0 ? v.StrikeDurationSeconds / 3600 : null);
            }

            private static string Day(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim()[..Math.Min(10, value.Trim().Length)];

            /// <summary>Over all fields: values (or list items) the AI gave that were kept, and all it gave.</summary>
            public (int Kept, int Given) Totals() =>
                (_fields.Values.Sum(c => c.Kept), _fields.Values.Sum(c => c.Kept + c.Changed + c.Removed));

            public IReadOnlyList<AiReportField> Result() => _order
                .Where(f => _fields.TryGetValue(f.Field, out var c) && c.Kept + c.Changed + c.Removed + c.Added > 0) // never filled by either
                .Select(f =>
                {
                    var c = _fields[f.Field];
                    var items = c.Items
                        .Select(i => new AiReportItem(i.Key, i.Value[0], i.Value[1], i.Value[2]))
                        .OrderByDescending(i => i.Kept + i.Removed + i.Added)
                        .ThenBy(i => i.Name, StringComparer.Create(CultureInfo.GetCultureInfo("tr-TR"), true))
                        .ToList();
                    return new AiReportField(f.Field, f.Label, f.IsList, c.Kept, c.Changed, c.Removed, c.Added, items);
                })
                .ToList();
        }
    }
}
