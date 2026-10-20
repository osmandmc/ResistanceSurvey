using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RS.COMMON.Entities;
using RS.EF;
using RS.MVC.Models;

namespace RS.MVC.Features.Articles
{
    /// <summary>
    /// The app's thin News row for a pipeline article, used by dismissing and linking. The row's ArticleId is what
    /// marks the article as handled in the app (D-010).
    /// </summary>
    public class ArticleNewsService(RSDBContext db, IOptions<PipelineSettings> settings)
    {
        private static readonly TimeSpan TurkeyOffset = TimeSpan.FromHours(3);

        public Task<bool> IsHandledAsync(string articleId) =>
            db.News.AnyAsync(n => n.ArticleId == articleId);

        /// <summary>
        /// The row for this article: an older row with the same link (imported the old way, no ArticleId yet) is
        /// reused; otherwise a new one is added (not saved). The caller sets Status and saves.
        /// </summary>
        public async Task<News> FindOrAddAsync(PipelineArticleDetail article)
        {
            var news = await db.News.FirstOrDefaultAsync(n => n.ArticleId == null && n.Link == article.Url);
            if (news == null)
            {
                news = new News
                {
                    Header = article.Title,
                    Link = article.Url,
                    Date = (article.PublishedAt ?? DateTimeOffset.UtcNow).ToOffset(TurkeyOffset).DateTime,
                    Source = settings.Value.Sources.FirstOrDefault(s => s.Value == article.SourceId).Key,
                };
                db.News.Add(news);
            }
            news.ArticleId = article.ArticleId;
            return news;
        }
    }
}
