using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RS.MVC.Features.Archive;

namespace RS.MVC.Controllers
{
    /// <summary>The new app's article archive (/app/archive): every article from the news pipeline, with its history.</summary>
    [ApiController]
    [Route("api/archive")]
    [Authorize]
    public class ArchiveController(GetArchiveHandler getArchive) : ControllerBase
    {
        /// <summary>GET /api/archive?month=2026-11&amp;source=evrensel&amp;q=işçi&amp;filter=failed&amp;next=… (all optional).</summary>
        [HttpGet]
        public async Task<ActionResult<ArchivePage>> Get(string month, string source, string q, string filter, string next)
        {
            var parsedFilter = ArchiveFilter.All;
            if (!string.IsNullOrEmpty(filter) && !Enum.TryParse(filter, ignoreCase: true, out parsedFilter))
                return BadRequest(new { error = "'filter' is one of: all, open, handled, failed" });
            try
            {
                return await getArchive.Handle(new GetArchiveQuery(month, source, q, parsedFilter, next));
            }
            catch (FormatException)
            {
                return BadRequest(new { error = "'month' is like 2026-11" });
            }
            catch (ArgumentException)
            {
                return BadRequest(new { error = $"Arama en az {GetArchiveHandler.MinSearchLength} karakter olmalı." });
            }
        }
    }
}
