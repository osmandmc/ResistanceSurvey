using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.COMMON.Entities;
using RS.EF;
using RS.MVC.Features.History;

namespace RS.MVC.Features.Activity
{
    /// <summary>One log line for the screens. At is UTC. Changes: the fields it changed (R39; on the case page only).</summary>
    public record ActivityRow(long Id, DateTime At, string Username, string Action, int? CaseId, string ArticleId, string Subject, int? SubjectId, string Summary,
        IReadOnlyList<FieldChangeRow> Changes = null)
    {
        /// <summary>The action of a Geçmiş line made only of recorded changes (e.g. an import, or a save that wrote no line).</summary>
        public const string ChangesOnly = "history.change";

        public static ActivityRow Of(ActivityEntry e) =>
            new(e.Id, DateTime.SpecifyKind(e.At, DateTimeKind.Utc), e.Username, e.Action, e.CaseId, e.ArticleId, e.Subject, e.SubjectId, e.Summary);
    }

    /// <summary>Area: "inbox", "cases", "definitions", "users", "uploads" or "prompts" (null: all).</summary>
    public record ListActivityQuery(string Username = null, string Area = null, int Page = 1);

    /// <summary>Usernames: everyone who appears in the log, for the person filter.</summary>
    public record ActivityPage(IReadOnlyList<ActivityRow> Items, int Total, int Page, int PageSize, IReadOnlyList<string> Usernames);

    /// <summary>Kayıtlar (yönetici only): the whole log, newest first, by person and area.</summary>
    public class ListActivityHandler(RSDBContext db)
    {
        public const int PageSize = 50;

        private static readonly Dictionary<string, string[]> Areas = new()
        {
            ["inbox"] = ["article."],
            ["cases"] = ["case.", "protest."],
            ["definitions"] = ["definition."],
            ["users"] = ["user."],
            ["uploads"] = ["upload."],
            ["prompts"] = ["prompt."],
        };

        public static bool IsArea(string area) => area == null || Areas.ContainsKey(area);

        public async Task<ActivityPage> Handle(ListActivityQuery query)
        {
            var page = Math.Max(1, query.Page);
            var entries = db.ActivityEntry.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(query.Username))
                entries = entries.Where(e => e.Username == query.Username);
            if (query.Area != null && Areas.TryGetValue(query.Area, out var prefixes))
            {
                entries = prefixes.Length == 1
                    ? entries.Where(e => e.Action.StartsWith(prefixes[0]))
                    : entries.Where(e => e.Action.StartsWith(prefixes[0]) || e.Action.StartsWith(prefixes[1]));
            }

            var total = await entries.CountAsync();
            var items = await entries.OrderByDescending(e => e.At).ThenByDescending(e => e.Id)
                .Skip((page - 1) * PageSize).Take(PageSize).ToListAsync();
            var usernames = await db.ActivityEntry.Where(e => e.Username != null).Select(e => e.Username).Distinct().OrderBy(u => u).ToListAsync();
            return new ActivityPage(items.Select(ActivityRow.Of).ToList(), total, page, PageSize, usernames);
        }
    }

    /// <summary>
    /// The case page's Geçmiş: what was done to the case and its protests and articles, newest first, each line with
    /// the fields it changed (R39). Changes that came without a line (an import, the system) are lines of their own.
    /// </summary>
    public class CaseActivityHandler(RSDBContext db)
    {
        public const int Max = 200;

        public async Task<IReadOnlyList<ActivityRow>> Handle(int caseId)
        {
            var lines = (await db.ActivityEntry.AsNoTracking().Where(e => e.CaseId == caseId)
                .OrderByDescending(e => e.At).ThenByDescending(e => e.Id).Take(Max).ToListAsync())
                .Select(ActivityRow.Of).ToList();
            var lineIds = lines.Select(l => l.Id).ToList();
            var changes = await db.FieldChange.AsNoTracking()
                .Where(c => c.CaseId == caseId || c.ActivityEntryId != null && lineIds.Contains(c.ActivityEntryId.Value))
                .OrderBy(c => c.Id).Take(Max * 20).ToListAsync();
            var texts = (await new ChangeText(db).RowsAsync(changes)).Zip(changes).ToList();

            var byLine = texts.Where(t => t.Second.ActivityEntryId != null).ToLookup(t => t.Second.ActivityEntryId.Value, t => t.First);
            var withChanges = lines.Select(l => byLine.Contains(l.Id) ? l with { Changes = byLine[l.Id].ToList() } : l);
            // Negative ids: these lines are not in the activity log.
            var changesOnly = texts.Where(t => t.Second.ActivityEntryId == null)
                .GroupBy(t => (t.Second.At, t.Second.Username, t.Second.Source))
                .Select(g => new ActivityRow(-g.Min(t => t.Second.Id), DateTime.SpecifyKind(g.Key.At, DateTimeKind.Utc), g.Key.Username,
                    ActivityRow.ChangesOnly, caseId, null, null, null, SourceLabel(g.Key.Source), g.Select(t => t.First).ToList()));
            return withChanges.Concat(changesOnly).OrderByDescending(l => l.At).ThenByDescending(l => Math.Abs(l.Id)).Take(Max).ToList();
        }

        private static string SourceLabel(string source) => source switch
        {
            ChangeSources.Import => "içe aktarma",
            ChangeSources.Migration => "veri taşıma",
            ChangeSources.System => "sistem",
            _ => null,
        };
    }
}
