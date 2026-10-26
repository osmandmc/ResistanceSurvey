using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.COMMON.Entities;
using RS.EF;

namespace RS.MVC.Features.Activity
{
    /// <summary>One log line for the screens. At is UTC.</summary>
    public record ActivityRow(long Id, DateTime At, string Username, string Action, int? CaseId, string ArticleId, string Subject, int? SubjectId, string Summary)
    {
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

    /// <summary>The case page's Geçmiş: what was done to the case and its protests and articles, newest first.</summary>
    public class CaseActivityHandler(RSDBContext db)
    {
        public const int Max = 200;

        public async Task<IReadOnlyList<ActivityRow>> Handle(int caseId) =>
            (await db.ActivityEntry.AsNoTracking().Where(e => e.CaseId == caseId)
                .OrderByDescending(e => e.At).ThenByDescending(e => e.Id).Take(Max).ToListAsync())
            .Select(ActivityRow.Of).ToList();
    }
}
