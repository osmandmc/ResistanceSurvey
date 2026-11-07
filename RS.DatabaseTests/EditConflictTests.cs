using Microsoft.EntityFrameworkCore;

namespace RS.DatabaseTests;

/// <summary>
/// Edit conflicts on SQL Server (IVersioned): two researchers with the same case open, each on their own connection.
/// The handlers compare versions first; this is the moment between that check and the UPDATE.
/// </summary>
[Collection(SqlServerCollection.Name)]
public class EditConflictTests(SqlServerFixture server) : IAsyncLifetime
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
    public async Task TheSecondOfTwoSavesFromTheSameVersionFails()
    {
        await using var ayse = _database.Context();
        await using var mehmet = _database.Context();
        var hers = await ayse.Resistance.SingleAsync(r => r.Id == _data.MetalCase.Id);
        var his = await mehmet.Resistance.SingleAsync(r => r.Id == _data.MetalCase.Id);

        hers.Name = "Ayşe'nin adı";
        await ayse.SaveChangesAsync();
        his.Name = "Mehmet'in adı";

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => mehmet.SaveChangesAsync());
        await using var check = _database.Context();
        var saved = await check.Resistance.SingleAsync(r => r.Id == _data.MetalCase.Id);
        Assert.Equal(("Ayşe'nin adı", 1), (saved.Name, saved.Version));
    }

    [Fact]
    public async Task TheVersionAFormLoadedIsWhatTheSaveChecks()
    {
        await using var db = _database.Context();
        var r = await db.Resistance.SingleAsync(x => x.Id == _data.MetalCase.Id);
        r.Name = "Önce kaydedilen";
        await db.SaveChangesAsync(); // version 1

        db.ExpectVersion(r, 0); // a form loaded before that save
        r.Name = "Eski formdan";

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task TheStartDateTheAppKeepsDoesNotChangeTheVersion()
    {
        await using (var db = _database.Context())
        {
            var r = await db.Resistance.SingleAsync(x => x.Id == _data.MetalCase.Id);
            r.StartDate = new DateTime(2026, 8, 1);
            await db.SaveChangesAsync();
        }

        await using var check = _database.Context();
        Assert.Equal(0, (await check.Resistance.SingleAsync(x => x.Id == _data.MetalCase.Id)).Version);
    }
}
