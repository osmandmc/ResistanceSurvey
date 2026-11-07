using RS.MVC.Features.Cases;
using RS.MVC.Features.Definitions;
using RS.MVC.Features.Review;

namespace RS.DatabaseTests;

/// <summary>
/// Searches follow Turkish letter rules on SQL Server (F9, R41). The InMemory tests cannot show this: there a search
/// compares letters exactly, so “işçi” never finds “İŞÇİ”.
/// </summary>
[Collection(SqlServerCollection.Name)]
public class SearchTests(SqlServerFixture server) : IAsyncLifetime
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

    private async Task<IEnumerable<int>> ListCases(string text)
    {
        await using var db = _database.Context();
        return (await new ListCasesHandler(db).Handle(new ListCasesQuery(new CaseFilter(Text: text)))).Items.Select(c => c.Id);
    }

    [Theory]
    [InlineData("işçi")] // case name and company: İŞÇİLER …, İŞÇİ METAL
    [InlineData("İŞÇİ")]
    [InlineData("metal işçileri")] // the organizing union
    public async Task TheCaseListFindsSmallAndCapitalTurkishLetters(string text) =>
        Assert.Equal([_data.MetalCase.Id, _data.OldCase.Id], await ListCases(text));

    [Fact]
    public async Task DotlessIIsItsOwnLetter()
    {
        // “ışık” is “IŞIK” in capitals; with Latin rules it would be “ISIK”, and “işık” would match too.
        Assert.Equal([_data.IsikCase.Id], await ListCases("ışık"));
        Assert.Empty(await ListCases("işık"));
    }

    [Fact]
    public async Task WithoutTurkishLettersNothingMatches()
    {
        // Known limit (F9): typing “isci” for “işçi” finds nothing; a folded copy would be a later task.
        Assert.Empty(await ListCases("isci"));
    }

    [Fact]
    public async Task ReviewSearchFindsTheCase()
    {
        await using var db = _database.Context();

        var found = await new SearchCasesHandler(db).Handle(new SearchCasesQuery("işçiler"));

        Assert.Equal([_data.MetalCase.Id], found.Select(c => c.Id));
    }

    [Fact]
    public async Task CompanySearchAndTheCompanyListFindTheCompany()
    {
        await using var db = _database.Context();

        var found = await new SearchCompaniesHandler(db).Handle(new SearchCompaniesQuery("işçi metal"));
        var listed = await new ListDefinitionsHandler(db).Handle(new ListDefinitionsQuery(DefinitionKind.Companies, "ışık"));

        Assert.Equal([_data.Metal.Id], found.Select(c => c.Id));
        Assert.Equal([_data.Isik.Id], listed.Items.Select(c => c.Id));
    }
}
