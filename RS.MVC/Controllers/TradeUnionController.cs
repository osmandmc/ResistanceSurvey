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
    public class TradeUnionController : Controller
    {
        private readonly RSDBContext db;

        public TradeUnionController(RSDBContext db)
        {
            this.db = db;
        }
        public IActionResult Index()
        {
            ViewBag.TradeUnions = new SelectList(db.TradeUnion.Select(s => new LookupEntity { Id = s.Id, Name = s.Name }).ToList(), "Id", "Name"); ;
            return View();
        }
        [Authorize]
        public IActionResult _List(FilterModel filter)
        {
            var tradeUnions = db.TradeUnion.ToPagedFilteredResult(filter);
            return PartialView(tradeUnions);
        }
        //[Authorize]
        //[HttpPost]
        //public IActionResult Save(TradeUnion model)
        //{
        //    var tradeUnion = db.TradeUnion.Find(model.Id);
        //    tradeUnion.Name = model.Name;
        //    db.Entry(tradeUnion).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        //    db.SaveChanges();
        //    return Ok();
        //}
        //[Authorize]
        //[HttpPost]
        //public IActionResult SetApproval(TradeUnion model)
        //{
        //    var tradeUnion = db.TradeUnion.Find(model.Id);
        //    tradeUnion.Approved = model.Approved;
        //    db.Entry(tradeUnion).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        //    db.SaveChanges();
        //    return Ok();
        //}
        [Authorize]
        public IActionResult CheckTradeUnion(int id)
        {
            var used = db.Resistance.Any(s => !s.Deleted && s.TradeUnionId == id);
            return Json(used);
        }
        [Authorize]
        [HttpPost]
        public IActionResult ReplaceTradeUnion(ReplaceModel model)
        {
            var usedResistances = db.Resistance.Where(s=>s.TradeUnionId == model.Id).ToList();
            foreach (var item in usedResistances)
            {
                item.TradeUnionId = model.NewId;
                db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            }
            var tradeUnion = db.TradeUnion.Find(model.Id);
            db.TradeUnion.Remove(tradeUnion);
            db.SaveChanges();
            return Ok();
        }
        [Authorize]
        [HttpPost]
        public IActionResult DeleteTradeUnion(PostBaseModel model)
        {
            var tradeUnion = db.TradeUnion.Find(model.Id);
            db.TradeUnion.Remove(tradeUnion);
            db.SaveChanges();
            return Ok();
        }

    }

}