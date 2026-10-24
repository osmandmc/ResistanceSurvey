using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.EF;
using RS.MVC.Applications;
using RS.MVC.Features.Articles;
using RS.MVC.Features.Definitions;

namespace RS.MVC.Features.Review
{
    // "AI ile doldur" (R19): the researcher copies a prompt into their own AI chat and pastes the JSON reply back. The
    // prompt is the pipeline's docs/extraction/prompt.md (AiPrompt.md here); the reply is checked against the lists and
    // turned into form values. The pipeline's automatic step (R11) will produce the same JSON.

    public record GetAiPromptQuery(string ArticleId);

    public record AiPrompt(string Text, string Version);

    public record AiPromptResult(ReviewStatus Status, AiPrompt Prompt = null, string Error = null);

    public class GetAiPromptHandler(IPipelineClient pipeline, RSDBContext db)
    {
        public const string Version = "v2";

        private static readonly Lazy<string> Template = new(() =>
        {
            using var stream = typeof(GetAiPromptHandler).Assembly.GetManifestResourceStream("AiPrompt.md")
                ?? throw new InvalidOperationException("AiPrompt.md is not embedded.");
            return new StreamReader(stream).ReadToEnd();
        });

        public async Task<AiPromptResult> Handle(GetAiPromptQuery query)
        {
            var article = await pipeline.GetArticleAsync(query.ArticleId);
            if (article == null)
                return new AiPromptResult(ReviewStatus.NotFound);
            if (string.IsNullOrWhiteSpace(article.Body))
                return new AiPromptResult(ReviewStatus.Invalid, Error: "Haberin metni henüz okunmadı; AI ile doldurmak için metin gerekiyor.");

            var lists = new (string Title, List<string> Values)[]
            {
                ("Kategoriler", await db.Category.OrderBy(e => e.Name).Select(e => e.Name).ToListAsync()),
                ("Direniş nedenleri", await db.ResistanceReason.OrderBy(e => e.Name).Select(e => e.Name).ToListAsync()),
                ("İstihdam biçimleri", await db.EmploymentType.OrderBy(e => e.Name).Select(e => e.Name).ToListAsync()),
                ("Sendika yetkisi", await db.TradeUnionAuthority.OrderBy(e => e.Name).Select(e => e.Name).ToListAsync()),
                ("Eylem türleri", await db.ProtestoType.OrderBy(e => e.Name).Select(e => e.Name).ToListAsync()),
                ("Eylem yerleri", await db.ProtestoPlace.OrderBy(e => e.Name).Select(e => e.Name).ToListAsync()),
                ("Müdahaleler", await db.InterventionType.OrderBy(e => e.Name).Select(e => e.Name).ToListAsync()),
                ("İller", await db.City.OrderBy(e => e.Name).Select(e => e.Name).ToListAsync()),
            };
            var listText = string.Join("\n\n", lists.Select(l =>
                $"### {l.Title}\n" + string.Join('\n', l.Values.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => "- " + v.Trim()))));

