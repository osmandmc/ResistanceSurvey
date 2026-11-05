using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RS.COMMON.Entities.LookupEntity;
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
    /// Sends the lists once at startup and then whenever a save changed them (R30, R45). No timer: a query every few
    /// minutes would keep the database from pausing on an always-on host. A failed send is logged and goes out with the
    /// next change, the next startup or "Listeleri şimdi gönder"; until the first send the pipeline uses its own snapshot.
    /// </summary>
    public class ListsSync(IServiceScopeFactory scopes, ILogger<ListsSync> logger) : BackgroundService
    {
        /// <summary>Let the app start first.</summary>
        public TimeSpan StartWait { get; init; } = TimeSpan.FromSeconds(30);

        /// <summary>Saves close together (a merge, values edited one after another) are sent once.</summary>
        public TimeSpan ChangeWait { get; init; } = TimeSpan.FromSeconds(5);

        // Holds at most one "changed": more changes before the send are covered by it.
        private readonly Channel<bool> _changes = Channel.CreateBounded<bool>(new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropWrite });

        /// <summary>A save changed a list value (<see cref="ListsChangeInterceptor"/>).</summary>
        public void Changed() => _changes.Writer.TryWrite(true);

        /// <summary>For tests: a change is waiting to be sent.</summary>
        public bool ChangeWaiting => _changes.Reader.Count > 0;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(StartWait, stoppingToken);
            await SendAsync();
            while (!stoppingToken.IsCancellationRequested)
            {
                await _changes.Reader.ReadAsync(stoppingToken);
                await Task.Delay(ChangeWait, stoppingToken);
                _changes.Reader.TryRead(out _); // a change during the wait goes out with this send
                await SendAsync();
            }
        }

        private async Task SendAsync()
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
                logger.LogWarning(e, "Could not send the lists to the pipeline; they go out with the next change or startup");
            }
        }
    }

    /// <summary>
    /// Tells <see cref="ListsSync"/> when a save added, changed or deleted a value of a list in <see cref="AiLists"/> (R45).
    /// Inside a transaction (e.g. merging definitions) it waits for the commit, so the send reads the new values.
    /// </summary>
    public class ListsChangeInterceptor(IServiceProvider services) : SaveChangesInterceptor, IDbTransactionInterceptor
    {
        private static readonly HashSet<Type> ListTypes =
        [
            typeof(Category), typeof(ResistanceReason), typeof(EmploymentType), typeof(TradeUnionAuthority),
            typeof(ProtestoType), typeof(ProtestoPlace), typeof(InterventionType), typeof(City),
        ];

        // The contexts whose saves changed a list and are not sent yet.
        private readonly ConditionalWeakTable<DbContext, object> _changed = new();

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            Note(eventData.Context);
            return result;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
            InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            Note(eventData.Context);
            return ValueTask.FromResult(result);
        }

        public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            SendUnlessInTransaction(eventData.Context);
            return result;
        }

        public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
            CancellationToken cancellationToken = default)
        {
            SendUnlessInTransaction(eventData.Context);
            return ValueTask.FromResult(result);
        }

        public void TransactionCommitted(DbTransaction transaction, TransactionEndEventData eventData) => Send(eventData.Context);

        public Task TransactionCommittedAsync(DbTransaction transaction, TransactionEndEventData eventData,
            CancellationToken cancellationToken = default)
        {
            Send(eventData.Context);
            return Task.CompletedTask;
        }

        public void TransactionRolledBack(DbTransaction transaction, TransactionEndEventData eventData) => Forget(eventData.Context);

        public Task TransactionRolledBackAsync(DbTransaction transaction, TransactionEndEventData eventData,
            CancellationToken cancellationToken = default)
        {
            Forget(eventData.Context);
            return Task.CompletedTask;
        }

        private void Note(DbContext context)
        {
            if (context != null && context.ChangeTracker.Entries().Any(e =>
                    e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted && ListTypes.Contains(e.Metadata.ClrType)))
                _changed.AddOrUpdate(context, true);
        }

        private void SendUnlessInTransaction(DbContext context)
        {
            if (context != null && context.Database.CurrentTransaction == null)
                Send(context);
        }

        private void Send(DbContext context)
        {
            if (context != null && _changed.Remove(context))
                services.GetService<ListsSync>()?.Changed(); // none when the pipeline is not configured
        }

        private void Forget(DbContext context)
        {
            if (context != null)
                _changed.Remove(context);
        }
    }
}
