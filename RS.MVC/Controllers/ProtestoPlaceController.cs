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
    public class ProtestoPlaceController : Controller
    {
        private readonly RSDBContext db;

        public ProtestoPlaceController(RSDBContext db)
        {
            this.db = db;
        }
        public IActionResult Index()
        {
            ViewBag.ProtestoPlaces = new SelectList(db.ProtestoPlace.Select(s => new LookupEntity { Id = s.Id, Name = s.Name }).ToList(), "Id", "Name"); ;
            return View();
        }
        [Authorize]
        public IActionResult _List(FilterModel filter)
        {
            var protestoPlaces = db.ProtestoPlace.ToPagedFilteredResult(filter);
            return PartialView(protestoPlaces);
        }
        [Authorize]
        [HttpPost]
        public IActionResult Save(ProtestoPlace model)
        {
            var protestoPlace = db.ProtestoPlace.Find(model.Id);
            protestoPlace.Name = model.Name;
            db.Entry(protestoPlace).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            db.SaveChanges();
            return Ok();
        }
        [Authorize]
        [HttpPost]
        public IActionResult SetApproval(ProtestoPlace model)
        {
            var protestoPlace = db.ProtestoPlace.Find(model.Id);
            protestoPlace.Approved = model.Approved;
            db.Entry(protestoPlace).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            db.SaveChanges();
            return Ok();
        }
        [Authorize]
        public IActionResult CheckProtestoPlace(int id)
        {
            var used = db.ProtestoProtestoPlace.Any(s => !s.Protesto.Deleted && s.ProtestoPlaceId == id);
            return Json(used);
        }
        [Authorize]
        [HttpPost]
        public IActionResult ReplaceProtestoPlace(ReplaceModel model)
        {
            var anyResistanceProtestoPlace = db.ProtestoProtestoPlace.Any(s => s.ProtestoPlaceId == model.NewId);
            var usedResistances = db.ProtestoProtestoPlace.Where(s => s.ProtestoPlaceId == model.Id).ToList();
            if (!anyResistanceProtestoPlace)
            {
                db.ProtestoProtestoPlace.RemoveRange(usedResistances);
            }
            else
            {
                foreach (var item in usedResistances)
                {
                    item.ProtestoPlaceId = model.NewId;
                    db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }
            }
            var protestoPlace = db.ProtestoPlace.Find(model.Id);
            db.ProtestoPlace.Remove(protestoPlace);
            db.SaveChanges();
            return Ok();
        }
        [Authorize]
        [HttpPost]
        public IActionResult DeleteProtestoPlace(PostBaseModel model)
        {
            var protestoPlace = db.ProtestoPlace.Find(model.Id);
            db.ProtestoPlace.Remove(protestoPlace);
            db.SaveChanges();
            return Ok();
        }

    }

}