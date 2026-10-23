using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        SuggestCasesForArticleHandler suggestCases,
        GetAiPromptHandler getAiPrompt,
        CheckAiAnswerHandler checkAiAnswer) : ControllerBase
    {
        /// <summary>GET /api/review/{articleId}/ai-prompt — "AI ile doldur": the prompt with the article and the lists.</summary>
        [HttpGet("{articleId}/ai-prompt")]
        public async Task<ActionResult<AiPrompt>> AiPrompt(string articleId)
        {
            var result = await getAiPrompt.Handle(new GetAiPromptQuery(articleId));
            return result.Status switch
            {
                ReviewStatus.Ok => result.Prompt,
                ReviewStatus.Invalid => BadRequest(new { error = result.Error }),
                _ => NotFound(new { error = "Haber bulunamadı." }),
            };
        }

        public record AiCheckRequest(string Answer);

        /// <summary>POST /api/review/ai-check — the pasted AI answer checked against the lists, as form values.</summary>
        [HttpPost("ai-check")]
        public async Task<ActionResult<AiCheckResult>> AiCheck(AiCheckRequest request)
        {
            var result = await checkAiAnswer.Handle(new CheckAiAnswerCommand(request?.Answer));
            return result.Error == null ? result : BadRequest(new { error = result.Error });
        }

        /// <summary>GET /api/review/{articleId}/suggestions — "Olası vakalar": cases of companies named in the article.</summary>
        [HttpGet("{articleId}/suggestions")]
        public async Task<ActionResult<IReadOnlyList<CaseSuggestion>>> Suggestions(string articleId)
        {
            return Ok(await suggestCases.Handle(new SuggestCasesForArticleQuery(articleId)));
        }

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

        /// <summary>
        /// GET /api/review/resistances?query=…&amp;companyId=…&amp;cityId=…&amp;since=2026-07-01 — existing cases to add a protest
        /// to or link to; every parameter is optional.
        /// </summary>
        [HttpGet("resistances")]
        public async Task<IReadOnlyList<ResistanceOption>> SearchResistances(string query, int? companyId, int? cityId, DateTime? since) =>
            await searchCases.Handle(new SearchCasesQuery(query, companyId, cityId, since));

        /// <summary>GET /api/review/{articleId} — the article with its full text.</summary>
        [HttpGet("{articleId}")]
        public async Task<ActionResult<ReviewArticle>> Get(string articleId)
        {
            var result = await getArticle.Handle(new GetReviewArticleQuery(articleId));
            return result.Status switch
            {
                ReviewStatus.Ok => result.Article,
                ReviewStatus.AlreadyHandled => Conflict(new { error = "Bu haber zaten işlendi." }),
                _ => NotFound(new { error = "Haber bulunamadı." }),
            };
        }

        /// <summary>POST /api/review/{articleId} — new case, new protest on a case, or link only. 400 lists missing fields.</summary>
        [HttpPost("{articleId}")]
        public async Task<ActionResult<SaveResponse>> Save(string articleId, ReviewSaveRequest request)
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
    }
}
