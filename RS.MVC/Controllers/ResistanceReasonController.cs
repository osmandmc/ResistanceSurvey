using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RS.COMMON.DTO;
using RS.COMMON.Entities.LookupEntity;
using RS.EF;

namespace RS.MVC.Controllers
{
    public class ResistanceReasonController : Controller
    {
        private readonly RSDBContext db;

        public ResistanceReasonController(RSDBContext db)
        {
            this.db = db;
        }
        public IActionResult Index()
        {
            ViewBag.ResistanceReasons = new SelectList(db.ResistanceReason.OrderBy(s=>s.Name).Select(s => new LookupEntity { Id = s.Id, Name = s.Name }).ToList(), "Id", "Name");
            return View();
        }
        
        [Authorize]
        public IActionResult List()
        {
            var resistanceReasons = db.ResistanceReason.OrderBy(s=>s.Name).Select(s => new LookupEntity { Id = s.Id, Name = s.Name }).ToList();
            return Json(resistanceReasons);
        }
        [Authorize]
        public IActionResult _List(ResistanceReasonFilterModel filter)
        {
            var resistanceReasons = db.ResistanceReason.OrderBy(s=>s.Name).Where(s=>String.IsNullOrEmpty(filter.Name) || s.Name.Contains(filter.Name))
            .ToPagedFilteredResult(filter);
            return PartialView(resistanceReasons);
        }
        [Authorize]
        [HttpPost]
        public IActionResult Save(ResistanceReason model)
        {
            var resistanceReason = db.ResistanceReason.Find(model.Id);
            resistanceReason.Name = model.Name;
            db.Entry(resistanceReason).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            db.SaveChanges();
            return Ok();
        }
        [Authorize]
        [HttpPost]
        public IActionResult SetApproval(ResistanceReason model)
        {
            var resistanceReason = db.ResistanceReason.Find(model.Id);
            resistanceReason.Approved = model.Approved;
            db.Entry(resistanceReason).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            db.SaveChanges();
            return Ok();
        }
        [Authorize]
        public IActionResult CheckResistanceReason(int id)
        {
            var used = db.ResistanceResistanceReason.Any(s => !s.Resistance.Deleted && s.ResistanceReasonId == id);
            return Json(used);
        }
        [Authorize]
        [HttpPost]
        public IActionResult ReplaceResistanceReason(ReplaceModel model)
        {
            var anyResistanceResistanceReason = db.ResistanceResistanceReason.Any(s => s.ResistanceReasonId == model.NewId);
            var usedResistances = db.ResistanceResistanceReason.Where(s => s.ResistanceReasonId == model.Id).ToList();
            if (!anyResistanceResistanceReason)
            {
                db.ResistanceResistanceReason.RemoveRange(usedResistances);
            }
            else
            {
                foreach (var item in usedResistances)
                {
                    item.ResistanceReasonId = model.NewId;
                    db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }
            }
            var resistanceReason = db.ResistanceReason.Find(model.Id);
            db.ResistanceReason.Remove(resistanceReason);
            db.SaveChanges();
            return Ok();
        }
        [Authorize]
        [HttpPost]
        public IActionResult DeleteResistanceReason(PostBaseModel model)
        {
            var resistanceReason = db.ResistanceReason.Find(model.Id);
            db.ResistanceReason.Remove(resistanceReason);
            db.SaveChanges();
            return Ok();
        }

    }

}