using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.EF;
using RS.MVC.Applications;
using RS.MVC.Features.Articles;
using RS.MVC.Features.Inbox;
using RS.MVC.Models;
using static RS.COMMON.Constants.Enums;

namespace RS.MVC.Features.Archive
{
    /// <summary>Which articles the archive lists.</summary>
    public enum ArchiveFilter
    {
        All,

        /// <summary>Parsed and waiting for a decision: what the inbox shows.</summary>
        Open,

        /// <summary>Linked to a case or dismissed.</summary>
        Handled,

        /// <summary>A pipeline stage failed (download, reading the page …).</summary>
        Failed,
    }

    /// <summary>Where an article stands, the app's decision first (D-010). Sent as its name.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter<ArchiveState>))]
    public enum ArchiveState
    {
        /// <summary>Submitted but not readable yet.</summary>
        Processing,
        Failed,
        Open,
        Linked,
        Dismissed,
    }

    /// <summary>Month: "yyyy-MM" or empty for all months. Search: title text or a link.</summary>
    public record GetArchiveQuery(string Month, string SourceId, string Search, ArchiveFilter Filter, string Next);

    public record ArchiveCase(int Id, string Name);

    /// <summary>
    /// One step of an article's history. Step: a pipeline stage ("Submitted", "Fetched", "Parsed" …) or "Outcome".
    /// Answer: what the stage concluded (the AI's verdict for "Relevance").
    /// </summary>
    public record ArchiveEvent(string Step, string Status, DateTimeOffset At, string Reason, string Answer = null);

    /// <summary>Reason: why it failed or why it was dismissed.</summary>
    public record ArchiveItem(
        string ArticleId,
        string Title,
        string Url,
        string SourceId,
        string SourceName,
        DateTimeOffset? PublishedAt,
        ArchiveState State,
        string Reason,
        IReadOnlyList<ArchiveCase> Cases,
        IReadOnlyList<ArchiveEvent> Timeline);

    public record ArchivePage(IReadOnlyList<ArchiveItem> Items, string Next);

    /// <summary>
    /// All articles the pipeline has, with their history. The pipeline knows the stages; the app knows the decisions
    /// (which case, or dismissed), so the app's record wins when the two disagree.
    /// </summary>
    public class GetArchiveHandler(RSDBContext db, IPipelineClient pipeline)
    {
        public const int PageSize = 50;
        public const int MinSearchLength = 2;

        // The pipeline's stages in order; later ones (relevance, extraction) appear once they exist.
        private static readonly string[] StageOrder = ["Submitted", "Fetched", "Parsed", "Relevance", "Extracted", "Validated"];

        /// <exception cref="FormatException">Month is not "yyyy-MM".</exception>
        /// <exception cref="ArgumentException">Search is shorter than <see cref="MinSearchLength"/>.</exception>
        public async Task<ArchivePage> Handle(GetArchiveQuery query)
        {
            var search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim();
            if (search is { Length: < MinSearchLength })
                throw new ArgumentException($"Search needs at least {MinSearchLength} characters.", nameof(query));

            DateOnly? from = null, to = null;
            if (!string.IsNullOrWhiteSpace(query.Month))
                (from, to) = GetInboxHandler.MonthRange(query.Month);

            var (status, outcome) = query.Filter switch
            {
                ArchiveFilter.Open => ("parsed", "open"),
                ArchiveFilter.Handled => ("all", "handled"),
                ArchiveFilter.Failed => ("failed", "any"),
                _ => ("all", "any"),
            };
            var page = await pipeline.ListArticlesAsync(
                new PipelineArticleQuery(from, to, query.SourceId, status, outcome, PageSize, query.Next, search));

            var ids = page.Items.Select(i => i.ArticleId).ToList();
            var decisions = await db.News
                .Where(n => n.ArticleId != null && ids.Contains(n.ArticleId))
                .Select(n => new { n.Id, n.ArticleId, n.Status })
                .ToListAsync();
            var newsIds = decisions.Select(d => d.Id).ToList();
            var cases = (await db.ResistanceNews
                    .Where(rn => newsIds.Contains(rn.NewsId) && !rn.Resistance.Deleted)
                    .Select(rn => new { rn.NewsId, rn.ResistanceId, rn.Resistance.Name })
                    .ToListAsync())
                .ToLookup(c => c.NewsId, c => new ArchiveCase(c.ResistanceId, c.Name));
            var byArticle = decisions.ToDictionary(d => d.ArticleId);

            var items = page.Items.Select(article =>
            {
                byArticle.TryGetValue(article.ArticleId, out var decision);
                var (state, reason) = StateOf(article, decision?.Status);
                return new ArchiveItem(
                    article.ArticleId, article.Title, article.Url, article.SourceId, SourceCatalog.Name(article.SourceId), article.PublishedAt,
                    state, reason, decision == null ? [] : cases[decision.Id].ToList(), Timeline(article));
            }).ToList();
            return new ArchivePage(items, page.Next);
        }

        private static (ArchiveState State, string Reason) StateOf(PipelineArticleSummary article, Status? appStatus)
        {
            var dismissReason = article.Outcome?.Kind == PipelineOutcomeKind.Dismissed ? article.Outcome.Reason : null;
            if (appStatus == Status.Linked)
                return (ArchiveState.Linked, null);
            if (appStatus == Status.Passive)
                return (ArchiveState.Dismissed, dismissReason);
            switch (article.Outcome?.Kind)
            {
                case PipelineOutcomeKind.Digested:
                    return (ArchiveState.Linked, null);
                case PipelineOutcomeKind.Dismissed:
                    return (ArchiveState.Dismissed, dismissReason);
            }

            var stages = article.Stages ?? new Dictionary<string, PipelineStageResult>();
            var failed = stages.Where(s => s.Value.Status == "Failed").OrderByDescending(s => s.Value.At).Select(s => s.Value).FirstOrDefault();
            if (failed != null)
                return (ArchiveState.Failed, failed.Reason);
            return stages.TryGetValue("Parsed", out var parsed) && parsed.Status == "Succeeded"
                ? (ArchiveState.Open, null)
                : (ArchiveState.Processing, null);
        }

        private static IReadOnlyList<ArchiveEvent> Timeline(PipelineArticleSummary article)
        {
            var events = (article.Stages ?? new Dictionary<string, PipelineStageResult>())
                .OrderBy(s => Array.IndexOf(StageOrder, s.Key) is var i and >= 0 ? i : StageOrder.Length)
                .ThenBy(s => s.Value.At)
                .Select(s => new ArchiveEvent(s.Key, s.Value.Status, s.Value.At, s.Value.Reason, s.Value.Answer))
                .ToList();
            if (article.Outcome is { } outcome)
                events.Add(new ArchiveEvent("Outcome", outcome.Kind, outcome.At, outcome.Reason));
            return events;
        }
    }
}
