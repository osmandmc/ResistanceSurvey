using RS.MVC.Infrastructure;
using RS.MVC.Models;

namespace RS.Tests;

/// <summary>F11: a local run uses local resources only.</summary>
public class LocalDevTests
{
    [Theory]
    [InlineData("Server=localhost,1433;Database=ResistanceSurvey;User Id=sa;Password=x;TrustServerCertificate=True", true)]
    [InlineData("Data Source=127.0.0.1;Initial Catalog=RS;Integrated Security=True", true)]
    [InlineData("Server=(localdb)\\MSSQLLocalDB;Database=RS", true)]
    [InlineData("Server=tcp:sql-rs-test.database.windows.net,1433;Database=rs", false)]
    [InlineData("Data Source=SQL1234.site4now.net;Initial Catalog=db_rs;User Id=u;Password=p", false)]
    [InlineData("", false)]
    [InlineData("not a connection string", false)]
    public void OnlyADatabaseOnThisMachineCountsAsLocal(string connectionString, bool local) =>
        Assert.Equal(local, LocalDatabase.IsLocal(connectionString));

    [Theory]
    [InlineData(null, null, true)]
    [InlineData("http://localhost:7071/", null, true)] // func start: no key
    [InlineData("https://func-x.azurewebsites.net", null, false)]
    [InlineData("https://func-x.azurewebsites.net", "key", true)]
    [InlineData("not a url", "key", false)]
    public void APipelineOnThisMachineNeedsNoKey(string? baseUrl, string? key, bool valid) =>
        Assert.Equal(valid, new PipelineSettings { BaseUrl = baseUrl!, FunctionKey = key! }.IsValid());
}
