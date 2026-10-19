using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.COMMON.Entities;
using RS.EF;
using RS.MVC.Models;

namespace RS.MVC.Applications
{
    /// <summary>
    /// The app's thin News row for a pipeline article, shared by dismissing (inbox) and linking (review).
    /// The row's ArticleId is what marks the article as handled in the app (D-010).
    /// </summary>
    public static class ArticleNews
    {
        private static readonly TimeSpan TurkeyOffset = TimeSpan.FromHours(3);

        // Pipeline source ids → names shown to users.
        private static readonly Dictionary<string, string> SourceNames = new()
        {
            ["evrensel"] = "Evrensel",
            ["kizilbayrak"] = "Kızılbayrak",
            ["sendika"] = "Sendika.org",
            ["birgun"] = "BirGün",
        };

        public static string SourceName(string sourceId) =>
            sourceId != null && SourceNames.TryGetValue(sourceId, out var name) ? name : sourceId;

        /// <summary>
        /// The row for this article: an older row with the same link (imported the old way, no ArticleId yet) is
        /// reused; otherwise a new one is added (not saved). The caller sets Status and saves.
        /// </summary>
        public static async Task<News> FindOrAddAsync(RSDBContext db, PipelineArticleDetail article, PipelineSettings settings)
        {
            var news = await db.News.FirstOrDefaultAsync(n => n.ArticleId == null && n.Link == article.Url);
            if (news == null)
            {
                news = new News
                {
                    Header = article.Title,
                    Link = article.Url,
                    Date = (article.PublishedAt ?? DateTimeOffset.UtcNow).ToOffset(TurkeyOffset).DateTime,
                    Source = settings.Sources.FirstOrDefault(s => s.Value == article.SourceId).Key,
                };
                db.News.Add(news);
            }
            news.ArticleId = article.ArticleId;
            return news;
        }
    }
}
