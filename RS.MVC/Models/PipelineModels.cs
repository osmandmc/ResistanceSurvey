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

    /// <summary>Filters for GET /api/articles. From/To are whole days (Turkey time); Status: parsed|failed|all; Outcome: open|handled|any.</summary>
    public record PipelineArticleQuery(
        DateOnly? From = null,
        DateOnly? To = null,
        string SourceId = null,
        string Status = "parsed",
        string Outcome = "open",
        int PageSize = 50,
        string Next = null);

    /// <summary>A page of GET /api/articles; pass Next back to load more (null = no more).</summary>
    public record PipelineArticleList(IReadOnlyList<PipelineArticleSummary> Items, string Next);

    public record PipelineArticleSummary(string ArticleId, string SourceId, string Url, string Title, DateTimeOffset? PublishedAt);

    /// <summary>GET /api/articles/{id}: one article with its latest text (Body is null until parsed).</summary>
    public record PipelineArticleDetail(string ArticleId, string SourceId, string Url, string Title, DateTimeOffset? PublishedAt, string Body);

    /// <summary>Body of POST /api/articles/{id}/outcome. Kind: "Digested" (with ResistanceId) or "Dismissed".</summary>
    public record PipelineOutcomeRequest(string Kind, int? ResistanceId = null, string Reason = null);

    public static class PipelineOutcomeKind
    {
        public const string Digested = "Digested";
        public const string Dismissed = "Dismissed";
    }
}
