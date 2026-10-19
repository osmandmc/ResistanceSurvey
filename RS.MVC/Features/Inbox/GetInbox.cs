using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.EF;
using RS.MVC.Applications;
using RS.MVC.Features.Articles;
using RS.MVC.Models;
using static RS.COMMON.Constants.Enums;

namespace RS.MVC.Features.Inbox
{
    /// <summary>Open, parsed articles for a month ("yyyy-MM"; empty = this month) and optionally one source.</summary>
    public record GetInboxQuery(string Month, string SourceId, string Next);

    public record InboxItem(string ArticleId, string Title, string SourceId, string SourceName, DateTimeOffset? PublishedAt, string Url);

    public record InboxPage(IReadOnlyList<InboxItem> Items, string Next);

    /// <summary>
    /// Lists open articles from the pipeline. Articles already handled in the app (its database decides, D-010) are
    /// left out: the pipeline still lists them only because an outcome call failed, so the outcome is re-sent here.
    /// </summary>
    public class GetInboxHandler(RSDBContext db, IPipelineClient pipeline, OutcomeSender outcomes)
    {
        public const int PageSize = 50;

        private static readonly TimeSpan TurkeyOffset = TimeSpan.FromHours(3);

        /// <exception cref="FormatException">Month is not "yyyy-MM".</exception>
        public async Task<InboxPage> Handle(GetInboxQuery query)
        {
            var (from, to) = MonthRange(query.Month);
            var page = await pipeline.ListArticlesAsync(new PipelineArticleQuery(from, to, query.SourceId, PageSize: PageSize, Next: query.Next));

            var ids = page.Items.Select(i => i.ArticleId).ToList();
            var handled = await db.News
                .Where(n => n.ArticleId != null && ids.Contains(n.ArticleId))
                .Select(n => new { n.Id, n.ArticleId, n.Status })
                .ToListAsync();

            foreach (var news in handled)
            {
                await ResendOutcomeAsync(news.Id, news.ArticleId, news.Status);
            }

            var handledIds = handled.Select(h => h.ArticleId).ToHashSet();
            var items = page.Items
                .Where(i => !handledIds.Contains(i.ArticleId))
                .Select(i => new InboxItem(i.ArticleId, i.Title, i.SourceId, SourceCatalog.Name(i.SourceId), i.PublishedAt, i.Url))
                .ToList();
            return new InboxPage(items, page.Next);
        }

        private async Task ResendOutcomeAsync(int newsId, string articleId, Status status)
        {
            if (status == Status.Passive)
            {
                await outcomes.TrySendAsync(articleId, new PipelineOutcomeRequest(PipelineOutcomeKind.Dismissed));
            }
            else if (status == Status.Linked)
            {
                var resistanceId = await db.ResistanceNews.Where(rn => rn.NewsId == newsId).Select(rn => (int?)rn.ResistanceId).FirstOrDefaultAsync();
                await outcomes.TrySendAsync(articleId, new PipelineOutcomeRequest(PipelineOutcomeKind.Digested, resistanceId));
            }
        }

        /// <summary>"yyyy-MM" → first and last day; empty = the current month in Turkey.</summary>
        public static (DateOnly From, DateOnly To) MonthRange(string month)
        {
            var first = string.IsNullOrWhiteSpace(month)
                ? DateOnly.FromDateTime(DateTimeOffset.UtcNow.ToOffset(TurkeyOffset).DateTime)
                : DateOnly.ParseExact(month, "yyyy-MM", CultureInfo.InvariantCulture);
            first = new DateOnly(first.Year, first.Month, 1);
            return (first, first.AddMonths(1).AddDays(-1));
        }
    }
}
