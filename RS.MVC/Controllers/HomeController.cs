using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RS.COMMON.DTO;
using RS.MVC.Applications;
using RS.MVC.Models;

namespace RS.MVC.Controllers
{
   
    public class HomeController : Controller
    {
        private IUserApplication _userApplication;
        public HomeController(IUserApplication userApplication)
        {
            _userApplication = userApplication;
        }
        
        [AllowAnonymous]
        
        public IActionResult Index()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            var result = _userApplication.Authenticate(model.UserName, model.Password);
            
            if (!result.Success)
            {
                return Json(result);
            }
            return Ok(result);
        }
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
