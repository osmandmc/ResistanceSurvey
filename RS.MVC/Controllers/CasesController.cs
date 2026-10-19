using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RS.MVC.Features.Cases;
using RS.MVC.Models;

namespace RS.MVC.Controllers
{
    /// <summary>The new app's Vakalar screen (/app/cases).</summary>
    [ApiController]
    [Route("api/cases")]
    [Authorize]
    public class CasesController(
        ListCasesHandler listCases,
        ExportCasesHandler exportCases,
        GetCaseHandler getCase,
        UpdateCaseHandler updateCase,
        SaveProtestHandler saveProtest,
        DeleteProtestHandler deleteProtest,
        DeleteCaseHandler deleteCase,
        SearchNewsHandler searchNews,
        AttachArticleHandler attachArticle,
        DetachArticleHandler detachArticle) : ControllerBase
    {
        public record AttachArticleRequest(int NewsId);

        private string UserName => User.Identity?.Name;

        /// <summary>GET /api/cases/{id} — the case with its protests and articles.</summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CaseDetail>> Get(int id) =>
            await getCase.Handle(new GetCaseQuery(id)) is { } detail ? detail : NotFound(new { error = "Vaka bulunamadı." });

        /// <summary>PUT /api/cases/{id} — the case part; 400 lists missing fields.</summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, CaseInput request) =>
            ToResult(await updateCase.Handle(new UpdateCaseCommand(id, request, UserName)));

        /// <summary>DELETE /api/cases/{id}</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id) =>
            ToResult(await deleteCase.Handle(new DeleteCaseCommand(id, UserName)));

        /// <summary>POST /api/cases/{id}/protests — add a protest.</summary>
        [HttpPost("{id:int}/protests")]
        public async Task<IActionResult> AddProtest(int id, ProtestInput request) =>
            ToResult(await saveProtest.Handle(new SaveProtestCommand(id, null, request, UserName)));

        /// <summary>PUT /api/cases/{id}/protests/{protestId}</summary>
        [HttpPut("{id:int}/protests/{protestId:int}")]
        public async Task<IActionResult> UpdateProtest(int id, int protestId, ProtestInput request) =>
            ToResult(await saveProtest.Handle(new SaveProtestCommand(id, protestId, request, UserName)));

        /// <summary>DELETE /api/cases/{id}/protests/{protestId}</summary>
        [HttpDelete("{id:int}/protests/{protestId:int}")]
        public async Task<IActionResult> DeleteProtest(int id, int protestId) =>
            ToResult(await deleteProtest.Handle(new DeleteProtestCommand(id, protestId, UserName)));

        /// <summary>GET /api/cases/news?query=…&amp;year=2026&amp;month=10 — articles in the app, to attach to a case.</summary>
        [HttpGet("news")]
        public async Task<IReadOnlyList<NewsOption>> SearchNews(string query, int? year, int? month) =>
            await searchNews.Handle(new SearchNewsQuery(query, year, month));

        /// <summary>POST /api/cases/{id}/articles — attach an article ({ newsId }).</summary>
        [HttpPost("{id:int}/articles")]
        public async Task<IActionResult> AttachArticle(int id, AttachArticleRequest request) =>
            ToResult(await attachArticle.Handle(new AttachArticleCommand(id, request.NewsId)));

        /// <summary>DELETE /api/cases/{id}/articles/{newsId}</summary>
        [HttpDelete("{id:int}/articles/{newsId:int}")]
        public async Task<IActionResult> DetachArticle(int id, int newsId) =>
            ToResult(await detachArticle.Handle(new DetachArticleCommand(id, newsId)));

        private IActionResult ToResult(CaseCommandResult result) => result.Status switch
        {
            CaseCommandStatus.Ok => Ok(new { id = result.Id }),
            CaseCommandStatus.Invalid => BadRequest(new { error = "Lütfen eksik alanları doldurun.", fields = result.Errors }),
            _ => NotFound(new { error = "Bulunamadı." }),
        };

        /// <summary>GET /api/cases?text=…&amp;companyId=…&amp;year=2026&amp;month=10&amp;page=1 — every filter is optional.</summary>
        [HttpGet]
        public async Task<CaseListPage> List(
            string text, int? companyId, int? mainCompanyId, int? categoryId, int? year, int? month,
            int? cityId, bool? hasNote, bool ongoingOnly = false, int page = 1, int pageSize = ListCasesHandler.DefaultPageSize) =>
            await listCases.Handle(new ListCasesQuery(
                new CaseFilter(text, companyId, mainCompanyId, categoryId, year, month, cityId, hasNote, ongoingOnly), page, pageSize));

        /// <summary>GET /api/cases/export?… — the same filters; an Excel file with one row per protest.</summary>
        [HttpGet("export")]
        public async Task<IActionResult> Export(
            string text, int? companyId, int? mainCompanyId, int? categoryId, int? year, int? month,
            int? cityId, bool? hasNote, bool ongoingOnly = false)
        {
            var file = await exportCases.Handle(new ExportCasesQuery(
                new CaseFilter(text, companyId, mainCompanyId, categoryId, year, month, cityId, hasNote, ongoingOnly)));
            return File(file, ExportCasesHandler.ContentType, $"Vakalar-{DateTime.Now:yyyy-MM-dd}.xlsx");
        }
    }
}
