using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RS.COMMON.Entities;
using RS.EF;
using RS.MVC.Applications;
using RS.MVC.Models;
using static RS.COMMON.Constants.Enums;

namespace RS.Tests;

public class InboxApplicationTests
{
    private static readonly DateTimeOffset Nov3 = new(2026, 11, 3, 10, 42, 0, TimeSpan.FromHours(3));

    private readonly RSDBContext _db = new(new DbContextOptionsBuilder<RSDBContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private readonly FakePipeline _pipeline = new();
    private readonly InboxApplication _inbox;

    public InboxApplicationTests()
    {
        var settings = Options.Create(new PipelineSettings { Sources = new() { [1] = "evrensel" } });
        _inbox = new InboxApplication(_db, _pipeline, settings, NullLogger<InboxApplication>.Instance);
        _pipeline.Add("a1", "https://evrensel.net/haber/1/x", "Tekstil işçileri iş bıraktı");
        _pipeline.Add("a2", "https://evrensel.net/haber/2/y", "Belediye işçileri eylemde");
    }

    [Fact]
    public async Task ListsOpenArticlesForTheMonth()
    {
        var page = await _inbox.GetPageAsync("2026-11", "evrensel", next: null);

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
        var result = await _inbox.DismissAsync("a1", "  İlgisiz  ");

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

        await _inbox.DismissAsync("a1", null);

        var news = Assert.Single(_db.News);
        Assert.Equal(("a1", Status.Passive, "Eski"), (news.ArticleId, news.Status, news.Header));
    }

    [Fact]
    public async Task AnOldRowAlreadyLinkedToACaseCountsAsHandled()
    {
        _db.News.Add(new News { Link = "https://evrensel.net/haber/1/x", Header = "Eski", Status = Status.Linked, Source = 1 });
        await _db.SaveChangesAsync();

        var result = await _inbox.DismissAsync("a1", null);

        Assert.Equal(DismissResult.AlreadyHandled, result);
        var news = Assert.Single(_db.News);
        Assert.Equal(("a1", Status.Linked), (news.ArticleId, news.Status));
        Assert.Empty(_pipeline.Outcomes);
    }

    [Fact]
    public async Task DismissingTwiceIsAlreadyHandled()
    {
        await _inbox.DismissAsync("a1", null);

        Assert.Equal(DismissResult.AlreadyHandled, await _inbox.DismissAsync("a1", null));
        Assert.Single(_db.News);
    }

    [Fact]
    public async Task UnknownArticleIsNotFound()
    {
        Assert.Equal(DismissResult.NotFound, await _inbox.DismissAsync("nope", null));
        Assert.Empty(_db.News);
    }

    [Fact]
    public async Task WhenThePipelineMissesTheOutcomeTheInboxHidesTheArticleAndResendsIt()
    {
        _pipeline.FailOutcomes = true;
        Assert.Equal(DismissResult.DismissedOutcomePending, await _inbox.DismissAsync("a1", null));
        Assert.Single(_db.News); // still handled in the app

        _pipeline.FailOutcomes = false;
        var page = await _inbox.GetPageAsync("2026-11", null, null);

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

        var page = await _inbox.GetPageAsync("2026-11", null, null);

        Assert.Equal(["a1"], page.Items.Select(i => i.ArticleId));
        Assert.Equal(("a2", new PipelineOutcomeRequest("Digested", 412)), Assert.Single(_pipeline.Outcomes));
    }

    [Fact]
    public async Task AFailedResendDoesNotBreakThePage()
    {
        _db.News.Add(new News { Link = "x", ArticleId = "a2", Status = Status.Passive });
        await _db.SaveChangesAsync();
        _pipeline.FailOutcomes = true;

        var page = await _inbox.GetPageAsync("2026-11", null, null);

        Assert.Equal(["a1"], page.Items.Select(i => i.ArticleId));
    }

    [Theory]
    [InlineData("2026-02", "2026-02-01", "2026-02-28")]
    [InlineData("2028-02", "2028-02-01", "2028-02-29")]
    [InlineData("2026-12", "2026-12-01", "2026-12-31")]
    public void MonthRangeCoversTheWholeMonth(string month, string from, string to) =>
        Assert.Equal((DateOnly.Parse(from), DateOnly.Parse(to)), InboxApplication.MonthRange(month));

    [Fact]
    public void AMalformedMonthIsRejected() =>
        Assert.Throws<FormatException>(() => InboxApplication.MonthRange("11/2026"));

    private sealed class FakePipeline : IPipelineClient
    {
        private readonly List<PipelineArticleDetail> _articles = [];

        public List<PipelineArticleQuery> Queries { get; } = [];
        public List<(string ArticleId, PipelineOutcomeRequest Outcome)> Outcomes { get; } = [];
        public bool FailOutcomes { get; set; }

        public void Add(string id, string url, string title) =>
            _articles.Add(new PipelineArticleDetail(id, "evrensel", url, title, Nov3, "Metin"));

        public Task<IReadOnlyList<PipelineSubmitResult>> SubmitAsync(IReadOnlyList<PipelineArticle> articles, string origin) =>
            throw new NotSupportedException();

        public Task<PipelineArticleList> ListArticlesAsync(PipelineArticleQuery query)
        {
            Queries.Add(query);
            var items = _articles.Select(a => new PipelineArticleSummary(a.ArticleId, a.SourceId, a.Url, a.Title, a.PublishedAt)).ToList();
            return Task.FromResult(new PipelineArticleList(items, null!));
        }

        public Task<PipelineArticleDetail> GetArticleAsync(string articleId) =>
            Task.FromResult(_articles.FirstOrDefault(a => a.ArticleId == articleId)!);

        public Task RecordOutcomeAsync(string articleId, PipelineOutcomeRequest outcome)
        {
            if (FailOutcomes)
                throw new HttpRequestException("pipeline down");
            Outcomes.Add((articleId, outcome));
            return Task.CompletedTask;
        }
    }
}
