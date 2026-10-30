using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using RS.COMMON.Entities;
using RS.COMMON.Entities.LookupEntity;
using RS.EF;
using RS.MVC.Features.Cases;

namespace RS.Tests;

public class CaseListTests
{
    private readonly RSDBContext _db = new(new DbContextOptionsBuilder<RSDBContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static readonly City Bursa = new() { Id = 16, Name = "Bursa" };

    // Every case has a category (a required relationship), as in the real database.
    public CaseListTests()
    {
        _db.Category.AddRange(new Category { Id = 1, Name = "Ücret" }, new Category { Id = 2, Name = "İşten çıkarma" });
        _db.SaveChanges();
    }

    private Resistance Add(string name, DateTime lastProtest, DateTime? end = null, Company? company = null, Company? main = null,
        int categoryId = 1, string? note = null, string? description = null, City? city = null, int articles = 0, Corporation? organizer = null)
    {
        var r = new Resistance
        {
            Name = name,
            Description = description,
            Company = company,
            MainCompany = main,
            ResistanceCorporations = organizer == null ? [] : [new ResistanceCorporation { Corporation = organizer, Role = OrganizationRole.Organizing }],
            CategoryId = categoryId,
            Note = note,
            Protestos = [new Protesto { StartDate = lastProtest, EndDate = end, Locations = city == null ? [] : [new ProtestoLocation { City = city, CityId = city.Id }] }],
            ResistanceNews = Enumerable.Range(0, articles).Select(i => new ResistanceNews { News = new News { Header = $"Haber {i}", Link = $"https://x/{i}" } }).ToList(),
        };
        _db.Resistance.Add(r);
        _db.SaveChanges();
        return r;
    }

    private Task<CaseListPage> List(CaseFilter filter, int page = 1, int pageSize = 25) =>
        new ListCasesHandler(_db).Handle(new ListCasesQuery(filter, page, pageSize));

    [Fact]
    public async Task MostRecentProtestFirstWithCountsAndCities()
    {
        Add("Eski", new DateTime(2026, 1, 5));
        Add("Yeni", new DateTime(2026, 10, 1), company: new Company { Name = "Örnek Tekstil" }, city: Bursa, articles: 2);

        var page = await List(new CaseFilter());

        Assert.Equal(["Yeni", "Eski"], page.Items.Select(i => i.Name));
        var item = page.Items[0];
        Assert.Equal(("Örnek Tekstil", 1, 2, true), (item.Target, item.ProtestCount, item.ArticleCount, item.Ongoing));
        Assert.Equal(["Bursa"], item.Cities);
    }

    [Fact]
    public async Task ACaseWithoutAWorkplaceShowsItsOrganizers()
    {
        // R31: Genel and Dayanışma cases have no workplace; the list shows who organized them, and the search finds them.
        Add("1 Mayıs çağrısı", new DateTime(2026, 4, 28), organizer: new Corporation { Name = "DİSK Genel-İş" });

        Assert.Equal("DİSK Genel-İş", (await List(new CaseFilter())).Items.Single().Target);
        Assert.Single((await List(new CaseFilter(Text: "Genel-İş"))).Items);
    }

    [Fact]
    public async Task PagesAndTotal()
    {
        for (var i = 1; i <= 7; i++) Add($"Vaka {i}", new DateTime(2026, 1, i));

        var second = await List(new CaseFilter(), page: 2, pageSize: 3);

        Assert.Equal((7, 2, 3), (second.Total, second.Page, second.PageSize));
        Assert.Equal(["Vaka 4", "Vaka 3", "Vaka 2"], second.Items.Select(i => i.Name));
    }

    [Fact]
    public async Task YearWorksWithoutAMonthUnlikeTheOldList()
    {
        Add("2025", new DateTime(2025, 6, 1));
        Add("2026 Mart", new DateTime(2026, 3, 10));
        Add("2026 Ekim", new DateTime(2026, 10, 10));

        Assert.Equal(["2026 Ekim", "2026 Mart"], (await List(new CaseFilter(Year: 2026))).Items.Select(i => i.Name));
        Assert.Equal(["2026 Mart"], (await List(new CaseFilter(Year: 2026, Month: 3))).Items.Select(i => i.Name));
    }

    [Fact]
    public async Task CompanyMainCompanyCategoryNotesCityAndOngoing()
    {
        var holding = new Company { Name = "Örnek Holding" };
        Add("Taşeron", new DateTime(2026, 9, 1), new DateTime(2026, 9, 2), company: new Company { Name = "Temiz A.Ş." }, main: holding, categoryId: 2, note: "not var", city: Bursa);
        Add("Süren", new DateTime(2026, 8, 1), description: "vardiya düzeni");

        Assert.Equal(["Taşeron"], (await List(new CaseFilter(MainCompanyId: holding.Id))).Items.Select(i => i.Name));
        Assert.Equal(["Taşeron"], (await List(new CaseFilter(CategoryId: 2))).Items.Select(i => i.Name));
        Assert.Equal(["Taşeron"], (await List(new CaseFilter(HasNote: true))).Items.Select(i => i.Name));
        Assert.Equal(["Süren"], (await List(new CaseFilter(HasNote: false))).Items.Select(i => i.Name));
        Assert.Equal(["Taşeron"], (await List(new CaseFilter(CityId: 16))).Items.Select(i => i.Name));
        Assert.Equal(["Süren"], (await List(new CaseFilter(OngoingOnly: true))).Items.Select(i => i.Name));
        Assert.Equal(["Süren"], (await List(new CaseFilter(Text: "vardiya"))).Items.Select(i => i.Name));
    }

    [Fact]
    public void ListAndExportQueriesTranslateToSqlServer()
    {
        using var sqlServer = new RSDBContext(new DbContextOptionsBuilder<RSDBContext>().UseSqlServer("Server=none;Database=none").Options);
        var filter = new CaseFilter("tekstil", 1, 2, 3, 2026, 10, 16, true, true);

        var listSql = ListCasesHandler.Build(filter.Apply(sqlServer.Resistance)).Skip(25).Take(25).ToQueryString();
        var exportSql = new ExportCasesHandler(sqlServer).Protests(filter).ToQueryString();

        Assert.Contains("ORDER BY", listSql);
        Assert.Contains("ORDER BY", exportSql);
    }

    [Fact]
    public void TheExportKeepsTheOldColumnsInOrder()
    {
        string[] expected =
        [
            "Direnişin ID", "Açıklama", "Kategori Adı", "Şirket Adı", "Şirket İş Kolu", "Ana Şirket Adı", "Ana Şirket İş Kolu",
            "Üretim Karşıtı", "Kalkınma Hakkı", "Çalışan Sayısı", "Çalışan Sayısı (Sayısal)", "Sendika Var mı", "Sendika Otoritesi",
            "Direniş Nedenleri", "Kuruluşlar", "İstihdam Türleri", "Tepki Gösteren Sendika", "Cinsiyet", "İşten Çıkarılan Çalışan Sayısı",
            "Hukuki Müdahale", "Hukuki Müdahale Açıklaması", "Protesto ID", "Protesto Sayısı", "Protesto Türleri", "Protesto Yerleri",
            "Başlangıç Tarihi", "Bitiş Tarihi", "Grev Süresi", "Konumlar", "Protestoda Çalışan Sayısı", "Protestoda Çalışan Sayısı (Sayısal)",
            "Müdahale Türleri", "Gözaltı Sayısı", "Not", "Protesto Notu", "Direniş Sonucu", "Direniş Haberleri", "Direniş Haberleri Linki",
            "Örgütleyen Kurumlar", "Desteklenen Vaka", // R31, at the end
        ];

        Assert.Equal(expected, ExportCasesHandler.Columns.Select(c => c.Header));
    }

    [Fact]
    public void TheExcelFileHasOneRowPerProtest()
    {
        var resistance = new Resistance
        {
            Id = 412,
            Name = "Tekstil",
            Company = new Company { Name = "Örnek Tekstil" },
            ResistanceCorporations = [new ResistanceCorporation { Corporation = new TradeUnion { Name = "Birleşik Metal-İş", CorporationTypeId = 1 } }],
            ResistanceResistanceReasons = [new ResistanceResistanceReason { ResistanceReason = new ResistanceReason { Name = "Ücret alacağı" } }],
            ResistanceEmploymentTypes = [],
            ResistanceNews = [],
        };
        Protesto Protest(int id) => new()
        {
            Id = id,
            Resistance = resistance,
            StartDate = new DateTime(2026, 10, id),
            ProtestoProtestoTypes = [],
            ProtestoProtestoPlaces = [],
            ProtestoInterventionTypes = [],
            Locations = [new ProtestoLocation { City = Bursa, District = new District { Name = "Nilüfer" }, Place = "fabrika önü" }],
        };

        var bytes = ExportCasesHandler.ToExcel([Protest(1), Protest(2)]);

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage(new MemoryStream(bytes));
        var sheet = package.Workbook.Worksheets["Report"];
        Assert.Equal(3, sheet.Dimension.Rows); // heading + two protests
        Assert.Equal("Örnek Tekstil", sheet.Cells[2, 4].Text);
        Assert.Equal("EVET", sheet.Cells[2, 12].Text); // union among the corporations
        Assert.Equal("Bursa / Nilüfer fabrika önü", sheet.Cells[3, 29].Text);
    }
}
