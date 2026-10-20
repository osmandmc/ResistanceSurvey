using Microsoft.EntityFrameworkCore;
using RS.COMMON.Entities;
using RS.COMMON.Entities.LookupEntity;
using RS.EF;
using RS.MVC.Features.Review;

namespace RS.Tests;

public class FormOptionsAndCompanyTests
{
    private readonly RSDBContext _db = new(new DbContextOptionsBuilder<RSDBContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task FormOptionsMarkUnionsAndKeepDistrictsWithTheirCity()
    {
        _db.Corporation.AddRange(new Corporation { Name = "Birleşik Metal-İş", CorporationTypeId = GetCaseFormOptionsHandler.TradeUnionCorporationTypeId },
            new Corporation { Name = "Bir Dernek", CorporationTypeId = 2 });
        _db.City.Add(new City { Id = 16, Name = "Bursa" });
        _db.District.Add(new District { Id = 160, Name = "Nilüfer", CityId = 16 });
        await _db.SaveChangesAsync();

        var options = await new GetCaseFormOptionsHandler(_db).Handle(new GetCaseFormOptionsQuery());

        Assert.Equal([("Bir Dernek", false), ("Birleşik Metal-İş", true)], options.Corporations.Select(c => (c.Name, c.IsTradeUnion)));
        Assert.Equal(new DistrictOption(160, "Nilüfer", 16), Assert.Single(options.Districts));
    }

    [Fact]
    public async Task TheCasesUnionIsChosenFromCorporationsUnionsFirst()
    {
        // The separate TradeUnion table must not be offered: the case's union points to a corporation.
        _db.TradeUnion.Add(new TradeUnion { Id = 30, Name = "HAK-İş Öz Gıda-İş" });
        _db.Corporation.AddRange(
            new Corporation { Id = 30, Name = "TTB", CorporationTypeId = 3 },
            new Corporation { Id = 14, Name = "DİSK Genel-İş", CorporationTypeId = GetCaseFormOptionsHandler.TradeUnionCorporationTypeId },
            new Corporation { Id = 58, Name = "Bilinmiyor", CorporationTypeId = GetCaseFormOptionsHandler.TradeUnionCorporationTypeId });
        await _db.SaveChangesAsync();

        var options = await new GetCaseFormOptionsHandler(_db).Handle(new GetCaseFormOptionsQuery());

        Assert.Equal([new Option(58, "Bilinmiyor"), new Option(14, "DİSK Genel-İş"), new Option(30, "TTB")], options.TradeUnions);
    }

    [Fact]
    public async Task CompanySearchLeavesOutDeletedCompanies()
    {
        _db.Company.AddRange(new Company { Name = "Örnek Tekstil" }, new Company { Name = "Eski Tekstil", Deleted = true }, new Company { Name = "Liman A.Ş." });
        await _db.SaveChangesAsync();

        var found = await new SearchCompaniesHandler(_db).Handle(new SearchCompaniesQuery("Tekstil"));

        Assert.Equal(["Örnek Tekstil"], found.Select(c => c.Name));
    }

    [Fact]
    public async Task CreatingACompanyThatExistsReturnsTheExistingOne()
    {
        _db.Company.Add(new Company { Name = "Örnek Tekstil A.Ş." });
        await _db.SaveChangesAsync();
        var handler = new CreateCompanyHandler(_db);

        var same = await handler.Handle(new CreateCompanyCommand(" örnek  tekstil a.ş.", null, null, null));
        var fresh = await handler.Handle(new CreateCompanyCommand("Yeni Lojistik", 1, 2, 3));
        var blank = await handler.Handle(new CreateCompanyCommand("   ", null, null, null));

        Assert.True(same.Existed);
        Assert.Equal("Örnek Tekstil A.Ş.", same.Company.Name);
        Assert.False(fresh.Existed);
        var created = _db.Company.Single(c => c.Id == fresh.Company.Id);
        Assert.Equal(("Yeni Lojistik", 1, 2, 3), (created.Name, created.CompanyTypeId, created.CompanyScaleId, created.CompanyWorkLineId));
        Assert.NotNull(blank.Error);
        Assert.Equal(2, _db.Company.Count());
    }
}
