using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RS.COMMON.Constants;
using RS.MVC.Features.Activity;

namespace RS.MVC.Controllers
{
    /// <summary>The activity log (F4b): Kayıtlar for yöneticiler, a case's Geçmiş for everyone.</summary>
    [ApiController]
    [Route("api/activity")]
    [Authorize]
    public class ActivityController(ListActivityHandler list, CaseActivityHandler caseActivity) : ControllerBase
    {
        /// <summary>GET /api/activity?user=ayse&amp;area=definitions&amp;page=1</summary>
        [HttpGet]
        [Authorize(Roles = Role.Admin)]
        public async Task<ActionResult<ActivityPage>> List(string user, string area, int page = 1)
        {
            if (!ListActivityHandler.IsArea(area))
                return BadRequest(new { error = "'area' is inbox, cases, definitions, users or uploads" });
            return await list.Handle(new ListActivityQuery(user, area, page));
        }

        /// <summary>GET /api/activity/cases/{id}</summary>
        [HttpGet("cases/{id:int}")]
        public async Task<IReadOnlyList<ActivityRow>> Case(int id) => await caseActivity.Handle(id);
    }
}
