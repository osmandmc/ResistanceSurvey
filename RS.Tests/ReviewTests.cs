using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RS.COMMON.Entities;
using RS.COMMON.Entities.LookupEntity;
using RS.EF;
using RS.MVC.Applications;
using RS.MVC.Controllers;
using RS.MVC.Features.Articles;
using RS.MVC.Features.Review;
using RS.MVC.Models;
using static RS.COMMON.Constants.Enums;

namespace RS.Tests;

public class ReviewTests
{
    private static readonly DateTimeOffset Oct19 = new(2026, 10, 19, 17, 58, 0, TimeSpan.FromHours(3));
    private const string ArticleUrl = "https://www.evrensel.net/haber/6005665/bursa-nev-hastanesinde-eylem";

    // The in-memory database has no transactions; the code still uses one (real SQL Server in the app).
    private readonly RSDBContext _db = new(new DbContextOptionsBuilder<RSDBContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
        .Options);

    private readonly FakePipeline _pipeline = new();
    private readonly GetReviewArticleHandler _getArticle;
    private readonly SearchCasesHandler _searchCases;
    private readonly CreateCaseFromArticleHandler _createCase;
    private readonly AddProtestFromArticleHandler _addProtest;
    private readonly LinkArticleToCaseHandler _linkToCase;

    public ReviewTests()
    {
        var settings = Options.Create(new PipelineSettings { Sources = new() { [1] = "evrensel" } });
        var articleNews = new ArticleNewsService(_db, settings);
        var linker = new ArticleCaseLinker(_db, _pipeline, articleNews, new OutcomeSender(_pipeline, NullLogger<OutcomeSender>.Instance));
        var resistances = new ResistanceApplication(_db);
        _getArticle = new GetReviewArticleHandler(_pipeline, articleNews);
        _searchCases = new SearchCasesHandler(_db);
        _createCase = new CreateCaseFromArticleHandler(linker, resistances, new ListValueResolver(_db));
        _addProtest = new AddProtestFromArticleHandler(linker, resistances);
        _linkToCase = new LinkArticleToCaseHandler(linker);
        _pipeline.Add("a1", ArticleUrl, "Bursa NEV Hastanesi’nde eylem", Oct19, "Hekimler ve sağlık çalışanları 6 aydır maaşlarını alamıyor.");
    }

    // Dispatches by action, as ReviewController does.
    private Task<ReviewSaveResult> Save(string articleId, ReviewSaveRequest request, string userName) => request.Action switch
    {
        ReviewAction.NewCase => _createCase.Handle(new CreateCaseFromArticleCommand(articleId, request.Case!, request.Protest!, userName)),
        ReviewAction.AddProtest => _addProtest.Handle(new AddProtestFromArticleCommand(articleId, request.ResistanceId, request.Protest!, userName)),
        _ => _linkToCase.Handle(new LinkArticleToCaseCommand(articleId, request.ResistanceId)),
    };

    private Task<ReviewArticleResult> GetArticle(string articleId) => _getArticle.Handle(new GetReviewArticleQuery(articleId));

    private Resistance AddResistance(string name, string? company, params DateTime[] protestDates)
    {
        var resistance = new Resistance
        {
            Name = name,
            Company = company == null ? null : new Company { Name = company },
            Protestos = protestDates.Select(d => new Protesto { StartDate = d }).ToList(),
            ResistanceNews = [],
        };
        _db.Resistance.Add(resistance);
        _db.SaveChanges();
        return resistance;
    }

    [Fact]
    public async Task TheArticleComesWithItsFullText()
    {
        var result = await GetArticle("a1");

        Assert.Equal(ReviewStatus.Ok, result.Status);
        Assert.Equal("Evrensel", result.Article!.SourceName);
        Assert.Equal("Hekimler ve sağlık çalışanları 6 aydır maaşlarını alamıyor.", result.Article.Body);
        Assert.Equal(ReviewStatus.NotFound, (await GetArticle("nope")).Status);
    }

