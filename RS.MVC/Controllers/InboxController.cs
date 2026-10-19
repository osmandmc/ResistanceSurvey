using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RS.MVC.Applications;

namespace RS.MVC.Controllers
{
    /// <summary>The new app's inbox (/app/inbox): open articles from the news pipeline.</summary>
    [ApiController]
    [Route("api/inbox")]
    [Authorize]
    public class InboxController(IInboxApplication inbox) : ControllerBase
    {
        public record DismissRequest(string Reason);

        public record DismissResponse(bool OutcomeSent);

        /// <summary>GET /api/inbox?month=2026-11&amp;source=evrensel&amp;next=… (month defaults to the current one).</summary>
        [HttpGet]
        public async Task<ActionResult<InboxPage>> Get(string month, string source, string next)
        {
            try
            {
                return await inbox.GetPageAsync(month, source, next);
            }
            catch (FormatException)
            {
                return BadRequest(new { error = "'month' is like 2026-11" });
            }
            catch (HttpRequestException)
            {
                return StatusCode(StatusCodes.Status502BadGateway, new { error = "Haber işleme hattına ulaşılamadı." });
            }
        }

        [HttpPost("{articleId}/dismiss")]
        public async Task<ActionResult<DismissResponse>> Dismiss(string articleId, DismissRequest request)
        {
            try
            {
                return await inbox.DismissAsync(articleId, request?.Reason) switch
                {
                    DismissResult.Dismissed => new DismissResponse(OutcomeSent: true),
                    DismissResult.DismissedOutcomePending => new DismissResponse(OutcomeSent: false),
                    DismissResult.AlreadyHandled => Conflict(new { error = "Bu haber zaten işlendi." }),
                    _ => NotFound(new { error = "Haber bulunamadı." }),
                };
            }
            catch (HttpRequestException)
            {
                return StatusCode(StatusCodes.Status502BadGateway, new { error = "Haber işleme hattına ulaşılamadı." });
            }
        }
    }
}
