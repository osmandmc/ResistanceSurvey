using System;
using System.Collections.Generic;

namespace RS.MVC.Models
{
    // The app's own copies of the pipeline API's JSON shapes (camelCase on the wire). The JSON is the contract.

    /// <summary>Body of POST /api/articles. Origin: "Csv", "Collector" or "Manual".</summary>
    public record PipelineSubmitRequest(IReadOnlyList<PipelineArticle> Articles, string Origin);

    /// <summary>One article to submit. With Content the pipeline stores it as already parsed and does not fetch the page.</summary>
    public record PipelineArticle(string Url, string Title, DateTimeOffset? PublishedAt, string Content);

    public record PipelineSubmitResponse(IReadOnlyList<PipelineSubmitResult> Results);

    /// <summary>Status: "Accepted", "Duplicate" or "Rejected" (with Reason).</summary>
    public record PipelineSubmitResult(string Url, string Status, string ArticleId, string Reason);

    /// <summary>
    /// Filters for GET /api/articles. From/To are whole days (Turkey time); Status: parsed|failed|all; Outcome: open|handled|any.
    /// Search matches titles (case and Turkish letters ignored); a link finds that one article whatever the other filters.
    /// Relevance (pipeline B10): "notRelevant" = what the AI judged not relevant; "other" = everything else; null = all.
    /// Draft (pipeline B12): "ready" = an AI draft with a labour action; "none" = no such draft; null = all.
    /// </summary>
    public record PipelineArticleQuery(
        DateOnly? From = null,
        DateOnly? To = null,
        string SourceId = null,
        string Status = "parsed",
        string Outcome = "open",
        int PageSize = 50,
        string Next = null,
        string Search = null,
        string Relevance = null,
        string Draft = null);

    /// <summary>A page of GET /api/articles; pass Next back to load more (null = no more).</summary>
    public record PipelineArticleList(IReadOnlyList<PipelineArticleSummary> Items, string Next);

    /// <summary>Stages: stage name ("Submitted", "Fetched", "Parsed" …) → its result. Outcome: null while open.</summary>
    public record PipelineArticleSummary(
        string ArticleId,
        string SourceId,
        string Url,
        string Title,
        DateTimeOffset? PublishedAt,
        IReadOnlyDictionary<string, PipelineStageResult> Stages = null,
        PipelineOutcome Outcome = null);

    /// <summary>
    /// Status: "Succeeded", "Failed" or "Skipped"; Reason says why it failed, or why the stage concluded what it did.
    /// Answer: what the stage concluded; for "Relevance" the AI's verdict ("Relevant", "NotRelevant", "Uncertain").
    /// </summary>
    public record PipelineStageResult(string Status, DateTimeOffset At, string Version = null, string Reason = null, string Answer = null);

    /// <summary>The researchers' decision as the pipeline last heard it. Kind: "Digested" or "Dismissed".</summary>
    public record PipelineOutcome(string Kind, DateTimeOffset At, int? ResistanceId = null, string Reason = null);

    /// <summary>GET /api/articles/{id}: one article with its latest text (Body is null until parsed).</summary>
    public record PipelineArticleDetail(
        string ArticleId, string SourceId, string Url, string Title, DateTimeOffset? PublishedAt, string Body,
        IReadOnlyDictionary<string, PipelineStageResult> Stages = null);

    /// <summary>
    /// GET /api/articles/{id}/draft (pipeline B12): the model's JSON in the shape of the extraction prompt, checked by the
    /// app like a pasted answer. Version: prompt · model. Relevant false: the model found no labour action.
    /// </summary>
    public record PipelineDraft(string ArticleId, string Version, string Answer, bool Relevant, string Summary, DateTimeOffset CreatedAt);

    /// <summary>GET /api/metrics?days=14: per source, per day (Turkey time) and the latest failures.</summary>
    public record PipelineMetrics(
        DateOnly From,
        DateOnly To,
        IReadOnlyList<PipelineSourceMetrics> Sources,
        IReadOnlyList<PipelineDailyMetrics> Days,
        IReadOnlyList<PipelineFailure> RecentFailures,
        PipelineAiMetrics Ai = null);

    /// <summary>LastReceivedAt: latest article in the period (null = none). Open: waiting in the inbox now. Origin: "Csv", "Collector", "Manual".</summary>
    /// OpenWithDraft, OpenNotRelevant, OpenNotAssessed (pipeline B11): of the open ones, with an AI draft, rejected by the AI, not assessed yet.
    public record PipelineSourceMetrics(
        string SourceId, DateTimeOffset? LastReceivedAt, string LastReceivedOrigin, int Received, int Failed, int Open,
        int OpenWithDraft = 0, int OpenNotRelevant = 0, int OpenNotAssessed = 0, int OpenWithOlderVerdict = 0);

    /// <summary>Assessed…: the AI's verdicts given that day; Drafted: AI drafts made that day (pipeline B11).</summary>
    public record PipelineDailyMetrics(
        DateOnly Day, string SourceId, int Received, int Parsed, int Failed, int Digested, int Dismissed,
        int AssessedRelevant = 0, int AssessedUncertain = 0, int AssessedNotRelevant = 0, int Drafted = 0);

    /// <summary>Enabled: a model key is configured. Agreement: per verdict, how many decided articles were linked or dismissed.</summary>
    public record PipelineAiMetrics(bool Enabled, IReadOnlyList<PipelineVerdictAgreement> Agreement);

    /// <summary>Verdict: "Relevant", "NotRelevant", "Uncertain".</summary>
    public record PipelineVerdictAgreement(string Verdict, int Digested, int Dismissed);

    /// <summary>GET /api/prompts (pipeline B13): the prompts the AI stages run, and open articles judged by an older relevance prompt.</summary>
    public record PipelinePrompts(IReadOnlyList<PipelinePrompt> Prompts, int OpenWithOlderVerdict);

    /// <summary>Name: "relevance" or "extraction". Text: the prompt with its placeholders.</summary>
    public record PipelinePrompt(string Name, string Version, string Model, string Text);

    /// <summary>POST /api/articles/assess and /api/articles/draft: articles queued for the AI, SpacingSeconds apart.</summary>
    public record PipelineQueueResponse(int Queued, int SpacingSeconds, bool More, bool ModelConfigured);

    public record PipelineFailure(string ArticleId, string SourceId, string Url, string Title, string Stage, DateTimeOffset At, string Reason);

    /// <summary>POST /api/collect: one run over the feeds. Error: why a feed could not be read (then the counts are 0).</summary>
    public record PipelineFeedRun(string SourceId, int Found, int Accepted, int Duplicate, int Rejected, string Error);

    public record PipelineCollectResponse(IReadOnlyList<PipelineFeedRun> Feeds);

    /// <summary>Body of POST /api/articles/{id}/outcome. Kind: "Digested" (with ResistanceId) or "Dismissed".</summary>
    public record PipelineOutcomeRequest(string Kind, int? ResistanceId = null, string Reason = null);

    public static class PipelineOutcomeKind
    {
        public const string Digested = "Digested";
        public const string Dismissed = "Dismissed";
    }
}
