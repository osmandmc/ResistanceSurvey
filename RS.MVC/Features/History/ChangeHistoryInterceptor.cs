using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RS.COMMON.Entities;
using RS.COMMON.Entities.LookupEntity;
using RS.EF;

namespace RS.MVC.Features.History
{
    /// <summary>How a change was made (FieldChange.Source).</summary>
    public static class ChangeSources
    {
        public const string Person = "person";
        public const string Import = "import";
        public const string Migration = "migration";
        public const string System = "system";
    }

    /// <summary>
    /// The change history's central part (R39), run on every save: each changed field of a case, protest, company,
    /// workplace or organization becomes a FieldChange row with its old and new value, in the same save. A new record's
    /// first values are not written (the activity log says it was created; its values are the ones before its first
    /// change). Lists that a save replaces (remove all, add the new ones) only record what really changed. The rows go
    /// with the request's activity line, whether it is written in the same save or in another save of the request.
    /// </summary>
    public class ChangeHistoryInterceptor(IHttpContextAccessor http) : SaveChangesInterceptor
    {
        private const string LineKey = "history:line";
        private const string WaitingKey = "history:waiting";

        private static readonly HashSet<string> NotRecorded =
            ["Id", nameof(AuditEntity.CreateDate), nameof(AuditEntity.UpdateDate), nameof(AuditEntity.Creator), nameof(AuditEntity.Updater)];

        /// <summary>The records whose fields are recorded, by name; Corporation includes unions and confederations.</summary>
        private static readonly Dictionary<Type, string> Records = new()
        {
            [typeof(Resistance)] = "Resistance",
            [typeof(Protesto)] = "Protesto",
            [typeof(Company)] = "Company",
            [typeof(Workplace)] = "Workplace",
            [typeof(Corporation)] = "Corporation",
        };

        /// <summary>A list on a case or protest: its link table, the parent's key and the value a link row stands for.</summary>
        private record ListOf(Type Parent, string ParentKey, string Field, Func<Func<string, object>, string> Value);

        private static readonly Dictionary<Type, ListOf> Lists = new()
        {
            [typeof(ResistanceResistanceReason)] = new(typeof(Resistance), "ResistanceId", "Reasons", v => Text(v("ResistanceReasonId"))),
            [typeof(ResistanceCorporation)] = new(typeof(Resistance), "ResistanceId", "Organizations", v => Text(v("CorporationId"))),
            [typeof(ResistanceEmploymentType)] = new(typeof(Resistance), "ResistanceId", "EmploymentTypes", v => Text(v("EmploymentTypeId"))),
            [typeof(ResistanceNews)] = new(typeof(Resistance), "ResistanceId", "Articles", v => Text(v("NewsId"))),
            [typeof(ProtestoProtestoType)] = new(typeof(Protesto), "ProtestoId", "ProtestTypes", v => Text(v("ProtestoTypeId"))),
            [typeof(ProtestoProtestoPlace)] = new(typeof(Protesto), "ProtestoId", "Places", v => Text(v("ProtestoPlaceId"))),
            [typeof(ProtestoInterventionType)] = new(typeof(Protesto), "ProtestoId", "Interventions", v => Text(v("InterventionTypeId"))),
            // A location: "city/district/place", e.g. "16//fabrika önü" (no district).
            [typeof(ProtestoLocation)] = new(typeof(Protesto), "ProtestoId", "Locations",
                v => $"{Text(v("CityId"))}/{Text(v("DistrictId"))}/{Text(v("Place"))}"),
        };

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            RecordAsync(eventData.Context, sync: true, default).AsTask().GetAwaiter().GetResult();
            return result;
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
            InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            await RecordAsync(eventData.Context, sync: false, cancellationToken);
            return result;
        }

        private async ValueTask RecordAsync(DbContext context, bool sync, CancellationToken cancellationToken)
        {
            if (context is not RSDBContext db)
                return;
            db.ChangeTracker.DetectChanges();
            var entries = db.ChangeTracker.Entries().ToList();
            var changes = entries.SelectMany(FieldsOf).ToList();
            changes.AddRange(await ListChangesAsync(db, entries, sync, cancellationToken));

            var request = http.HttpContext;
            var username = request?.User?.Identity?.Name;
            var at = DateTime.UtcNow;
            foreach (var change in changes)
            {
                change.At = at;
                change.Username = username;
                change.Source = db.ChangeSource ?? (username != null ? ChangeSources.Person : ChangeSources.System);
            }
            db.FieldChange.AddRange(changes);
            LinkToActivityLine(db, entries, changes, request);
        }

        /// <summary>
        /// Puts the changes with the activity line of this save, or of an earlier save in the request; without one yet,
        /// they wait for a later save of the request to add it (a protest's fields are saved before its line is).
        /// </summary>
        private static void LinkToActivityLine(RSDBContext db, List<EntityEntry> entries, List<FieldChange> changes, HttpContext request)
        {
            var line = entries.Where(e => e.State == EntityState.Added).Select(e => e.Entity).OfType<ActivityEntry>().FirstOrDefault()
                ?? request?.Items[LineKey] as ActivityEntry;
            if (line != null && db.Entry(line).State == EntityState.Detached)
                line = null; // from another context
            var waiting = request?.Items[WaitingKey] as List<FieldChange> ?? [];
            if (line == null)
            {
                if (request != null && changes.Count > 0)
                    request.Items[WaitingKey] = waiting.Concat(changes).ToList();
                return;
            }
            foreach (var change in waiting.Where(c => db.Entry(c).State != EntityState.Detached).Concat(changes))
                db.Entry(change).Reference(c => c.ActivityEntry).CurrentValue = line;
            if (request != null)
            {
                request.Items[LineKey] = line;
                request.Items.Remove(WaitingKey);
            }
        }

