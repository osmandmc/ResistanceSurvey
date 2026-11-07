using Microsoft.EntityFrameworkCore;
using RS.COMMON.Entities;
using RS.MVC.Features.Definitions;

namespace RS.DatabaseTests;

/// <summary>What SQL Server enforces and InMemory does not (R41): unique indexes, foreign keys, cascades, transactions.</summary>
[Collection(SqlServerCollection.Name)]
public class ConstraintTests(SqlServerFixture server) : IAsyncLifetime
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
    public async Task AnArticleIdIsUniqueButMayBeEmpty()
    {
        await using var db = _database.Context();
        News Article(string? id) => new() { Header = "Uydurma haber", Link = $"https://example.org/{Guid.NewGuid():N}", ArticleId = id };

        db.News.AddRange(Article(null), Article(null), Article("a1"));
        await db.SaveChangesAsync();

        db.News.Add(Article("a1"));
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task AListValueInUseCannotBeDeleted()
    {
        await using var db = _database.Context();

        db.Category.Remove(await db.Category.SingleAsync(c => c.Id == _data.Wages.Id));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task DeletingACaseTakesItsProtestsAlong()
    {
        await using (var db = _database.Context())
        {
            db.Resistance.Remove(await db.Resistance.SingleAsync(r => r.Id == _data.IsikCase.Id));
            await db.SaveChangesAsync();
        }

        await using var check = _database.Context();
        Assert.False(await check.Protesto.AnyAsync(p => p.ResistanceId == _data.IsikCase.Id));
        Assert.True(await check.Company.AnyAsync(c => c.Id == _data.Isik.Id)); // the company stays
    }

    [Fact]
    public async Task MergingCompaniesMovesCasesAndFactsInOneTransaction()
    {
        int typoId;
        await using (var db = _database.Context())
        {
            var typo = new Company { Name = "Ornek Holding" };
            db.CompanyRelation.Add(new CompanyRelation { Company = await db.Company.SingleAsync(c => c.Id == _data.Isik.Id), RelatedCompany = typo,
                Kind = CompanyRelationKind.Group, CreatedBy = "test", CreatedAt = new DateTime(2026, 11, 6) });
            (await db.Resistance.SingleAsync(r => r.Id == _data.IsikCase.Id)).MainCompany = typo;
            await db.SaveChangesAsync();
            typoId = typo.Id;

            await new MergeDefinitionsHandler(db).Handle(new MergeDefinitionsCommand(DefinitionKind.Companies, typoId, _data.Holding.Id));
        }

        await using var check = _database.Context();
        Assert.Equal(_data.Holding.Id, (await check.Resistance.SingleAsync(r => r.Id == _data.IsikCase.Id)).MainCompanyId);
        Assert.Equal(_data.Holding.Id, (await check.CompanyRelation.SingleAsync(r => r.CompanyId == _data.Isik.Id)).RelatedCompanyId);
        Assert.False(await check.Resistance.AnyAsync(r => r.CompanyId == typoId || r.MainCompanyId == typoId));
    }
}
