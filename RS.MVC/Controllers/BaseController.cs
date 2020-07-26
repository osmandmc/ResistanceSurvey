using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RS.MVC.Controllers
{
    public class BaseController : Controller
    {
        public string UserName => HttpContext.User.Identity.Name;
    }
}