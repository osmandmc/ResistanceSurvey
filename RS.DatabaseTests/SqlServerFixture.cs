using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RS.EF;
using Testcontainers.MsSql;

namespace RS.DatabaseTests;

/// <summary>
/// One SQL Server 2022 container for all tests (R41). The server keeps its default collation
/// (SQL_Latin1_General_CP1_CI_AS, like a new Azure SQL database), so Turkish rules come only from the columns, as in Azure.
/// Each test class builds its own empty database with the EF migrations.
/// </summary>
public sealed class SqlServerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _server = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    public Task InitializeAsync() => _server.StartAsync();

    public Task DisposeAsync() => _server.DisposeAsync().AsTask();

    /// <summary>The connection string of a database on this server (it need not exist yet).</summary>
    public string ConnectionString(string database) =>
        new SqlConnectionStringBuilder(_server.GetConnectionString()) { InitialCatalog = database }.ConnectionString;

    /// <summary>A new, empty database built by the migrations, as a fresh install would be.</summary>
    public async Task<TestDatabase> NewDatabaseAsync()
    {
        var database = new TestDatabase(ConnectionString("t" + Guid.NewGuid().ToString("N")));
        await using var db = database.Context();
        await db.Database.MigrateAsync();
        return database;
    }
}

/// <summary>A database of the test server. Each Context() is new, so reads after a save come from SQL Server, not EF's cache.</summary>
public sealed class TestDatabase(string connectionString)
{
    public string ConnectionString { get; } = connectionString;

    public RSDBContext Context() =>
        new(new DbContextOptionsBuilder<RSDBContext>().UseSqlServer(ConnectionString).Options);
}

[CollectionDefinition(Name)]
public sealed class SqlServerCollection : ICollectionFixture<SqlServerFixture>
{
    public const string Name = "SQL Server";
}
