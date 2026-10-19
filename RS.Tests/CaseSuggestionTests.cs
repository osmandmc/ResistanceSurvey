using Microsoft.EntityFrameworkCore;
using RS.COMMON.Entities;
using RS.EF;
using RS.MVC.Features.Review;

namespace RS.Tests;

public class CaseSuggestionTests
{
    [Theory]
    [InlineData("Örnek Tekstil San. ve Tic. A.Ş.", "örnek tekstil")]
    [InlineData("İZMİR METRO A.Ş.", "izmir metro")]
    [InlineData("Bosch Ltd. Şti.", "bosch")]
    [InlineData("ABC A.Ş.", null)]          // too short to mean one company
    [InlineData("Sanayi ve Ticaret A.Ş.", null)] // only legal words
    public void TheCoreDropsLegalWords(string name, string? core) => Assert.Equal(core, CompanyNameMatcher.Core(name));

    [Theory]
    [InlineData("İzmir Metro işçileri iş bıraktı", true)]
    [InlineData("İZMİR METRO'da grev", true)]          // apostrophe and capitals
    [InlineData("İzmir Metropol Belediyesi açıkladı", false)] // whole words only
    [InlineData("Ankara metrosunda arıza", false)]
    public void CompaniesAreFoundAsWholeWords(string text, bool found)
    {
        var matches = CompanyNameMatcher.Find([(1, "İzmir Metro A.Ş.")], text, "");
        Assert.Equal(found, matches.Count == 1);
    }

    [Fact]
    public void TitleMatchesComeFirst()
    {
        var matches = CompanyNameMatcher.Find(
            [(1, "Örnek Tekstil A.Ş."), (2, "Liman İşletmeleri A.Ş.")],
            title: "Liman İşletmeleri'nde eylem",
            body: "Örnek Tekstil işçileri de destek verdi. Liman İşletmeleri yönetimi açıklama yapmadı.");

        Assert.Equal([(2, true), (1, false)], matches.Select(m => (m.CompanyId, m.InTitle)));
    }

    private readonly RSDBContext _db = new(new DbContextOptionsBuilder<RSDBContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private readonly FakePipeline _pipeline = new();

    private SuggestCasesForArticleHandler Handler() => new(_db, _pipeline, new SearchCasesHandler(_db));

    private Resistance AddCase(string name, Company company, Company? main = null, DateTime? lastProtest = null)
    {
        var r = new Resistance
        {
            Name = name,
            Company = company,
            MainCompany = main,
            Protestos = [new Protesto { StartDate = lastProtest ?? new DateTime(2026, 9, 1), Locations = [] }],
        };
        _db.Resistance.Add(r);
        _db.SaveChanges();
        return r;
    }

    [Fact]
    public async Task SuggestsTheCasesOfCompaniesNamedInTheArticle()
    {
        var tekstil = new Company { Name = "Örnek Tekstil San. ve Tic. A.Ş." };
        var liman = new Company { Name = "Liman İşletmeleri A.Ş." };
        var temizlik = new Company { Name = "Parlak Temizlik Ltd. Şti." };
        var tekstilCase = AddCase("Tekstil ücret direnişi", tekstil, lastProtest: new DateTime(2026, 8, 1));
        var subcontracted = AddCase("Temizlik işçileri direnişi", temizlik, main: liman);
        AddCase("Başka bir direniş", new Company { Name = "Uzak Lojistik A.Ş." });
        _pipeline.Add("a1", "https://evrensel.net/haber/1", "Liman İşletmeleri önünde eylem",
            DateTimeOffset.Now, "Taşeron işçiler eylem yaptı. Örnek Tekstil'de de grev sürüyor.");

        var suggestions = await Handler().Handle(new SuggestCasesForArticleQuery("a1"));

        // The title names Liman İşletmeleri: its subcontractor's case comes first.
        Assert.Equal([subcontracted.Id, tekstilCase.Id], suggestions.Select(s => s.Case.Id));
        Assert.Equal(("Liman İşletmeleri A.Ş.", true), (suggestions[0].MatchedCompany, suggestions[0].InTitle));
    }

    [Fact]
    public async Task NoKnownCompanyOrUnknownArticleGivesNothing()
    {
        AddCase("Tekstil ücret direnişi", new Company { Name = "Örnek Tekstil A.Ş." });
        _pipeline.Add("a1", "https://evrensel.net/haber/1", "Asgari ücret tartışması", DateTimeOffset.Now, "Sendikalar açıklama yaptı.");

        Assert.Empty(await Handler().Handle(new SuggestCasesForArticleQuery("a1")));
        Assert.Empty(await Handler().Handle(new SuggestCasesForArticleQuery("nope")));
    }

    [Fact]
    public void TheCompanyListQueryTranslatesToSqlServer()
    {
        using var sqlServer = new RSDBContext(new DbContextOptionsBuilder<RSDBContext>().UseSqlServer("Server=none;Database=none").Options);

        var sql = new SearchCasesHandler(sqlServer).Build(new SearchCasesQuery("", CompanyIds: [1, 2])).ToQueryString();

        Assert.Contains("ORDER BY", sql);
    }
}
