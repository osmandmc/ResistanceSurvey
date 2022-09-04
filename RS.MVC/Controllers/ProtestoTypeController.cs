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
    public class ProtestoTypeController : Controller
    {
        private readonly RSDBContext db;

        public ProtestoTypeController(RSDBContext db)
        {
            this.db = db;
        }
        public IActionResult Index()
        {
            ViewBag.ProtestoTypes = new SelectList(db.ProtestoType.OrderBy(s=>s.Name).Select(s => new LookupEntity { Id = s.Id, Name = s.Name }).ToList(), "Id", "Name"); ;
            return View();
        }
        [Authorize]
        public IActionResult _List(ProtestoTypeFilterModel filter)
        {
            var protestoTypes = db.ProtestoType.OrderBy(s=>s.Name)
                .Where(s=>String.IsNullOrEmpty(filter.Name) || s.Name.Contains(filter.Name))
                .ToPagedFilteredResult(filter);
            return PartialView(protestoTypes);
        }
        [Authorize]
        [HttpPost]
        public IActionResult Save(ProtestoType model)
        {
            var protestoType = db.ProtestoType.Find(model.Id);
            protestoType.Name = model.Name;
            db.Entry(protestoType).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            db.SaveChanges();
            return Ok();
        }
        [Authorize]
        [HttpPost]
        public IActionResult SetApproval(ProtestoType model)
        {
            var protestoType = db.ProtestoType.Find(model.Id);
            protestoType.Approved = model.Approved;
            db.Entry(protestoType).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            db.SaveChanges();
            return Ok();
        }
        [Authorize]
        [HttpPost]
        public IActionResult SetAgainstProduction(ProtestoType model)
        {
            var protestoType = db.ProtestoType.Find(model.Id);
            protestoType.AgainstProduction = model.AgainstProduction;
            db.Entry(protestoType).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            db.SaveChanges();
            return Ok();
        }
        [Authorize]
        public IActionResult CheckProtestoType(int id)
        {
            var used = db.ProtestoProtestoType.Any(s => !s.Protesto.Deleted && s.ProtestoTypeId == id);
            return Json(used);
        }
        [Authorize]
        [HttpPost]
        public IActionResult ReplaceProtestoType(ReplaceModel model)
        {
            var anyResistanceProtestoType = db.ProtestoProtestoType.Any(s => s.ProtestoTypeId == model.NewId);
            var usedResistances = db.ProtestoProtestoType.Where(s => s.ProtestoTypeId == model.Id).ToList();
            if (!anyResistanceProtestoType)
            {
                db.ProtestoProtestoType.RemoveRange(usedResistances);
            }
            else
            {
                foreach (var item in usedResistances)
                {
                    item.ProtestoTypeId = model.NewId;
                    db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }
            }
            var protestoType = db.ProtestoType.Find(model.Id);
            db.ProtestoType.Remove(protestoType);
            db.SaveChanges();
            return Ok();
        }
        [Authorize]
        [HttpPost]
        public IActionResult DeleteProtestoType(PostBaseModel model)
        {
            var protestoType = db.ProtestoType.Find(model.Id);
            db.ProtestoType.Remove(protestoType);
            db.SaveChanges();
            return Ok();
        }

    }

}