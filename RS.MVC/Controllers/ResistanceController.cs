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
        public IActionResult ResistanceList(ResistanceFilterModel filter)
        {
            ViewBag.Companies = new SelectList(_db.Company.Select(s=> new LookupEntity { Id = s.Id, Name = s.Name}).OrderBy(s => s.Name).ToList(), "Id", "Name");
            var resistances = _rsApplication.GetPaged(filter);
            return PartialView("_ResistanceList", resistances);
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
            var companies = _db.Company.Select(x => new { Id = x.Id, Name = x.Name, IsOutsource = x.IsOutsource }).ToList();
            ViewBag.Companies = new SelectList(companies.Where(s => !s.IsOutsource), "Id", "Name");
            SetLookups();
            var name = HttpContext.User.Identity.Name;
            return PartialView("_Create");
        }
        [Authorize]
        [HttpPost]
        public IActionResult Create(ResistanceCreateModel model)
        {
            if (ModelState.IsValid)
            {
                model.UserName = UserName;
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
        public IActionResult AddCompany()
        {
          
            ViewBag.Worklines = new SelectList(_utilities.GetLookup("companyworkline"), "Id", "Name");
            ViewBag.CompanyTypes = new SelectList(_utilities.GetLookup("companytype"), "Id", "Name");
            ViewBag.CompanyScales = new SelectList(_utilities.GetLookup("companyscale"), "Id", "Name");
            return PartialView("_AddCompany");
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
        [HttpPost]
        public IActionResult AddOutsourceCompany(OutsourceCompanyCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = _rsApplication.CreateOutsourceCompany(model);
                return Ok(result);
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
            return Json(ModelState.Values.Where(m => m.Errors.Count > 1).Select(s => s.Errors));
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
            var companies = _db.Company.Select(x => new { Id = x.Id, Name = x.Name, IsOutsource = x.IsOutsource }).ToList();
            var outsourceCompanies = (from c in _db.Company
                                      join coc in _db.CompanyOutsourceCompany on c.Id equals coc.OutsourceCompanyId into oc
                                      from o in oc.DefaultIfEmpty()
                                      where o.CompanyId == resistance.CompanyId
                                      select new { Id = o.OutsourceCompanyId, Name = o.OutsourceCompany.Name })
                                     .ToList();

            ViewBag.Companies = new SelectList(companies.Where(s => !s.IsOutsource), "Id", "Name");
            ViewBag.OutsourceCompanies = new SelectList(outsourceCompanies, "Id", "Name");
            SetLookups();
            return PartialView("_EditResistance", resistance);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Edit(ResistanceEditModel viewModel)
        {
            if (ModelState.IsValid)
            {
                viewModel.UserName = UserName;
                _rsApplication.UpdateResistance(viewModel);
                return Ok();
            }
            var errors = ModelState.Values.Where(m => m.Errors.Count > 0).Select(s => s.Errors).ToList();
            return Json(errors);
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
          
            ViewBag.Categories = new SelectList(_utilities.GetLookup("category"), "Id", "Name");
            
            
            ViewBag.Worklines = new SelectList(_utilities.GetLookup("companyworkline"), "Id", "Name");
            ViewBag.EmployeeCount = new SelectList(_utilities.GetLookup("employeecount"), "Id", "Name");
            ViewBag.EmployeeCountInProtesto = new SelectList(_utilities.GetLookup("ProtestoEmployeeCount"), "Id", "Name");
            ViewBag.Corporations = new SelectList(_utilities.GetLookup("corporation").OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.TradeUnions = new SelectList(_utilities.GetLookup("tradeunion"), "Id", "Name");
            ViewBag.TradeUnionAuthorities = new SelectList(_utilities.GetLookup("tradeunionauthority"), "Id", "Name");
            ViewBag.EmploymentTypes = new SelectList(_utilities.GetLookup("employmenttype"), "Id", "Name");
            ViewBag.EmployeeCountInProtesto = new SelectList(_utilities.GetLookup("ProtestoEmployeeCount"), "Id", "Name");
            ViewBag.ProtestoTypes = new MultiSelectList(_utilities.GetLookup("protestotype"), "Id", "Name");
            ViewBag.ProtestoPlaces = new MultiSelectList(_utilities.GetLookup("protestoplace"), "Id", "Name");
            ViewBag.ResistanceReasons = new MultiSelectList(_utilities.GetLookup("resistancereason"), "Id", "Name");
            ViewBag.Genders = new SelectList(_utilities.GetLookup("gender"), "Id", "Name");
            ViewBag.ProtestoCities = new SelectList(_utilities.GetLookup("city"), "Id", "Name");
            ViewBag.InterventionTypes = new SelectList(_utilities.GetLookup("interventiontype"), "Id", "Name");
            ViewBag.Cities = new SelectList(_utilities.GetLookup("city").OrderBy(s => s.Name).ToList(), "Id", "Name");
            ViewBag.Districts = new SelectList(_utilities.GetLookup("district").OrderBy(s => s.Name).ToList(), "Id", "Name");
        }

        [Authorize]
        public JsonResult GetDistricts(int cityId)
        {
            return Json(_utilities.GetDistricts(cityId));
        }
        [Authorize]
        public JsonResult GetOutsourceCompanies(int companyId)
        {
            var outsourceCompanies = (from c in _db.Company
                                      join coc in _db.CompanyOutsourceCompany on c.Id equals coc.OutsourceCompanyId into oc
                                      from o in oc.DefaultIfEmpty()
                                      where o.CompanyId == companyId
                                      select new { value = o.OutsourceCompanyId, name = o.OutsourceCompany.Name, text = o.OutsourceCompany.Name })
                                     .ToList();
            return Json(outsourceCompanies);
        }
    }
}