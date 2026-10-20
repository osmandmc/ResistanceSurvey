using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RS.EF;

namespace RS.MVC.Applications
{
    /// <summary>
    /// Test environment (Azure) only: applies pending EF migrations in the background after the app has started.
    /// The test database pauses when idle and can take about a minute to wake, and Azure gives an app 120 seconds
    /// to start, so the migration must never block or fail startup. Production migrations stay a manual step.
    /// </summary>
    public class TestDatabaseMigrator(IServiceScopeFactory scopes, ILogger<TestDatabaseMigrator> logger) : BackgroundService
    {
        private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(20);
        private static readonly TimeSpan GiveUpAfter = TimeSpan.FromMinutes(10);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var started = DateTime.UtcNow;
            for (var attempt = 1; !stoppingToken.IsCancellationRequested; attempt++)
            {
                try
                {
                    using var scope = scopes.CreateScope();
                    await scope.ServiceProvider.GetRequiredService<RSDBContext>().Database.MigrateAsync(stoppingToken);
                    logger.LogInformation("Test database migrations applied (attempt {Attempt})", attempt);
                    return;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    if (DateTime.UtcNow - started > GiveUpAfter)
                    {
                        logger.LogError(ex, "Gave up applying test database migrations after {Attempt} attempts", attempt);
                        return;
                    }
                    logger.LogWarning("Test database not ready (attempt {Attempt}): {Message}", attempt, ex.Message);
                    await Task.Delay(RetryDelay, stoppingToken);
                }
            }
        }
    }
}
