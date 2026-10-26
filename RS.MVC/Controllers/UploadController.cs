using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RS.EF;
using RS.MVC.Features.Activity;
using RS.MVC.Features.Upload;

namespace RS.MVC.Controllers
{
    /// <summary>The new app's Yükleme screen (/app/upload).</summary>
    [ApiController]
    [Route("api/upload")]
    [Authorize]
    public class UploadController(UploadNewsHandler uploadNews, SubmitLinksHandler submitLinks, SendLocalNewsToPipelineHandler localNews, RSDBContext db)
        : ControllerBase
    {
        public record SubmitLinksRequest(string Text);

        /// <summary>POST /api/upload/links — { text }: article links, one per line (or separated by spaces, commas, semicolons).</summary>
        [HttpPost("links")]
        public async Task<ActionResult<SubmitLinksResult>> Links(SubmitLinksRequest request)
        {
            try
            {
                var result = await submitLinks.Handle(new SubmitLinksCommand(request?.Text));
                if (result.Error != null)
                    return BadRequest(new { error = result.Error });
                var count = (string status) => result.Links.Count(l => l.Status == status);
                await LogAsync(Actions.UploadLinks,
                    $"{result.Links.Count} bağlantı: {count("Accepted")} yeni, {count("Duplicate")} zaten vardı, {count("Rejected") + count("Invalid")} alınmadı");
                return result;
            }
            catch (HttpRequestException)
            {
                return StatusCode(StatusCodes.Status502BadGateway, new { error = "Haber işleme hattına ulaşılamadı. Bağlantıları tekrar göndermek güvenlidir." });
            }
        }

        /// <summary>POST /api/upload (form: file, source).</summary>
        [HttpPost]
        [RequestSizeLimit(UploadNewsHandler.MaxBytes + 64 * 1024)]
        public async Task<ActionResult<UploadResult>> Upload([FromForm] IFormFile file, [FromForm] int source)
        {
            if (file == null)
                return BadRequest(new { error = "Dosya seçiniz.", errors = new[] { "Dosya seçiniz." } });
            try
            {
                await using var stream = file.OpenReadStream();
                var result = await uploadNews.Handle(new UploadNewsCommand(stream, file.FileName, file.Length, source));
                if (!result.IsValid)
                    return BadRequest(new { error = result.Errors.First(), errors = result.Errors });
                await LogAsync(Actions.UploadExcel,
                    $"{file.FileName}: {result.Rows} satır, {result.Accepted} yeni, {result.Duplicate} zaten vardı, {result.Rejected?.Count ?? 0} alınmadı");
                return result;
            }
            catch (HttpRequestException)
            {
                return StatusCode(StatusCodes.Status502BadGateway,
                    new { error = "Haber işleme hattına ulaşılamadı. Dosyayı tekrar yüklemek güvenlidir." });
            }
        }
    
        /// <summary>
        /// GET /api/upload/local-news — counts only. POST /api/upload/local-news?skip=0 — sends the next batch (R12b).
        /// One-off for the move to the redesign (R14): the old, never triaged news of the database that stays go to the
        /// pipeline. Not for the test copy: triage done there would not be in the real database.
        /// </summary>
        [HttpGet("local-news")]
        public async Task<LocalNewsTransfer> LocalNews() => await localNews.Handle(dryRun: true);

        [HttpPost("local-news")]
        public async Task<LocalNewsTransfer> SendLocalNews(int skip = 0)
        {
            var result = await localNews.Handle(dryRun: false, skip);
            await LogAsync(Actions.UploadLocalNews, $"{result.Sent} eski haber gönderildi: {result.Accepted} yeni, {result.Duplicate} zaten vardı");
            return result;
        }

        // Uploads change nothing in the app's database (the news go to the pipeline); the log still says who sent them.
        private async Task LogAsync(string action, string summary)
        {
            db.LogActivity(action, summary);
            await db.SaveChangesAsync();
        }
    }
}
