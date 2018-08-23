using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using ResistanceSurvey.Models;
using RS.COMMON;

namespace ResistanceSurvey.Controllers
{
    public class ResistanceController : Controller
    {
        private readonly IResistanceApplication _rsApplication;
        public ResistanceController(IResistanceApplication rsApplication)
        {
            _rsApplication = rsApplication;
        }
        public IActionResult Index()
        {
            var resistances = _rsApplication.GetAll();
            return View(resistances);
        }
        [HttpGet]
        public IActionResult Create()
        {
            var foo = _rsApplication.GetCategories();
             
           
            return View();
        }
        [HttpPost]
        public IActionResult Create(ResistanceCreateModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Index));
            return View(model);
        }
    }
}