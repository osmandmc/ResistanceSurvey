using System.Collections.Generic;
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
        public ResistanceController(IResistanceApplication rsApplication, IStorageUtilities utilities)
        {
            _rsApplication = rsApplication;
            _utilities = utilities;
        }
        public IActionResult Index()
        {
            var resistances = _rsApplication.GetAll();
            return View(resistances);
        }
        [HttpGet]
        public IActionResult Create()
        {
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
            ViewBag.Worklines = new SelectList(_utilities.GetLookup("workline"), "Id", "Name");
            ViewBag.CompanyTypes = new SelectList(_utilities.GetLookup("companytype"), "Id", "Name");
            ViewBag.CompanyScales = new SelectList(_utilities.GetLookup("companyscale"), "Id", "Name");
            return PartialView("_AddOutsourceCompany");
        }
        public IActionResult AddCompany()
        {
            ViewBag.Worklines = new SelectList(_utilities.GetLookup("workline"), "Id", "Name");
            ViewBag.CompanyTypes = new SelectList(_utilities.GetLookup("companytype"), "Id", "Name");
            ViewBag.CompanyScales = new SelectList(_utilities.GetLookup("companyscale"), "Id", "Name");
            return PartialView("_AddCompany");
        }
        [HttpPost]
        public IActionResult AddCompany(CompanyCreateViewModel model)
        {
            if(ModelState.IsValid)
            {
                var result = new CompanyReturnDto();
                if(model.CompanyId==null)
                    result = _rsApplication.CreateCompany(model);
                else
                    result = _rsApplication.CreateOutsourceCompany(model);
                
                return Ok(result);

            }
            ViewBag.Worklines = new SelectList(_utilities.GetLookup("workline"), "Id", "Name");
            ViewBag.CompanyTypes = new SelectList(_utilities.GetLookup("companytype"), "Id", "Name");
            ViewBag.CompanyScales = new SelectList(_utilities.GetLookup("companyscale"), "Id", "Name");
            return PartialView("_AddCompany");
        }
        public IActionResult View(int id)
        {
            var resistance = _rsApplication.GetResistanceDetail(id);
            SetLookups();
            return View(resistance);
        }

        public IActionResult ExistingResistance(int companyId, int categoryId)
        {
            return Ok(_rsApplication.ExistingResistance(companyId, categoryId));
        }
        public void SetLookups()
        {
            ViewBag.Categories = new SelectList(_utilities.GetLookup("category"), "Id", "Name");
            ViewBag.Companies = new SelectList(_utilities.GetLookup("company"), "Id", "Name");
            ViewBag.Worklines = new SelectList(_utilities.GetLookup("workline"), "Id", "Name");
            ViewBag.EmployeeCount = new SelectList(_utilities.GetLookup("employeecount"), "Id", "Name");
            ViewBag.EmployeeCountInProtesto = new SelectList(_utilities.GetLookup("employeecountinprotesto"), "Id", "Name");
            ViewBag.Corporations = new SelectList(_utilities.GetLookup("corporation"), "Id", "Name");
            ViewBag.TradeUnions = new SelectList(_utilities.GetLookup("tradeunion"), "Id", "Name");
            ViewBag.TradeUnionAuthorities = new SelectList(_utilities.GetLookup("tradeunionauthority"), "Id", "Name");
            ViewBag.EmploymentTypes = new SelectList(_utilities.GetLookup("employmenttype"), "Id", "Name");
           
            ViewBag.EmployeeCountInProtesto = new SelectList(_utilities.GetLookup("employeecountinprotesto"), "Id", "Name");

            ViewBag.ProtestoTypes = new MultiSelectList(_utilities.GetLookup("protestotype"), "Id", "Name");
            ViewBag.ProtestoPlaces = new MultiSelectList(_utilities.GetLookup("protestoplace"), "Id", "Name");
            ViewBag.ProtestoReasons = new MultiSelectList(_utilities.GetLookup("protestoreason"), "Id", "Name");

            ViewBag.Genders = new SelectList(_utilities.GetLookup("gender"), "Id", "Name");
            ViewBag.ProtestoCities = new SelectList(_utilities.GetLookup("city"), "Id", "Name");
            ViewBag.InterventionTypes = new SelectList(_utilities.GetLookup("interventiontype"), "Id", "Name");
        }
    }
}