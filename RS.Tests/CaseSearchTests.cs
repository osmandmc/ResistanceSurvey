using Microsoft.EntityFrameworkCore;
using RS.COMMON.Entities;
using RS.COMMON.Entities.LookupEntity;
using RS.EF;
using RS.MVC.Features.Review;

namespace RS.Tests;

public class CaseSearchTests
{
    private readonly RSDBContext _db = new(new DbContextOptionsBuilder<RSDBContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private readonly City _bursa = new() { Id = 16, Name = "Bursa" };
    private readonly City _izmir = new() { Id = 35, Name = "İzmir" };

    private Resistance AddCase(string name, Company? company, Company? mainCompany = null, string? description = null, params Protesto[] protests)
    {
        var resistance = new Resistance { Name = name, Description = description, Company = company, MainCompany = mainCompany, Protestos = protests.ToList() };
        _db.Resistance.Add(resistance);
        _db.SaveChanges();
        return resistance;
    }

    private Protesto Protest(DateTime start, DateTime? end = null, City? city = null) => new()
    {
        StartDate = start,
        EndDate = end,
        Locations = city == null ? [] : [new ProtestoLocation { City = city, CityId = city.Id }],
    };

    private Task<IReadOnlyList<RS.MVC.Models.ResistanceOption>> Search(SearchCasesQuery query) => new SearchCasesHandler(_db).Handle(query);

    [Fact]
    public void TheQueryTranslatesToSqlServer()
    {
        // No database needed: EF builds the SQL and fails here if any part cannot be translated.
        using var sqlServer = new RSDBContext(new DbContextOptionsBuilder<RSDBContext>().UseSqlServer("Server=none;Database=none").Options);

        var sql = new SearchCasesHandler(sqlServer)
            .Build(new SearchCasesQuery("tekstil", CompanyId: 1, CityId: 16, LastProtestSince: new DateTime(2026, 7, 1)))
            .ToQueryString();

        Assert.Contains("ORDER BY", sql);
    }

    [Fact]
    public async Task ACompanyListsAllItsCasesIncludingAsMainCompany()
    {
        var main = new Company { Name = "Örnek Holding" };
        var own = AddCase("Holding direnişi", main, null, null, Protest(new DateTime(2026, 9, 1)));
        var outsourced = AddCase("Taşeron direnişi", new Company { Name = "Örnek Temizlik" }, main, null, Protest(new DateTime(2026, 8, 1)));
        AddCase("Başka direniş", new Company { Name = "Başka A.Ş." }, null, null, Protest(new DateTime(2026, 10, 1)));

        var found = await Search(new SearchCasesQuery("", CompanyId: main.Id));

        Assert.Equal([own.Id, outsourced.Id], found.Select(c => c.Id));
    }

    [Fact]
    public async Task OngoingCasesComeFirstThenTheMostRecent()
    {
        AddCase("Bitmiş, yeni", null, null, null, Protest(new DateTime(2026, 10, 1), new DateTime(2026, 10, 2)));
        AddCase("Süren, eski", null, null, null, Protest(new DateTime(2026, 6, 1)));
        AddCase("Bitmiş, eski", null, null, null, Protest(new DateTime(2026, 5, 1), new DateTime(2026, 5, 3)));

        var found = await Search(new SearchCasesQuery(""));

        Assert.Equal(["Süren, eski", "Bitmiş, yeni", "Bitmiş, eski"], found.Select(c => c.Name));
        Assert.True(found[0].Ongoing);
    }

    [Fact]
    public async Task TextAlsoMatchesTheDescription()
    {
        AddCase("Liman direnişi", null, null, "Vardiya düzenine karşı imza kampanyası", Protest(new DateTime(2026, 9, 1)));
        AddCase("Başka", null, null, "Ücret alacağı", Protest(new DateTime(2026, 9, 1)));

        // Case-sensitivity is the database's collation; the in-memory test database compares exactly.
        var found = await Search(new SearchCasesQuery("Vardiya"));

        Assert.Equal(["Liman direnişi"], found.Select(c => c.Name));
    }

    [Fact]
    public async Task CityAndPeriodNarrowTheSearchAndCasesShowCitiesAndCounts()
    {
        AddCase("Bursa, yeni", null, null, null, Protest(new DateTime(2026, 9, 1), city: _bursa), Protest(new DateTime(2026, 8, 1), city: _izmir));
        AddCase("Bursa, eski", null, null, null, Protest(new DateTime(2025, 1, 1), city: _bursa));
        AddCase("İzmir", null, null, null, Protest(new DateTime(2026, 9, 1), city: _izmir));

        var found = await Search(new SearchCasesQuery("", CityId: 16, LastProtestSince: new DateTime(2026, 7, 1)));

        var hit = Assert.Single(found);
        Assert.Equal(("Bursa, yeni", 2), (hit.Name, hit.ProtestCount));
        Assert.Equal(["Bursa", "İzmir"], hit.Cities.Order());
    }
}
