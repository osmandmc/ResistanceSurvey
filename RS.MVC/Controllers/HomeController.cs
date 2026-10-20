using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RS.MVC.Infrastructure;
using RS.MVC.Models;

namespace RS.MVC.Controllers
{
    /// <summary>The site root leads to the app (/app); the error page catches what the API handler does not (F6).</summary>
    [AllowAnonymous]
    public class HomeController : Controller
    {
        public IActionResult Index() => Redirect("/app/");

        public IActionResult Error() => View(new ErrorViewModel { RequestId = ApiExceptionHandler.ErrorCode(HttpContext) });
    }
}
