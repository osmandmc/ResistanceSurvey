using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RS.MVC.Features.Cases;

namespace RS.MVC.Controllers
{
    /// <summary>The new app's Vakalar screen (/app/cases).</summary>
    [ApiController]
    [Route("api/cases")]
    [Authorize]
    public class CasesController(ListCasesHandler listCases, ExportCasesHandler exportCases) : ControllerBase
    {
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
