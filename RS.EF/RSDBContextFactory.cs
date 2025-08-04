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
            .AddJsonFile("appsettings.Development.json") // or Production
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<RSDBContext>();
        var connectionString = config.GetConnectionString("RSConnectionString");

        optionsBuilder.UseSqlServer(connectionString);

        return new RSDBContext(optionsBuilder.Options);
    }
}