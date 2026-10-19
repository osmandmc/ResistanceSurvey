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

    public Task<IReadOnlyList<PipelineSubmitResult>> SubmitAsync(IReadOnlyList<PipelineArticle> articles, string origin) =>
        throw new NotSupportedException();

    public Task<PipelineArticleList> ListArticlesAsync(PipelineArticleQuery query)
    {
        Queries.Add(query);
        var items = _articles.Select(a => new PipelineArticleSummary(a.ArticleId, a.SourceId, a.Url, a.Title, a.PublishedAt)).ToList();
        return Task.FromResult(new PipelineArticleList(items, null!));
    }

    public Task<PipelineArticleDetail> GetArticleAsync(string articleId) =>
        Task.FromResult(_articles.FirstOrDefault(a => a.ArticleId == articleId)!);

    public Task RecordOutcomeAsync(string articleId, PipelineOutcomeRequest outcome)
    {
        if (FailOutcomes)
            throw new HttpRequestException("pipeline down");
        Outcomes.Add((articleId, outcome));
        return Task.CompletedTask;
    }
}
