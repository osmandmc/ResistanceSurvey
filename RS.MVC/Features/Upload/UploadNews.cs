using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using RS.COMMON.Entities;
using RS.MVC.Applications;
using RS.MVC.Models;
using static RS.COMMON.Constants.Enums;

namespace RS.MVC.Features.Upload
{
    /// <summary>
    /// An Excel file of one source's news (columns: Link, Tarih, Başlık, Metin; first row is the heading). Every row
    /// goes to the news pipeline with its text and appears in the inbox under its own date (R12b; before, months
    /// before a cut-over were imported into the app's News table).
    /// </summary>
    public record UploadNewsCommand(Stream File, string FileName, long Length, int Source);

    /// <summary>Accepted / Duplicate / Rejected as the pipeline answered; Errors when the file was not sent at all.</summary>
    public record UploadResult(
        int Rows,
        int Accepted = 0,
        int Duplicate = 0,
        IReadOnlyList<string> Rejected = null,
        IReadOnlyList<string> Errors = null)
    {
        public bool IsValid => Errors is not { Count: > 0 };
    }

    public class UploadNewsHandler(IPipelineClient pipeline, IOptions<PipelineSettings> settings)
    {
        public const long MaxBytes = 4 * 1024 * 1024;
        public const int MaxErrorsShown = 10;

        /// <summary>App source numbers on the upload page (the old page offered these two).</summary>
        public static readonly IReadOnlyDictionary<int, string> Sources = new Dictionary<int, string> { [1] = "Evrensel", [2] = "Kızılbayrak" };

        private static readonly TimeSpan TurkeyOffset = TimeSpan.FromHours(3);
        private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");

        public async Task<UploadResult> Handle(UploadNewsCommand command)
        {
            var problems = new List<string>();
            if (!string.Equals(Path.GetExtension(command.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
                problems.Add("Dosya .xlsx olmalı.");
            if (command.Length is <= 0 or > MaxBytes)
                problems.Add("Dosya boş ya da 4 MB’tan büyük.");
            if (!Sources.ContainsKey(command.Source))
                problems.Add("Kaynak seçiniz.");
            else if (!settings.Value.UsesPipeline(command.Source))
                problems.Add("Bu kaynak işleme hattına bağlı değil.");
            if (problems.Count > 0)
                return new UploadResult(0, Errors: problems);

            var (rows, rowErrors) = ReadRows(command.File, command.Source);
            if (rowErrors.Count > 0)
                return new UploadResult(rows.Count, Errors: rowErrors);
            return await SubmitToPipelineAsync(rows);
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

        /// <summary>A news row as the pipeline takes it: its date is Turkey time, its text comes along so it is not fetched.</summary>
        public static PipelineArticle ToPipelineArticle(News n) =>
            new(n.Link, n.Header, new DateTimeOffset(DateTime.SpecifyKind(n.Date, DateTimeKind.Unspecified), TurkeyOffset), n.Content);

        private static DateTime? ReadDate(object value) => value switch
        {
            DateTime d => d,
            double serial => DateTime.FromOADate(serial),
            string s when DateTime.TryParse(s, Turkish, DateTimeStyles.None, out var d) => d,
            string s when DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) => d,
            _ => null,
        };

        // Nothing is stored in the app. Re-uploading is safe (known articles come back as Duplicate).
        private async Task<UploadResult> SubmitToPipelineAsync(List<News> rows)
        {
            var results = await pipeline.SubmitAsync(rows.Select(ToPipelineArticle).ToList(), "Csv");
            return new UploadResult(
                rows.Count,
                Accepted: results.Count(r => r.Status == "Accepted"),
                Duplicate: results.Count(r => r.Status == "Duplicate"),
                Rejected: results.Where(r => r.Status == "Rejected").Select(r => $"{r.Url}: {r.Reason}").ToList());
        }
    }
}
