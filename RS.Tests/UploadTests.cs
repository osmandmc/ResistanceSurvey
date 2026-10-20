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

    private UploadNewsHandler Handler(string? cutover = null) =>
        new(_db, _pipeline, Options.Create(new PipelineSettings { CutoverMonth = cutover, Sources = new() { [1] = "evrensel" } }));

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

    private Task<UploadResult> Upload(MemoryStream file, string? cutover = null, string name = "ekim.xlsx", int source = 1) =>
        Handler(cutover).Handle(new UploadNewsCommand(file, name, file.Length, source, 2026, 10));

    [Fact]
    public async Task ImportsNewRowsReadingTextAndExcelDates()
    {
        var result = await Upload(Excel(
            ["https://evrensel.net/1", "03.10.2026", "Birinci haber", "metin"],
            ["https://evrensel.net/2", new DateTime(2026, 10, 4), "İkinci haber", null]));

        Assert.True(result.IsValid);
        Assert.Equal((false, 2, 2), (result.ToPipeline, result.Rows, result.Imported));
        Assert.Equal([new DateTime(2026, 10, 3), new DateTime(2026, 10, 4)], _db.News.OrderBy(n => n.Date).Select(n => n.Date));
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
        Assert.Empty(_db.News);
    }

    [Theory]
    [InlineData("ekim.csv", 1, "Dosya .xlsx olmalı.")]
    [InlineData("ekim.xlsx", 9, "Kaynak seçiniz.")]
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
    public async Task ReUploadingKeepsHandledRowsAndDoesNotDoubleThem()
    {
        var linked = new News { Source = 1, Link = "https://evrensel.net/linked", Header = "Bağlı", Date = new DateTime(2026, 10, 2), Status = Status.Linked };
        var dismissed = new News { Source = 1, Link = "https://evrensel.net/dismissed", Header = "İlgisiz", Date = new DateTime(2026, 10, 2), Status = Status.Passive };
        var pending = new News { Source = 1, Link = "https://evrensel.net/pending", Header = "Bekleyen", Date = new DateTime(2026, 10, 2), Status = Status.Pending };
        _db.News.AddRange(linked, dismissed, pending);
        _db.ResistanceNews.Add(new ResistanceNews { News = linked, ResistanceId = 1 });
        await _db.SaveChangesAsync();

        var result = await Upload(Excel(
            ["https://evrensel.net/linked", "02.10.2026", "Bağlı"],
            ["https://evrensel.net/dismissed", "02.10.2026", "İlgisiz"],
            ["https://evrensel.net/pending", "02.10.2026", "Bekleyen"],
            ["https://evrensel.net/new", "05.10.2026", "Yeni"]));

        Assert.Equal((2, 2), (result.Imported, result.Kept));
        Assert.Equal(4, _db.News.Count());
        Assert.Equal(Status.Passive, _db.News.Single(n => n.Link == "https://evrensel.net/dismissed").Status); // dismissal kept
        Assert.Single(_db.News, n => n.Link == "https://evrensel.net/linked");
    }

    [Fact]
    public async Task FromTheCutOverAPipelineSourceGoesToThePipeline()
    {
        _pipeline.KnownUrls.Add("https://evrensel.net/2");

        var result = await Upload(Excel(
            ["https://evrensel.net/1", "03.10.2026", "Birinci", "metin"],
            ["https://evrensel.net/2", "04.10.2026", "İkinci"],
            ["evrensel.net/3", "05.10.2026", "Bozuk bağlantı"]), cutover: "2026-10");

        Assert.Equal((true, 1, 1), (result.ToPipeline, result.Accepted, result.Duplicate));
        Assert.Equal(["evrensel.net/3: Not an absolute http(s) URL"], result.Rejected);
        Assert.Equal(new DateTimeOffset(2026, 10, 3, 0, 0, 0, TimeSpan.FromHours(3)), _pipeline.Submitted[0].PublishedAt);
        Assert.Empty(_db.News); // nothing stored in the app
    }

    [Fact]
    public void ASourceThePipelineDoesNotHandleStaysLocal()
    {
        var handler = Handler("2026-10");

        Assert.True(handler.GoesToPipeline(1, 2026, 10));
        Assert.False(handler.GoesToPipeline(2, 2026, 10)); // Kızılbayrak is not a pipeline source yet
        Assert.False(handler.GoesToPipeline(1, 2026, 9)); // before the cut-over
    }
}
