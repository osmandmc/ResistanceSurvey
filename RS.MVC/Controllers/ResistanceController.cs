using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using ResistanceSurvey.Models;
using RS.COMMON;
using RS.COMMON.DTO;
using RS.COMMON.Entities.LookupEntity;
using RS.EF;
using RS.MVC.Applications;
using RS.MVC.Controllers;
using RS.MVC.Models;

namespace ResistanceSurvey.Controllers
{

    public class ResistanceController : BaseController
    {
        private readonly IResistanceApplication _rsApplication;
        private readonly RSDBContext _db;
        public ResistanceController(IResistanceApplication rsApplication, RSDBContext db)
        {
            _rsApplication = rsApplication;
            _db = db;
        }
        public IActionResult Index()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        public IActionResult ResistanceList(ResistanceFilterModel filter)
        {
            ViewBag.Categories = new SelectList(_db.Category.OrderBy(s => s.Name).Select(s => new LookupEntity { Id = s.Id, Name = s.Name }).ToList(), "Id", "Name");
            ViewBag.Companies = new SelectList(_db.Company.Where(s => !s.Deleted).Select(s => new LookupEntity { Id = s.Id, Name = s.Name }).OrderBy(s => s.Name).ToList(), "Id", "Name");
            var result = _rsApplication.GetPaged(filter);
            result.Filter = filter;
            return PartialView("_ResistanceList", result);
        }
        [Authorize]
        public IActionResult NewsList(int year, int month)
        {
            ViewBag.News = _rsApplication.GetNewsList(year, month);
            return PartialView("_NewsList");
        }

        [Authorize]
        [HttpGet]

        public IActionResult Create()
        {
            var companies = _db.Company.Where(s => !s.Deleted).Select(x => new { Id = x.Id, Name = x.Name }).ToList();
            ViewBag.Companies = new SelectList(companies, "Id", "Name");
            SetLookups();
            var name = HttpContext.User.Identity.Name;
            return PartialView("_Create");
        }
        [Authorize]
        [HttpPost]
        public IActionResult Create(ResistanceForm model)
        {
            if (ModelState.IsValid)
            {
                //model.UserName = UserName;
                _rsApplication.Create(model);
                return Ok();
            }
            var values = ModelState.Values.Where(m => m.Errors.Count > 0).Select(s => s.Errors).ToList();
            return StatusCode(StatusCodes.Status500InternalServerError, values);
        }

        [Authorize]
        public IActionResult AddCompany(bool isMain)
        {
            ViewBag.Worklines = new SelectList(_db.CompanyWorkLine.Select(s => new LookupEntity { Id = s.Id, Name = s.Name }).ToList(), "Id", "Name");
            ViewBag.CompanyTypes = new SelectList(_db.CompanyType.Select(s => new LookupEntity { Id = s.Id, Name = s.Name }).ToList(), "Id", "Name");
            ViewBag.CompanyScales = new SelectList(_db.CompanyScale.Select(s => new LookupEntity { Id = s.Id, Name = s.Name }).ToList(), "Id", "Name");
            return PartialView("_AddCompany", new CompanyCreateViewModel { IsMain = isMain });
        }
        [Authorize]
        [HttpPost]
        public IActionResult AddCompany(CompanyCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result1 = _rsApplication.CreateCompany(model);
                return Ok(result1);
            }
            return Json(ModelState.Values.Where(m => m.Errors.Count > 0).Select(s => s.Errors));
        }

        [Authorize]
        public IActionResult AddProtesto(int id)
        {
            ViewBag.ResistanceId = id;
            ViewBag.ResistanceName = _rsApplication.GetResistanceName(id);
            SetLookups();
            return PartialView("_AddProtesto");
        }