            var text = Template.Value
                .Replace("{{LISTS}}", listText)
                .Replace("{{SOURCE}}", SourceCatalog.Name(article.SourceId))
                .Replace("{{DATE}}", article.PublishedAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "bilinmiyor")
                .Replace("{{TITLE}}", article.Title ?? "")
                .Replace("{{TEXT}}", article.Body.Trim());
            return new AiPromptResult(ReviewStatus.Ok, new AiPrompt(text, Version));
        }
    }

    public record CheckAiAnswerCommand(string Answer);

    [JsonConverter(typeof(JsonStringEnumConverter<AiItemStatus>))]
    public enum AiItemStatus
    {
        /// <summary>Found as written.</summary>
        Matched,

        /// <summary>Found after ignoring case, punctuation and Turkish letters.</summary>
        Corrected,

        /// <summary>Not registered yet; created when the case is saved (organizations) or when applied (companies).</summary>
        New,

        /// <summary>Not a value of the list: left out.</summary>
        NotInList,

        /// <summary>Researchers fill this field themselves (see the pipeline's docs/extraction/findings.md).</summary>
        Skipped,
    }

    /// <param name="Answer">What the AI wrote.</param>
    /// <param name="Value">What goes into the form (the list's own spelling), or null.</param>
    public record AiItem(string Answer, string Value, AiItemStatus Status, string Note = null);

    /// <param name="Field">Form field, e.g. "case.reasons" or "protest.locations".</param>
    public record AiField(string Field, string Label, IReadOnlyList<AiItem> Items);

    /// <summary>Case form values from the answer; a company not registered yet comes as a name only.</summary>
    public record AiCaseValues(
        int? CategoryId,
        int? TargetType,
        int? CompanyId,
        string NewCompanyName,
        bool IsOutsource,
        int? MainCompanyId,
        string NewMainCompanyName,
        int? TargetCorporationId,
        IReadOnlyList<int> ReasonIds,
        IReadOnlyList<int> CorporationIds,
        IReadOnlyList<string> NewCorporationNames,
        IReadOnlyList<int> EmploymentTypeIds,
        int? EmployeeCount,
        int? FiredEmployeeCount,
        string Description);

    public record AiLocation(int CityId, int? DistrictId, string Place);

    /// <param name="StartDate">"yyyy-MM-dd".</param>
    public record AiProtestValues(
        IReadOnlyList<int> ProtestTypeIds,
        string StartDate,
        string EndDate,
        IReadOnlyList<int> PlaceIds,
        IReadOnlyList<AiLocation> Locations,
        int? EmployeeCount,
        int? EmployeeCountRangeId,
        IReadOnlyList<int> InterventionTypeIds,
        int? CustodyCount,
        int? StrikeHours);

    /// <param name="Error">Why the answer could not be read; everything else is empty then.</param>
    public record AiCheckResult(
        string Error,
        bool Relevant,
        string RelevanceReason,
        IReadOnlyList<AiField> Fields,
        AiCaseValues Case,
        AiProtestValues Protest)
    {
        public static AiCheckResult Failed(string error) => new(error, false, null, [], null, null);
    }

    public class CheckAiAnswerHandler(RSDBContext db)
    {
        private static readonly JsonDocumentOptions JsonOptions = new() { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip };

        public async Task<AiCheckResult> Handle(CheckAiAnswerCommand command)
        {
            JsonElement root;
            try
            {
                root = ReadJson(command.Answer);
            }
            catch (FormatException e)
            {
                return AiCheckResult.Failed(e.Message);
            }

            if (Bool(root, "relevant") == false)
                return new AiCheckResult(null, false, Text(root, "relevance_reason"), [], null, null);

            var c = Object(root, "case");
            var p = Object(root, "protest");
            var fields = new List<AiField>();
            void Add(string field, string label, params AiItem[] items)
            {
                if (items.Length > 0)
                    fields.Add(new AiField(field, label, items));
            }

            // Case.
            var categories = await ListAsync(db.Category.Select(e => new { e.Id, e.Name }).ToListAsync(), e => e.Id, e => e.Name);
            var category = One(categories, Text(c, "category"));
            Add("case.category", "Kategori", Items(category));

            int? targetType = Text(c, "target_type") switch { "company" => 1, "corporation" => 2, _ => null };

            var companies = await ListAsync(db.Company.Where(e => !e.Deleted).Select(e => new { e.Id, e.Name }).ToListAsync(), e => e.Id, e => e.Name, DefinitionKind.Companies);
            var company = Record(companies, Text(c, "company"), "şirket");
            Add("case.company", "Şirket / kurum", Items(company));
            var mainCompany = Record(companies, Text(c, "main_company"), "şirket");
            Add("case.main_company", "Asıl işveren", Items(mainCompany));

            var corporations = await ListAsync(db.Corporation.Select(e => new { e.Id, e.Name }).ToListAsync(), e => e.Id, e => e.Name, DefinitionKind.Corporations);
            var targetCorporation = Text(c, "target_corporation") is { } targetName ? One(corporations, targetName, "kayıtlı değil, kurum seçin") : null;
            Add("case.target_corporation", "Hedef kurum", Items(targetCorporation));
            var organizations = Texts(c, "organizations").Select(o => Record(corporations, o, "örgüt")).ToList();
            Add("case.organizations", "Örgütler", Items(organizations));

            var reasons = await Many(db.ResistanceReason.Select(e => new { e.Id, e.Name }).ToListAsync(), e => e.Id, e => e.Name, Texts(c, "reasons"));
            Add("case.reasons", "Direniş nedenleri", Items(reasons));
            var employmentTypes = await Many(db.EmploymentType.Select(e => new { e.Id, e.Name }).ToListAsync(), e => e.Id, e => e.Name, Texts(c, "employment_types"));
            Add("case.employment_types", "İstihdam biçimi", Items(employmentTypes));

            var employeeCount = Int(c, "employee_count");
            Add("case.employee_count", "Çalışan sayısı", Plain(employeeCount));
            var firedCount = Int(c, "fired_count");
            Add("case.fired_count", "Atılan işçi sayısı", Plain(firedCount));
            var description = Text(c, "description");
            Add("case.description", "Açıklama", Plain(description));
            foreach (var (key, label) in new[] { ("union_authority", "Sendika yetkisi"), ("employee_count_range", "İşyeri büyüklüğü"), ("develops_right", "Hak geliştirme"), ("result", "Sonuç") })
            {
                if (Raw(c, key) is { } skipped)
                    Add("case." + key, label, new AiItem(skipped, null, AiItemStatus.Skipped, "araştırmacı seçer"));
            }

            // Protest.
            var startDate = Date(p, "start_date");
            Add("protest.start_date", "Başlangıç", Plain(startDate));
            var endDate = Date(p, "end_date");
            Add("protest.end_date", "Bitiş", Plain(endDate));
            var types = await Many(db.ProtestoType.Select(e => new { e.Id, e.Name }).ToListAsync(), e => e.Id, e => e.Name, Texts(p, "types"));
            Add("protest.types", "Eylem türleri", Items(types));
            var places = await Many(db.ProtestoPlace.Select(e => new { e.Id, e.Name }).ToListAsync(), e => e.Id, e => e.Name, Texts(p, "places"));
            Add("protest.places", "Eylem yerleri", Items(places));
            var locations = await LocationsAsync(p);
            Add("protest.locations", "Konum", locations.Select(l => l.Item).ToArray());
            var participants = Int(p, "participant_count");
            Add("protest.participant_count", "Katılımcı sayısı", Plain(participants));
            var participantRange = participants is int count ? await RangeAsync(count) : null;
            if (participantRange != null)
                Add("protest.participant_range", "Katılımcı aralığı", new AiItem(participants.Value.ToString(CultureInfo.InvariantCulture), participantRange.Value.Name, AiItemStatus.Matched, "sayıdan hesaplandı"));
            var custody = Int(p, "custody_count");
            Add("protest.custody_count", "Gözaltı", Plain(custody));
            var interventions = await Many(db.InterventionType.Select(e => new { e.Id, e.Name }).ToListAsync(), e => e.Id, e => e.Name, Texts(p, "interventions"));
            Add("protest.interventions", "Müdahale", Items(interventions));
            var strikeHours = Int(p, "strike_hours");
            Add("protest.strike_hours", "Grev / iş bırakma (saat)", Plain(strikeHours));
            if (Raw(p, "gender") is { } gender)
                Add("protest.gender", "Cinsiyet", new AiItem(gender, null, AiItemStatus.Skipped, "araştırmacı seçer"));

            var caseValues = new AiCaseValues(
                CategoryId: category?.Id,
                TargetType: targetType,
                CompanyId: company?.Id,
                NewCompanyName: company is { Id: null } ? company.Item.Value : null,
                IsOutsource: mainCompany != null,
                MainCompanyId: mainCompany?.Id,
                NewMainCompanyName: mainCompany is { Id: null } ? mainCompany.Item.Value : null,
                TargetCorporationId: targetCorporation?.Id,
                ReasonIds: Ids(reasons),
                CorporationIds: Ids(organizations),
                NewCorporationNames: organizations.Where(o => o.Id == null && o.Item.Status == AiItemStatus.New).Select(o => o.Item.Value).ToList(),
                EmploymentTypeIds: Ids(employmentTypes),
                EmployeeCount: employeeCount,
                FiredEmployeeCount: firedCount,
                Description: description);
            var protestValues = new AiProtestValues(
                ProtestTypeIds: Ids(types),
                StartDate: startDate,
                EndDate: endDate,
                PlaceIds: Ids(places),
                Locations: locations.Where(l => l.Location != null).Select(l => l.Location).ToList(),
                EmployeeCount: participants,
                EmployeeCountRangeId: participantRange?.Id,
                InterventionTypeIds: Ids(interventions),
                CustodyCount: custody,
                StrikeHours: strikeHours);
            return new AiCheckResult(null, true, Text(root, "relevance_reason"), fields, caseValues, protestValues);
        }

        // ---- reading the answer ----

        /// <summary>The JSON object in the answer, also when the chat put text or a ```json fence around it.</summary>
        public static JsonElement ReadJson(string answer)
        {
            var start = answer?.IndexOf('{') ?? -1;
            var end = answer?.LastIndexOf('}') ?? -1;
            if (start < 0 || end <= start)
                throw new FormatException("Yanıtta JSON bulunamadı. AI'ın yanıtının tamamını yapıştırın.");
            try
            {
                using var document = JsonDocument.Parse(answer[start..(end + 1)], JsonOptions);
                return document.RootElement.Clone();
            }
            catch (JsonException)
            {
                throw new FormatException("JSON okunamadı; yanıt yarıda kesilmiş olabilir. AI'dan yanıtı yalnızca JSON olarak yeniden vermesini isteyin.");
            }
        }

        private static JsonElement? Object(JsonElement parent, string name) =>
            parent.ValueKind == JsonValueKind.Object && parent.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Object ? value : null;

        private static JsonElement? Property(JsonElement? parent, string name) =>
            parent is { ValueKind: JsonValueKind.Object } p && p.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null ? value : null;

        private static string Text(JsonElement? parent, string name) =>
            Property(parent, name) is { ValueKind: JsonValueKind.String } v && !string.IsNullOrWhiteSpace(v.GetString()) ? v.GetString().Trim() : null;

        private static bool? Bool(JsonElement parent, string name) =>
            Property(parent, name) is { } v && (v.ValueKind == JsonValueKind.True || v.ValueKind == JsonValueKind.False) ? v.GetBoolean() : null;

        /// <summary>A whole number, also when written as a string ("200").</summary>
        private static int? Int(JsonElement? parent, string name) => Property(parent, name) switch
        {
            { ValueKind: JsonValueKind.Number } v when v.TryGetDouble(out var d) && d >= 0 && d < int.MaxValue => (int)Math.Round(d),
            { ValueKind: JsonValueKind.String } v when int.TryParse(v.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) && i >= 0 => i,
            _ => null,
        };

        private static string Date(JsonElement? parent, string name) =>
            Text(parent, name) is { } text && DateOnly.TryParseExact(text.Length >= 10 ? text[..10] : text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
                ? date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                : null;

        private static List<string> Texts(JsonElement? parent, string name) =>
            Property(parent, name) is { ValueKind: JsonValueKind.Array } array
                ? array.EnumerateArray().Where(v => v.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(v.GetString())).Select(v => v.GetString().Trim()).Distinct().ToList()
                : [];

        /// <summary>Any non-empty value as text (for fields that are shown but not taken).</summary>
        private static string Raw(JsonElement? parent, string name) => Property(parent, name) switch
        {
            null => null,
            { ValueKind: JsonValueKind.String } v => string.IsNullOrWhiteSpace(v.GetString()) ? null : v.GetString().Trim(),
            { ValueKind: JsonValueKind.Array } v => v.GetArrayLength() == 0 ? null : v.GetRawText(),
            { } v => v.GetRawText(),
        };

        // ---- matching with the lists ----

        private record Entry(int Id, string Name);

        private sealed class ValueList(List<Entry> entries, DefinitionKind kind)
        {
            public readonly List<Entry> Entries = entries;
            public readonly ILookup<string, Entry> ByKey = entries.ToLookup(e => DuplicateKey.For(kind, e.Name ?? ""));
            public string Key(string name) => DuplicateKey.For(kind, name);
        }

        private sealed record Match(int? Id, AiItem Item);

        private static async Task<ValueList> ListAsync<T>(Task<List<T>> rows, Func<T, int> id, Func<T, string> name, DefinitionKind kind = DefinitionKind.Reasons) =>
            new((await rows).Where(r => !string.IsNullOrWhiteSpace(name(r))).Select(r => new Entry(id(r), name(r).Trim())).ToList(), kind);

        /// <summary>A list value: as written, or after folding; otherwise left out.</summary>
        private static Match One(ValueList list, string answer, string notFound = "listede yok, alınmadı")
        {
            if (answer == null)
                return null;
            var exact = list.Entries.FirstOrDefault(e => e.Name == answer);
            if (exact != null)
                return new Match(exact.Id, new AiItem(answer, exact.Name, AiItemStatus.Matched));
            var folded = list.ByKey[list.Key(answer)].FirstOrDefault();
            return folded != null
                ? new Match(folded.Id, new AiItem(answer, folded.Name, AiItemStatus.Corrected))
                : new Match(null, new AiItem(answer, null, AiItemStatus.NotInList, notFound));
        }

        /// <summary>A company or organization: a registered one when the names match, otherwise a new one by that name.</summary>
        private static Match Record(ValueList list, string answer, string what)
        {
            if (answer == null)
                return null;
            var found = One(list, answer);
            return found.Id != null
                ? found with { Item = found.Item with { Note = $"kayıtlı {what}" } }
                : new Match(null, new AiItem(answer, answer, AiItemStatus.New, $"yeni {what}"));
        }

        private async Task<List<Match>> Many<T>(Task<List<T>> rows, Func<T, int> id, Func<T, string> name, List<string> answers)
        {
            if (answers.Count == 0)
                return [];
            var list = await ListAsync(rows, id, name);
            return answers.Select(a => One(list, a)).ToList();
        }

        private async Task<List<(AiItem Item, AiLocation Location)>> LocationsAsync(JsonElement? protest)
        {
            if (Property(protest, "locations") is not { ValueKind: JsonValueKind.Array } array || array.GetArrayLength() == 0)
                return [];
            var cities = await ListAsync(db.City.Select(e => new { e.Id, e.Name }).ToListAsync(), e => e.Id, e => e.Name);
            var districts = await db.District.Select(e => new { e.Id, e.Name, e.CityId }).ToListAsync();
            var result = new List<(AiItem, AiLocation)>();
            foreach (var location in array.EnumerateArray().Where(l => l.ValueKind == JsonValueKind.Object))
            {
                var cityName = Text(location, "city");
                var districtName = Text(location, "district");
                var place = Text(location, "place");
                var answer = string.Join(" / ", new[] { cityName, districtName, place }.Where(s => s != null));
                var city = cityName == null ? null : One(cities, cityName);
                if (city?.Id is not int cityId)
                {
                    result.Add((new AiItem(answer, null, AiItemStatus.NotInList, "il bulunamadı, alınmadı"), null));
                    continue;
                }
                var district = districtName == null ? null : new ValueList(districts.Where(d => d.CityId == cityId).Select(d => new Entry(d.Id, d.Name)).ToList(), DefinitionKind.Reasons) is var inCity
                    ? One(inCity, districtName) : null;
                var value = string.Join(" / ", new[] { city.Item.Value, district?.Id != null ? district.Item.Value : null, place }.Where(s => s != null));
                var status = district is { Id: null } ? AiItemStatus.Corrected : city.Item.Status;
                var note = district is { Id: null } ? "ilçe bulunamadı, seçin" : null;
                result.Add((new AiItem(answer, value, status, note), new AiLocation(cityId, district?.Id, place)));
            }
            return result;
        }

        /// <summary>The protest's participant range ("101-250", "100.001+") that holds the count.</summary>
        private async Task<(int Id, string Name)?> RangeAsync(int count)
        {
            foreach (var range in await db.ProtestoEmployeeCount.Select(e => new { e.Id, e.Name }).ToListAsync())
            {
                var bounds = (range.Name ?? "").Replace(".", "").Replace(" ", "").TrimEnd('+').Split('-');
                if (!int.TryParse(bounds[0], out var low))
                    continue;
                var high = bounds.Length > 1 && int.TryParse(bounds[1], out var h) ? h : int.MaxValue;
                if (count >= low && count <= high)
                    return (range.Id, range.Name);
            }
            return null;
        }

        private static AiItem[] Items(Match match) => match == null ? [] : [match.Item];

        private static AiItem[] Items(IEnumerable<Match> matches) => matches.Select(m => m.Item).ToArray();

        private static List<int> Ids(IEnumerable<Match> matches) => matches.Where(m => m.Id != null).Select(m => m.Id.Value).Distinct().ToList();

        private static AiItem[] Plain(int? value) =>
            value is int v ? [new AiItem(v.ToString(CultureInfo.InvariantCulture), v.ToString(CultureInfo.InvariantCulture), AiItemStatus.Matched)] : [];

        private static AiItem[] Plain(string value) => value == null ? [] : [new AiItem(value, value, AiItemStatus.Matched)];
    }

    public record GetAiDraftQuery(string ArticleId);

    /// <param name="Answer">The draft's JSON, sent back with the decision so it is kept beside it (R19c).</param>
    /// <param name="Origin">"pipeline · prompt-v2 · model": what made the draft.</param>
    /// <param name="Check">The draft checked against the lists now, exactly as a pasted answer.</param>
    public record AiDraft(string Answer, string Origin, DateTimeOffset CreatedAt, AiCheckResult Check);

    /// <summary>The pipeline's AI draft of the case form for an article (R11, pipeline B12); null when it has none.</summary>
    public class GetAiDraftHandler(IPipelineClient pipeline, CheckAiAnswerHandler check)
    {
        public async Task<AiDraft> Handle(GetAiDraftQuery query)
        {
            var draft = await pipeline.GetDraftAsync(query.ArticleId);
            if (draft == null || string.IsNullOrWhiteSpace(draft.Answer))
                return null;
            var result = await check.Handle(new CheckAiAnswerCommand(draft.Answer));
            return result.Error != null ? null : new AiDraft(draft.Answer, "pipeline · " + draft.Version, draft.CreatedAt, result);
        }
    }
}

