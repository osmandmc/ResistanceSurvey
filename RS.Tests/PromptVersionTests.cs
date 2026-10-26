using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RS.COMMON.Entities;
using RS.COMMON.Entities.LookupEntity;
using RS.EF;
using RS.MVC.Features.Activity;
using RS.MVC.Features.Dashboard;
using RS.MVC.Features.Review;
using RS.MVC.Models;
using static RS.COMMON.Constants.Enums;

namespace RS.Tests;

/// <summary>R28: prompt versions edited in the app, tried and activated in the pipeline.</summary>
public class PromptVersionTests
{
    private readonly TestUser _user = new() { Name = "ayse" };
    private readonly RSDBContext _db;
    private readonly FakePipeline _pipeline = new();

    public PromptVersionTests() =>
        _db = new(_user.Track(new DbContextOptionsBuilder<RSDBContext>().UseInMemoryDatabase(Guid.NewGuid().ToString())).Options);

    private TryPromptHandler Try() => new(_pipeline, _db, new CheckAiAnswerHandler(_db));

    private News AddNews(string articleId, Status status)
    {
        var news = new News { ArticleId = articleId, Header = articleId, Link = $"https://x/{articleId}", Date = DateTime.Today, Status = status };
        _db.News.Add(news);
        _db.SaveChanges();
        return news;
    }

    [Fact]
    public async Task ANewVersionIsSavedInThePipelineAndLogged()
    {
        var saved = await new CreatePromptVersionHandler(_pipeline, _db).Handle(new CreatePromptVersionCommand("relevance", "Yeni {{TEXT}}", "Daha sıkı", "ayse"));
        var refused = await new CreatePromptVersionHandler(_pipeline, _db).Handle(new CreatePromptVersionCommand("relevance", "Metinsiz", null!, "ayse"));

        Assert.Equal("relevance-e1", saved.Version.Version);
        Assert.Contains("{{TEXT}}", refused.Error);
        var line = Assert.Single(_db.ActivityEntry);
        Assert.Equal(("ayse", Actions.PromptCreate, "İlgi kontrolü: relevance-e1 (Daha sıkı)"), (line.Username, line.Action, line.Summary));
    }

    [Fact]
    public async Task ActivatingIsLogged()
    {
        Assert.True(await new ActivatePromptHandler(_pipeline, _db).Handle(new ActivatePromptCommand("extraction", "builtin", "ayse")));

        Assert.Equal(("extraction", "builtin", "ayse"), Assert.Single(_pipeline.Activated));
        Assert.Equal("Taslak istemi: yerleşik sürüm", Assert.Single(_db.ActivityEntry).Summary);
    }

    [Fact]
    public async Task ARelevanceTryComparesBothPromptsWithWhatResearchersDecided()
    {
        AddNews("linked-1", Status.Linked);
        AddNews("linked-2", Status.Linked);
        var notLabour = AddNews("dismissed-1", Status.Passive);
        var recorded = AddNews("dismissed-2", Status.Passive);
        _db.AiSuggestion.Add(new AiSuggestion { NewsId = recorded.Id, Decision = AiDecision.Dismissed, DismissKind = DismissKind.AlreadyRecorded, Origin = "pasted", Answer = "{}" });
        _db.AiSuggestion.Add(new AiSuggestion { NewsId = notLabour.Id, Decision = AiDecision.Dismissed, DismissKind = DismissKind.NotLabour, Origin = "pasted", Answer = "{}" });
        await _db.SaveChangesAsync();
        // The running prompt rejected one linked article; the tried one does not.
        _pipeline.TryAnswer = id => id switch
        {
            "linked-1" => new(id, id, "Relevant", "Grev.", null!, "NotRelevant", "Analiz.", "relevance-v3.1 · gemini"),
            "linked-2" => new(id, id, "Uncertain", "Belki.", null!, "Relevant", null!, "relevance-v3.1 · gemini"),
            "dismissed-1" => new(id, id, "NotRelevant", "Spor.", null!, "NotRelevant", null!, "relevance-v3.1 · gemini"),
            _ => new(id, id, "Relevant", "Grev.", null!, "Relevant", null!, "relevance-v3.1 · gemini"),
        };

        var result = await Try().Handle("relevance", "relevance-v3.1-e1", "ayse");

        Assert.Equal(4, Assert.Single(_pipeline.Tried).Count);
        Assert.Equal((3, 2, 3), (result!.Compared, result.CurrentAgrees, result.TriedAgrees)); // "already recorded" says nothing
        var rows = result.Rows.ToDictionary(r => r.ArticleId);
        Assert.Equal(("linked", false, true), (rows["linked-1"].Decision, rows["linked-1"].CurrentAgrees, rows["linked-1"].TriedAgrees));
        Assert.Equal(("dismissed:AlreadyRecorded", (bool?)null), (rows["dismissed-2"].Decision, rows["dismissed-2"].TriedAgrees));
        Assert.Equal(Actions.PromptTry, Assert.Single(_db.ActivityEntry).Action);
    }

