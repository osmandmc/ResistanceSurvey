using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using RS.COMMON.Constants;
using RS.COMMON.DTO;
using RS.COMMON.Entities;
using RS.MVC.Models;

namespace RS.MVC.Applications
{
    public interface IUserApplication
    {
        Result<UserModel> Authenticate(string username, string password);
    }

    /// <summary>
    /// Two fixed users whose passwords come from configuration (AppSettings:AdminPass / AppSettings:UserPass); the app
    /// signs them in with a cookie (AuthController). Per-person accounts replace this (F4).
    /// </summary>
    public class UserApplication : IUserApplication
    {
        private readonly List<User> _users;

        public UserApplication(IOptions<AppSettings> appSettings)
        {
            var settings = appSettings.Value;
            _users =
            [
                new User { Id = 1, FirstName = "Admin", LastName = "User", Username = "admin", Password = settings.AdminPass, Role = Role.Admin },
                new User { Id = 2, FirstName = "Normal", LastName = "User", Username = "user", Password = settings.UserPass, Role = Role.User },
            ];
        }

        public Result<UserModel> Authenticate(string username, string password)
        {
            var result = new Result<UserModel>();
            var user = _users.Where(x => x.Username == username && x.Password == password)
                .Select(u => new UserModel { FirstName = u.FirstName, LastName = u.LastName, Role = u.Role, Username = u.Username })
                .SingleOrDefault();
            if (user == null)
            {
                result.Success = false;
                result.ErrorMessages.Add("Kullanıcı adı veya şifre hatalı");
                return result;
            }
            result.Response = user;
            return result;
        }
    }
}
