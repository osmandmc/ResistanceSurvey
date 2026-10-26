using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RS.COMMON.Entities;
using RS.MVC.Features.Accounts;

namespace RS.MVC.Controllers
{
    /// <summary>
    /// Cookie sign-in for the new app (/app), per-person accounts (F4a). The cookie is HttpOnly, so page scripts never
    /// see it. An invitation link (/app/welcome?token=…) lets a person set their own password.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController(SignInHandler signIn, GetInvitationHandler getInvitation, AcceptInvitationHandler acceptInvitation) : ControllerBase
    {
        public record LoginRequest(string Username, string Password);

        public record AcceptRequest(string Token, string Password);

        public record CurrentUser(int Id, string Username, string Role);

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<CurrentUser>> Login(LoginRequest request)
        {
            var result = await signIn.Handle(request.Username, request.Password);
            if (result.User == null)
                return Unauthorized(new { error = result.Error });
            return await SignInAsync(result.User);
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
                int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0,
                User.Identity?.Name,
                User.FindFirstValue(ClaimTypes.Role));

        /// <summary>GET /api/auth/invitation?token=… — whose link it is; 404 when it no longer works.</summary>
        [AllowAnonymous]
        [HttpGet("invitation")]
        public async Task<ActionResult<Invitation>> Invitation(string token)
        {
            var invitation = await getInvitation.Handle(token);
            return invitation == null ? NotFound(new { error = Accounts.LinkNotValid }) : invitation;
        }

        /// <summary>POST /api/auth/invitation — set the password through the link, then signed in.</summary>
        [AllowAnonymous]
        [HttpPost("invitation")]
        public async Task<ActionResult<CurrentUser>> Accept(AcceptRequest request)
        {
            var result = await acceptInvitation.Handle(request.Token, request.Password);
            if (result.User == null)
                return BadRequest(new { error = result.Error });
            return await SignInAsync(result.User);
        }

        private async Task<CurrentUser> SignInAsync(AppUser user)
        {
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, Accounts.Principal(user));
            return new CurrentUser(user.Id, user.Username, user.Role);
        }
    }
}