        /// <summary>The changed fields of a recorded record: changed values, or every value of a removed record.</summary>
        private static IEnumerable<FieldChange> FieldsOf(EntityEntry entry)
        {
            if (entry.State is not (EntityState.Modified or EntityState.Deleted) || RecordName(entry.Entity.GetType()) is not { } name)
                yield break;
            var recordId = (int)entry.Property("Id").OriginalValue!;
            int? caseId = entry.Entity switch
            {
                Resistance => recordId,
                Protesto => (int)entry.Property(nameof(Protesto.ResistanceId)).OriginalValue!,
                _ => null,
            };
            foreach (var property in entry.Properties.Where(p => !NotRecorded.Contains(p.Metadata.Name)))
            {
                var old = Text(property.OriginalValue);
                var now = entry.State == EntityState.Deleted ? null : Text(property.CurrentValue);
                if (old == now || entry.State == EntityState.Modified && !property.IsModified)
                    continue;
                yield return new FieldChange { Entity = name, RecordId = recordId, CaseId = caseId, Field = property.Metadata.Name, OldValue = old, NewValue = now };
            }
        }

        private static string RecordName(Type type)
        {
            for (var t = type; t != null; t = t.BaseType)
            {
                if (Records.TryGetValue(t, out var name))
                    return name;
            }
            return null;
        }

        /// <summary>
        /// Values added to and removed from the lists of existing cases and protests. A list that was removed and added
        /// again with the same values records nothing; a new case's or protest's lists are not written.
        /// </summary>
        private static async Task<List<FieldChange>> ListChangesAsync(RSDBContext db, List<EntityEntry> entries, bool sync, CancellationToken cancellationToken)
        {
            var newParents = entries.Where(e => e.State == EntityState.Added && e.Entity is Resistance or Protesto)
                .Select(e => (e.Entity.GetType(), (int)e.Property("Id").CurrentValue!))
                .ToHashSet();
            var links = entries
                .Where(e => e.State is EntityState.Added or EntityState.Deleted && Lists.ContainsKey(e.Entity.GetType()))
                .Select(e =>
                {
                    var list = Lists[e.Entity.GetType()];
                    Func<string, object> value = e.State == EntityState.Deleted ? p => e.Property(p).OriginalValue : p => e.Property(p).CurrentValue;
                    return (List: list, ParentId: (int)value(list.ParentKey)!, Value: list.Value(value), Added: e.State == EntityState.Added);
                })
                .Where(l => !newParents.Contains((l.List.Parent, l.ParentId)))
                .ToList();

            var changes = new List<FieldChange>();
            foreach (var group in links.GroupBy(l => (l.List, l.ParentId)))
            {
                var removed = group.Where(l => !l.Added).Select(l => l.Value).ToHashSet();
                var added = group.Where(l => l.Added).Select(l => l.Value).ToHashSet();
                if (removed.SetEquals(added))
                    continue;
                var (list, parentId) = group.Key;
                var caseId = list.Parent == typeof(Resistance) ? parentId : await CaseOfProtestAsync(db, entries, parentId, sync, cancellationToken);
                var entity = list.Parent == typeof(Resistance) ? "Resistance" : "Protesto";
                changes.AddRange(removed.Except(added).Order().Select(v =>
                    new FieldChange { Entity = entity, RecordId = parentId, CaseId = caseId, Field = list.Field, OldValue = v }));
                changes.AddRange(added.Except(removed).Order().Select(v =>
                    new FieldChange { Entity = entity, RecordId = parentId, CaseId = caseId, Field = list.Field, NewValue = v }));
            }
            return changes;
        }

        private static async Task<int?> CaseOfProtestAsync(RSDBContext db, List<EntityEntry> entries, int protestId, bool sync, CancellationToken cancellationToken)
        {
            if (entries.Select(e => e.Entity).OfType<Protesto>().FirstOrDefault(p => p.Id == protestId) is { } tracked)
                return tracked.ResistanceId;
            var query = db.Protesto.AsNoTracking().Where(p => p.Id == protestId).Select(p => (int?)p.ResistanceId);
            return sync ? query.FirstOrDefault() : await query.FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>A value as stored: text as it is (empty counts as none), dates as yyyy-MM-dd, enums by name.</summary>
        public static string Text(object value) => value switch
        {
            null => null,
            string s => s.Length == 0 ? null : s,
            DateTime d => d.ToString(d.TimeOfDay == TimeSpan.Zero ? "yyyy-MM-dd" : "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            bool b => b ? "true" : "false",
            Enum e => e.ToString(),
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString(),
        };
    }
}
