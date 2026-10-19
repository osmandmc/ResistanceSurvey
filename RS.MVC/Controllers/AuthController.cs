using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RS.MVC.Applications;

namespace RS.MVC.Controllers
{
    /// <summary>
    /// Cookie sign-in for the new app (/app). The cookie is HttpOnly, so page scripts never see it.
    /// The existing Vue form keeps using POST /Home/Login and a bearer token.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IUserApplication users) : ControllerBase
    {
        public record LoginRequest(string Username, string Password);

        public record CurrentUser(string Username, string FirstName, string LastName, string Role);

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<CurrentUser>> Login(LoginRequest request)
        {
            var result = users.Authenticate(request.Username, request.Password);
            if (!result.Success)
            {
                return Unauthorized(new { error = "Kullanıcı adı veya şifre hatalı" });
            }

            var user = result.Response;
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Role, user.Role),
                new(ClaimTypes.GivenName, user.FirstName ?? ""),
                new(ClaimTypes.Surname, user.LastName ?? ""),
            };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return new CurrentUser(user.Username, user.FirstName, user.LastName, user.Role);
        }

        [AllowAnonymous]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return NoContent();
        }

        /// <summary>Who is signed in; 401 when nobody is. The new app calls this on start.</summary>
        [Authorize]
        [HttpGet("me")]
        public ActionResult<CurrentUser> Me() =>
            new CurrentUser(
                User.Identity?.Name,
                User.FindFirstValue(ClaimTypes.GivenName),
                User.FindFirstValue(ClaimTypes.Surname),
                User.FindFirstValue(ClaimTypes.Role));
    }
}
