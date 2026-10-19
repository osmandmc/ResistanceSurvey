using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RS.COMMON.Entities;
using RS.EF;
using RS.MVC.Models;
using static RS.COMMON.Constants.Enums;

namespace RS.MVC.Applications
{
    public interface IInboxApplication
    {
        /// <summary>Open, parsed articles for the month ("yyyy-MM"); articles already handled in the app are left out.</summary>
        Task<InboxPage> GetPageAsync(string month, string sourceId, string next);

        Task<DismissResult> DismissAsync(string articleId, string reason);
    }

    public record InboxItem(string ArticleId, string Title, string SourceId, string SourceName, DateTimeOffset? PublishedAt, string Url);

    public record InboxPage(IReadOnlyList<InboxItem> Items, string Next);

    public enum DismissResult
    {
        Dismissed,

        /// <summary>Saved in the app, but the pipeline could not be told; the inbox re-sends it later.</summary>
        DismissedOutcomePending,

        /// <summary>Someone already linked or dismissed this article.</summary>
        AlreadyHandled,

        NotFound,
    }

    /// <summary>
    /// The inbox: open articles from the news pipeline. The app's database decides what is handled (D-010):
    /// handling an article creates a thin News row with its ArticleId, then the outcome is sent to the pipeline.
    /// If that call failed, the pipeline still lists the article as open; the inbox hides it and re-sends the outcome.
    /// </summary>
    public class InboxApplication : IInboxApplication
    {
        public const int PageSize = 50;

        private static readonly TimeSpan TurkeyOffset = TimeSpan.FromHours(3);

        private readonly RSDBContext _db;
        private readonly IPipelineClient _pipeline;
        private readonly PipelineSettings _settings;
        private readonly ILogger<InboxApplication> _logger;

        public InboxApplication(RSDBContext db, IPipelineClient pipeline, IOptions<PipelineSettings> settings, ILogger<InboxApplication> logger)
        {
            _db = db;
            _pipeline = pipeline;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<InboxPage> GetPageAsync(string month, string sourceId, string next)
        {
            var (from, to) = MonthRange(month);
            var page = await _pipeline.ListArticlesAsync(new PipelineArticleQuery(from, to, sourceId, PageSize: PageSize, Next: next));

            var ids = page.Items.Select(i => i.ArticleId).ToList();
            var handled = await _db.News
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
                .Select(i => new InboxItem(i.ArticleId, i.Title, i.SourceId, ArticleNews.SourceName(i.SourceId), i.PublishedAt, i.Url))
                .ToList();
            return new InboxPage(items, page.Next);
        }

        public async Task<DismissResult> DismissAsync(string articleId, string reason)
        {
            if (await _db.News.AnyAsync(n => n.ArticleId == articleId))
                return DismissResult.AlreadyHandled;

            var article = await _pipeline.GetArticleAsync(articleId);
            if (article == null)
                return DismissResult.NotFound;

            // An article imported the old way (Excel → local table) has no ArticleId yet: that row is reused.
            var news = await ArticleNews.FindOrAddAsync(_db, article, _settings);
            if (news.Status == Status.Linked)
            {
                await _db.SaveChangesAsync();
                return DismissResult.AlreadyHandled;
            }
            news.Status = Status.Passive;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // A colleague saved it a moment earlier (unique ArticleId); anything else is a real error.
                _db.ChangeTracker.Clear();
                if (await _db.News.AnyAsync(n => n.ArticleId == articleId))
                    return DismissResult.AlreadyHandled;
                throw;
            }

            try
            {
                await _pipeline.RecordOutcomeAsync(articleId, new PipelineOutcomeRequest(PipelineOutcomeKind.Dismissed, Reason: Clean(reason)));
                return DismissResult.Dismissed;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Dismissed {ArticleId} in the app, but the pipeline could not be told", articleId);
                return DismissResult.DismissedOutcomePending;
            }
        }

        // Handled in the app but still open in the pipeline: an earlier outcome call failed. Try again; never fail the page.
        private async Task ResendOutcomeAsync(int newsId, string articleId, Status status)
        {
            PipelineOutcomeRequest outcome;
            if (status == Status.Passive)
            {
                outcome = new PipelineOutcomeRequest(PipelineOutcomeKind.Dismissed);
            }
            else if (status == Status.Linked)
            {
                var resistanceId = await _db.ResistanceNews.Where(rn => rn.NewsId == newsId).Select(rn => (int?)rn.ResistanceId).FirstOrDefaultAsync();
                outcome = new PipelineOutcomeRequest(PipelineOutcomeKind.Digested, resistanceId);
            }
            else
            {
                return;
            }

            try
            {
                await _pipeline.RecordOutcomeAsync(articleId, outcome);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Could not re-send the outcome for {ArticleId}", articleId);
            }
        }

        private static string Clean(string reason) =>
            string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();

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
