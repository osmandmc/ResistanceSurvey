using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RS.MVC.Features.Dashboard;

namespace RS.MVC.Controllers
{
    /// <summary>The new app's dashboard (/app/status): what the news pipeline received and did, per source and day.</summary>
    [ApiController]
    [Route("api/status")]
    [Authorize]
    public class StatusController(GetStatusHandler getStatus, ILogger<StatusController> logger) : ControllerBase
    {
        /// <summary>GET /api/status?days=14 (1–31).</summary>
        [HttpGet]
        public async Task<ActionResult<StatusPage>> Get(int? days)
        {
            if (days is < 1 or > GetStatusHandler.MaxDays)
                return BadRequest(new { error = $"'days' is a number from 1 to {GetStatusHandler.MaxDays}" });
            try
            {
                return await getStatus.Handle(days ?? GetStatusHandler.DefaultDays);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                // Same message as the inbox: the cause (status, or connection/timeout) on screen and in the log.
                var cause = ex switch
                {
                    HttpRequestException { StatusCode: { } status } => $"HTTP {(int)status}",
                    TaskCanceledException => "zaman aşımı",
                    _ => "bağlantı hatası",
                };
                logger.LogWarning(ex, "Pipeline unavailable for {Path}: {Cause}", Request.Path, cause);
                return StatusCode(StatusCodes.Status502BadGateway, new { error = $"Haber işleme hattına ulaşılamadı ({cause})." });
            }
        }
    }
}
