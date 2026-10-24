using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RS.MVC.Applications;
using RS.MVC.Features.Articles;

namespace RS.MVC.Features.Dashboard
{
    /// <summary>Assessed…: the AI's verdicts given that day; Drafted: AI drafts made that day.</summary>
    public record StatusDay(DateOnly Day, int Received, int Parsed, int Failed, int Digested, int Dismissed,
        int AssessedRelevant, int AssessedUncertain, int AssessedNotRelevant, int Drafted);

    /// <summary>One source over the period. LastReceivedAt is null when nothing arrived (a silent source). Open: in the inbox now.</summary>
    public record StatusSource(
        string SourceId,
        string SourceName,
        DateTimeOffset? LastReceivedAt,
        string LastReceivedOrigin,
        int Received,
        int Failed,
        int Open,
        int OpenWithDraft,
        int OpenNotRelevant,
        int OpenNotAssessed,
        IReadOnlyList<StatusDay> Days);

    /// <summary>The AI (R22): whether a key is configured, and per verdict how many decided articles were linked or dismissed.</summary>
    public record StatusAi(bool Enabled, IReadOnlyList<StatusAgreement> Agreement);

    public record StatusAgreement(string Verdict, int Digested, int Dismissed);

    public record StatusFailure(string ArticleId, string SourceName, string Url, string Title, string Stage, DateTimeOffset At, string Reason);

    public record StatusPage(DateOnly From, DateOnly To, IReadOnlyList<StatusSource> Sources, IReadOnlyList<StatusFailure> RecentFailures, StatusAi Ai);

    /// <summary>The dashboard (/app/status): the pipeline's counts per source and day, with source names, newest day first.</summary>
    public class GetStatusHandler(IPipelineClient pipeline)
    {
        public const int DefaultDays = 14;
        public const int MaxDays = 31;

        public async Task<StatusPage> Handle(int days)
        {
            var metrics = await pipeline.GetMetricsAsync(Math.Clamp(days, 1, MaxDays));
            var sources = metrics.Sources
                .Select(s => new StatusSource(
                    s.SourceId, SourceCatalog.Name(s.SourceId), s.LastReceivedAt, s.LastReceivedOrigin, s.Received, s.Failed, s.Open,
                    s.OpenWithDraft, s.OpenNotRelevant, s.OpenNotAssessed,
                    metrics.Days
                        .Where(d => d.SourceId == s.SourceId)
                        .OrderByDescending(d => d.Day)
                        .Select(d => new StatusDay(d.Day, d.Received, d.Parsed, d.Failed, d.Digested, d.Dismissed,
                            d.AssessedRelevant, d.AssessedUncertain, d.AssessedNotRelevant, d.Drafted))
                        .ToList()))
                .OrderBy(s => s.SourceName, StringComparer.Create(new System.Globalization.CultureInfo("tr-TR"), ignoreCase: true))
                .ToList();
            var failures = metrics.RecentFailures
                .Select(f => new StatusFailure(f.ArticleId, SourceCatalog.Name(f.SourceId), f.Url, f.Title, f.Stage, f.At, f.Reason))
                .ToList();
            var ai = new StatusAi(metrics.Ai?.Enabled ?? false,
                (metrics.Ai?.Agreement ?? []).Select(a => new StatusAgreement(a.Verdict, a.Digested, a.Dismissed)).ToList());
            return new StatusPage(metrics.From, metrics.To, sources, failures, ai);
        }
    }
}

namespace RS.MVC.Features.Dashboard
{
    /// <summary>What one feed brought in, with the source's name. Error: why it could not be read.</summary>
    public record CollectedFeed(string SourceId, string SourceName, int Found, int Accepted, int Duplicate, int Rejected, string Error);

    /// <summary>"Şimdi topla": the pipeline reads the feeds now instead of waiting for its two-hourly timer.</summary>
    public class CollectFeedsHandler(RS.MVC.Applications.IPipelineClient pipeline)
    {
        public async Task<IReadOnlyList<CollectedFeed>> Handle()
        {
            var result = await pipeline.CollectFeedsAsync();
            return result.Feeds
                .Select(f => new CollectedFeed(f.SourceId, RS.MVC.Features.Articles.SourceCatalog.Name(f.SourceId), f.Found, f.Accepted, f.Duplicate, f.Rejected, f.Error))
                .ToList();
        }
    }
}

namespace RS.MVC.Features.Dashboard
{
    /// <summary>"AI ile işle" for one source: how many articles went to the AI for a verdict and for a draft.</summary>
    public record QueuedForAi(string SourceId, string SourceName, int ToAssess, int ToDraft, bool More, bool Enabled, int SpacingSeconds);

    /// <summary>
    /// "AI ile işle" (R22): the pipeline queues the source's open articles the AI has not handled yet: those without a
    /// verdict for the relevance check, and the relevant ones without a draft for the Extract stage. Returns at once.
    /// </summary>
    public class QueueForAiHandler(RS.MVC.Applications.IPipelineClient pipeline)
    {
        public async Task<QueuedForAi> Handle(string sourceId)
        {
            var (assess, draft) = await pipeline.QueueForAiAsync(sourceId);
            return new QueuedForAi(sourceId, SourceCatalog.Name(sourceId), assess.Queued, draft.Queued, assess.More || draft.More,
                assess.ModelConfigured, Math.Max(assess.SpacingSeconds, draft.SpacingSeconds));
        }
    }
}
