using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using RS.COMMON.Entities;
using RS.EF;
using RS.MVC.Features.Upload;
using RS.MVC.Models;
using static RS.COMMON.Constants.Enums;

namespace RS.Tests;

public class UploadTests
{
    private readonly RSDBContext _db = new(new DbContextOptionsBuilder<RSDBContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private readonly FakePipeline _pipeline = new();

    private static readonly IOptions<PipelineSettings> Settings = Options.Create(new PipelineSettings { Sources = new() { [1] = "evrensel" } });

    private UploadNewsHandler Handler() => new(_pipeline, Settings);

    /// <summary>An .xlsx like the researchers' files: heading row, then Link, Tarih, Başlık, Metin.</summary>
    private static MemoryStream Excel(params object?[][] rows)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add("Haberler");
        sheet.Cells[1, 1].Value = "Link"; sheet.Cells[1, 2].Value = "Tarih"; sheet.Cells[1, 3].Value = "Başlık"; sheet.Cells[1, 4].Value = "Metin";
        for (var r = 0; r < rows.Length; r++)
            for (var c = 0; c < rows[r].Length; c++)
                sheet.Cells[r + 2, c + 1].Value = rows[r][c];
        return new MemoryStream(package.GetAsByteArray());
    }

    private Task<UploadResult> Upload(MemoryStream file, string name = "ekim.xlsx", int source = 1) =>
        Handler().Handle(new UploadNewsCommand(file, name, file.Length, source));

    [Fact]
    public async Task RowsGoToThePipelineWithTheirTextAndTurkishDates()
    {
        _pipeline.KnownUrls.Add("https://evrensel.net/2");

        var result = await Upload(Excel(
            ["https://evrensel.net/1", "03.10.2026", "Birinci", "metin"],
            ["https://evrensel.net/2", new DateTime(2026, 10, 4), "İkinci"],
            ["evrensel.net/3", "05.10.2026", "Bozuk bağlantı"]));

        Assert.True(result.IsValid);
        Assert.Equal((3, 1, 1), (result.Rows, result.Accepted, result.Duplicate));
        Assert.Equal(["evrensel.net/3: Not an absolute http(s) URL"], result.Rejected);
        Assert.Equal(new DateTimeOffset(2026, 10, 3, 0, 0, 0, TimeSpan.FromHours(3)), _pipeline.Submitted[0].PublishedAt);
        Assert.Equal("metin", _pipeline.Submitted[0].Content);
        Assert.Empty(_db.News); // nothing stored in the app
    }

    [Fact]
    public async Task ProblemsAreListedByRowAndNothingIsSaved()
    {
        var result = await Upload(Excel(
            ["https://evrensel.net/1", "03.10.2026", "Doğru satır"],
            ["", "dün", "Bağlantısız"],
            ["https://evrensel.net/3", "04.10.2026", ""]));

        Assert.False(result.IsValid);
        Assert.Equal(["Satır 3: bağlantı (A sütunu) boş.", "Satır 3: tarih (B sütunu) okunamadı.", "Satır 4: başlık (C sütunu) boş."], result.Errors);
        Assert.Empty(_pipeline.Submitted);
    }

    [Theory]
    [InlineData("ekim.csv", 1, "Dosya .xlsx olmalı.")]
    [InlineData("ekim.xlsx", 9, "Kaynak seçiniz.")]
    [InlineData("ekim.xlsx", 2, "Bu kaynak işleme hattına bağlı değil.")] // a known source missing from Pipeline:Sources
    public async Task WrongFileOrSourceIsRejected(string name, int source, string error)
    {
        var result = await Upload(Excel(["https://evrensel.net/1", "03.10.2026", "Haber"]), name: name, source: source);

        Assert.Contains(error, result.Errors!);
    }

    [Fact]
    public async Task ANotAnExcelFileIsRejectedClearly()
    {
        var file = new MemoryStream("Link;Tarih;Başlık"u8.ToArray());

        var result = await Upload(file);

        Assert.Equal(["Dosya okunamadı; geçerli bir Excel (.xlsx) dosyası mı?"], result.Errors);
    }

    [Fact]
    public async Task OldNeverTriagedNewsGoesToThePipelineInRuns()
    {
        News Local(int day, Status status = Status.Pending, int source = 1, string? articleId = null) => new()
        {
            Source = source, Link = $"https://evrensel.net/haber/{day}-{status}-{source}", Header = $"Haber {day}", Content = $"Metin {day}",
            Date = new DateTime(2024, 12, day), Status = status, ArticleId = articleId,
        };
        _db.News.AddRange(Local(1), Local(2), Local(3), Local(4, Status.Linked), Local(5, Status.Passive), Local(6, source: 7), Local(7, articleId: "abc"));
        await _db.SaveChangesAsync();
        var handler = new SendLocalNewsToPipelineHandler(_db, _pipeline, Settings);

        var preview = await handler.Handle(dryRun: true);
        Assert.Equal((3, true, 0), (preview.Total, preview.DryRun, preview.Sent));
        Assert.Equal(new LocalNewsYear(2024, 4, 3), Assert.Single(preview.Years)); // the unknown source is pending but not sendable; the one with an ArticleId is neither
        Assert.Empty(_pipeline.Submitted);

        var first = await handler.Handle(dryRun: false, skip: 0, limit: 2);
        var second = await handler.Handle(dryRun: false, skip: first.NextSkip!.Value, limit: 2);

        Assert.Equal((2, 2), (first.Sent, first.NextSkip!.Value));
        Assert.Equal((1, (int?)null), (second.Sent, second.NextSkip));
        Assert.Equal(["Haber 3", "Haber 2", "Haber 1"], _pipeline.Submitted.Select(a => a.Title)); // newest first
        Assert.Equal("Metin 3", _pipeline.Submitted[0].Content);
        Assert.All(_db.News, n => Assert.True(n.Status != Status.Pending || n.ArticleId == null || n.ArticleId == "abc")); // rows untouched
    }
}
