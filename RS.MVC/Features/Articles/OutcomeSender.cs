using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RS.MVC.Applications;
using RS.MVC.Models;

namespace RS.MVC.Features.Articles
{
    /// <summary>
    /// Tells the pipeline what happened to an article (D-011). The app's database already holds the decision, so a
    /// failure is logged, not thrown: the inbox re-sends missing outcomes when it next lists the article.
    /// </summary>
    public class OutcomeSender(IPipelineClient pipeline, ILogger<OutcomeSender> logger)
    {
        /// <summary>True when the pipeline accepted it.</summary>
        public async Task<bool> TrySendAsync(string articleId, PipelineOutcomeRequest outcome)
        {
            try
            {
                await pipeline.RecordOutcomeAsync(articleId, outcome);
                return true;
            }
            catch (HttpRequestException ex)
            {
                logger.LogWarning(ex, "Could not send {Kind} for {ArticleId} to the pipeline", outcome.Kind, articleId);
                return false;
            }
        }
    }
}
