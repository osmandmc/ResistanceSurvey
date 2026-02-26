using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
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

        public ActionResult Export(ResistanceFilterModel filter)
        {
            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var protestos = _db.Protesto
                .Include(item => item.Resistance)
                .ThenInclude(resistance => resistance.EmployeeCount)
                .Include("Resistance.Category")
                .Include("Resistance.Company.CompanyWorkLine")
                .Include("Resistance.MainCompany.CompanyWorkLine")
                .Include("Resistance.ResistanceEmploymentTypes")
                .Include("Resistance.ResistanceEmploymentTypes.EmploymentType")
                .Include("Resistance.TradeUnion")
                .Include("Resistance.TradeUnionAuthority")
                .Include("Resistance.ResistanceResistanceReasons")
                .Include("Resistance.ResistanceResistanceReasons.ResistanceReason")
                .Include("Resistance.ResistanceCorporations")
                .Include("Resistance.ResistanceCorporations.Corporation")
                .Include("Resistance.ResistanceCorporations.Corporation.CorporationType")
                .Include("Gender")
                .Include("ProtestoEmployeeCount")
                .Include("ProtestoProtestoTypes")
                .Include("ProtestoProtestoTypes.ProtestoType")
                .Include("ProtestoProtestoPlaces")
                .Include("ProtestoProtestoPlaces.ProtestoPlace")
                .Include("ProtestoInterventionTypes")
                .Include("ProtestoInterventionTypes.InterventionType")
                .Include("Locations")
                .Include("Locations.City")
                .Include("Locations.District")
                .Include("Resistance.ResistanceNews")
                .Include("Resistance.ResistanceNews.News")
                .Where(rs => !rs.Deleted && !rs.Resistance.Deleted
                            && (filter.CompanyId == null || rs.Resistance.CompanyId == filter.CompanyId)
                            && (filter.MainCompanyId == null || rs.Resistance.MainCompanyId == filter.MainCompanyId)
                            && (filter.CategoryId == null || rs.Resistance.CategoryId == filter.CategoryId)
                            && (filter.YearId == null || (rs.StartDate.Year == filter.YearId && (filter.MonthId == null || filter.MonthId == 0 || rs.StartDate.Month == filter.MonthId)))
                            && (filter.PersonalNote == null || String.IsNullOrEmpty(rs.Resistance.Note) == !filter.PersonalNote)
                            )
                .ToList();

            var rows = new List<Dictionary<string, object>>();
            foreach (var item in protestos)
            {
                var row = new Dictionary<string, object>
                {
                    { "Direnişin ID", item.Resistance.Id },
                    { "Açıklama", item.Resistance.Description ?? "" },
                    { "Kategori Adı", item.Resistance.Category?.Name ?? "" },
                    { "Şirket Adı", item.Resistance.Company?.Name ?? "" },
                    { "Şirket İş Kolu", ((Company)item.Resistance.Company)?.CompanyWorkLine?.Name ?? "" },
                    { "Ana Şirket Adı", item.Resistance.MainCompany?.Name ?? "" },
                    { "Ana Şirket İş Kolu", item.Resistance.MainCompany?.CompanyWorkLine?.Name ?? "" },
                    { "Üretim Karşıtı", item.ProtestoProtestoTypes.Any(s => s.ProtestoType.AgainstProduction) ? "EVET" : "HAYIR" },
                    { "Kalkınma Hakkı", item.Resistance.DevelopRight ? "EVET" : "HAYIR" },
                    { "Çalışan Sayısı", item.Resistance.EmployeeCount?.Name ?? "" },
                    { "Çalışan Sayısı (Sayısal)", item.Resistance.EmployeeCountNumber },
                    { "Sendika Var mı", item.Resistance.ResistanceCorporations.Any(s => s.Corporation.CorporationTypeId == 1) ? "EVET" : "HAYIR" },
                    { "Sendika Otoritesi", item.Resistance.TradeUnionAuthority?.Name ?? "" },
                    { "Direniş Nedenleri", string.Join("; ", item.Resistance.ResistanceResistanceReasons.Select(x => x.ResistanceReason.Name)) },
                    { "Kuruluşlar", string.Join("; ", item.Resistance.ResistanceCorporations?.Select(x => x.Corporation.Name) ?? new List<string>()) },
                    { "İstihdam Türleri", string.Join("; ", item.Resistance.ResistanceEmploymentTypes?.Select(x => x.EmploymentType.Name) ?? new List<string>()) },
                    { "Tepki Gösteren Sendika", item.Resistance.TradeUnion?.Name ?? "" },
                    { "Cinsiyet", item.Gender?.Name ?? "" },
                    { "İşten Çıkarılan Çalışan Sayısı", item.Resistance.FiredEmployeeCountByProtesto },
                    { "Hukuki Müdahale", item.Resistance.AnyLegalIntervention.HasValue ? (item.Resistance.AnyLegalIntervention.Value ? "EVET" : "HAYIR") : "BİLİNMİYOR" },
                    { "Hukuki Müdahale Açıklaması", item.Resistance.LegalInterventionDesc ?? "" },
                    { "Protesto ID", item.Id },
                    { "Protesto Sayısı", item.Locations.Count },
                    { "Protesto Türleri", string.Join("; ", item.ProtestoProtestoTypes?.Select(x => x.ProtestoType.Name) ?? new List<string>()) },
                    { "Protesto Yerleri", string.Join("; ", item.ProtestoProtestoPlaces?.Select(x => x.ProtestoPlace.Name) ?? new List<string>()) },
                    { "Başlangıç Tarihi", item.StartDate.ToShortDateString() },
                    { "Bitiş Tarihi", item.EndDate?.ToShortDateString() ?? "" },
                    { "Grev Süresi", item.StrikeDuration },
                    { "Konumlar", string.Join("; ", item.Locations.Select(x => x.District != null ? $"{x.City.Name} / {x.District.Name} {x.Place}" : $"{x.City.Name} {x.Place}")) },
                    { "Protestoda Çalışan Sayısı", item.ProtestoEmployeeCount?.Name ?? "" },
                    { "Protestoda Çalışan Sayısı (Sayısal)", item.EmployeeCountNumber },
                    { "Müdahale Türleri", string.Join("; ", item.ProtestoInterventionTypes?.Select(x => x.InterventionType.Name) ?? new List<string>()) },
                    { "Gözaltı Sayısı", item.CustodyCount },
                    { "Not", item.Resistance.Note ?? "" },
                    { "Protesto Notu", item.Note ?? "" },
                    { "Direniş Sonucu", item.Resistance.ResistanceResult.ToString() },
                    { "Direniş Haberleri", string.Join("; ", item.Resistance.ResistanceNews?.Select(x => x.News?.Header ?? "") ?? new List<string>()) },
                    { "Direniş Haberleri Linki", string.Join("; ", item.Resistance.ResistanceNews?.Select(x => x.News?.Link ?? "") ?? new List<string>()) },
                };
                rows.Add(row);
            }

            // Create Excel file
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Report");
                
                if (rows.Count > 0)
                {
                    // Add headers
                    var headers = rows[0].Keys.ToList();
                    for (int i = 0; i < headers.Count; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = headers[i];
                        // Style header
                        worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                        worksheet.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    // Add data rows
                    for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
                    {
                        var row = rows[rowIndex];
                        var values = row.Values.ToList();
                        for (int colIndex = 0; colIndex < values.Count; colIndex++)
                        {
                            worksheet.Cells[rowIndex + 2, colIndex + 1].Value = values[colIndex];
                        }
                    }

                    // Auto-fit columns
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                }

                // Return the file
                var fileBytes = package.GetAsByteArray();
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Report.xlsx");
            }
        }
    }
}
