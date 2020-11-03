using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
        private readonly IStorageUtilities _utilities;
        private readonly RSDBContext _db;
        public ResistanceController(IResistanceApplication rsApplication, IStorageUtilities utilities, RSDBContext db)
        {
            _rsApplication = rsApplication;
            _utilities = utilities;
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
            ViewBag.Companies = new SelectList(_db.Company.Where(s => !s.Deleted).Select(s=> new LookupEntity { Id = s.Id, Name = s.Name}).OrderBy(s => s.Name).ToList(), "Id", "Name");
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
        public IActionResult AddOutsourceCompany()
        {
            ViewBag.Companies = new SelectList(_utilities.GetLookup("company"), "Id", "Name");
            ViewBag.Worklines = new SelectList(_utilities.GetLookup("companyworkline"), "Id", "Name");
            ViewBag.CompanyTypes = new SelectList(_utilities.GetLookup("companytype"), "Id", "Name");
            ViewBag.CompanyScales = new SelectList(_utilities.GetLookup("companyscale"), "Id", "Name");
            return PartialView("_AddOutsourceCompany");
        }
        [Authorize]
        public IActionResult AddCompany(bool isMain)
        {
            ViewBag.Worklines = new SelectList(_utilities.GetLookup("companyworkline"), "Id", "Name");
            ViewBag.CompanyTypes = new SelectList(_utilities.GetLookup("companytype"), "Id", "Name");
            ViewBag.CompanyScales = new SelectList(_utilities.GetLookup("companyscale"), "Id", "Name");
            return PartialView("_AddCompany", new CompanyCreateViewModel { IsMain = isMain});
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
            ViewBag.ProtestoTypes = new MultiSelectList(_db.ProtestoType.OrderBy(s=>s.Name).Select(s=> new LookupEntity { Id = s.Id, Name = s.Name}).ToList(), "Id", "Name");
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
            return Json(_db.District.Where(s=>s.CityId == cityId).OrderBy(s=>s.Name).Select(s=> new LookupEntity { Id = s.Id, Name = s.Name}).ToList());
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
        public IActionResult Export(ResistanceFilterModel filter)
        {
            var result = _db.Resistance
                    .Include("Category")
                    .Include("Company")
                    .Include("Company.CompanyWorkLine")
                    .Include("Company.OutsourceCompanies")
                    .Include("Company.OutsourceCompanies.Company")
                    .Include("Company.OutsourceCompanies.Company.CompanyWorkLine")
                    .Include("ResistanceEmploymentTypes")
                    .Include("ResistanceEmploymentTypes.EmploymentType")
                    .Include("TradeUnion")
                    .Include("ResistanceResistanceReasons")
                    .Include("ResistanceResistanceReasons.ResistanceReason")
                    .Include("ResistanceCorporations")
                    .Include("ResistanceCorporations.Corporation")
                    .Include("Protestos")
                    .Include("Protestos.Gender")
                    .Include("Protestos.ProtestoEmployeeCount")
                    .Include("Protestos.ProtestoProtestoTypes")
                    .Include("Protestos.ProtestoProtestoTypes.ProtestoType")
                    .Include("Protestos.ProtestoProtestoPlaces")
                    .Include("Protestos.ProtestoProtestoPlaces.ProtestoPlace")
                    .Include("Protestos.ProtestoInterventionTypes")
                    .Include("Protestos.ProtestoInterventionTypes.InterventionType")
                    .Include("Protestos.Locations")
                    .Include("Protestos.Locations.City")
                    .Include("Protestos.Locations.District")
                    .Include("ResistanceNews")
                    .Include("ResistanceNews.News")
                    .Where(rs => !rs.Deleted
                    && (filter.CompanyId == null || rs.CompanyId == filter.CompanyId)
                    && (filter.CategoryId == null || rs.CategoryId == filter.CategoryId)
                    && (filter.YearId == null || rs.Protestos.Any(s => s.StartDate.Year == filter.YearId && s.StartDate.Month == filter.MonthId))
                    && (filter.PersonalNote == null || String.IsNullOrEmpty(rs.Note) == !filter.PersonalNote)
                    )
                    .ToList();

            return Json(result);
        }
    }
}