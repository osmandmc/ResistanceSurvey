using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using RS.COMMON.Entities;
using RS.EF;
using RS.MVC.Applications;
using RS.MVC.Models;
using static RS.COMMON.Constants.Enums;

namespace RS.MVC.Features.Upload
{
    /// <summary>
    /// An Excel file of one source's news for one month (columns: Link, Tarih, Başlık, Metin; first row is the
    /// heading). From the cut-over month on, a pipeline source's rows go to the news pipeline; otherwise they are
    /// imported into the app's News table as before.
    /// </summary>
    public record UploadNewsCommand(Stream File, string FileName, long Length, int Source, int Year, int Month);

    /// <summary>
    /// Pipeline: Accepted / Duplicate / Rejected from the pipeline. Local: Imported new rows; Kept rows already
    /// handled (linked or dismissed) that the upload left alone and did not import again.
    /// </summary>
    public record UploadResult(
        bool ToPipeline,
        int Rows,
        int Imported = 0,
        int Kept = 0,
        int Accepted = 0,
        int Duplicate = 0,
        IReadOnlyList<string> Rejected = null,
        IReadOnlyList<string> Errors = null)
    {
        public bool IsValid => Errors is not { Count: > 0 };
    }

    public class UploadNewsHandler(RSDBContext db, IPipelineClient pipeline, IOptions<PipelineSettings> settings)
    {
        public const long MaxBytes = 4 * 1024 * 1024;
        public const int MaxErrorsShown = 10;

        /// <summary>App source numbers on the upload page (the old page offered these two).</summary>
        public static readonly IReadOnlyDictionary<int, string> Sources = new Dictionary<int, string> { [1] = "Evrensel", [2] = "Kızılbayrak" };

        private static readonly TimeSpan TurkeyOffset = TimeSpan.FromHours(3);
        private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");

        /// <summary>True when this source's rows for this month go to the pipeline.</summary>
        public bool GoesToPipeline(int source, int year, int month) =>
            year is >= 2000 and <= 2100 && month is >= 1 and <= 12 && settings.Value.UsesPipeline(source, new DateTime(year, month, 1));

