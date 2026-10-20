using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RS.MVC.Features.Inbox;

namespace RS.MVC.Controllers
{
    /// <summary>The new app's inbox (/app/inbox): open articles from the news pipeline.</summary>
    [ApiController]
    [Route("api/inbox")]
    [Authorize]
    public class InboxController(GetInboxHandler getInbox, DismissArticleHandler dismissArticle) : ControllerBase
    {
        public record DismissRequest(string Reason);

        public record DismissResponse(bool OutcomeSent);

        /// <summary>GET /api/inbox?month=2026-11&amp;source=evrensel&amp;next=… (month defaults to the current one).</summary>
        [HttpGet]
        public async Task<ActionResult<InboxPage>> Get(string month, string source, string next)
        {
            try
            {
                return await getInbox.Handle(new GetInboxQuery(month, source, next));
            }
            catch (FormatException)
            {
                return BadRequest(new { error = "'month' is like 2026-11" });
            }
        }

        [HttpPost("{articleId}/dismiss")]
        public async Task<ActionResult<DismissResponse>> Dismiss(string articleId, DismissRequest request)
        {
            return await dismissArticle.Handle(new DismissArticleCommand(articleId, request?.Reason)) switch
            {
                DismissResult.Dismissed => new DismissResponse(OutcomeSent: true),
                DismissResult.DismissedOutcomePending => new DismissResponse(OutcomeSent: false),
                DismissResult.AlreadyHandled => Conflict(new { error = "Bu haber zaten işlendi." }),
                _ => NotFound(new { error = "Haber bulunamadı." }),
            };
        }
    }
}
