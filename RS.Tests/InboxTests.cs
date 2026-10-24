using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RS.COMMON.Entities;
using RS.EF;
using RS.MVC.Features.Articles;
using RS.MVC.Features.Inbox;
using RS.MVC.Features.Review;
using RS.MVC.Models;
using static RS.COMMON.Constants.Enums;

namespace RS.Tests;

public class InboxTests
{
    private static readonly DateTimeOffset Nov3 = new(2026, 11, 3, 10, 42, 0, TimeSpan.FromHours(3));

    private readonly RSDBContext _db = new(new DbContextOptionsBuilder<RSDBContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private readonly FakePipeline _pipeline = new();
    private readonly GetInboxHandler _getInbox;
    private readonly DismissArticleHandler _dismiss;

    public InboxTests()
    {
        var settings = Options.Create(new PipelineSettings { Sources = new() { [1] = "evrensel" } });
        var outcomes = new OutcomeSender(_pipeline, NullLogger<OutcomeSender>.Instance);
        _getInbox = new GetInboxHandler(_db, _pipeline, outcomes);
        _dismiss = new DismissArticleHandler(_db, _pipeline, new ArticleNewsService(_db, settings), outcomes, new AiSuggestionRecorder(_db, new CheckAiAnswerHandler(_db)));
        _pipeline.Add("a1", "https://evrensel.net/haber/1/x", "Tekstil işçileri iş bıraktı", Nov3);
        _pipeline.Add("a2", "https://evrensel.net/haber/2/y", "Belediye işçileri eylemde", Nov3);
    }

    [Fact]
    public async Task ListsOpenArticlesForTheMonth()
    {
        var page = await _getInbox.Handle(new GetInboxQuery("2026-11", "evrensel", null));

        Assert.Equal(["a1", "a2"], page.Items.Select(i => i.ArticleId));
        Assert.Equal("Evrensel", page.Items[0].SourceName);
        var query = Assert.Single(_pipeline.Queries);
        Assert.Equal(new DateOnly(2026, 11, 1), query.From);
        Assert.Equal(new DateOnly(2026, 11, 30), query.To);
        Assert.Equal("evrensel", query.SourceId);
        Assert.Equal(("parsed", "open"), (query.Status, query.Outcome));
    }

    [Fact]
    public async Task DismissSavesAThinNewsRowThenTellsThePipeline()
    {
        var result = await _dismiss.Handle(new DismissArticleCommand("a1", "  İlgisiz  "));

        Assert.Equal(DismissResult.Dismissed, result);
        var news = Assert.Single(_db.News);
        Assert.Equal(("a1", Status.Passive, 1), (news.ArticleId, news.Status, news.Source));
        Assert.Equal("https://evrensel.net/haber/1/x", news.Link);
        Assert.Equal(new DateTime(2026, 11, 3, 10, 42, 0), news.Date); // Turkey time
        Assert.Equal(("a1", new PipelineOutcomeRequest("Dismissed", null, "İlgisiz")), Assert.Single(_pipeline.Outcomes));
    }

    [Fact]
    public async Task DismissReusesARowImportedTheOldWay()
    {
        _db.News.Add(new News { Link = "https://evrensel.net/haber/1/x", Header = "Eski", Status = Status.Pending, Source = 1 });
        await _db.SaveChangesAsync();

        await _dismiss.Handle(new DismissArticleCommand("a1", null));

        var news = Assert.Single(_db.News);
        Assert.Equal(("a1", Status.Passive, "Eski"), (news.ArticleId, news.Status, news.Header));
    }

    [Fact]
    public async Task AnOldRowAlreadyLinkedToACaseCountsAsHandled()
    {
        _db.News.Add(new News { Link = "https://evrensel.net/haber/1/x", Header = "Eski", Status = Status.Linked, Source = 1 });
        await _db.SaveChangesAsync();

        var result = await _dismiss.Handle(new DismissArticleCommand("a1", null));

        Assert.Equal(DismissResult.AlreadyHandled, result);
        var news = Assert.Single(_db.News);
        Assert.Equal(("a1", Status.Linked), (news.ArticleId, news.Status));
        Assert.Empty(_pipeline.Outcomes);
    }

    [Fact]
    public async Task DismissingTwiceIsAlreadyHandled()
    {
        await _dismiss.Handle(new DismissArticleCommand("a1", null));

        Assert.Equal(DismissResult.AlreadyHandled, await _dismiss.Handle(new DismissArticleCommand("a1", null)));
        Assert.Single(_db.News);
    }

    [Fact]
    public async Task UnknownArticleIsNotFound()
    {
        Assert.Equal(DismissResult.NotFound, await _dismiss.Handle(new DismissArticleCommand("nope", null)));
        Assert.Empty(_db.News);
    }

    [Fact]
    public async Task WhenThePipelineMissesTheOutcomeTheInboxHidesTheArticleAndResendsIt()
    {
        _pipeline.FailOutcomes = true;
        Assert.Equal(DismissResult.DismissedOutcomePending, await _dismiss.Handle(new DismissArticleCommand("a1", null)));
        Assert.Single(_db.News); // still handled in the app

        _pipeline.FailOutcomes = false;
        var page = await _getInbox.Handle(new GetInboxQuery("2026-11", null, null));

        Assert.Equal(["a2"], page.Items.Select(i => i.ArticleId));
        Assert.Equal(("a1", new PipelineOutcomeRequest("Dismissed")), Assert.Single(_pipeline.Outcomes));
    }

    [Fact]
    public async Task ALinkedArticleIsResentAsDigestedWithItsCase()
    {
        var news = new News { Link = "https://evrensel.net/haber/2/y", ArticleId = "a2", Status = Status.Linked, Source = 1 };
        _db.News.Add(news);
        await _db.SaveChangesAsync();
        _db.ResistanceNews.Add(new ResistanceNews { NewsId = news.Id, ResistanceId = 412 });
        await _db.SaveChangesAsync();

        var page = await _getInbox.Handle(new GetInboxQuery("2026-11", null, null));

        Assert.Equal(["a1"], page.Items.Select(i => i.ArticleId));
        Assert.Equal(("a2", new PipelineOutcomeRequest("Digested", 412)), Assert.Single(_pipeline.Outcomes));
    }

    [Fact]
    public async Task AFailedResendDoesNotBreakThePage()
    {
        _db.News.Add(new News { Link = "x", ArticleId = "a2", Status = Status.Passive });
        await _db.SaveChangesAsync();
        _pipeline.FailOutcomes = true;

        var page = await _getInbox.Handle(new GetInboxQuery("2026-11", null, null));

        Assert.Equal(["a1"], page.Items.Select(i => i.ArticleId));
    }

    [Theory]
    [InlineData("2026-02", "2026-02-01", "2026-02-28")]
    [InlineData("2028-02", "2028-02-01", "2028-02-29")]
    [InlineData("2026-12", "2026-12-01", "2026-12-31")]
    public void MonthRangeCoversTheWholeMonth(string month, string from, string to) =>
        Assert.Equal((DateOnly.Parse(from), DateOnly.Parse(to)), GetInboxHandler.MonthRange(month));

    [Fact]
    public void AMalformedMonthIsRejected() =>
        Assert.Throws<FormatException>(() => GetInboxHandler.MonthRange("11/2026"));

    [Fact]
    public async Task DismissSendsTheKindWithTheReasonAndKeepsTheAiAnswer()
    {
        var ai = new AiUse { Answer = """{"relevant": false, "relevance_reason": "Eylem yok."}""", PromptVersion = "v2" };

        var result = await _dismiss.Handle(new DismissArticleCommand("a1", " Önceki haberle aynı ", DismissKind.AlreadyRecorded, ai, "admin"));
        await _dismiss.Handle(new DismissArticleCommand("a2", null, DismissKind.NotLabour));

        Assert.Equal(DismissResult.Dismissed, result);
        Assert.Equal(
            [new PipelineOutcomeRequest("Dismissed", null, "Zaten kayıtlı / tekrar haber: Önceki haberle aynı"), new PipelineOutcomeRequest("Dismissed", null, "Emek hareketiyle ilgisiz")],
            _pipeline.Outcomes.Select(o => o.Outcome));
        var kept = Assert.Single(_db.AiSuggestion);
        Assert.Equal((AiDecision.Dismissed, DismissKind.AlreadyRecorded, false, false, (string?)null, (int?)null),
            (kept.Decision, kept.DismissKind, kept.Relevant, kept.Applied, kept.Suggested, kept.ResistanceId));
        Assert.Equal(_db.News.Single(n => n.ArticleId == "a1").Id, kept.NewsId);
    }

    [Fact]
    public async Task ItemsCarryTheAiVerdictAndTheFilterGoesToThePipeline()
    {
        _pipeline.States["a1"] = (new Dictionary<string, PipelineStageResult>
        {
            ["Parsed"] = new("Succeeded", Nov3),
            ["Relevance"] = new("Succeeded", Nov3, "relevance-v1 · gemini", "Spor haberi.", "NotRelevant"),
        }, null!);
        _pipeline.States["a2"] = (new Dictionary<string, PipelineStageResult>
        {
            ["Parsed"] = new("Succeeded", Nov3),
            ["Relevance"] = new("Failed", Nov3, "relevance-v1", "No JSON"),
        }, null!);

        var page = await _getInbox.Handle(new GetInboxQuery("2026-11", null, null, "notRelevant"));

        Assert.Equal("notRelevant", Assert.Single(_pipeline.Queries).Relevance);
        Assert.Equal(new AiRelevance("NotRelevant", "Spor haberi."), page.Items.Single(i => i.ArticleId == "a1").Ai);
        Assert.Null(page.Items.Single(i => i.ArticleId == "a2").Ai); // a failed assessment is no verdict
    }
}

