using Microsoft.EntityFrameworkCore;
using RS.COMMON.Entities;
using RS.COMMON.Entities.LookupEntity;
using RS.EF;
using RS.MVC.Features.Review;

namespace RS.Tests;

/// <summary>R20: existing cases for an article, from its AI answer.</summary>
public class AiCaseMatchTests
{
    private readonly RSDBContext _db = new(new DbContextOptionsBuilder<RSDBContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    private readonly FakePipeline _pipeline = new();
    private static readonly DateTime Oct14 = new(2026, 10, 14);

    private readonly City _kocaeli = new() { Id = 41, Name = "Kocaeli" };
    private readonly City _istanbul = new() { Id = 34, Name = "İstanbul" };
    private readonly ProtestoType _strike = new() { Id = 50, Name = "Fiili grev" };
    private readonly ProtestoType _statement = new() { Id = 51, Name = "Basın açıklaması" };
    private readonly Company _metal = new() { Id = 10, Name = "Örnek Metal Sanayi ve Ticaret A.Ş." };
    private readonly Corporation _disk = new() { Id = 20, Name = "DİSK Birleşik Metal-İş" };

    public AiCaseMatchTests()
    {
        _db.AddRange(_kocaeli, _istanbul, _strike, _statement, _metal, _disk);
        _db.SaveChanges();
        _pipeline.Add("a1", "https://www.evrensel.net/haber/1/x", "Örnek Metal işçileri iş bıraktı", new DateTimeOffset(Oct14.AddHours(9), TimeSpan.FromHours(3)));
    }

    private Resistance AddCase(string name, Company? company, Corporation? organization, params Protesto[] protests)
    {
        var resistance = new Resistance
        {
            Name = name,
            Company = company,
            Protestos = protests.ToList(),
            ResistanceCorporations = organization == null ? [] : [new ResistanceCorporation { Corporation = organization }],
        };
        _db.Resistance.Add(resistance);
        _db.SaveChanges();
        return resistance;
    }

    private Protesto Protest(DateTime start, City city, ProtestoType type, DateTime? end = null) => new()
    {
        StartDate = start,
        EndDate = end,
        Locations = [new ProtestoLocation { City = city, CityId = city.Id }],
        ProtestoProtestoTypes = [new ProtestoProtestoType { ProtestoType = type, ProtestoTypeId = type.Id }],
    };

    private static string Answer(string? company, string city = "Kocaeli", string type = "Fiili grev", string date = "2026-10-14") => $$"""
        { "relevant": true, "case": { "company": {{(company == null ? "null" : $"\"{company}\"")}}, "organizations": ["DİSK Birleşik Metal-İş"] },
          "protest": { "start_date": "{{date}}", "types": ["{{type}}"], "locations": [{ "city": "{{city}}" }] } }
        """;

    private Task<AiCaseMatches> Match(string answer) =>
        new MatchCasesFromAiHandler(_db, new CheckAiAnswerHandler(_db), _pipeline, new SearchCasesHandler(_db)).Handle(new MatchCasesFromAiCommand("a1", answer));

    [Fact]
    public async Task TheCompanysOngoingCaseIsLikelyAndTheSameProtestLooksRecorded()
    {
        var current = AddCase("Örnek Metal TİS grevi", _metal, _disk,
            Protest(Oct14.AddDays(-20), _kocaeli, _statement),
            Protest(Oct14.AddDays(-1), _kocaeli, _strike)); // the protest the article tells of, recorded from another article
        var old = AddCase("Örnek Metal 2023", _metal, null, Protest(Oct14.AddYears(-2), _kocaeli, _strike, Oct14.AddYears(-2)));

        var matches = await Match(Answer("Örnek Metal"));

        var likely = Assert.Single(matches.Likely);
        Assert.Equal(current.Id, likely.Case.Id);
        Assert.Equal(["aynı şirket", "aynı il", "devam ediyor", "son eylem 1 gün önce"], likely.Reasons);
        Assert.Equal((Oct14.AddDays(-1), "Kocaeli", "Fiili grev"), (likely.Recorded!.StartDate, likely.Recorded.Cities.Single(), likely.Recorded.Types.Single()));
        Assert.Equal([old.Id], matches.Older.Select(m => m.Case.Id)); // context, not a match
    }

    [Fact]
    public async Task AProtestOfAnotherTypeOrCityIsNotTheSameOne()
    {
        AddCase("Örnek Metal TİS grevi", _metal, null, Protest(Oct14.AddDays(-1), _kocaeli, _statement));

        var matches = await Match(Answer("Örnek Metal"));

        Assert.Null(Assert.Single(matches.Likely).Recorded);
    }

    [Fact]
    public async Task WithoutACompanyTheOrganizationInTheSameCityCountsOnlyWithARecentProtest()
    {
        var recent = AddCase("DİSK: Gebze yürüyüşü", null, _disk, Protest(Oct14.AddDays(-10), _kocaeli, _statement));
        AddCase("DİSK: eski eylem", null, _disk, Protest(Oct14.AddYears(-1), _kocaeli, _statement, Oct14.AddYears(-1)));
        AddCase("DİSK: İstanbul", null, _disk, Protest(Oct14.AddDays(-5), _istanbul, _statement));

        var matches = await Match(Answer(null));

        Assert.Equal([recent.Id], matches.Likely.Select(m => m.Case.Id));
        Assert.Contains("aynı örgüt, aynı il", matches.Likely[0].Reasons);
        Assert.Empty(matches.Older);
    }

    [Fact]
    public async Task WithACompanyOtherCasesOfTheOrganizationDoNotCount()
    {
        AddCase("Renault", new Company { Name = "Renault" }, _disk, Protest(Oct14.AddDays(-3), _kocaeli, _strike));

        var matches = await Match(Answer("Örnek Metal"));

        Assert.Equal(AiCaseMatches.None.Likely, matches.Likely);
    }

    [Fact]
    public async Task ANotRelevantOrUnreadableAnswerMatchesNothing()
    {
        AddCase("Örnek Metal TİS grevi", _metal, null, Protest(Oct14.AddDays(-1), _kocaeli, _strike));

        Assert.Empty((await Match("""{"relevant": false}""")).Likely);
        Assert.Empty((await Match("bozuk")).Likely);
    }

    [Fact]
    public void BothCandidateQueriesTranslateToSqlServer()
    {
        // No database needed: EF builds the SQL and fails here if any part cannot be translated.
        using var sqlServer = new RSDBContext(new DbContextOptionsBuilder<RSDBContext>().UseSqlServer("Server=none;Database=none").Options);
        var handler = new MatchCasesFromAiHandler(sqlServer, new CheckAiAnswerHandler(sqlServer), _pipeline, new SearchCasesHandler(sqlServer));

        var byCompany = handler.Candidates([10], [], [41], Oct14)!.ToQueryString();
        var byOrganization = handler.Candidates([], [20], [41], Oct14)!.ToQueryString();

        Assert.Contains("SELECT", byCompany);
        Assert.Contains("ResistanceCorporation", byOrganization);
        Assert.Null(handler.Candidates([], [], [41], Oct14));
    }
}