    [Fact]
    public async Task AnExtractionTryComparesTheDraftsWithTheSavedValues()
    {
        _db.ProtestoType.AddRange(new ProtestoType { Id = 50, Name = "Fiili grev" }, new ProtestoType { Id = 51, Name = "Basın açıklaması" });
        _db.City.Add(new City { Id = 41, Name = "Kocaeli" });
        var news = AddNews("a", Status.Linked);
        _db.AiSuggestion.Add(new AiSuggestion
        {
            NewsId = news.Id, Decision = AiDecision.AddProtest, Origin = "pasted", Answer = "{}",
            Saved = JsonSerializer.Serialize(new { Protest = new ProtestInput { ProtestTypeIds = [50], Locations = [new LocationInput { CityId = 41 }] } }, JsonSerializerOptions.Web),
        });
        await _db.SaveChangesAsync();
        const string current = """{ "relevant": true, "protest": { "types": ["Basın açıklaması"], "locations": [{ "city": "Kocaeli" }] } }""";
        const string tried = """{ "relevant": true, "protest": { "types": ["Fiili grev"], "locations": [{ "city": "Kocaeli" }] } }""";
        _pipeline.TryAnswer = id => new(id, "Haber", tried, "Fiili grev · Kocaeli", null!, current, "Basın açıklaması · Kocaeli", "prompt-v2.1 · lists · gemini");

        var result = await Try().Handle("extraction", "prompt-v2.1-e1", "ayse");

        var row = Assert.Single(result!.Rows);
        Assert.Equal(("addProtest", 50, 100), (row.Decision, row.KeptCurrent, row.KeptTried));
        var types = result.Fields.Single(f => f.Field == "protest.types");
        Assert.Equal((0, 100), (types.KeptCurrent, types.KeptTried));
    }

    [Fact]
    public async Task WithoutDecidedArticlesThereIsNothingToTry()
    {
        var result = await Try().Handle("extraction", "prompt-v2.1-e1", "ayse");

        Assert.NotNull(result!.Message);
        Assert.Empty(_pipeline.Tried);
    }

    [Fact]
    public async Task AiIleDoldurUsesTheActiveEditedExtractionPrompt()
    {
        _pipeline.Add("a", "https://x/a", "Başlık", DateTimeOffset.UtcNow, "Metin burada.");
        _pipeline.Prompts = new PipelinePrompts(
            [new PipelinePrompt("extraction", "prompt-v2.1-e2", "gemini", "DÜZENLENMİŞ {{TITLE}} {{TEXT}} {{LISTS}}", "prompt-v2.1")], 0);

        var prompt = await new GetAiPromptHandler(_pipeline, _db).Handle(new GetAiPromptQuery("a"));

        Assert.Equal("prompt-v2.1-e2", prompt.Prompt!.Version);
        Assert.StartsWith("DÜZENLENMİŞ Başlık Metin burada.", prompt.Prompt.Text);

        _pipeline.Prompts = new PipelinePrompts([new PipelinePrompt("extraction", "prompt-v2.1", "gemini", "x", "prompt-v2.1")], 0);
        Assert.Equal(GetAiPromptHandler.Version, (await new GetAiPromptHandler(_pipeline, _db).Handle(new GetAiPromptQuery("a"))).Prompt!.Version);
    }
}
