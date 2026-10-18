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
}
