using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RS.EF;
using RS.MVC.Applications;
using RS.MVC.Models;

namespace RS.MVC.Features.Review
{
    /// <summary>
    /// Sends the app's list values to the pipeline (R30), so its drafts offer the lists researchers keep in Tanımlar.
    /// Only when they changed since the last send (remembered while the app runs; the pipeline also ignores a repeat).
    /// </summary>
    public class ListsPublisher(RSDBContext db, IPipelineClient pipeline)
    {
        private static readonly object Gate = new();
        private static string _lastSent;

        /// <param name="force">Send even when nothing changed since the last send ("Listeleri şimdi gönder").</param>
        /// <returns>The pipeline's answer, or null when nothing was sent.</returns>
        public async Task<PipelineListsResponse> PublishAsync(bool force = false)
        {
            var lists = (await AiLists.LoadAsync(db)).ToDictionary(l => l.Key, l => l.Values);
            var fingerprint = JsonSerializer.Serialize(lists);
            lock (Gate)
            {
                if (!force && fingerprint == _lastSent)
                    return null;
            }
            var response = await pipeline.PutListsAsync(lists);
            lock (Gate)
            {
                _lastSent = fingerprint;
            }
            return response;
        }

        /// <summary>For tests: forget what was sent.</summary>
        public static void Forget()
        {
            lock (Gate)
            {
                _lastSent = null;
            }
        }
    }

    /// <summary>
    /// Sends the lists at startup and then every <see cref="Interval"/> when they changed (R30). Failures are logged and
    /// retried at the next round; until the first send the pipeline uses its own snapshot.
    /// </summary>
    public class ListsSync(IServiceScopeFactory scopes, ILogger<ListsSync> logger) : BackgroundService
    {
        public static readonly TimeSpan Interval = TimeSpan.FromMinutes(15);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // let the app start first
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = scopes.CreateScope();
                    var sent = await scope.ServiceProvider.GetRequiredService<ListsPublisher>().PublishAsync();
                    if (sent != null)
                        logger.LogInformation("Lists sent to the pipeline: {Changed}, {Lists}", sent.Changed ? "new version" : "unchanged",
                            string.Join(", ", sent.Counts.Select(c => $"{c.Key} {c.Value}")));
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    logger.LogWarning(e, "Could not send the lists to the pipeline; trying again in {Minutes} minutes", Interval.TotalMinutes);
                }
                await Task.Delay(Interval, stoppingToken);
            }
        }
    }
}
