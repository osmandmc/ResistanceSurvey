using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RS.MVC.Features.Upload;

namespace RS.MVC.Controllers
{
    /// <summary>The new app's Yükleme screen (/app/upload).</summary>
    [ApiController]
    [Route("api/upload")]
    [Authorize]
    public class UploadController(UploadNewsHandler uploadNews, SubmitLinksHandler submitLinks) : ControllerBase
    {
        public record SubmitLinksRequest(string Text);

        /// <summary>POST /api/upload/links — { text }: article links, one per line (or separated by spaces, commas, semicolons).</summary>
        [HttpPost("links")]
        public async Task<ActionResult<SubmitLinksResult>> Links(SubmitLinksRequest request)
        {
            try
            {
                var result = await submitLinks.Handle(new SubmitLinksCommand(request?.Text));
                return result.Error == null ? result : BadRequest(new { error = result.Error });
            }
            catch (HttpRequestException)
            {
                return StatusCode(StatusCodes.Status502BadGateway, new { error = "Haber işleme hattına ulaşılamadı. Bağlantıları tekrar göndermek güvenlidir." });
            }
        }

        /// <summary>GET /api/upload/target?source=1&amp;year=2026&amp;month=10 — where this month's rows will go.</summary>
        [HttpGet("target")]
        public object Target(int source, int year, int month) => new { toPipeline = uploadNews.GoesToPipeline(source, year, month) };

        /// <summary>POST /api/upload (form: file, source, year, month).</summary>
        [HttpPost]
        [RequestSizeLimit(UploadNewsHandler.MaxBytes + 64 * 1024)]
        public async Task<ActionResult<UploadResult>> Upload([FromForm] IFormFile file, [FromForm] int source, [FromForm] int year, [FromForm] int month)
        {
            if (file == null)
                return BadRequest(new { error = "Dosya seçiniz.", errors = new[] { "Dosya seçiniz." } });
            try
            {
                await using var stream = file.OpenReadStream();
                var result = await uploadNews.Handle(new UploadNewsCommand(stream, file.FileName, file.Length, source, year, month));
                return result.IsValid ? result : BadRequest(new { error = result.Errors.First(), errors = result.Errors });
            }
            catch (HttpRequestException)
            {
                return StatusCode(StatusCodes.Status502BadGateway,
                    new { error = "Haber işleme hattına ulaşılamadı. Dosyayı tekrar yüklemek güvenlidir." });
            }
        }
    }
}
