using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RS.MVC.Features.Dashboard;

namespace RS.MVC.Controllers
{
    /// <summary>The new app's dashboard (/app/status): what the news pipeline received and did, per source and day.</summary>
    [ApiController]
    [Route("api/status")]
    [Authorize]
    public class StatusController(GetStatusHandler getStatus, CollectFeedsHandler collectFeeds, QueueForAiHandler queueForAi) : ControllerBase
    {
        /// <summary>GET /api/status?days=14 (1–31).</summary>
        [HttpGet]
        public async Task<ActionResult<StatusPage>> Get(int? days)
        {
            if (days is < 1 or > GetStatusHandler.MaxDays)
                return BadRequest(new { error = $"'days' is a number from 1 to {GetStatusHandler.MaxDays}" });
            return await getStatus.Handle(days ?? GetStatusHandler.DefaultDays);
        }
    
        /// <summary>POST /api/status/collect — "Şimdi topla": the pipeline reads the feeds now; one result per feed.</summary>
        [HttpPost("collect")]
        public async Task<IReadOnlyList<CollectedFeed>> Collect() => await collectFeeds.Handle();

        /// <summary>POST /api/status/ai?source=evrensel — "AI ile işle": queue the source's articles the AI has not handled.</summary>
        [HttpPost("ai")]
        public async Task<ActionResult<QueuedForAi>> QueueForAi(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
                return BadRequest(new { error = "'source' is required" });
            return await queueForAi.Handle(source);
        }
    }
}
