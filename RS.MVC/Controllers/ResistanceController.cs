using Microsoft.AspNetCore.Mvc;
using RS.MVC.Controllers;

namespace ResistanceSurvey.Controllers
{
    public class ResistanceController : BaseController
    {
        public IActionResult Index()
        {
            return RedirectToAction(nameof(IndexVue));
        }

        public IActionResult IndexVue()
        {
            return View();
        }
    }
}
