using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RS.COMMON.Constants;
using RS.MVC.Features.Accounts;

namespace RS.MVC.Controllers
{
    /// <summary>
    /// Tanımlar → Kullanıcılar (F4a), yönetici only. A new account or "Yeni bağlantı" answers with the link's token
    /// once; the page shows the whole link to copy and send to the person.
    /// </summary>
    [ApiController]
    [Route("api/users")]
    [Authorize(Roles = Role.Admin)]
    public class UsersController(ListUsersHandler list, CreateUserHandler create, NewLinkHandler newLink, ChangeUserHandler change) : ControllerBase
    {
        public record CreateRequest(string Username, string Role);

        public record ChangeRequest(string Role, bool? Disabled);

        [HttpGet]
        public async Task<IReadOnlyList<UserRow>> List() => await list.Handle();

        [HttpPost]
        public async Task<ActionResult<LinkResult>> Create(CreateRequest request) =>
            Answer(await create.Handle(new CreateUserCommand(request.Username, request.Role, User.Identity?.Name)));

        /// <summary>POST /api/users/{id}/link — "Yeni bağlantı"; the earlier link stops working.</summary>
        [HttpPost("{id:int}/link")]
        public async Task<ActionResult<LinkResult>> NewLink(int id) => Answer(await newLink.Handle(id));

        /// <summary>PATCH /api/users/{id} — { role } and/or { disabled }.</summary>
        [HttpPatch("{id:int}")]
        public async Task<ActionResult<LinkResult>> Change(int id, ChangeRequest request)
        {
            var actingUserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var acting) ? acting : 0;
            return Answer(await change.Handle(new ChangeUserCommand(id, request.Role, request.Disabled, actingUserId)));
        }

        private ActionResult<LinkResult> Answer(LinkResult result) =>
            result == null ? NotFound(new { error = "Kullanıcı bulunamadı." })
            : result.Error != null ? BadRequest(new { error = result.Error })
            : result;
    }
}
