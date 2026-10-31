using System.Threading.Tasks;
using RS.MVC.Applications;
using RS.MVC.Features.Articles;
using RS.MVC.Models;

namespace RS.MVC.Features.Review
{
    /// <summary>The article shown beside the form, with its full text from the pipeline.</summary>
    public record GetReviewArticleQuery(string ArticleId);

    public record ReviewArticleResult(ReviewStatus Status, ReviewArticle Article = null);

    public class GetReviewArticleHandler(IPipelineClient pipeline, ArticleNewsService articleNews)
    {
        public async Task<ReviewArticleResult> Handle(GetReviewArticleQuery query)
        {
            if (await articleNews.IsHandledAsync(query.ArticleId))
                return new ReviewArticleResult(ReviewStatus.AlreadyHandled);

            var article = await pipeline.GetArticleAsync(query.ArticleId);
            return article == null
                ? new ReviewArticleResult(ReviewStatus.NotFound)
                : new ReviewArticleResult(ReviewStatus.Ok, new ReviewArticle(
                    article.ArticleId, article.Title, article.Url, SourceCatalog.Name(article.SourceId), article.PublishedAt, article.Body,
                    AiRelevance.From(article.Stages), article.ImageUrl));
        }
    }
}
