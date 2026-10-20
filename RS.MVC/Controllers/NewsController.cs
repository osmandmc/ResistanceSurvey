using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using RS.COMMON.DTO;
using RS.COMMON.Entities;
using RS.EF;
using RS.MVC.Applications;
using RS.MVC.Features.Upload;

namespace ResistanceSurvey.Controllers
{
       public class NewsController : Controller
    {
        private readonly INewsApplication _newsApplication;
        public NewsController(INewsApplication newsApplication)
        {
            _newsApplication = newsApplication;
        }

        public ActionResult Index()
        {
            return View();
        }
        
        [Authorize]
        public IActionResult Filter(int? year, int? month)
        {
             var yearVal = year ?? DateTime.Now.Year;
             var monthVal = month ?? DateTime.Now.Month;
             var news = _newsApplication.GetNewsByPeriod(yearVal, monthVal);
             return PartialView("_List", news);
        }
        [Authorize]
        [HttpPost]
        public ActionResult MarkAsResistanceNews(int newsId)
        {
            _newsApplication.MarkAsResistanceNews(newsId);
            return Json(true);

        }
        [Authorize]
        [HttpPost]
        public ActionResult MarkAsUnrelatedNews(int newsId)
        {
            _newsApplication.MarkAsUnrelatedNews(newsId);
            return Json(true);

        }
        [Authorize]
        public async Task<ActionResult> UploadFile(int source, int year, int month, [FromServices] UploadNewsHandler uploadNews)
        {
            var result = new Result();
            if (HttpContext.Request.Form.Files.Count > 0)
            {
                var file = HttpContext.Request.Form.Files[0];

                if (file != null && file.Length > 0 && file.Length < (4 * 1024 * 1024))
                {
                    var ext = Path.GetExtension(file.FileName);
                    if (ext != ".xlsx")
                    {
                            result.Success = false;
                            result.ErrorMessages.Add("Dosya formatı uyumsuz!");
                            return Json(ToJson(result));
                    }

                    using (var stream = file.OpenReadStream())
                    {
                        try{
                            
                            var upload = await uploadNews.Handle(new UploadNewsCommand(stream, file.FileName, file.Length, source));
                            if (!upload.IsValid)
                            {
                                result.Success = false;
                                result.ErrorMessages.Add(string.Join("\n", upload.Errors));
                                return Json(ToJson(result));
                            }
                            result.SuccessMessages.Add($"Haberler işleme hattına gönderildi: {upload.Accepted} yeni, {upload.Duplicate} zaten vardı, {upload.Rejected.Count} reddedildi.");
                        }
                        catch(HttpRequestException ex)
                        {
                            result.Success = false;
                            result.ErrorMessages.Add("Haber işleme hattına ulaşılamadı (" + ex.Message + "). Dosyayı tekrar yüklemek güvenlidir.");
                            return Json(ToJson(result));
                        }
                        catch(Exception ex)
                        {
                            result.Success = false;
                            result.ErrorMessages.Add(ex.Message);
                            return Json(ToJson(result));
                        }
                    }
                }
                else
                {
                    result.Success = false;
                    result.ErrorMessages.Add("Dosya boyutu belirtilen aralıkta değil.");
                    return Json(ToJson(result));
                }
            }
            return Json(ToJson(result));
        }

        // Result exposes public fields, which System.Text.Json skips; map them explicitly.
        private static object ToJson(Result result) => new
        {
            success = result.Success,
            successMessages = result.SuccessMessages,
            errorMessages = result.ErrorMessages
        };
        [Authorize]
        public ActionResult DownloadFile(int year, int month)
        {
            byte[] byteArray = _newsApplication.GetNewsFile(year, month);
            return File(byteArray, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", string.Format("{0}-Tarihli-Lisans-Raporu.xlsx", DateTime.Now.ToShortDateString()));

        }
        
    }
}