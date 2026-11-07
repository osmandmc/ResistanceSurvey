using Microsoft.EntityFrameworkCore;
using RS.COMMON.Entities;
using RS.EF;
using RS.MVC.Features.Cases;
using static RS.COMMON.Constants.Enums;

namespace RS.Tests;

/// <summary>The full text of a linked article in the case's Haberler list.</summary>
public class CaseArticleTests
{
    private static readonly DateTimeOffset Oct19 = new(2026, 10, 19, 9, 30, 0, TimeSpan.FromHours(3));

    private readonly RSDBContext _db = new(new DbContextOptionsBuilder<RSDBContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options);

    private readonly FakePipeline _pipeline = new();
    private readonly GetCaseArticleHandler _handler;
    private readonly Resistance _case;
    private readonly Resistance _otherCase;

    public CaseArticleTests()
    {
        _handler = new GetCaseArticleHandler(_db, _pipeline);
        _pipeline.Add("a1", "https://www.evrensel.net/haber/1/eylem", "Bursa NEV Hastanesi’nde eylem", Oct19,
            "Hekimler ve sağlık çalışanları 6 aydır maaşlarını alamıyor.", "https://www.evrensel.net/images/1.jpg");
        _pipeline.Add("a2", "https://www.evrensel.net/haber/2/grev", "Başka bir grev", Oct19);
        _case = new Resistance
        {
            Name = "Hastane eylemi",
            ResistanceNews = [new ResistanceNews { News = new News { Header = "Bursa NEV Hastanesi’nde eylem", Link = "https://www.evrensel.net/haber/1/eylem", ArticleId = "a1", Status = Status.Linked } }],
        };
        _otherCase = new Resistance
        {
            Name = "Grev",
            ResistanceNews = [new ResistanceNews { News = new News { Header = "Başka bir grev", Link = "https://www.evrensel.net/haber/2/grev", ArticleId = "a2", Status = Status.Linked } }],
        };
        _db.Resistance.AddRange(_case, _otherCase);
        _db.SaveChanges();
    }

    private Task<RS.MVC.Models.ReviewArticle> Get(int caseId, string articleId) => _handler.Handle(new GetCaseArticleQuery(caseId, articleId));

    [Fact]
    public async Task ALinkedArticleComesWithItsFullTextAlthoughItIsHandled()
    {
        var article = await Get(_case.Id, "a1");

        Assert.NotNull(article);
        Assert.Equal("Bursa NEV Hastanesi’nde eylem", article.Title);
        Assert.Equal("Evrensel", article.SourceName);
        Assert.Equal(Oct19, article.PublishedAt);
        Assert.Equal("Hekimler ve sağlık çalışanları 6 aydır maaşlarını alamıyor.", article.Body);
        Assert.Equal("https://www.evrensel.net/images/1.jpg", article.ImageUrl);
        Assert.Null(article.Ai);
    }

    [Fact]
    public async Task AnArticleLinkedToAnotherCaseIsNotShown()
    {
        Assert.Null(await Get(_case.Id, "a2"));
        Assert.Null(await Get(_case.Id, "unknown"));
    }

    [Fact]
    public async Task ADeletedCaseShowsNoArticle()
    {
        _case.Deleted = true;
        _db.SaveChanges();

        Assert.Null(await Get(_case.Id, "a1"));
    }

    [Fact]
    public async Task ALinkedArticleThePipelineNoLongerHasIsNotFound()
    {
        _db.News.Add(new News { Header = "Silinmiş", Link = "https://x/3", ArticleId = "gone", Status = Status.Linked });
        _db.SaveChanges();
        _db.ResistanceNews.Add(new ResistanceNews { ResistanceId = _case.Id, NewsId = _db.News.Single(n => n.ArticleId == "gone").Id });
        _db.SaveChanges();

        Assert.Null(await Get(_case.Id, "gone"));
    }

    [Fact]
    public async Task ThePipelineNotAnsweringIsAnError()
    {
        _pipeline.FailArticles = true;

        // ApiExceptionHandler turns this into 502 “Haber işleme hattına ulaşılamadı”.
        await Assert.ThrowsAsync<HttpRequestException>(() => Get(_case.Id, "a1"));
    }
}
