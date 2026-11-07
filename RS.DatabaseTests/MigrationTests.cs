using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using RS.EF;

namespace RS.DatabaseTests;

/// <summary>The migrations build a working database from nothing, and undo cleanly (R41).</summary>
[Collection(SqlServerCollection.Name)]
public class MigrationTests(SqlServerFixture server)
{
    [Fact]
    public async Task AnEmptyDatabaseGetsEveryMigration()
    {
        var database = await server.NewDatabaseAsync();
        await using var db = database.Context();

        Assert.Empty(await db.Database.GetPendingMigrationsAsync());
        Assert.Equal(db.Database.GetMigrations(), await db.Database.GetAppliedMigrationsAsync());
    }

    // The model says which text columns follow Turkish rules (F9); the database must say the same. Compared on the
    // columns the database really has, so a migration that forgot a collation shows up here.
    [Fact]
    public async Task EveryTextColumnHasTheModelsCollation()
    {
        var database = await server.NewDatabaseAsync();
        await using var db = database.Context();
        var model = db.GetService<IDesignTimeModel>().Model;

        var expected = model.GetEntityTypes()
            .Where(e => e.GetTableName() != null)
            .SelectMany(e => e.GetProperties()
                .Where(p => p.ClrType == typeof(string))
                .Select(p => $"{e.GetTableName()}.{p.GetColumnName()} {p.GetCollation()}"))
            .Distinct()
            .Order()
            .ToList();

        var actual = new List<string>();
        await using var connection = new SqlConnection(database.ConnectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand("""
            SELECT TABLE_NAME + '.' + COLUMN_NAME + ' ' + COLLATION_NAME FROM INFORMATION_SCHEMA.COLUMNS
            WHERE COLLATION_NAME IS NOT NULL AND TABLE_NAME <> '__EFMigrationsHistory'
            """, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            actual.Add(reader.GetString(0));

        // A column without a collation in the model takes the database default. Names are compared ignoring case, as
        // SQL Server does: Baseline kept production's spelling (Resistance.description).
        var defaultCollation = "SQL_Latin1_General_CP1_CI_AS";
        Assert.Equal(
            expected.Select(c => (c.EndsWith(' ') ? c + defaultCollation : c).ToLowerInvariant()).Order(StringComparer.Ordinal),
            actual.Select(c => c.ToLowerInvariant()).Order(StringComparer.Ordinal));
    }

    // SchemaFixes (F12) is not undone by design (a backup is restored instead); every migration after it must be.
    [Fact]
    public async Task EveryMigrationAfterSchemaFixesCanBeUndoneAndRedone()
    {
        const string schemaFixes = "20261020222827_SchemaFixes";
        var database = await server.NewDatabaseAsync();
        await using var db = database.Context();
        var migrator = db.GetService<IMigrator>();

        await migrator.MigrateAsync(schemaFixes);
        Assert.Equal(schemaFixes, (await db.Database.GetAppliedMigrationsAsync()).Last());

        await migrator.MigrateAsync();
        Assert.Empty(await db.Database.GetPendingMigrationsAsync());
    }
}
