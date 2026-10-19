using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using RS.EF;

public class RSDBContextFactory : IDesignTimeDbContextFactory<RSDBContext>
{
    public RSDBContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../RS.MVC");

        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.Development.json", optional: true)
            // Same user-secrets store as RS.MVC (its UserSecretsId), then environment variables.
            .AddUserSecrets("122e3cf6-514c-4268-ba49-23503409d1d5")
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<RSDBContext>();
        var connectionString = config.GetConnectionString("RSConnectionString");

        optionsBuilder.UseSqlServer(connectionString);

        return new RSDBContext(optionsBuilder.Options);
    }
}