        [Authorize]
        [HttpPost]
        public IActionResult AddProtesto(ProtestoAddModel viewModel)
        {
            if (ModelState.IsValid)
            {
                viewModel.UserName = UserName;
                _rsApplication.AddProtesto(viewModel);
                return Ok();
            }
            var values = ModelState.Values.Where(m => m.Errors.Count > 0).Select(s => s.Errors).ToList();
            return StatusCode(StatusCodes.Status500InternalServerError, values);
        }
        [Authorize]
        public IActionResult EditProtesto(int id)
        {
            var protesto = _rsApplication.GetProtestoDetail(id);
            SetLookups();
            return PartialView("_EditProtesto", protesto);
        }

        [Authorize]
        [HttpPost]
        public IActionResult EditProtesto(ProtestoEditModel viewModel)
        {
            if (ModelState.IsValid)
            {
                viewModel.Updater = UserName;
                _rsApplication.UpdateProtesto(viewModel);
                return Ok();
            }
            var errors = ModelState.Values.Where(m => m.Errors.Count > 1).Select(s => s.Errors).ToList();
            return StatusCode(StatusCodes.Status500InternalServerError, errors);
        }
        [Authorize]
        [HttpPost]
        public IActionResult DeleteProtesto(ProtestoDeleteModel viewModel)
        {
            viewModel.UserName = UserName;
            _rsApplication.DeleteProtesto(viewModel);
            return Ok();
        }
        [Authorize]
        public IActionResult EditResistance(int id)
        {
            var resistance = _rsApplication.GetResistanceDetail(id);
            var companies = _db.Company.Where(s => !s.Deleted).Where(s => !s.Deleted).Select(x => new { Id = x.Id, Name = x.Name }).ToList();
            var outsourceCompanies = (from c in _db.Company
                                      join coc in _db.CompanyOutsourceCompany on c.Id equals coc.OutsourceCompanyId into oc
                                      from o in oc.DefaultIfEmpty()
                                      where o.CompanyId == resistance.CompanyId && !c.Deleted
                                      select new { Id = o.OutsourceCompanyId, Name = o.OutsourceCompany.Name })
                                     .ToList();

            ViewBag.Companies = new SelectList(companies, "Id", "Name");
            ViewBag.OutsourceCompanies = new SelectList(outsourceCompanies, "Id", "Name");
            SetLookups();
            return PartialView("_EditResistance", resistance);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Edit(ResistanceEditForm viewModel)
        {
            if (ModelState.IsValid)
            {
                viewModel.Resistance.UserName = UserName;
                _rsApplication.UpdateResistance(viewModel);
                return Ok();
            }
            var values = ModelState.Values.Where(m => m.Errors.Count > 0).Select(s => s.Errors).ToList();
            return StatusCode(StatusCodes.Status500InternalServerError, values);
        }
        [Authorize]
        [HttpPost]
        public IActionResult DeleteResistance(ResistanceDeleteModel viewModel)
        {
            viewModel.UserName = UserName;
            _rsApplication.DeleteResistance(viewModel);
            return Ok();
        }
        [Authorize]
        public IActionResult ExistingResistance(int companyId, int categoryId)
        {
            return Ok(_rsApplication.ExistingResistance(companyId, categoryId));
        }
        [Authorize]
        public IActionResult GetNewsContent(int newsId)
        {
            return Ok(_rsApplication.GetNewsContent(newsId));
        }
        public void SetLookups()
        {

            ViewBag.Categories = new SelectList(_db.Category.OrderBy(s => s.Name).Select(s => new LookupEntity { Id = s.Id, Name = s.Name }).ToList(), "Id", "Name");
            ViewBag.Worklines = new SelectList(_db.CompanyWorkLine.OrderBy(s => s.Name).Select(s => new LookupEntity { Id = s.Id, Name = s.Name }).ToList(), "Id", "Name");
            ViewBag.EmployeeCount = new SelectList(_db.EmployeeCount.OrderBy(s => s.Name).Select(s => new LookupEntity { Id = s.Id, Name = s.Name }).ToList(), "Id", "Name");
            ViewBag.Corporations = new SelectList(_db.Corporation.OrderBy(s => s.Name).Select(s => new LookupEntity { Id = s.Id, Name = s.Name }).ToList(), "Id", "Name");
            ViewBag.TradeUnions = new SelectList(_db.TradeUnion.OrderBy(s => s.Name).Select(s => new LookupEntity { Id = s.Id, Name = s.Name }).ToList(), "Id", "Name");
            ViewBag.TradeUnionAuthorities = new SelectList(_db.TradeUnionAuthority.OrderBy(s => s.Name).Select(s => new LookupEntity { Id = s.Id, Name = s.Name }).ToList(), "Id", "Name");
            ViewBag.EmploymentTypes = new SelectList(_db.EmploymentType.OrderBy(s => s.Name).Select(s => new LookupEntity { Id = s.Id, Name = s.Name }).ToList(), "Id", "Name");
            ViewBag.EmployeeCountInProtesto = new SelectList(_db.ProtestoEmployeeCount.Select(s => new LookupEntity { Id = s.Id, Name = s.Name }).ToList(), "Id", "Name");
            ViewBag.ProtestoTypes = new MultiSelectList(_db.ProtestoType.OrderBy(s => s.Name).Select(s => new LookupEntity { Id = s.Id, Name = s.Name }).ToList(), "Id", "Name");
            ViewBag.ProtestoPlaces = new MultiSelectList(_db.ProtestoPlace.OrderBy(s => s.Name).Select(s => new LookupEntity { Id = s.Id, Name = s.Name }).ToList(), "Id", "Name");
            ViewBag.ResistanceReasons = new MultiSelectList(_db.ResistanceReason.OrderBy(s => s.Name).Select(s => new LookupEntity { Id = s.Id, Name = s.Name }).ToList(), "Id", "Name");
            ViewBag.Genders = new SelectList(_db.Gender.Select(s => new LookupEntity { Id = s.Id, Name = s.Name }).ToList(), "Id", "Name");
            ViewBag.ProtestoCities = new SelectList(_db.City.OrderBy(s => s.Name).Select(s => new LookupEntity { Id = s.Id, Name = s.Name }).ToList(), "Id", "Name");
            ViewBag.InterventionTypes = new SelectList(_db.InterventionType.OrderBy(s => s.Name).Select(s => new LookupEntity { Id = s.Id, Name = s.Name }).ToList(), "Id", "Name");
            ViewBag.Cities = new SelectList(_db.City.OrderBy(s => s.Name).Select(s => new LookupEntity { Id = s.Id, Name = s.Name }).ToList(), "Id", "Name");
            ViewBag.Districts = new SelectList(_db.District.OrderBy(s => s.Name).Select(s => new LookupEntity { Id = s.Id, Name = s.Name }).ToList(), "Id", "Name");
        }

        [Authorize]
        public JsonResult GetDistricts(int cityId)
        {
            return Json(_db.District.Where(s => s.CityId == cityId).OrderBy(s => s.Name).Select(s => new LookupEntity { Id = s.Id, Name = s.Name }).ToList());
        }
        [Authorize]
        public JsonResult GetOutsourceCompanies(int companyId)
        {
            var outsourceCompanies = (from c in _db.Company
                                      join coc in _db.CompanyOutsourceCompany on c.Id equals coc.OutsourceCompanyId into oc
                                      from o in oc.DefaultIfEmpty()
                                      where o.CompanyId == companyId && !c.Deleted
                                      select new { value = o.OutsourceCompanyId, name = o.OutsourceCompany.Name, text = o.OutsourceCompany.Name })
                                     .ToList();
            return Json(outsourceCompanies);
        }
        [Authorize]
        public ActionResult Export(ResistanceFilterModel filter)
        {
            var protestos = _db.Protesto
                .Include("Resistance")
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
                   && (filter.CategoryId == null || rs.Resistance.CategoryId == filter.CategoryId)
                   && (filter.YearId == null || rs.StartDate.Year == filter.YearId && rs.StartDate.Month == filter.MonthId)
                   && (filter.PersonalNote == null || String.IsNullOrEmpty(rs.Resistance.Note) == !filter.PersonalNote)
                   )
                   .ToList();
            try
            {


                var rows = new List<dynamic>();
                foreach (var item in protestos)
                {
                    var row = new
                    {
                        ResistanceId = item.Resistance.Id,
                        CategoryName = item.Resistance.Category.Name,
                        CompanyName = item.Resistance.Company.Name,
                        CompanyWorkline = item.Resistance.Company.CompanyWorkLine != null ? item.Resistance.Company.CompanyWorkLine.Name : "",
                        MainCompanyName = item.Resistance.MainCompany != null ? item.Resistance.MainCompany.Name : "",
                        MainCompanyWorkline = item.Resistance.MainCompany != null ? item.Resistance.MainCompany.CompanyWorkLine != null ? item.Resistance.MainCompany.CompanyWorkLine.Name : "" : "",
                        DevelopRight = item.Resistance.DevelopRight ? "EVET" : "HAYIR",
                        EmployeeCount = item.Resistance.EmployeeCount != null ? item.Resistance.EmployeeCount.Name : "",
                        EmployeeCountNumber = item.Resistance.EmployeeCountNumber,
                        HasTradeUnion = item.Resistance.HasTradeUnion ? "EVET" : "HAYIR",
                        TradeUnionAuthority = item.Resistance.TradeUnionAuthority != null ? item.Resistance.TradeUnionAuthority.Name : "",
                        ResistanceReasons = new List<string>(),
                        Corporations = new List<string>(),
                        EmploymentTypes = new List<string>(),
                        TradeUnionReacted = item.Resistance.TradeUnion != null ? item.Resistance.TradeUnion.Name : "",
                        Gender = item.Gender != null ? item.Gender.Name : "",
                        FiredEmployeeCount = item.Resistance.FiredEmployeeCountByProtesto,
                        AnyLegalIntervention = item.Resistance.AnyLegalIntervention.HasValue ? item.Resistance.AnyLegalIntervention.HasValue ?  "EVET": "HAYIR": "BÝLÝNMÝYOR",
                        LegalInterventionDesc = item.Resistance.LegalInterventionDesc,
                        ProtestoId = item.Id,
                        ProtestoTypes = new List<string>(),
                        StartDate = item.StartDate.ToShortDateString(),
                        EndDate = item.EndDate != null ? item.EndDate.Value.ToShortDateString() : "",
                        Locations = new List<string>(),
                        EmployeeCountInProtesto = item.ProtestoEmployeeCount != null ? item.ProtestoEmployeeCount.Name : "",
                        EmployeeCountInProtestoNumber = item.EmployeeCountNumber,
                        InterventionTypes = new List<string>(),
                        CustodyCount = item.CustodyCount,
                        Note = item.Resistance.Note,
                    };
                    item.Resistance.ResistanceResistanceReasons.ForEach(x => row.ResistanceReasons.Add(x.ResistanceReason.Name));

                    if (item.Resistance.ResistanceCorporations != null) item.Resistance.ResistanceCorporations.ForEach(x => row.Corporations.Add(x.Corporation.Name));
                    if (item.Resistance.ResistanceEmploymentTypes != null) item.Resistance.ResistanceEmploymentTypes.ForEach(x => row.EmploymentTypes.Add(x.EmploymentType.Name));
                    if (item.ProtestoProtestoTypes != null) item.ProtestoProtestoTypes.ToList().ForEach(x => row.ProtestoTypes.Add(x.ProtestoType.Name));
                    if (item.ProtestoInterventionTypes != null) item.ProtestoInterventionTypes.ForEach(x => row.InterventionTypes.Add(x.InterventionType.Name));
                    item.Locations.ForEach(x =>
                    {
                        if (x.District != null)
                            row.Locations.Add($"{x.City.Name} / {x.District.Name} {x.Place}");
                        else
                            row.Locations.Add($"{x.City.Name} {x.Place}");
                    });
                    rows.Add(row);
                }
                var resistanceReasonCount = rows.Max(s => s.ResistanceReasons.Count);
                var corporationCount = rows.Max(s => s.Corporations.Count);
                var employmentTypeCount = rows.Max(s => s.EmploymentTypes.Count);
                var protestoTypeCount = rows.Max(s => s.ProtestoTypes.Count);
                var locationCount = rows.Max(s => s.Locations.Count);
                var interventionTypeCount = rows.Max(s => s.InterventionTypes.Count);
                byte[] byteArray;
                using (ExcelPackage pck = new ExcelPackage())
                {
                    int index = 1;
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Rapor");
                    ws.Row(1).Style.Font.Bold = true;
                    ws.Cells[1, index].Value = "Vaka"; index++;
                    ws.Cells[1, index].Value = "Kýsa Açýklama"; index++;
                    ws.Cells[1, index].Value = "Eylem"; index++;
                    ws.Cells[1, index].Value = "Vaka Niteliði"; index++;
                    ws.Cells[1, index].Value = "Þirket"; index++;
                    ws.Cells[1, index].Value = "Þirket Ýþkolu"; index++;
                    ws.Cells[1, index].Value = "Ana Þirket"; index++;
                    ws.Cells[1, index].Value = "Ana Þirket Ýþkolu"; index++;

                    for (int i = 0; i < resistanceReasonCount; i++)
                    {
                        ws.Cells[1, i + index].Value = $"Vaka Nedeni {i + 1}";
                    }
                    index += resistanceReasonCount;
                    ws.Cells[1, index].Value = "Hak Geliþtirme/Hak Savunma Özelliði"; index++;
                    ws.Cells[1, index].Value = "Ýþ Yerindeki Ýþçi Sayýsý"; index++;
                    ws.Cells[1, index].Value = "Ýþ Yerindeki Ýþçi Sayýsý (Tam)"; index++;
                    ws.Cells[1, index].Value = "Örgütleyen Sendika Var Mý?"; index++;
                    ws.Cells[1, index].Value = "Sendikanýn Yetki Durumu"; index++;
                    for (int i = 0; i < corporationCount; i++)
                    {
                        ws.Cells[1, i + index].Value = $"Kurumsallýk {i + 1}";

                    }
                    index += corporationCount;
                    ws.Cells[1, index].Value = "Tepki Gösterilen Sendika"; index++;
                    for (int i = 0; i < employmentTypeCount; i++)
                    {
                        ws.Cells[1, i + index].Value = $"Ýstihdam Türü {i + 1}";
                    }
                    index += employmentTypeCount;
                    ws.Cells[1, index].Value = "Cinsiyet"; index++;
                    ws.Cells[1, index].Value = "Mücadele Ettiði için Ýþten Atýlan Ýþçi Sayýsý"; index++;
                    ws.Cells[1, index].Value = "Hukuki Giriþim"; index++;
                    ws.Cells[1, index].Value = "Hukuki Giriþim Açýklama"; index++;
                    for (int i = 0; i < protestoTypeCount; i++)
                    {
                        ws.Cells[1, i + index].Value = $"Eylem Türü {i+1}";
                    }
                    index += protestoTypeCount;
                    ws.Cells[1, index].Value = "Eylemin Baþlangýç Tarihi"; index++;
                    ws.Cells[1, index].Value = "Eylemin Bitiþ Tarihi"; index++;
                    for (int i = 0; i < locationCount; i++)
                    {
                        ws.Cells[1, i + index].Value = $"Ýl-Ýlçe {i + 1}";
                    }
                    index += locationCount;
                    ws.Cells[1, index].Value = "Eylemdeki Ýþçi Sayýsý (Tam)"; index++;
                    ws.Cells[1, index].Value = "Eylemdeki Ýþçi Sayýsý"; index++;
                    for (int i = 0; i < interventionTypeCount; i++)
                    {
                        ws.Cells[1, i + index].Value = $"Müdahale Tipi {i + 1}";
                    }
                    index += interventionTypeCount;
                    ws.Cells[1, index].Value = "Gözaltý sayýsý"; index++;
                    ws.Cells[1, index].Value = "Notlar"; index++;

                    for (int i = 0; i < rows.Count; i++)
                    {
                        int r_index = 1;
                        ws.Cells[i + 2, r_index].Value = rows[i].ResistanceId; r_index++;
                        ws.Cells[i + 2, r_index].Value = rows[i].CategoryName; r_index++;
                        ws.Cells[i + 2, r_index].Value = rows[i].ProtestoId; r_index++;
                        ws.Cells[i + 2, r_index].Value = rows[i].CategoryName; r_index++;
                        ws.Cells[i + 2, r_index].Value = rows[i].CompanyName; r_index++;
                        ws.Cells[i + 2, r_index].Value = rows[i].CompanyWorkline; r_index++;
                        ws.Cells[i + 2, r_index].Value = rows[i].MainCompanyName; r_index++;
                        ws.Cells[i + 2, r_index].Value = rows[i].MainCompanyWorkline; r_index++;

                        for (int k = 0; k < rows[i].ResistanceReasons.Count; k++)
                        {
                            ws.Cells[i + 2, k + r_index].Value = rows[i].ResistanceReasons[k];
                        }
                        r_index += resistanceReasonCount;
                        ws.Cells[i + 2, r_index].Value = rows[i].DevelopRight; r_index++;
                        ws.Cells[i + 2, r_index].Value = rows[i].EmployeeCount; r_index++;
                        ws.Cells[i + 2, r_index].Value = rows[i].EmployeeCountNumber; r_index++;
                        ws.Cells[i + 2, r_index].Value = rows[i].HasTradeUnion; r_index++;
                        ws.Cells[i + 2, r_index].Value = rows[i].TradeUnionAuthority; r_index++;
                        for (int j = 0; j < rows[i].Corporations.Count; j++)
                        {
                            ws.Cells[i + 2, j + r_index].Value = rows[i].Corporations[j];

                        }
                        r_index += corporationCount;
                        ws.Cells[i + 2, r_index].Value = rows[i].TradeUnionReacted; r_index++;

                        for (int m = 0; m < rows[i].EmploymentTypes.Count; m++)
                        {
                            ws.Cells[i + 2, m + r_index].Value = rows[i].EmploymentTypes[m];
                        }
                        r_index += employmentTypeCount;
                        ws.Cells[i + 2, r_index].Value = rows[i].Gender; r_index++;
                        ws.Cells[i + 2, r_index].Value = rows[i].FiredEmployeeCount; r_index++;
                        ws.Cells[i + 2, r_index].Value = rows[i].AnyLegalIntervention; r_index++;
                        ws.Cells[i + 2, r_index].Value = rows[i].LegalInterventionDesc; r_index++;
                        for (int n = 0; n < rows[i].ProtestoTypes.Count; n++)
                        {
                            ws.Cells[i + 2, n + r_index].Value = rows[i].ProtestoTypes[n];
                        }
                        r_index += protestoTypeCount;
                        ws.Cells[i + 2, r_index].Value = rows[i].StartDate; r_index++;
                        ws.Cells[i + 2, r_index].Value = rows[i].EndDate; r_index++;
                        for (int l = 0; l < rows[i].Locations.Count; l++)
                        {
                            ws.Cells[i + 2, l + r_index].Value = rows[i].Locations[l];
                        }
                        r_index += locationCount;
                        ws.Cells[i + 2, r_index].Value = rows[i].EmployeeCountInProtestoNumber; r_index++;
                        ws.Cells[i + 2, r_index].Value = rows[i].EmployeeCountInProtesto; r_index++;
                        for (int z = 0; z < rows[i].InterventionTypes.Count; z++)
                        {
                            ws.Cells[i + 2, z + r_index].Value = rows[i].InterventionTypes[z];
                        }
                        r_index += interventionTypeCount;
                        ws.Cells[i + 2, r_index].Value = rows[i].CustodyCount; r_index++;
                        ws.Cells[i + 2, r_index].Value = rows[i].Note; r_index++;
                    }


                    ws.Cells.AutoFitColumns();
                    byteArray = pck.GetAsByteArray();
                }

                return File(byteArray, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", string.Format("{0}-Tarihli-EÇT Raporu.xlsx", DateTime.Now.ToShortDateString()));
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}