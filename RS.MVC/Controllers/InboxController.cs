using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RS.COMMON.Entities;
using RS.MVC.Features.Inbox;
using RS.MVC.Models;

namespace RS.MVC.Controllers
{
    /// <summary>The new app's inbox (/app/inbox): open articles from the news pipeline.</summary>
    [ApiController]
    [Route("api/inbox")]
    [Authorize]
    public class InboxController(GetInboxHandler getInbox, DismissArticleHandler dismissArticle) : ControllerBase
    {
        public record DismissRequest(string Reason, DismissKind? Kind = null, AiUse Ai = null);

        public record DismissResponse(bool OutcomeSent);

        /// <summary>
        /// GET /api/inbox?month=2026-11&amp;source=evrensel&amp;relevance=other&amp;next=… (month defaults to the current one;
        /// relevance: "other" or "notRelevant", the AI's rejections; leave out for all).
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<InboxPage>> Get(string month, string source, string next, string relevance)
        {
            if (relevance is not (null or "" or "other" or "notRelevant"))
                return BadRequest(new { error = "'relevance' is 'other' or 'notRelevant'" });
            try
            {
                return await getInbox.Handle(new GetInboxQuery(month, source, next, string.IsNullOrEmpty(relevance) ? null : relevance));
            }
            catch (FormatException)
            {
                return BadRequest(new { error = "'month' is like 2026-11" });
            }
        }

        [HttpPost("{articleId}/dismiss")]
        public async Task<ActionResult<DismissResponse>> Dismiss(string articleId, DismissRequest request)
        {
            return await dismissArticle.Handle(new DismissArticleCommand(articleId, request?.Reason, request?.Kind, request?.Ai, User.Identity?.Name)) switch
            {
                DismissResult.Dismissed => new DismissResponse(OutcomeSent: true),
                DismissResult.DismissedOutcomePending => new DismissResponse(OutcomeSent: false),
                DismissResult.AlreadyHandled => Conflict(new { error = "Bu haber zaten işlendi." }),
                _ => NotFound(new { error = "Haber bulunamadı." }),
            };
        }
    }
}
