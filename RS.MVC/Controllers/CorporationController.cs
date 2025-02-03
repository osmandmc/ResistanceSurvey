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
    public class CorporationController : Controller
    {
        private readonly RSDBContext db;

        public CorporationController(RSDBContext db)
        {
            this.db = db;
        }
        public IActionResult Index()
        {
            ViewBag.Corporations = new SelectList(db.Corporation.Select(s => new LookupEntity { Id = s.Id, Name = s.Name }).ToList(), "Id", "Name"); ;
            return View();
        }
        [Authorize]
        public IActionResult List()
        {
            var corporations = db.Corporation.Select(s => new LookupEntity { Id = s.Id, Name = s.Name }).ToList();
            return Json(corporations);
        }
        
        [Authorize]
        public IActionResult _List(CorporationFilterModel filter)
        {
            ViewBag.CorporationTypes = new SelectList(db.CorporationType.Select(s => new LookupEntity { Id = s.Id, Name = s.Name }).ToList(), "Id", "Name");
            var corporations = db.Corporation
                .Where(s=>String.IsNullOrEmpty(filter.Name) || s.Name.Contains(filter.Name))
                .Select(s => new CorporationListView { Id=s.Id, Name = s.Name, CorporationTypeId = s.CorporationTypeId, CorporationTypeName = s.CorporationType != null ? s.CorporationType.Name : "", Approved = s.Approved })
                .OrderBy(s=>s.Name)
                .ToPagedFilteredResult(filter);
            return PartialView(corporations);
        }
        [Authorize]
        [HttpPost]
        public IActionResult Save(Corporation model)
        {
            var corporation = db.Corporation.Find(model.Id);
            corporation.Name = model.Name;
            db.Entry(corporation).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            db.SaveChanges();
            return Ok();
        }
        [Authorize]
        [HttpPost]
        public IActionResult SetApproval(Corporation model)
        {
            var corporation = db.Corporation.Find(model.Id);
            corporation.Approved = model.Approved;
            db.Entry(corporation).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            db.SaveChanges();
            return Ok();
        }
        [Authorize]
        [HttpPost]
        public IActionResult SetCorporationType(Corporation model)
        {
            var corporation = db.Corporation.Find(model.Id);
            corporation.CorporationTypeId = model.CorporationTypeId;
            db.Entry(corporation).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            db.SaveChanges();
            return Ok();
        }
        [Authorize]
        public IActionResult CheckCorporation(int id)
        {
            var used = db.ResistanceCorporation.Any(s => !s.Resistance.Deleted && s.CorporationId == id);
            return Json(used);
        }
        [Authorize]
        [HttpPost]
        public IActionResult ReplaceCorporation(ReplaceModel model)
        {
            var anyResistanceCorporation = db.ResistanceCorporation.Any(s => s.CorporationId == model.NewId);
            var usedResistances = db.ResistanceCorporation.Where(s => s.CorporationId == model.Id).ToList();
            if (!anyResistanceCorporation)
            {
                db.ResistanceCorporation.RemoveRange(usedResistances);
            }
            else
            {
                foreach (var item in usedResistances)
                {
                    item.CorporationId = model.NewId;
                    db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }
            }
            var corporation = db.Corporation.Find(model.Id);
            db.Corporation.Remove(corporation);
            db.SaveChanges();
            return Ok();
        }
        [Authorize]
        [HttpPost]
        public IActionResult DeleteCorporation(PostBaseModel model)
        {
            var corporation = db.Corporation.Find(model.Id);
            db.Corporation.Remove(corporation);
            db.SaveChanges();
            return Ok();
        }
        [HttpPost]
        public ActionResult IsTradeUnion(List<int> corporationIds)
        {
            var ids = corporationIds.Select(s => Convert.ToInt32(s)).ToList();
            var isTradeUnion = db.Corporation.Any(s => ids.Contains(s.Id) && s.CorporationTypeId == 1);
            return Json(isTradeUnion);
        }
        [HttpGet]
        public ActionResult CheckTradeUnion(List<int> corporationIds)
        {
            var isTradeUnion = db.Corporation
                .Any(s => corporationIds.Contains(s.Id) && s.CorporationTypeId == 1);
            return Json(isTradeUnion);
        }
    }

}