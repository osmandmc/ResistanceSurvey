using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RS.MVC.Features.Review;
using RS.MVC.Models;

namespace RS.MVC.Controllers
{
    /// <summary>The new app's review screen (/app/review/{articleId}): an inbox article becomes part of a case.</summary>
    [ApiController]
    [Route("api/review")]
    [Authorize]
    public class ReviewController(
        GetReviewArticleHandler getArticle,
        SearchCasesHandler searchCases,
        CreateCaseFromArticleHandler createCase,
        AddProtestFromArticleHandler addProtest,
        LinkArticleToCaseHandler linkToCase,
        GetCaseFormOptionsHandler getFormOptions,
        SearchCompaniesHandler searchCompanies,
        CreateCompanyHandler createCompany,
        ILogger<ReviewController> logger) : ControllerBase
    {
        public record CreateCompanyRequest(string Name, int? TypeId, int? ScaleId, int? WorklineId);

        /// <summary>GET /api/review/form-options — every list the case form needs.</summary>
        [HttpGet("form-options")]
        public async Task<CaseFormOptions> FormOptions() => await getFormOptions.Handle(new GetCaseFormOptionsQuery());

        /// <summary>GET /api/review/companies?query=… — companies by name (at most 20).</summary>
        [HttpGet("companies")]
        public async Task<IReadOnlyList<Option>> SearchCompanies(string query) =>
            await searchCompanies.Handle(new SearchCompaniesQuery(query));

        /// <summary>POST /api/review/companies — "Şirket Ekle"; an existing company with the same name is returned instead.</summary>
        [HttpPost("companies")]
        public async Task<ActionResult<CreateCompanyResult>> CreateCompany(CreateCompanyRequest request)
        {
            var result = await createCompany.Handle(new CreateCompanyCommand(request.Name, request.TypeId, request.ScaleId, request.WorklineId));
            return result.Error == null
                ? result
                : BadRequest(new { error = result.Error, fields = new Dictionary<string, string> { ["name"] = result.Error } });
        }

        public record SaveResponse(int ResistanceId, bool OutcomeSent);

        /// <summary>GET /api/review/resistances?query=… — existing cases to add a protest to or link to.</summary>
        [HttpGet("resistances")]
        public async Task<IReadOnlyList<ResistanceOption>> SearchResistances(string query) =>
            await searchCases.Handle(new SearchCasesQuery(query));

        /// <summary>GET /api/review/{articleId} — the article with its full text.</summary>
        [HttpGet("{articleId}")]
        public async Task<ActionResult<ReviewArticle>> Get(string articleId)
        {
            try
            {
                var result = await getArticle.Handle(new GetReviewArticleQuery(articleId));
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
                var userName = User.Identity?.Name;
                var result = request?.Action switch
                {
                    ReviewAction.NewCase => await createCase.Handle(new CreateCaseFromArticleCommand(articleId, request.Case, request.Protest, userName)),
                    ReviewAction.AddProtest => await addProtest.Handle(new AddProtestFromArticleCommand(articleId, request.ResistanceId, request.Protest, userName)),
                    ReviewAction.LinkOnly => await linkToCase.Handle(new LinkArticleToCaseCommand(articleId, request.ResistanceId)),
                    _ => ReviewSaveResult.Invalid(new Dictionary<string, string> { ["action"] = "Lütfen bir işlem seçiniz." }),
                };
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
