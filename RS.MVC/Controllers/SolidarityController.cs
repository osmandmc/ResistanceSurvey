using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RS.MVC.Features.Cases;

namespace RS.MVC.Controllers
{
    /// <summary>R31c: Dayanışma cases and the struggles they supported.</summary>
    [ApiController]
    [Route("api/cases")]
    [Authorize]
    public class SolidarityController(
        ListSolidarityLinksHandler list,
        ResolveSolidarityHandler resolve,
        ResolveTargetCheckHandler targetCheck,
        CaseSolidarityHandler caseSolidarity,
        LinkSupportedCaseHandler linkSupported) : ControllerBase
    {
        public record SupportedCaseRequest(int SupportedCaseId);

        /// <summary>GET /api/cases/solidarity-links — the old Dayanışma groups still to link, and the İşletme cases to check.</summary>
        [HttpGet("solidarity-links")]
        public async Task<SolidarityLinksPage> Links() => await list.Handle();

        /// <summary>POST /api/cases/solidarity-links/resolve — { caseIds, action: Link|Organizer|Workplace|None, supportedCaseId }.</summary>
        [HttpPost("solidarity-links/resolve")]
        public async Task<IActionResult> Resolve(ResolveSolidarityCommand command) =>
            await resolve.Handle(command) is { } error ? BadRequest(new { error }) : NoContent();

        /// <summary>POST /api/cases/{id}/target-checked — an İşletme case's old target institution was checked.</summary>
        [HttpPost("{id:int}/target-checked")]
        public async Task<IActionResult> TargetChecked(int id) =>
            await targetCheck.Handle(id) ? NoContent() : NotFound(new { error = "Kontrol edilecek bir şey yok." });

        /// <summary>GET /api/cases/{id}/solidarity — Dayanışma cases linked to this case, and unlinked ones naming its workplace.</summary>
        [HttpGet("{id:int}/solidarity")]
        public async Task<CaseSolidarity> Solidarity(int id) => await caseSolidarity.Handle(id);

        /// <summary>POST /api/cases/{id}/supported-case — { supportedCaseId }: the Dayanışma case {id} supported that case.</summary>
        [HttpPost("{id:int}/supported-case")]
        public async Task<IActionResult> LinkSupported(int id, SupportedCaseRequest request) =>
            await linkSupported.Handle(new LinkSupportedCaseCommand(id, request.SupportedCaseId)) is { } error ? BadRequest(new { error }) : NoContent();
    }
}