        public async Task<UploadResult> Handle(UploadNewsCommand command)
        {
            var problems = new List<string>();
            if (!string.Equals(Path.GetExtension(command.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
                problems.Add("Dosya .xlsx olmalı.");
            if (command.Length is <= 0 or > MaxBytes)
                problems.Add("Dosya boş ya da 4 MB’tan büyük.");
            if (!Sources.ContainsKey(command.Source))
                problems.Add("Kaynak seçiniz.");
            if (command.Year is < 2000 or > 2100 || command.Month is < 1 or > 12)
                problems.Add("Yıl ve ay seçiniz.");
            if (problems.Count > 0)
                return new UploadResult(false, 0, Errors: problems);

            var (rows, rowErrors) = ReadRows(command.File, command.Source);
            if (rowErrors.Count > 0)
                return new UploadResult(false, rows.Count, Errors: rowErrors);

            return GoesToPipeline(command.Source, command.Year, command.Month)
                ? await SubmitToPipelineAsync(rows)
                : await ImportLocallyAsync(rows, command.Source, command.Year, command.Month);
        }

        /// <summary>Every row is checked first; any problem stops the upload, listed by row number.</summary>
        public static (List<News> Rows, List<string> Errors) ReadRows(Stream file, int source)
        {
            var rows = new List<News>();
            var errors = new List<string>();
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var package = new ExcelPackage(file);
                var sheet = package.Workbook.Worksheets.FirstOrDefault();
                var lastRow = sheet?.Dimension?.End.Row ?? 0;
                if (lastRow < 2)
                    return (rows, ["Dosyada haber satırı yok (ilk satır başlık olmalı)."]);

                for (var row = 2; row <= lastRow; row++)
                {
                    var link = sheet.Cells[row, 1].Text?.Trim();
                    var title = sheet.Cells[row, 3].Text?.Trim();
                    if (string.IsNullOrEmpty(link) && string.IsNullOrEmpty(title) && string.IsNullOrEmpty(sheet.Cells[row, 2].Text))
                        continue; // empty row
                    if (string.IsNullOrEmpty(link))
                        errors.Add($"Satır {row}: bağlantı (A sütunu) boş.");
                    if (string.IsNullOrEmpty(title))
                        errors.Add($"Satır {row}: başlık (C sütunu) boş.");
                    var date = ReadDate(sheet.Cells[row, 2].Value);
                    if (date == null)
                        errors.Add($"Satır {row}: tarih (B sütunu) okunamadı.");
                    if (errors.Count == 0)
                    {
                        rows.Add(new News
                        {
                            Source = source,
                            Link = link,
                            Date = date!.Value,
                            Header = title,
                            Content = sheet.Cells[row, 4].Text,
                            Status = Status.Pending,
                        });
                    }
                }
            }
            catch (Exception ex) when (ex is InvalidDataException or InvalidOperationException or IOException)
            {
                return (rows, ["Dosya okunamadı; geçerli bir Excel (.xlsx) dosyası mı?"]);
            }

            if (errors.Count > MaxErrorsShown)
                errors = [.. errors.Take(MaxErrorsShown), $"… ve {errors.Count - MaxErrorsShown} sorun daha."];
            return (rows, errors);
        }

        private static DateTime? ReadDate(object value) => value switch
        {
            DateTime d => d,
            double serial => DateTime.FromOADate(serial),
            string s when DateTime.TryParse(s, Turkish, DateTimeStyles.None, out var d) => d,
            string s when DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) => d,
            _ => null,
        };

        // Rows not yet handled are replaced (as before). Handled rows - linked to a case or dismissed - are kept, and
        // the file's copies of them are not imported again (before, a re-upload reset dismissals and doubled links).
        private async Task<UploadResult> ImportLocallyAsync(List<News> rows, int source, int year, int month)
        {
            var from = new DateTime(year, month, 1);
            var to = from.AddMonths(1);
            var existing = await db.News.Where(n => n.Source == source && n.Date >= from && n.Date < to).ToListAsync();
            var existingIds = existing.Select(n => n.Id).ToList();
            var linkedIds = (await db.ResistanceNews.Where(rn => existingIds.Contains(rn.NewsId)).Select(rn => rn.NewsId).ToListAsync()).ToHashSet();

            var kept = existing.Where(n => linkedIds.Contains(n.Id) || n.Status != Status.Pending || n.ArticleId != null).ToList();
            db.News.RemoveRange(existing.Except(kept));

            var keptLinks = kept.Select(n => n.Link).ToHashSet();
            var fresh = rows.Where(r => !keptLinks.Contains(r.Link)).GroupBy(r => r.Link).Select(g => g.First()).ToList();
            db.News.AddRange(fresh);
            await db.SaveChangesAsync();
            return new UploadResult(false, rows.Count, Imported: fresh.Count, Kept: rows.Count - fresh.Count);
        }

        // From the cut-over: to the pipeline; nothing is stored in the app. Re-uploading is safe (known articles come back as Duplicate).
        private async Task<UploadResult> SubmitToPipelineAsync(List<News> rows)
        {
            var articles = rows
                .Select(n => new PipelineArticle(n.Link, n.Header, new DateTimeOffset(DateTime.SpecifyKind(n.Date, DateTimeKind.Unspecified), TurkeyOffset), n.Content))
                .ToList();
            var results = await pipeline.SubmitAsync(articles, "Csv");
            return new UploadResult(
                true,
                rows.Count,
                Accepted: results.Count(r => r.Status == "Accepted"),
                Duplicate: results.Count(r => r.Status == "Duplicate"),
                Rejected: results.Where(r => r.Status == "Rejected").Select(r => $"{r.Url}: {r.Reason}").ToList());
        }
    }
}
