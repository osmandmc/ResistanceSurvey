using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RS.MVC.Features.Analysis;
using RS.MVC.Features.Cases;

namespace RS.MVC.Controllers
{
    /// <summary>The Analiz screen (R38): the status charts for the cases the case list's filters select.</summary>
    [ApiController]
    [Route("api/analysis")]
    [Authorize]
    public class AnalysisController(CaseAnalysisHandler analysis) : ControllerBase
    {
        /// <summary>GET /api/analysis?categoryId=1&amp;year=2024&amp;cityId=34 — the case list's filters, every one optional.</summary>
        [HttpGet]
        public async Task<CaseAnalysis> Get(
            string text, int? companyId, int? mainCompanyId, int? categoryId, int? year, int? month,
            int? cityId, bool? hasNote, bool ongoingOnly = false) =>
            await analysis.Handle(new CaseAnalysisQuery(
                new CaseFilter(text, companyId, mainCompanyId, categoryId, year, month, cityId, hasNote, ongoingOnly)));
    }
}
