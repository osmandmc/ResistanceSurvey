using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ResistanceSurvey.Models;
using RS.COMMON;
using RS.COMMON.DTO;
using RS.MVC.Applications;
using RS.MVC.Models;

namespace ResistanceSurvey.Controllers
{
    public class ResistanceController : Controller
    {
        private readonly IResistanceApplication _rsApplication;
        private readonly IStorageUtilities _utilities;
        private FilterModel filter;
        public ResistanceController(IResistanceApplication rsApplication, IStorageUtilities utilities)
        {
            _rsApplication = rsApplication;
            _utilities = utilities;
            filter = new FilterModel();
        }
        public IActionResult Index()
        {
            var resistances = _rsApplication.GetPaged(filter);
            return View(resistances);
        }
      
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.News = _rsApplication.GetNewsList("19/08");
            SetLookups();
            return View();
        }
        [HttpPost]
        public IActionResult Create(ResistanceCreateModel model)
        {
            if (ModelState.IsValid)
            {
                _rsApplication.Create(model);
                return Ok();
            }
            SetLookups();
            return View(model);
        }
        public IActionResult AddOutsourceCompany()
        {
            ViewBag.Companies = new SelectList(_utilities.GetLookup("company"), "Id", "Name");
            ViewBag.Worklines = new SelectList(_utilities.GetLookup("companyworkline"), "Id", "Name");
            ViewBag.CompanyTypes = new SelectList(_utilities.GetLookup("companytype"), "Id", "Name");
            ViewBag.CompanyScales = new SelectList(_utilities.GetLookup("companyscale"), "Id", "Name");
            return PartialView("_AddOutsourceCompany");
        }
        public IActionResult AddCompany()
        {
            ViewBag.Worklines = new SelectList(_utilities.GetLookup("companyworkline"), "Id", "Name");
            ViewBag.CompanyTypes = new SelectList(_utilities.GetLookup("companytype"), "Id", "Name");
            ViewBag.CompanyScales = new SelectList(_utilities.GetLookup("companyscale"), "Id", "Name");
            return PartialView("_AddCompany");
        }
        [HttpPost]
        public IActionResult AddCompany(CompanyCreateViewModel model)
        {
            if(ModelState.IsValid)
            {
                if(model.CompanyId==null){
                    var result1 = _rsApplication.CreateCompany(model);
                    return Ok(result1);
                }       
                var result = _rsApplication.CreateOutsourceCompany(model);
                return Ok(result);
            }
            ViewBag.Worklines = new SelectList(_utilities.GetLookup("companyworkline"), "Id", "Name");
            ViewBag.CompanyTypes = new SelectList(_utilities.GetLookup("companytype"), "Id", "Name");
            ViewBag.CompanyScales = new SelectList(_utilities.GetLookup("companyscale"), "Id", "Name");
            return PartialView("_AddCompany");
        }
        public IActionResult AddProtesto(int id)
        {
            ViewBag.ResistanceId = id;
            SetLookups();
            return View();
        }

        [HttpPost]
        public IActionResult AddProtesto(ProtestoAddModel viewModel)
        {
            if(ModelState.IsValid)
            {
                _rsApplication.AddProtesto(viewModel);
                return Ok();
            }
            SetLookups();
            return View(viewModel);
        }
        public IActionResult EditProtesto(int id)
        {
            var protesto = _rsApplication.GetProtestoDetail(id);
            SetLookups();
            return View(protesto);
        }

        [HttpPost]
        public IActionResult EditProtesto(ProtestoEditModel viewModel)
        {
            if(ModelState.IsValid)
            {
                _rsApplication.UpdateProtesto(viewModel);
                return Ok();
            }
            SetLookups();
            return View(viewModel);
        }
        public IActionResult Edit(int id)
        {
            var resistance = _rsApplication.GetResistanceDetail(id);
            SetLookups();
            return View(resistance);
        }

        [HttpPost]
        public IActionResult Edit(ResistanceEditModel viewModel)
        {
            if(ModelState.IsValid)
            {
                _rsApplication.UpdateResistance(viewModel);
                return Ok();
            }
            SetLookups();
            return View(viewModel);
        }

        public IActionResult ExistingResistance(int companyId, int categoryId)
        {
            return Ok(_rsApplication.ExistingResistance(companyId, categoryId));
        }
        public IActionResult GetNewsContent(int newsId)
        {
            return Ok(_rsApplication.GetNewsContent(newsId));
        }
        public void SetLookups()
        {
            
            ViewBag.Categories = new SelectList(_utilities.GetLookup("category"), "Id", "Name");
            ViewBag.Companies = new SelectList(_utilities.GetLookup("company"), "Id", "Name");
            ViewBag.Worklines = new SelectList(_utilities.GetLookup("companyworkline"), "Id", "Name");
            ViewBag.EmployeeCount = new SelectList(_utilities.GetLookup("employeecount"), "Id", "Name");
            ViewBag.EmployeeCountInProtesto = new SelectList(_utilities.GetLookup("ProtestoEmployeeCount"), "Id", "Name");
            ViewBag.Corporations = new SelectList(_utilities.GetLookup("corporation"), "Id", "Name");
            ViewBag.TradeUnions = new SelectList(_utilities.GetLookup("tradeunion"), "Id", "Name");
            ViewBag.TradeUnionAuthorities = new SelectList(_utilities.GetLookup("tradeunionauthority"), "Id", "Name");
            ViewBag.EmploymentTypes = new SelectList(_utilities.GetLookup("employmenttype"), "Id", "Name");
           
            ViewBag.EmployeeCountInProtesto = new SelectList(_utilities.GetLookup("ProtestoEmployeeCount"), "Id", "Name");

            ViewBag.ProtestoTypes = new MultiSelectList(_utilities.GetLookup("protestotype"), "Id", "Name");
            ViewBag.ProtestoPlaces = new MultiSelectList(_utilities.GetLookup("protestoplace"), "Id", "Name");
            ViewBag.ProtestoReasons = new MultiSelectList(_utilities.GetLookup("protestoreason"), "Id", "Name");

            ViewBag.Genders = new SelectList(_utilities.GetLookup("gender"), "Id", "Name");
            ViewBag.ProtestoCities = new SelectList(_utilities.GetLookup("city"), "Id", "Name");
            ViewBag.InterventionTypes = new SelectList(_utilities.GetLookup("interventiontype"), "Id", "Name");
        }
    }
}