    [Fact]
    public async Task ANewCaseSavesTheResistanceItsProtestAndTheLink()
    {
        var result = await Save("a1", ReviewValidationTests.CompleteNewCase(), "admin");

        Assert.Equal(ReviewStatus.Ok, result.Status);
        Assert.True(result.OutcomeSent);
        var resistance = _db.Resistance
            .Include(r => r.Protestos).ThenInclude(p => p.Locations)
            .Include(r => r.Protestos).ThenInclude(p => p.ProtestoProtestoTypes)
            .Include(r => r.ResistanceResistanceReasons)
            .Include(r => r.ResistanceNews)
            .Single();
        Assert.Equal(result.ResistanceId, resistance.Id);
        Assert.Equal(("Örnek Tekstil ücret direnişi", 1, 10, "admin"), (resistance.Name, resistance.CategoryId, resistance.CompanyId, resistance.Creator));
        Assert.Single(resistance.ResistanceResistanceReasons);
        var protest = Assert.Single(resistance.Protestos);
        Assert.Equal(2, protest.ProtestoProtestoTypes.Count);
        Assert.Equal("Fabrika önü", Assert.Single(protest.Locations).Place);
        Assert.Equal("Vardiya sonunda 15 dakika iş bırakma", protest.SimpleProtestoDescription);

        var news = Assert.Single(_db.News);
        Assert.Equal(("a1", Status.Linked, ArticleUrl), (news.ArticleId, news.Status, news.Link));
        Assert.Equal(news.Id, Assert.Single(resistance.ResistanceNews).NewsId);
        Assert.Equal(("a1", new PipelineOutcomeRequest("Digested", resistance.Id)), Assert.Single(_pipeline.Outcomes));
    }

    [Fact]
    public async Task TypedValuesUseExistingEntriesOrBecomeNewOnes()
    {
        _db.ResistanceReason.Add(new ResistanceReason { Name = "İşten Çıkarma" });
        _db.Corporation.Add(new Corporation { Name = "Birleşik Metal-İş", CorporationTypeId = 1 });
        await _db.SaveChangesAsync();
        var request = ReviewValidationTests.CompleteNewCase();
        request.Case!.ReasonIds = [];
        request.Case.NewReasonNames = ["  işten   çıkarma ", "Ücret alacağı", "ücret ALACAĞI"];
        request.Case.CorporationIds = [];
        request.Case.NewCorporationNames = ["Yeni Dayanışma Derneği"];

        var result = await Save("a1", request, "admin");

        Assert.Equal(ReviewStatus.Ok, result.Status);
        Assert.Equal(["İşten Çıkarma", "Ücret alacağı"], _db.ResistanceReason.Select(r => r.Name).OrderBy(n => n));
        Assert.Equal(2, _db.ResistanceResistanceReason.Count(r => r.ResistanceId == result.ResistanceId));
        var newCorporation = Assert.Single(_db.Corporation, c => c.Name == "Yeni Dayanışma Derneği");
        Assert.Null(newCorporation.CorporationTypeId); // name only, as in the old form
        Assert.Single(_db.ResistanceCorporation, rc => rc.CorporationId == newCorporation.Id);
    }

    [Fact]
    public async Task AddProtestAddsToTheExistingCaseAndLinksTheArticle()
    {
        var existing = AddResistance("NEV Hastanesi direnişi", "NEV Hastanesi", new DateTime(2026, 9, 1));
        var request = new ReviewSaveRequest { Action = ReviewAction.AddProtest, ResistanceId = existing.Id, Protest = ReviewValidationTests.CompleteProtest() };

        var result = await Save("a1", request, "admin");

        Assert.Equal((ReviewStatus.Ok, existing.Id), (result.Status, result.ResistanceId));
        Assert.Equal(2, _db.Protesto.Count(p => p.ResistanceId == existing.Id));
        Assert.Single(_db.Protesto, p => p.SimpleProtestoDescription == "Vardiya sonunda 15 dakika iş bırakma");
        Assert.Single(_db.ResistanceNews, rn => rn.ResistanceId == existing.Id);
        Assert.Equal(new PipelineOutcomeRequest("Digested", existing.Id), Assert.Single(_pipeline.Outcomes).Outcome);
    }

    [Fact]
    public async Task LinkOnlyAttachesTheArticleWithoutANewProtest()
    {
        var existing = AddResistance("NEV Hastanesi direnişi", "NEV Hastanesi", new DateTime(2026, 9, 1));

        var result = await Save("a1", new ReviewSaveRequest { Action = ReviewAction.LinkOnly, ResistanceId = existing.Id }, "admin");

        Assert.Equal(ReviewStatus.Ok, result.Status);
        Assert.Equal(1, _db.Protesto.Count());
        Assert.Single(_db.ResistanceNews, rn => rn.ResistanceId == existing.Id);
        Assert.Equal(Status.Linked, Assert.Single(_db.News).Status);
    }

