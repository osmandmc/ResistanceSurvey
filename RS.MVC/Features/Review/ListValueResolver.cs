using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.COMMON.Entities.LookupEntity;
using RS.EF;

namespace RS.MVC.Features.Review
{
    /// <summary>
    /// Lists where researchers may type a new value (Vaka Nedeni, Kurumsallık): a typed name that matches an
    /// existing entry uses that entry; otherwise the entry is created. Call inside the case's transaction, so a
    /// failed save leaves no new entries behind.
    /// </summary>
    public class ListValueResolver(RSDBContext db)
    {
        public Task<List<int>> ReasonIdsAsync(IEnumerable<int> ids, IEnumerable<string> newNames) =>
            ResolveAsync(db.ResistanceReason, ids, newNames, name => new ResistanceReason { Name = name });

        /// <summary>New corporations get only a name, as in the old form; admins review them on the Kurumlar page.</summary>
        public Task<List<int>> CorporationIdsAsync(IEnumerable<int> ids, IEnumerable<string> newNames) =>
            ResolveAsync(db.Corporation, ids, newNames, name => new Corporation { Name = name });

        private async Task<List<int>> ResolveAsync<T>(DbSet<T> set, IEnumerable<int> ids, IEnumerable<string> newNames, Func<string, T> create)
            where T : LookupEntity
        {
            var result = (ids ?? []).ToList();
            var names = (newNames ?? []).Select(NameMatching.Clean).Where(n => n != "").ToList();
            if (names.Count == 0)
                return result.Distinct().ToList();

            var existing = await set.Select(e => new { e.Id, e.Name }).ToListAsync();
            var byKey = existing
                .GroupBy(e => NameMatching.Key(e.Name))
                .ToDictionary(g => g.Key, g => g.First().Id);

            var created = new Dictionary<string, T>();
            foreach (var name in names)
            {
                var key = NameMatching.Key(name);
                if (byKey.TryGetValue(key, out var id))
                    result.Add(id);
                else if (!created.ContainsKey(key))
                    set.Add(created[key] = create(name));
            }
            if (created.Count > 0)
            {
                await db.SaveChangesAsync();
                result.AddRange(created.Values.Select(e => e.Id));
            }
            return result.Distinct().ToList();
        }
    }
}
