using Microsoft.EntityFrameworkCore;
using RS.MVC.Features.Analysis;
using RS.MVC.Features.Cases;
using RS.MVC.Features.Definitions;

namespace RS.DatabaseTests;

/// <summary>
/// The main read queries run on SQL Server and give the same answers as on InMemory (R41). InMemory runs any C# it is
/// given; SQL Server fails on a query EF cannot translate, and groups, dates and nulls follow SQL rules.
/// </summary>
[Collection(SqlServerCollection.Name)]
public class QueryTests(SqlServerFixture server) : IAsyncLifetime
{
    private TestDatabase _database = null!;
    private MadeUpData _data = null!;

    public async Task InitializeAsync()
    {
        _database = await server.NewDatabaseAsync();
        await using var db = _database.Context();
        _data = await MadeUpData.AddAsync(db);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task TheCaseListUsesEveryFilter()
    {
        await using var db = _database.Context();
        var handler = new ListCasesHandler(db);

        var all = await handler.Handle(new ListCasesQuery(new CaseFilter()));
        var filtered = await handler.Handle(new ListCasesQuery(new CaseFilter("işçi", _data.Metal.Id, _data.Holding.Id, _data.Wages.Id,
            Year: 2026, Month: 9, CityId: _data.Izmir.Id, HasNote: false, OngoingOnly: true)));

        Assert.Equal([_data.IsikCase.Id, _data.MetalCase.Id, _data.OldCase.Id], all.Items.Select(c => c.Id)); // latest protest first
        Assert.Equal(3, all.Total);
        Assert.Equal([_data.MetalCase.Id], filtered.Items.Select(c => c.Id));
    }

    [Fact]
    public async Task TheExportHasOneRowPerProtest()
    {
        await using var db = _database.Context();

        var protests = await new ExportCasesHandler(db).Protests(new CaseFilter(CompanyId: _data.Metal.Id)).ToListAsync();
        var file = await new ExportCasesHandler(db).Handle(new ExportCasesQuery(new CaseFilter()));

        Assert.Equal(2, protests.Count);
        Assert.NotEmpty(file);
    }

    [Fact]
    public async Task TheAnalysisCountsTheCases()
    {
        await using var db = _database.Context();

        var analysis = await new CaseAnalysisHandler(db).Handle(new CaseAnalysisQuery(new CaseFilter()));

        Assert.Equal((3, 3, 1), (analysis.Totals.Cases, analysis.Totals.Protests, analysis.Totals.Provinces));
        Assert.Equal(["İzmir"], analysis.Provinces.Select(p => p.Name));
    }

    [Fact]
    public async Task ACaseOpensWithItsCompanies()
    {
        await using var db = _database.Context();

        var detail = await new GetCaseHandler(db).Handle(new GetCaseQuery(_data.MetalCase.Id));

        Assert.Equal(("İŞÇİ METAL SANAYİ", "ÖRNEK HOLDİNG"), (detail!.CompanyName, detail.MainCompanyName));
    }

    [Theory]
    [InlineData(DefinitionKind.Companies)]
    [InlineData(DefinitionKind.Corporations)]
    [InlineData(DefinitionKind.Reasons)]
    [InlineData(DefinitionKind.ProtestTypes)]
    [InlineData(DefinitionKind.ProtestPlaces)]
    [InlineData(DefinitionKind.Workplaces)]
    public async Task EveryDefinitionListAndItsDuplicatesLoad(DefinitionKind kind)
    {
        await using var db = _database.Context();

        var page = await new ListDefinitionsHandler(db).Handle(new ListDefinitionsQuery(kind));
        await new FindDuplicatesHandler(db).Handle(kind);

        if (kind == DefinitionKind.Companies)
            Assert.Equal([_data.Metal.Id, _data.Holding.Id, _data.Isik.Id], page.Items.Select(i => i.Id).Order());
    }

    [Fact]
    public async Task UnusedCompaniesAreFound()
    {
        await using var db = _database.Context();
        db.Company.Add(new RS.COMMON.Entities.Company { Name = "Kullanılmayan A.Ş." });
        await db.SaveChangesAsync();

        var unused = await new ListDefinitionsHandler(db).Handle(new ListDefinitionsQuery(DefinitionKind.Companies, UnusedOnly: true));

        Assert.Equal(["Kullanılmayan A.Ş."], unused.Items.Select(i => i.Name));
    }
}
