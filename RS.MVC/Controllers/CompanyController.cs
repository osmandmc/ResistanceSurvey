using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.ConditionalFormatting;
using RS.COMMON;
using RS.COMMON.DTO;
using RS.COMMON.Entities.LookupEntity;
using RS.EF;

namespace RS.MVC.Controllers
{
    public class CompanyController : BaseController
    {
        private readonly RSDBContext db;
        public CompanyController(RSDBContext db)
        {
            this.db = db;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult _List(CompanyFilterModel filter)
        {

            var companies = (from c in db.Company
                             join coc in db.CompanyOutsourceCompany on c.Id equals coc.OutsourceCompanyId
                             into cod
                             from d in cod.DefaultIfEmpty()
                             //where String.IsNullOrEmpty(filter.Name) || d.Company.Name == filter.Name
                             select new ComapnyListViewModel
                             {
                                 Id = c.Id,
                                 Name = c.Name,
                                 MainCorporationName = d.Company != null ? d.Company.Name : "",
                                 Scale = c.CompanyScale != null ? c.CompanyScale.Name : "",
                                 Type = c.CompanyType != null ? c.CompanyType.Name : "",
                                 WorkLine = c.CompanyWorkLine != null ? c.CompanyWorkLine.Name : ""
                             }
                             )
                             .ToPagedFilteredResult(filter);
                             
            return PartialView(companies);
        }
        public ActionResult _Edit(int id)
        {
            ViewBag.Worklines = new SelectList(db.CompanyWorkLine.ToList(), "Id", "Name");
            ViewBag.CompanyTypes = new SelectList(db.CompanyType.ToList(), "Id", "Name");
            ViewBag.CompanyScales = new SelectList(db.CompanyScale.ToList(), "Id", "Name");
            var company = db.Company.Where(s => s.Id == id)
                .Select(s => new CompanyEditModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    ScaleId = s.CompanyScaleId,
                    TypeId = s.CompanyTypeId,
                    WorklineId = s.CompanyWorkLineId
                })
                .SingleOrDefault();
            return PartialView(company);
        }
        [HttpPost]
        public ActionResult Edit(CompanyEditModel model)
        {
            var company = db.Company.Find(model.Id);
            company.Name = model.Name;
            company.CompanyScaleId = model.ScaleId;
            company.CompanyTypeId = model.TypeId;
            company.CompanyWorkLineId = model.WorklineId;
            db.SaveChanges();
            return Ok();
        }
        public ActionResult _EditOutsource(int id)
        {
            ViewBag.Worklines = new SelectList(db.CompanyWorkLine.ToList(), "Id", "Name");
            ViewBag.CompanyTypes = new SelectList(db.CompanyType.ToList(), "Id", "Name");
            ViewBag.CompanyScales = new SelectList(db.CompanyScale.ToList(), "Id", "Name");
            ViewBag.Companies = new SelectList(db.Company.Select(s=>new LookupEntity { Id = s.Id, Name= s.Name}).ToList(), "Id", "Name");
            var company = (from c in db.Company
             join coc in db.CompanyOutsourceCompany on c.Id equals coc.OutsourceCompanyId
             into cod
             from d in cod.DefaultIfEmpty()
             where c.Id == id
             select new OutsourceCompanyEditModel
             {
                 Id = c.Id,
                 Name = c.Name,
                 ScaleId = c.CompanyScaleId,
                 TypeId = c.CompanyTypeId,
                 WorklineId = c.CompanyWorkLineId,
                 MainCompanyId = d.CompanyId
                })
                .SingleOrDefault();
            return PartialView(company);
        }
        [HttpPost]
        public ActionResult EditOutsource(OutsourceCompanyEditModel model)
        {
            var company = db.Company.Find(model.Id);
            company.Name = model.Name;
            company.CompanyScaleId = model.ScaleId;
            company.CompanyTypeId = model.TypeId;
            company.CompanyWorkLineId = model.WorklineId;
            var coc = db.CompanyOutsourceCompany.Where(s => s.OutsourceCompanyId == model.Id).FirstOrDefault();
            coc.CompanyId = model.MainCompanyId;
            db.SaveChanges();
            return Ok();
        }
    }
    
}