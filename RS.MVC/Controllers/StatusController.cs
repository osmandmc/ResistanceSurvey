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
    public class StatusController(GetStatusHandler getStatus, CollectFeedsHandler collectFeeds, QueueForAiHandler queueForAi, GetAiPromptsHandler getPrompts,
        RS.MVC.Features.Review.ListsPublisher lists) : ControllerBase
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

        /// <summary>
        /// POST /api/status/ai?source=evrensel — "AI ile işle": queue the source's articles the AI has not handled;
        /// &amp;again=true: those judged by an older relevance prompt, to be judged with the current one.
        /// </summary>
        [HttpPost("ai")]
        public async Task<ActionResult<QueuedForAi>> QueueForAi(string source, bool again = false)
        {
            if (string.IsNullOrWhiteSpace(source))
                return BadRequest(new { error = "'source' is required" });
            return await queueForAi.Handle(source, again);
        }

        /// <summary>GET /api/status/prompts — "AI istemleri": the prompts the pipeline's AI stages run now.</summary>
        [HttpGet("prompts")]
        public async Task<AiPrompts> Prompts() => await getPrompts.Handle();

        /// <summary>POST /api/status/lists — "Listeleri şimdi gönder": the app's list values to the pipeline now (R30).</summary>
        [HttpPost("lists")]
        public async Task<RS.MVC.Models.PipelineListsResponse> SendLists() => await lists.PublishAsync(force: true);
    }
}
