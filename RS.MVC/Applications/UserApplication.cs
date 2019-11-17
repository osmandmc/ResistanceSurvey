using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RS.COMMON.Constants;
using RS.COMMON.DTO;
using RS.COMMON.Entities;
using RS.MVC.Models;
using static RS.COMMON.Constants.Enums;

namespace RS.MVC.Applications
{
    public interface IUserApplication
    {
        Result<UserModel> Authenticate(string username, string password);
        IEnumerable<User> GetAll();
        User GetById(int id);
    }

    public class UserApplication : IUserApplication
    {
        // users hardcoded for simplicity, store in a db with hashed passwords in production applications
        private List<User> _users = new List<User>
        { 
            new User { Id = 1, FirstName = "Admin", LastName = "User", Username = "admin", Password = "admin", Role = Role.Admin },
            new User { Id = 2, FirstName = "Normal", LastName = "User", Username = "user", Password = "user", Role = Role.User } 
        };

        private readonly AppSettings _appSettings;

        public UserApplication(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public Result<UserModel> Authenticate(string username, string password)
        {
            var result = new Result<UserModel>();
            var user = _users.Where(x => x.Username == username && x.Password == password)
            .Select(u => new UserModel
            {
                FirstName = u.FirstName,
                LastName = u.LastName,
                Role = u.Role,
                Token = u.Token,
                Username = u.Username
            }).SingleOrDefault();

            // return null if user not found
            if (user == null)
            {
                result.Success = false;
                result.ErrorMessages.Add("Kullanıcı adı veya şifre hatalı");
            }

            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] 
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Token = tokenHandler.WriteToken(token);
            // remove password before returning
            result.Response = user;

            return result;
        }

        public IEnumerable<User> GetAll()
        {
            // return users without passwords
            return _users.Select(x => {
                x.Password = null;
                return x;
            });
        }

        public User GetById(int id) {
            var user = _users.FirstOrDefault(x => x.Id == id);

            // return user without password
            if (user != null) 
                user.Password = null;

            return user;
        }
    }

}