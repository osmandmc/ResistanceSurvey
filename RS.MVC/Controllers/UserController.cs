using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RS.COMMON.DTO;
using RS.COMMON.Entities;
using RS.MVC.Applications;

namespace RS.MVC.Controllers
{
     [Authorize]
    public class UserController: Controller
    {
        private IUserApplication _userApplication;
        public UserController(IUserApplication userApplication)
        {
            _userApplication = userApplication;
        }
    }
}