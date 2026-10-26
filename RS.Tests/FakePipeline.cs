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

    public List<string> QueuedForAi { get; } = [];

    public Task<(PipelineQueueResponse Assess, PipelineQueueResponse Draft)> QueueForAiAsync(string sourceId, bool again = false)
    {
        QueuedForAi.Add(again ? sourceId + " again" : sourceId);
        return Task.FromResult((new PipelineQueueResponse(3, 9, false, true), new PipelineQueueResponse(1, 10, false, true)));
    }

    public PipelinePrompts Prompts { get; set; } = new([], 0);

    public List<IReadOnlyDictionary<string, IReadOnlyList<string>>> SentLists { get; } = [];

    public Task<PipelineListsResponse> PutListsAsync(IReadOnlyDictionary<string, IReadOnlyList<string>> lists)
    {
        SentLists.Add(lists);
        return Task.FromResult(new PipelineListsResponse(DateTimeOffset.UtcNow, true, lists.ToDictionary(l => l.Key, l => l.Value.Count)));
    }

    public Task<PipelinePrompts> GetPromptsAsync() => Task.FromResult(Prompts);

    public List<(string Name, string Text, string Note, string By)> CreatedVersions { get; } = [];

    public Task<(PipelinePromptVersion Version, string Error)> CreatePromptVersionAsync(string name, string text, string note, string by)
    {
        CreatedVersions.Add((name, text, note, by));
        return Task.FromResult(text.Contains("{{TEXT}}")
            ? (new PipelinePromptVersion($"{name}-e{CreatedVersions.Count}", text, note, by, DateTimeOffset.UtcNow, null, false), (string)null!)
            : ((PipelinePromptVersion)null!, "The text must keep these placeholders: {{TEXT}}"));
    }

    /// <summary>What a try answers, per article id; the ids asked are kept in Tried.</summary>
    public Func<string, PipelinePromptTryResult> TryAnswer { get; set; } = id => new(id, id, null!, null!, "NoText", null!, null!, null!);
    public List<IReadOnlyList<string>> Tried { get; } = [];

    public Task<PipelinePromptTry> TryPromptVersionAsync(string name, string version, IReadOnlyList<string> articleIds)
    {
        Tried.Add(articleIds);
        return Task.FromResult(new PipelinePromptTry(version, "gemini", articleIds.Select(TryAnswer).ToList()));
    }

    public List<(string Name, string Version, string By)> Activated { get; } = [];

    public Task<bool> ActivatePromptAsync(string name, string version, string by)
    {
        Activated.Add((name, version, by));
        return Task.FromResult(true);
    }

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
