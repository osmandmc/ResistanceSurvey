using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using RS.COMMON.DTO;
using RS.COMMON.Entities;
using RS.EF;
using RS.MVC.Models;

namespace RS.MVC.Applications
{
    public interface INewsApplication
    {
        IEnumerable<NewsListModel> GetNewsByPeriod(int year, int month);
        /// <summary>Imports an uploaded Excel file and returns a summary message for the user.</summary>
        Task<string> ReadNewsAsync(Stream stream, int source, int year, int month);
        void MarkAsResistanceNews(int newsId);
        void MarkAsUnrelatedNews(int newsId);
        byte[] GetNewsFile(int year, int month);
    }
    public class NewsApplication : INewsApplication
    {
        public readonly RSDBContext _db;
        private readonly IPipelineClient _pipeline;
        private readonly PipelineSettings _pipelineSettings;
        public NewsApplication(RSDBContext db, IPipelineClient pipeline, IOptions<PipelineSettings> pipelineSettings)
        {
            _db = db;
            _pipeline = pipeline;
            _pipelineSettings = pipelineSettings.Value;
        }
        public IEnumerable<NewsListModel> GetNewsByPeriod(int year, int month)
        {
            return _db.News
            .Where(n => n.Date.Year == year && n.Date.Month == month)
            .OrderBy(o => o.Date)
            .Select(n =>
            new NewsListModel
            {
                Id = n.Id,
                Title = n.Header,
                Link = n.Link,
                Status = n.Status,
                Date = n.Date
            })
            .ToList();
        }
        public void MarkAsResistanceNews(int newsId)
        {
            var news = _db.News.Where(n => n.Id == newsId).FirstOrDefault();
            news.Status = COMMON.Constants.Enums.Status.Linked;
            _db.Entry(news).State = EntityState.Modified;
            _db.SaveChanges();
        }
        public void MarkAsUnrelatedNews(int newsId)
        {
            var news = _db.News.Where(n => n.Id == newsId).FirstOrDefault();
            news.Status = COMMON.Constants.Enums.Status.Passive;
            _db.Entry(news).State = EntityState.Modified;
            _db.SaveChanges();
        }

        public async Task<string> ReadNewsAsync(Stream stream, int source, int year, int month)
        {
            var rows = ReadRows(stream, source);
            if (_pipelineSettings.UsesPipeline(source, new DateTime(year, month, 1)))
                return await SubmitToPipelineAsync(rows);

            ImportLocally(rows, source, year, month);
            return "Haberler başarıyla yüklendi.";
        }

        private static List<News> ReadRows(Stream stream, int source)
        {
            var rows = new List<News>();
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension?.Rows;

                for (int row = 2; row <= rowCount.Value; row++)
                {
                    var news = new News();
                    news.Source = source;
                    news.Link = worksheet.Cells[row, 1].Value.ToString();
                    news.Date = DateTime.Parse(worksheet.Cells[row, 2].Value.ToString());
                    news.Header = worksheet.Cells[row, 3].Value.ToString();
                    news.Content = worksheet.Cells[row, 4].Value?.ToString();
                    rows.Add(news);
                }
            }
            return rows;
        }

        // Before the cut-over (or for sources the pipeline doesn't handle): replace the month's unlinked news, as before.
        private void ImportLocally(List<News> rows, int source, int year, int month)
        {
            var dateIntervalStart = new DateTime(year, month, 1);
            var dateIntervalEnd = new DateTime(year, month, DateTime.DaysInMonth(dateIntervalStart.Year, dateIntervalStart.Month));

            var existingNews = _db.News.Where(s => s.Date >= dateIntervalStart && s.Date <= dateIntervalEnd && s.Source == source).ToList();
            if (existingNews.Count != 0)
            {
                var attachedResistanceNewsIds = _db.ResistanceNews.Where(s => existingNews.Select(d => d.Id).Contains(s.NewsId)).Select(s => s.NewsId).ToList();
                var removableNews = existingNews.Where(s => !attachedResistanceNewsIds.Contains(s.Id)).ToList();
                _db.News.RemoveRange(removableNews);
            }
            _db.News.AddRange(rows);
            _db.SaveChanges();
        }

        // From the cut-over: send to the pipeline; nothing is stored locally. Re-uploading is safe (known articles come back as Duplicate).
        private async Task<string> SubmitToPipelineAsync(List<News> rows)
        {
            var turkeyOffset = TimeSpan.FromHours(3);
            var articles = rows
                .Select(n => new PipelineArticle(
                    n.Link,
                    n.Header,
                    new DateTimeOffset(DateTime.SpecifyKind(n.Date, DateTimeKind.Unspecified), turkeyOffset),
                    n.Content))
                .ToList();

            var results = await _pipeline.SubmitAsync(articles, "Csv");

            var accepted = results.Count(r => r.Status == "Accepted");
            var duplicate = results.Count(r => r.Status == "Duplicate");
            var rejected = results.Where(r => r.Status == "Rejected").ToList();

            var message = $"Haberler işleme hattına gönderildi: {accepted} yeni, {duplicate} zaten vardı, {rejected.Count} reddedildi.";
            foreach (var r in rejected.Take(5))
                message += $"\n- {r.Url}: {r.Reason}";
            if (rejected.Count > 5)
                message += $"\n… ve {rejected.Count - 5} tane daha.";
            return message;
        }
        public byte[] GetNewsFile(int year, int month)
        {
            var news = _db.News
            .Where(n => n.Date.Year == year && n.Date.Month == month)
            .Select(n =>
            new
            {
                Link = n.Link,
                Title = n.Header,
                Status = n.Status,
            })
            .ToList();
            byte[] byteArray;
            using (ExcelPackage pck = new ExcelPackage())
            {
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Haberler");
                ws.Cells["A1"].Value = "Link";
                ws.Cells["B1"].Value = "Başlık";
                ws.Cells["C1"].Value = "Durum";

                ws.Row(1).Style.Font.Bold = true;
                ws.Row(1).Style.Numberformat.Format = "General";

                ws.Cells["A2"].LoadFromCollection(news);

                ws.Cells.AutoFitColumns();
                byteArray = pck.GetAsByteArray();
            }
            return byteArray;
        }
    }
}