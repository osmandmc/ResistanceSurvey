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
            ViewBag.Companies = new SelectList(db.Company.Where(s => !s.Deleted).Select(s => new LookupEntity { Id = s.Id, Name = s.Name }).ToList(), "Id", "Name");
            return View();
        }
        [HttpPost]
        [Authorize]
        public IActionResult _List(CompanyFilterModel filter)
        {
            ViewBag.Companies = new SelectList(db.Company.Where(s => !s.Deleted).Select(s => new LookupEntity { Id = s.Id, Name = s.Name }).ToList(), "Id", "Name");
            var companies = (from c in db.Company
                             join coc in db.CompanyOutsourceCompany on c.Id equals coc.OutsourceCompanyId
                             into cod
                             from d in cod.DefaultIfEmpty()
                             where !c.Deleted
                             && (filter.CompanyId == null || c.Id == filter.CompanyId)
                             && (filter.MainCompanyId == null || d.CompanyId == filter.MainCompanyId)
                             orderby c.Name
                             select new ComapnyListViewModel
                             {
                                 Id = c.Id,
                                 Name = c.Name,
                                 MainCompanyId = d.Company != null ? d.CompanyId : 0,
                                 MainCorporationName = d.Company != null ? d.Company.Name : "",
                                 Scale = c.CompanyScale != null ? c.CompanyScale.Name : "",
                                 Type = c.CompanyType != null ? c.CompanyType.Name : "",
                                 WorkLine = c.CompanyWorkLine != null ? c.CompanyWorkLine.Name : ""
                             }
                             )
                             .ToPagedFilteredResult(filter);

            return PartialView(companies);
        }
        [Authorize]
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
        [Authorize]
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
        [Authorize]
        public ActionResult _EditOutsource(int id)
        {
            ViewBag.Worklines = new SelectList(db.CompanyWorkLine.ToList(), "Id", "Name");
            ViewBag.CompanyTypes = new SelectList(db.CompanyType.ToList(), "Id", "Name");
            ViewBag.CompanyScales = new SelectList(db.CompanyScale.ToList(), "Id", "Name");
            ViewBag.Companies = new SelectList(db.Company.Where(s => !s.Deleted).Select(s => new LookupEntity { Id = s.Id, Name = s.Name }).ToList(), "Id", "Name");
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
        [Authorize]
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
        [Authorize]
        public IActionResult CheckCompany(int id, int mainCompanyId)
        {
            var used = db.Resistance.Any(s =>
                !s.Deleted && s.CompanyId == id &&
                !s.Company.Deleted &&
                (mainCompanyId == 0 || mainCompanyId == s.MainCompanyId && !s.MainCompany.Deleted)
            );
            return Json(used);
        }
        [Authorize]
        [HttpPost]
        public IActionResult ReplaceCompany(ReplaceModel model)
        {

            var usedResistances = db.Resistance.Where(s => s.CompanyId == model.Id).ToList();
            foreach (var item in usedResistances)
            {
                item.CompanyId = model.NewId;
                db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            }
            var company = db.Company.Find(model.Id);
            company.Deleted = true;
            db.SaveChanges();
            return Ok();
        }
        [Authorize]
        [HttpPost]
        public IActionResult DeleteCompany(CompanyDeleteModel model)
        {
            var used = db.Resistance.Any(s =>
                !s.Deleted && s.CompanyId == model.CompanyId &&
                !s.Company.Deleted &&
                (model.MainCompanyId == 0 || model.MainCompanyId == s.MainCompanyId && !s.MainCompany.Deleted)
            );
            if (model.MainCompanyId != 0)
            {
                var coc = db.CompanyOutsourceCompany.Where(s => s.CompanyId == model.MainCompanyId && s.OutsourceCompanyId == model.CompanyId).FirstOrDefault();
                db.CompanyOutsourceCompany.Remove(coc);
            }
            if (!used)
            {
                var company = db.Company.Find(model.CompanyId);
                db.Company.Remove(company);
            }
            db.SaveChanges();
            return Ok();
        }

    }

}