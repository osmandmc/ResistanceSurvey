using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RS.MVC.Features.Cases;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using RS.COMMON.DTO;
using RS.COMMON.Entities;
using RS.EF;
using RS.MVC.Controllers;
using RS.MVC.Models;

namespace ResistanceSurvey.Controllers
{
    public class ResistanceController(RSDBContext db) : BaseController
    {
        private readonly RSDBContext _db = db;

        public IActionResult Index()
        {
            return RedirectToAction(nameof(IndexVue));
        }

        public IActionResult IndexVue()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public IActionResult News(int? year, int? month)
        {
            var query = _db.News.AsQueryable();

            if (year.HasValue)
            {
                query = query.Where(n => n.Date.Year == year.Value);
            }

            if (month.HasValue && month.Value > 0)
            {
                query = query.Where(n => n.Date.Month == month.Value);
            }

            var news = query
                .OrderByDescending(n => n.Date)
                .Select(n => new
                {
                    n.Id,
                    n.Header,
                    n.Content,
                    n.Date,
                    n.Link,
                    n.Status,
                    n.Source,
                    Added = false
                })
                .ToList();

            return Json(news);
        }

        /// <summary>The old list's "Dışa Aktar"; the export itself lives in ExportCasesHandler (shared with /app/cases).</summary>
        [Authorize]
        public async Task<ActionResult> Export(ResistanceFilterModel filter, [FromServices] ExportCasesHandler exportCases)
        {
            var file = await exportCases.Handle(new ExportCasesQuery(new CaseFilter(
                CompanyId: filter.CompanyId,
                MainCompanyId: filter.MainCompanyId,
                CategoryId: filter.CategoryId,
                Year: filter.YearId,
                Month: filter.MonthId is 0 ? null : filter.MonthId,
                HasNote: filter.PersonalNote)));
            return File(file, ExportCasesHandler.ContentType, "Report.xlsx");
        }
    }
}
