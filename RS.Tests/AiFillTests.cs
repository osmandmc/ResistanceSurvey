using Microsoft.EntityFrameworkCore;
using RS.COMMON.Entities;
using RS.COMMON.Entities.LookupEntity;
using RS.EF;
using RS.MVC.Features.Review;

namespace RS.Tests;

public class AiFillTests
{
    private readonly RSDBContext _db = new(new DbContextOptionsBuilder<RSDBContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private const string Answer = """
        İşte haberden çıkardığım bilgiler:

        ```json
        {
          "relevant": true,
          "relevance_reason": "İşçiler iş bıraktı.",
          "case": {
            "category": "İşletme/Kurum", "target_type": "company", "company": "Örnek Metal", "main_company": null,
            "organizations": ["DİSK Birleşik Metal-İş", "Gebze Dayanışma Platformu"], "union_authority": "Sendika yetkisi var",
            "reasons": ["TİS", "Ücret artışı"], "employment_types": ["özel kadrolu (mavi yakali)"],
            "employee_count": 450, "fired_count": null, "result": "Bilinmiyor",
            "description": "14.10.26: İşçiler iki saat iş bıraktı."
          },
          "protest": {
            "start_date": "2026-10-14", "end_date": "2026-10-14", "types": ["Fiili grev"], "places": ["İşyeri önü"],
            "locations": [{ "city": "Kocaeli", "district": "Gebze", "place": "fabrika önü" }, { "city": "Atlantis", "district": null, "place": null }],
            "participant_count": "200", "custody_count": null, "interventions": [], "strike_hours": 2, "gender": "Erkek Ağırlıklı",
          }
        }
        ```
        """;

    private async Task SeedAsync()
    {
        _db.Category.Add(new Category { Id = 1, Name = "İşletme/Kurum" });
        _db.Company.AddRange(new Company { Id = 10, Name = "Örnek Metal Sanayi ve Ticaret A.Ş." }, new Company { Id = 11, Name = "Örnek Metal", Deleted = true });
        _db.Corporation.Add(new Corporation { Id = 20, Name = "DİSK Birleşik Metal-İş" });
        _db.ResistanceReason.Add(new ResistanceReason { Id = 30, Name = "TİS" });
        _db.EmploymentType.Add(new EmploymentType { Id = 40, Name = "Özel Kadrolu (Mavi Yakalı)" });
        _db.ProtestoType.Add(new ProtestoType { Id = 50, Name = "Fiili grev" });
        _db.ProtestoPlace.Add(new ProtestoPlace { Id = 60, Name = "İşyeri önü" });
        _db.City.Add(new City { Id = 41, Name = "Kocaeli" });
        _db.District.AddRange(new District { Id = 410, Name = "Gebze", CityId = 41 }, new District { Id = 999, Name = "Gebze", CityId = 1 });
        _db.ProtestoEmployeeCount.AddRange(new ProtestoEmployeeCount { Id = 70, Name = "51-100" }, new ProtestoEmployeeCount { Id = 71, Name = "101-250" },
            new ProtestoEmployeeCount { Id = 72, Name = "100.001+" });
        await _db.SaveChangesAsync();
    }

    [Fact]
    public async Task AnAnswerBecomesFormValuesWithAStatusPerValue()
    {
        await SeedAsync();

        var result = await new CheckAiAnswerHandler(_db).Handle(new CheckAiAnswerCommand(Answer));

        Assert.Null(result.Error);
        Assert.True(result.Relevant);
        var c = result.Case;
        Assert.Equal((1, 1, 10, (string?)null, false), (c.CategoryId, c.TargetType, c.CompanyId, c.NewCompanyName, c.IsOutsource));
        Assert.Equal([20], c.CorporationIds);
        Assert.Equal(["Gebze Dayanışma Platformu"], c.NewCorporationNames);
        Assert.Equal([30], c.ReasonIds);
        Assert.Equal([40], c.EmploymentTypeIds);
        Assert.Equal(450, c.EmployeeCount);

        var p = result.Protest;
        Assert.Equal(("2026-10-14", "2026-10-14", 2), (p.StartDate, p.EndDate, p.StrikeHours));
        Assert.Equal([50], p.ProtestTypeIds);
        Assert.Equal([60], p.PlaceIds);
        Assert.Equal([new AiLocation(41, 410, "fabrika önü")], p.Locations);
        Assert.Equal((200, 71), (p.EmployeeCount, p.EmployeeCountRangeId));

        AiItemStatus Status(string field, int index = 0) => result.Fields.Single(f => f.Field == field).Items[index].Status;
        Assert.Equal(AiItemStatus.Corrected, Status("case.company")); // registered as "… Sanayi ve Ticaret A.Ş."; the deleted "Örnek Metal" is not used
        Assert.Equal(AiItemStatus.New, Status("case.organizations", 1));
        Assert.Equal(AiItemStatus.NotInList, Status("case.reasons", 1));
        Assert.Equal(AiItemStatus.Corrected, Status("case.employment_types"));
        Assert.Equal(AiItemStatus.NotInList, Status("protest.locations", 1));
        // Fields researchers fill are shown but not taken.
        Assert.Equal(AiItemStatus.Skipped, Status("case.union_authority"));
        Assert.Equal(AiItemStatus.Skipped, Status("protest.gender"));
        Assert.DoesNotContain(result.Fields, f => f.Field == "case.result" && f.Items.Any(i => i.Value != null));
    }

    [Fact]
    public async Task ACompanyThatIsNotRegisteredComesAsANewName()
    {
        await SeedAsync();
        var answer = """{ "relevant": true, "case": { "company": "Yeni Lojistik", "main_company": "Örnek Metal A.Ş." }, "protest": {} }""";

        var result = await new CheckAiAnswerHandler(_db).Handle(new CheckAiAnswerCommand(answer));

        Assert.Equal((null, "Yeni Lojistik"), (result.Case.CompanyId, result.Case.NewCompanyName));
        Assert.Equal((true, 10, (string?)null), (result.Case.IsOutsource, result.Case.MainCompanyId, result.Case.NewMainCompanyName));
    }

    [Fact]
    public async Task ANotRelevantAnswerCarriesOnlyTheReason()
    {
        var result = await new CheckAiAnswerHandler(_db).Handle(new CheckAiAnswerCommand("""{"relevant": false, "relevance_reason": "Eylem yok.", "case": null}"""));

        Assert.Equal((false, "Eylem yok.", (AiCaseValues?)null), (result.Relevant, result.RelevanceReason, result.Case));
    }

    [Theory]
    [InlineData("Üzgünüm, bu haberi okuyamadım.", "JSON bulunamadı")]
    [InlineData("""{ "relevant": true, "case": { "company": "Örnek""", "JSON bulunamadı")]
    [InlineData("""{ "relevant": true, "case": { "company": } }""", "yarıda kesilmiş")]
    public async Task AnAnswerWithoutReadableJsonSaysWhatToDo(string answer, string message)
    {
        var result = await new CheckAiAnswerHandler(_db).Handle(new CheckAiAnswerCommand(answer));

        Assert.Contains(message, result.Error);
    }

    [Fact]
    public async Task ThePromptHoldsTheArticleAndTheCurrentLists()
    {
        await SeedAsync();
        var pipeline = new FakePipeline();
        pipeline.Add("a1", "https://www.evrensel.net/haber/1/x", "Örnek Metal işçileri iş bıraktı", new DateTimeOffset(2026, 10, 14, 9, 0, 0, TimeSpan.FromHours(3)), "İşçiler bugün iş bıraktı.");
        pipeline.Add("a2", "https://www.evrensel.net/haber/2/y", "Başlık", DateTimeOffset.Now, "  ");
        var handler = new GetAiPromptHandler(pipeline, _db);

        var result = await handler.Handle(new GetAiPromptQuery("a1"));
        var empty = await handler.Handle(new GetAiPromptQuery("a2"));
        var missing = await handler.Handle(new GetAiPromptQuery("nope"));

        Assert.Equal(ReviewStatus.Ok, result.Status);
        var text = result.Prompt.Text;
        Assert.Contains("Kaynak: Evrensel · Tarih: 2026-10-14 · Başlık: Örnek Metal işçileri iş bıraktı", text);
        Assert.Contains("İşçiler bugün iş bıraktı.", text);
        Assert.Contains("### Direniş nedenleri\n- TİS", text);
        Assert.Contains("- Kocaeli", text);
        Assert.DoesNotContain("{{", text);
        Assert.Equal(GetAiPromptHandler.Version, result.Prompt.Version);
        Assert.Equal(ReviewStatus.Invalid, empty.Status);
        Assert.Equal(ReviewStatus.NotFound, missing.Status);
    }
}
