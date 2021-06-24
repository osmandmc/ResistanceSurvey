using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using RS.COMMON.DTO;
using RS.COMMON.Entities;
using RS.EF;

namespace RS.MVC.Applications
{
    public interface INewsApplication
    {
        IEnumerable<NewsListModel> GetNewsByPeriod(int year, int month);
        void ReadNews(Stream stream, int year, int month);
        void MarkAsResistanceNews(int newsId);
        void MarkAsUnrelatedNews(int newsId);
        byte[] GetNewsFile(int year, int month);
    }
    public class NewsApplication : INewsApplication
    {
        public readonly RSDBContext _db;
        public NewsApplication(RSDBContext db)
        {
            _db = db;
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

        public void ReadNews(Stream stream, int year, int month)
        {
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension?.Rows;
                var colCount = worksheet.Dimension?.Columns;

                if (!rowCount.HasValue || !colCount.HasValue)
                {

                }

                var dateIntervalStart = new DateTime(year, month, 1);
                var dateIntervalEnd = new DateTime(year, month, DateTime.DaysInMonth(dateIntervalStart.Year, dateIntervalStart.Month));

                var existingNews = _db.News.Where(s => s.Date >= dateIntervalStart && s.Date <= dateIntervalEnd).ToList();
                if (existingNews.Count != 0)
                {
                    var attachedResistanceNewsIds = _db.ResistanceNews.Where(s => existingNews.Select(d => d.Id).ToList().Contains(s.NewsId)).Select(s => s.NewsId).ToList();
                    var removableNews = existingNews.Where(s => !attachedResistanceNewsIds.Contains(s.Id)).ToList();
                    _db.News.RemoveRange(removableNews);
                }
                for (int row = 2; row <= rowCount.Value; row++)
                {
                    var news = new News();
                    news.Link = worksheet.Cells[row, 1].Value.ToString();
                    news.Date = DateTime.Parse(worksheet.Cells[row, 2].Value.ToString());
                    news.Header = worksheet.Cells[row, 3].Value.ToString();
                    news.Content = worksheet.Cells[row, 4].Value.ToString();
                    _db.News.Add(news);
                }
                _db.SaveChanges();
            }
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