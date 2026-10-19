using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RS.MVC.Applications;
using RS.MVC.Models;

namespace RS.MVC.Controllers
{
    /// <summary>The new app's review screen (/app/review/{articleId}): an inbox article becomes part of a case.</summary>
    [ApiController]
    [Route("api/review")]
    [Authorize]
    public class ReviewController(IReviewApplication review, ILogger<ReviewController> logger) : ControllerBase
    {
        public record SaveResponse(int ResistanceId, bool OutcomeSent);

        /// <summary>GET /api/review/resistances?query=… — existing cases to add a protest to or link to.</summary>
        [HttpGet("resistances")]
        public async Task<IReadOnlyList<ResistanceOption>> SearchResistances(string query) =>
            await review.SearchResistancesAsync(query);

        /// <summary>GET /api/review/{articleId} — the article with its full text.</summary>
        [HttpGet("{articleId}")]
        public async Task<ActionResult<ReviewArticle>> Get(string articleId)
        {
            try
            {
                var result = await review.GetArticleAsync(articleId);
                return result.Status switch
                {
                    ReviewStatus.Ok => result.Article,
                    ReviewStatus.AlreadyHandled => Conflict(new { error = "Bu haber zaten işlendi." }),
                    _ => NotFound(new { error = "Haber bulunamadı." }),
                };
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return PipelineUnavailable(ex);
            }
        }

        /// <summary>POST /api/review/{articleId} — new case, new protest on a case, or link only. 400 lists missing fields.</summary>
        [HttpPost("{articleId}")]
        public async Task<ActionResult<SaveResponse>> Save(string articleId, ReviewSaveRequest request)
        {
            try
            {
                var result = await review.SaveAsync(articleId, request, User.Identity?.Name);
                return result.Status switch
                {
                    ReviewStatus.Ok => new SaveResponse(result.ResistanceId!.Value, result.OutcomeSent),
                    ReviewStatus.Invalid => BadRequest(new { error = "Lütfen eksik alanları doldurun.", fields = result.Errors }),
                    ReviewStatus.AlreadyHandled => Conflict(new { error = "Bu haber zaten işlendi." }),
                    _ => NotFound(new { error = "Haber bulunamadı." }),
                };
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return PipelineUnavailable(ex);
            }
        }

        private ObjectResult PipelineUnavailable(Exception ex)
        {
            var cause = ex switch
            {
                HttpRequestException { StatusCode: { } status } => $"HTTP {(int)status}",
                TaskCanceledException => "zaman aşımı",
                _ => "bağlantı hatası",
            };
            logger.LogWarning(ex, "Pipeline unavailable for {Path}: {Cause}", Request.Path, cause);
            return StatusCode(StatusCodes.Status502BadGateway, new { error = $"Haber işleme hattına ulaşılamadı ({cause})." });
        }
    }
}