    [Fact]
    public async Task AnArticleImportedTheOldWayIsReused()
    {
        var existing = AddResistance("NEV Hastanesi direnişi", null);
        _db.News.Add(new News { Link = ArticleUrl, Header = "Eski", Status = Status.Pending, Source = 1 });
        await _db.SaveChangesAsync();

        await Save("a1", new ReviewSaveRequest { Action = ReviewAction.LinkOnly, ResistanceId = existing.Id }, "admin");

        var news = Assert.Single(_db.News);
        Assert.Equal(("a1", Status.Linked, "Eski"), (news.ArticleId, news.Status, news.Header));
    }

    [Fact]
    public async Task MissingFieldsSaveNothing()
    {
        var result = await Save("a1", new ReviewSaveRequest { Action = ReviewAction.NewCase, Case = new(), Protest = new() }, "admin");

        Assert.Equal(ReviewStatus.Invalid, result.Status);
        Assert.Contains("case.categoryId", result.Errors!.Keys);
        Assert.Empty(_db.News);
        Assert.Empty(_db.Resistance);
        Assert.Empty(_pipeline.Outcomes);
    }

    [Fact]
    public async Task AnUnknownCaseIsRejected()
    {
        var result = await Save("a1", new ReviewSaveRequest { Action = ReviewAction.LinkOnly, ResistanceId = 999 }, "admin");

        Assert.Equal(ReviewStatus.Invalid, result.Status);
        Assert.Equal(["resistanceId"], result.Errors!.Keys);
        Assert.Empty(_db.News);
    }

    [Fact]
    public async Task AnAlreadyHandledArticleIsNotSavedAgain()
    {
        await Save("a1", ReviewValidationTests.CompleteNewCase(), "admin");

        var second = await Save("a1", ReviewValidationTests.CompleteNewCase(), "user");

        Assert.Equal(ReviewStatus.AlreadyHandled, second.Status);
        Assert.Single(_db.Resistance);
        Assert.Equal(ReviewStatus.AlreadyHandled, (await GetArticle("a1")).Status);
    }

    [Fact]
    public async Task WhenThePipelineMissesTheOutcomeTheCaseIsStillSaved()
    {
        _pipeline.FailOutcomes = true;

        var result = await Save("a1", ReviewValidationTests.CompleteNewCase(), "admin");

        Assert.Equal(ReviewStatus.Ok, result.Status);
        Assert.False(result.OutcomeSent);
        Assert.Single(_db.Resistance);
        Assert.Equal(Status.Linked, Assert.Single(_db.News).Status); // the inbox re-sends Digested later
    }

    [Fact]
    public async Task TheControllerRejectsAnUnknownAction()
    {
        var controller = new ReviewController(
            _getArticle, _searchCases, _createCase, _addProtest, _linkToCase,
            new GetCaseFormOptionsHandler(_db), new SearchCompaniesHandler(_db), new CreateCompanyHandler(_db),
            new SuggestCasesForArticleHandler(_db, _pipeline, _searchCases), NullLogger<ReviewController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };

        var response = await controller.Save("a1", new ReviewSaveRequest { Action = "delete" });

        Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Empty(_db.News);
    }

    [Fact]
    public async Task SearchFindsCasesByNameOrCompanyMostRecentFirst()
    {
        AddResistance("Belediye işçileri direnişi", "İzmir Belediyesi", new DateTime(2026, 8, 1));
        AddResistance("Tekstil ücret direnişi", "Örnek Tekstil", new DateTime(2026, 10, 1), new DateTime(2026, 9, 1));
        var deleted = AddResistance("Eski tekstil direnişi", "Örnek Tekstil", new DateTime(2026, 10, 15));
        deleted.Deleted = true;
        await _db.SaveChangesAsync();

        var byCompany = await _searchCases.Handle(new SearchCasesQuery("Tekstil"));
        var all = await _searchCases.Handle(new SearchCasesQuery(""));

        var hit = Assert.Single(byCompany);
        Assert.Equal(("Tekstil ücret direnişi", "Örnek Tekstil", new DateTime(2026, 10, 1)), (hit.Name, hit.CompanyName, hit.LastProtestDate));
        Assert.Equal(["Tekstil ücret direnişi", "Belediye işçileri direnişi"], all.Select(r => r.Name));
    }
}
