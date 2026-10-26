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
        RS.MVC.Features.Review.ListsPublisher lists, AiReportHandler aiReport, CreatePromptVersionHandler createPrompt,
        TryPromptHandler tryPrompt, ActivatePromptHandler activatePrompt) : ControllerBase
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

        public record PromptVersionRequest(string Text, string Note);

        public record PromptActivateRequest(string Version);

        /// <summary>POST /api/status/prompts/{name}/versions — an edited prompt saved as a new version, not active yet (R28, yönetici).</summary>
        [HttpPost("prompts/{name}/versions")]
        [Authorize(Roles = RS.COMMON.Constants.Role.Admin)]
        public async Task<ActionResult<PromptVersionResult>> CreatePromptVersion(string name, PromptVersionRequest request)
        {
            if (!PromptNames.IsKnown(name))
                return NotFound(new { error = "Bilinmeyen istem." });
            var result = await createPrompt.Handle(new CreatePromptVersionCommand(name, request?.Text, request?.Note, User.Identity?.Name));
            return result.Error == null ? result : BadRequest(new { error = result.Error });
        }

        /// <summary>POST /api/status/prompts/{name}/versions/{version}/try — "Dene": the version on up to 10 decided articles (R28, yönetici).</summary>
        [HttpPost("prompts/{name}/versions/{version}/try")]
        [Authorize(Roles = RS.COMMON.Constants.Role.Admin)]
        public async Task<ActionResult<PromptTry>> TryPrompt(string name, string version)
        {
            if (!PromptNames.IsKnown(name))
                return NotFound(new { error = "Bilinmeyen istem." });
            return await tryPrompt.Handle(name, version, User.Identity?.Name) is { } result
                ? result
                : NotFound(new { error = "Bu sürüm işleme hattında yok." });
        }

        /// <summary>POST /api/status/prompts/{name}/active — { version } (or "builtin"): the pipeline runs it from now on (R28, yönetici).</summary>
        [HttpPost("prompts/{name}/active")]
        [Authorize(Roles = RS.COMMON.Constants.Role.Admin)]
        public async Task<IActionResult> ActivatePrompt(string name, PromptActivateRequest request)
        {
            if (!PromptNames.IsKnown(name) || string.IsNullOrWhiteSpace(request?.Version))
                return NotFound(new { error = "Bilinmeyen istem ya da sürüm." });
            return await activatePrompt.Handle(new ActivatePromptCommand(name, request.Version.Trim(), User.Identity?.Name))
                ? NoContent()
                : NotFound(new { error = "Bu sürüm işleme hattında yok." });
        }

        /// <summary>POST /api/status/lists — "Listeleri şimdi gönder": the app's list values to the pipeline now (R30).</summary>
        [HttpPost("lists")]
        public async Task<RS.MVC.Models.PipelineListsResponse> SendLists() => await lists.PublishAsync(force: true);

        /// <summary>
        /// GET /api/status/ai-report?days=90&amp;origin=…&amp;promptVersion=…&amp;relevanceVersion=… — "AI raporu" (R19d): how often
        /// researchers keep the AI's values, per field; the relevance verdicts against the decisions.
        /// </summary>
        [HttpGet("ai-report")]
        public async Task<ActionResult<AiReport>> AiReport(int? days, string origin, string promptVersion, string relevanceVersion)
        {
            if (days is < 1 or > AiReportHandler.MaxDays)
                return BadRequest(new { error = $"'days' is a number from 1 to {AiReportHandler.MaxDays}" });
            return await aiReport.Handle(new AiReportQuery(days ?? AiReportHandler.DefaultDays, origin, promptVersion, relevanceVersion));
        }
    }
}
