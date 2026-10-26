using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RS.COMMON.Entities;
using RS.COMMON.Entities.LookupEntity;
using RS.EF;
using RS.MVC.Features.Dashboard;
using RS.MVC.Features.Review;
using RS.MVC.Models;

namespace RS.Tests;

/// <summary>R19d: the AI report.</summary>
public class AiReportTests
{
    private readonly RSDBContext _db = new(new DbContextOptionsBuilder<RSDBContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    private readonly FakePipeline _pipeline = new();

    public AiReportTests()
    {
        _db.Company.AddRange(new Company { Id = 10, Name = "Yemek Sepeti A.Ş." }, new Company { Id = 11, Name = "Başka Şirket" });
        _db.ProtestoType.AddRange(new ProtestoType { Id = 1, Name = "Grev" }, new ProtestoType { Id = 2, Name = "Basın açıklaması" }, new ProtestoType { Id = 3, Name = "Yürüyüş" });
        _db.City.AddRange(new City { Id = 34, Name = "İstanbul" }, new City { Id = 6, Name = "Ankara" });
        _db.SaveChanges();
    }

    private static AiCaseValues Case(int? companyId = null, string newCompany = null!, int? employees = null) =>
        new(1, 1, companyId, newCompany, false, null, null, null, [], [], [], [], employees, null, "Açıklama");

    private static AiProtestValues Protest(int[] types, string start, params int[] cities) =>
        new(types, start, null, [], cities.Select(c => new AiLocation(c, null, null)).ToList(), null, null, [], null, null);

    private void Add(AiDecision decision, object suggested, object saved, string origin = "pasted", bool applied = true)
    {
        _db.AiSuggestion.Add(new AiSuggestion
        {
            NewsId = _db.AiSuggestion.Count() + 1,
            Decision = decision,
            Origin = origin,
            PromptVersion = "v2.1",
            Relevant = true,
            Applied = applied,
            Answer = "{}",
            Suggested = JsonSerializer.Serialize(suggested, JsonSerializerOptions.Web),
            Saved = saved == null ? null : JsonSerializer.Serialize(saved, JsonSerializerOptions.Web),
            CreatedAt = DateTime.UtcNow,
        });
        _db.SaveChanges();
    }

    private Task<AiReport> Report(string? origin = null) => new AiReportHandler(_db, _pipeline).Handle(new AiReportQuery(Origin: origin!));

    [Fact]
    public async Task EachFieldCountsKeptChangedRemovedAndAdded()
    {
        // A new company typed by the AI and the same company chosen from the list count as kept (legal words ignored).
        Add(AiDecision.NewCase,
            new { Case = Case(newCompany: "Yemek Sepeti", employees: 40), Protest = Protest([1, 2], "2026-10-20", 34) },
            new
            {
                Case = new CaseInput { CategoryId = 1, TargetType = ResistanceTargetType.Company, CompanyId = 10 },
                Protest = new ProtestInput { ProtestTypeIds = [1, 3], StartDate = new DateTime(2026, 10, 21), Locations = [new LocationInput { CityId = 34 }] },
            });
        // Adding a protest: only the protest is compared.
        Add(AiDecision.AddProtest,
            new { Case = Case(companyId: 11), Protest = Protest([1], "2026-10-22", 6) },
            new { Protest = new ProtestInput { ProtestTypeIds = [1], StartDate = new DateTime(2026, 10, 22), Locations = [new LocationInput { CityId = 6 }] } },
            applied: false);
        // Link only: nothing to compare.
        Add(AiDecision.LinkOnly, new { Case = Case(), Protest = Protest([1], "2026-10-22") }, null);

        var report = await Report();

        Assert.Equal((2, 1, 1), (report.Decisions, report.Applied, report.Others));
        var field = report.Fields.ToDictionary(f => f.Field);
        Assert.Equal((1, 0, 0, 0), Counts(field["case.company"]));
        Assert.Equal((0, 0, 1, 0), Counts(field["case.employeeCount"])); // 40 suggested, nothing saved
        Assert.Equal((1, 1, 0, 0), Counts(field["protest.startDate"])); // the 20th saved as the 21st; the 22nd kept
        Assert.Equal((2, 0, 1, 1), Counts(field["protest.types"]));
        var types = field["protest.types"].Items.ToDictionary(i => i.Name);
        Assert.Equal((2, 0, 0), (types["Grev"].Kept, types["Grev"].Removed, types["Grev"].Added));
        Assert.Equal((0, 1, 0), (types["Basın açıklaması"].Kept, types["Basın açıklaması"].Removed, types["Basın açıklaması"].Added));
        Assert.Equal((0, 0, 1), (types["Yürüyüş"].Kept, types["Yürüyüş"].Removed, types["Yürüyüş"].Added));
        Assert.Equal((2, 0, 0, 0), Counts(field["protest.cities"]));
        Assert.DoesNotContain("case.reasons", field.Keys); // nothing suggested or saved
    }

    private static (int, int, int, int) Counts(AiReportField f) => (f.Kept, f.Changed, f.Removed, f.Added);

    [Fact]
    public async Task AnswersSplitByOrigin()
    {
        Add(AiDecision.AddProtest, new { Protest = Protest([1], "2026-10-22") }, new { Protest = new ProtestInput { ProtestTypeIds = [1] } });
        Add(AiDecision.AddProtest, new { Protest = Protest([1], "2026-10-22") }, new { Protest = new ProtestInput { ProtestTypeIds = [2] } },
            origin: "pipeline · gemini-flash-lite-latest");

        var all = await Report();
        var pipeline = await Report("pipeline · gemini-flash-lite-latest");

        Assert.Equal(2, all.Groups.Count);
        Assert.Equal(2, all.Decisions);
        Assert.Equal(1, pipeline.Decisions);
        Assert.Equal((0, 0, 1, 1), Counts(pipeline.Fields.Single(f => f.Field == "protest.types")));
    }

    [Fact]
    public async Task VerdictsMeetDecisions()
    {
        var now = DateTimeOffset.UtcNow;
        PipelineStageResult Verdict(string answer, string version = "relevance-v3.1 · gemini-flash-lite-latest") =>
            new("Succeeded", now, version, null, answer);
        void Handled(string id, PipelineStageResult? relevance, PipelineOutcome outcome)
        {
            _pipeline.Add(id, $"https://x/{id}", id, now);
            _pipeline.States[id] = (relevance == null ? new Dictionary<string, PipelineStageResult>() : new() { ["Relevance"] = relevance }, outcome);
        }
        Handled("a", Verdict("Relevant"), new PipelineOutcome("Digested", now, 5));
        Handled("b", Verdict("NotRelevant"), new PipelineOutcome("Dismissed", now, null, "Emek hareketiyle ilgisiz: spor haberi"));
        Handled("c", Verdict("Uncertain", "relevance-v3 · gemini-flash-lite-latest"), new PipelineOutcome("Dismissed", now, null, "Kapsam dışı"));
        Handled("d", null, new PipelineOutcome("Dismissed", now, null, "eski bir neden"));

        var relevance = (await Report()).Relevance;

        var rows = relevance.Rows.ToDictionary(r => r.Verdict);
        Assert.Equal(1, rows["Relevant"].Linked);
        Assert.Equal(1, rows["NotRelevant"].Dismissed["NotLabour"]);
        Assert.Equal(1, rows["Uncertain"].Dismissed["OutOfScope"]);
        Assert.Equal(1, rows["NotAssessed"].Dismissed["Unknown"]);
        Assert.Equal(["relevance-v3", "relevance-v3.1"], relevance.Versions);
        Assert.Equal(("all", "handled"), (_pipeline.Queries[0].Status, _pipeline.Queries[0].Outcome));

        var v31 = await new AiReportHandler(_db, _pipeline).Handle(new AiReportQuery(RelevanceVersion: "relevance-v3.1"));
        Assert.Equal(0, v31.Relevance.Rows.Single(r => r.Verdict == "Uncertain").Dismissed["OutOfScope"]);
    }
}
