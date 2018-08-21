using Microsoft.AspNetCore.Mvc;
using ResistanceSurvey.Models;

namespace ResistanceSurvey.Controllers
{
    public class ResistanceController : Controller
    {
        public IActionResult Index()
        {

            return View();
        }
        [HttpGet]
        public IActionResult Create()
        {
             
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