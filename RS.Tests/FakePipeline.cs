using RS.MVC.Applications;
using RS.MVC.Models;

namespace RS.Tests;

/// <summary>Stands in for the news pipeline API; records what the app sends.</summary>
internal sealed class FakePipeline : IPipelineClient
{
    private readonly List<PipelineArticleDetail> _articles = [];

    public List<PipelineArticleQuery> Queries { get; } = [];
    public List<(string ArticleId, PipelineOutcomeRequest Outcome)> Outcomes { get; } = [];
    public bool FailOutcomes { get; set; }

    public void Add(string id, string url, string title, DateTimeOffset publishedAt, string body = "Metin") =>
        _articles.Add(new PipelineArticleDetail(id, "evrensel", url, title, publishedAt, body));

    public List<PipelineArticle> Submitted { get; } = [];

    /// <summary>Urls the pipeline already knows (answered as Duplicate); urls without "http" are Rejected.</summary>
    public HashSet<string> KnownUrls { get; } = [];

    public Task<IReadOnlyList<PipelineSubmitResult>> SubmitAsync(IReadOnlyList<PipelineArticle> articles, string origin)
    {
        Submitted.AddRange(articles);
        IReadOnlyList<PipelineSubmitResult> results = articles.Select(a =>
            !a.Url.StartsWith("http") ? new PipelineSubmitResult(a.Url, "Rejected", null!, "Not an absolute http(s) URL")
            : KnownUrls.Contains(a.Url) ? new PipelineSubmitResult(a.Url, "Duplicate", "x", null!)
            : new PipelineSubmitResult(a.Url, "Accepted", "x", null!)).ToList();
        return Task.FromResult(results);
    }

    /// <summary>Stages and outcome per article, as the pipeline's list returns them.</summary>
    public Dictionary<string, (IReadOnlyDictionary<string, PipelineStageResult> Stages, PipelineOutcome Outcome)> States { get; } = [];

    public Task<PipelineArticleList> ListArticlesAsync(PipelineArticleQuery query)
    {
        Queries.Add(query);
        var items = _articles.Select(a => States.TryGetValue(a.ArticleId, out var state)
            ? new PipelineArticleSummary(a.ArticleId, a.SourceId, a.Url, a.Title, a.PublishedAt, state.Stages, state.Outcome)
            : new PipelineArticleSummary(a.ArticleId, a.SourceId, a.Url, a.Title, a.PublishedAt)).ToList();
        return Task.FromResult(new PipelineArticleList(items, null!));
    }

    public Task<PipelineArticleDetail> GetArticleAsync(string articleId) =>
        Task.FromResult(_articles.FirstOrDefault(a => a.ArticleId == articleId)!);

    public PipelineMetrics Metrics { get; set; } = null!;
    public List<int> MetricsRequests { get; } = [];

    public Task<PipelineMetrics> GetMetricsAsync(int days)
    {
        MetricsRequests.Add(days);
        return Task.FromResult(Metrics);
    }

    public PipelineCollectResponse Collected { get; set; } = new([]);

    public Task<PipelineCollectResponse> CollectFeedsAsync() => Task.FromResult(Collected);

    /// <summary>AI drafts per article (pipeline B12).</summary>
    public Dictionary<string, PipelineDraft> Drafts { get; } = [];

    public Task<PipelineDraft> GetDraftAsync(string articleId) => Task.FromResult(Drafts.GetValueOrDefault(articleId)!);

    public Task RecordOutcomeAsync(string articleId, PipelineOutcomeRequest outcome)
    {
        if (FailOutcomes)
            throw new HttpRequestException("pipeline down");
        Outcomes.Add((articleId, outcome));
        return Task.CompletedTask;
    }
}
