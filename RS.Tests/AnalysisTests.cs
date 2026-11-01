using Microsoft.EntityFrameworkCore;
using RS.COMMON.Constants;
using RS.COMMON.Entities;
using RS.COMMON.Entities.LookupEntity;
using RS.EF;
using RS.MVC.Features.Analysis;
using RS.MVC.Features.Cases;
using static RS.COMMON.Constants.Enums;

namespace RS.Tests;

/// <summary>R38: the numbers behind the Analiz charts.</summary>
public class AnalysisTests
{
    private readonly RSDBContext _db = new(new DbContextOptionsBuilder<RSDBContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static readonly City Kocaeli = new() { Id = 41, Name = "Kocaeli" };
    private readonly Confederation _disk = new() { Name = "DİSK", CorporationTypeId = KnownListIds.ConfederationCorporationType };

    public AnalysisTests()
    {
        _db.Category.AddRange(
            new Category { Id = KnownListIds.WorkplaceCategory, Name = "İşletme" },
            new Category { Id = KnownListIds.SolidarityCategory, Name = "Dayanışma" },
            new Category { Id = KnownListIds.GeneralCategory, Name = "Genel" });
        _db.ResistanceReason.AddRange(Enumerable.Range(1, 10).Select(i => new ResistanceReason { Id = i, Name = $"Neden {i}" }));
        _db.ProtestoType.AddRange(new ProtestoType { Id = 1, Name = "Basın açıklaması" }, new ProtestoType { Id = 2, Name = "Fiili grev" });
        _db.EmploymentType.AddRange(new EmploymentType { Id = 1, Name = "Memur" }, new EmploymentType { Id = 3, Name = "Özel Kadrolu (Mavi Yakalı)" });
        _db.Corporation.Add(_disk);
        _db.SaveChanges();
    }

    private Resistance Add(int categoryId, params DateTime[] protests) => Add(categoryId, protests, null);

    private Resistance Add(int categoryId, DateTime[] protests, Action<Resistance>? setUp)
    {
        var r = new Resistance
        {
            Name = $"Vaka {_db.Resistance.Count() + 1}",
            CategoryId = categoryId,
            Protestos = protests.Select(d => new Protesto
            {
                StartDate = d,
                EndDate = d,
                Locations = [new ProtestoLocation { City = Kocaeli, CityId = Kocaeli.Id }],
                ProtestoProtestoTypes = [new ProtestoProtestoType { ProtestoTypeId = 1 }],
            }).ToList(),
            ResistanceCorporations = [],
            ResistanceEmploymentTypes = [],
            ResistanceResistanceReasons = [],
        };
        setUp?.Invoke(r);
        _db.Resistance.Add(r);
        _db.SaveChanges();
        return r;
    }

    private Task<CaseAnalysis> Analyse(CaseFilter? filter = null) => new CaseAnalysisHandler(_db).Handle(new CaseAnalysisQuery(filter ?? new CaseFilter()));

    private static DateTime D(int year, int month, int day = 1) => new(year, month, day);

    [Fact]
    public async Task ProtestsPerMonthByCategoryAndTheCasesActiveEachMonth()
    {
        Add(KnownListIds.WorkplaceCategory, D(2026, 1, 5), D(2026, 1, 20), D(2026, 3, 2));
        Add(KnownListIds.GeneralCategory, D(2026, 1, 1));

        var analysis = await Analyse();

        Assert.Equal(["2026-01", "2026-02", "2026-03"], analysis.Months);
        Assert.Equal(["İşletme", "Genel", "Dayanışma"], analysis.ProtestsByCategory.Select(s => s.Name));
        Assert.Equal([2, 0, 1], analysis.ProtestsByCategory[0].Values);
        Assert.Equal([1, 0, 0], analysis.ProtestsByCategory[1].Values);
        Assert.Equal([2, 0, 1], analysis.ActiveCases);
        Assert.Equal(new AnalysisTotals(2, 4, 1, 0), analysis.Totals);
    }

    [Fact]
    public async Task AFewEarlyEntriesDoNotStretchTheAxes()
    {
        Add(KnownListIds.WorkplaceCategory, D(2015, 6));
        Add(KnownListIds.WorkplaceCategory, Enumerable.Range(1, CaseAnalysisHandler.MinProtestsForFirstYear).Select(d => D(2025, 2, d)).ToArray());

        var analysis = await Analyse();

        Assert.Equal([2025], analysis.Years);
        Assert.Equal(["2025-02"], analysis.Months);
        Assert.Equal(1, analysis.Totals.Cases);
    }

    [Fact]
    public async Task TheYearFilterCountsOnlyThatYearsProtests()
    {
        Add(KnownListIds.WorkplaceCategory, D(2024, 12, 30), D(2025, 1, 3));
        Add(KnownListIds.WorkplaceCategory, D(2024, 5, 1));

        var analysis = await Analyse(new CaseFilter(Year: 2025));

        Assert.Equal(["2025-01"], analysis.Months);
        Assert.Equal((1, 1), (analysis.Totals.Cases, analysis.Totals.Protests));
    }

    [Fact]
    public async Task ReasonsBeyondTheTopAreSummedAsOther()
    {
        // Reason 1 on three cases, reasons 2–10 on one each: the top eight and “Diğer” for the last two.
        for (var i = 0; i < 3; i++)
            Add(KnownListIds.WorkplaceCategory, [D(2026, 2)], r => r.ResistanceResistanceReasons.Add(new ResistanceResistanceReason { ResistanceReasonId = 1 }));
        Add(KnownListIds.WorkplaceCategory, [D(2026, 3)], r => r.ResistanceResistanceReasons.AddRange(
            Enumerable.Range(2, 9).Select(i => new ResistanceResistanceReason { ResistanceReasonId = i })));

        var reasons = (await Analyse()).ReasonsByYear;

        Assert.Equal(CaseAnalysisHandler.TopN + 1, reasons.Count);
        Assert.Equal(("Neden 1", 3), (reasons[0].Name, reasons[0].Values.Single()));
        Assert.Equal((CaseAnalysisHandler.Other, 2), (reasons[^1].Name, reasons[^1].Values.Single()));
    }

    [Fact]
    public async Task ConfederationsAndResultsByUnion()
    {
        var union = new TradeUnion { Name = "Birleşik Metal-İş", CorporationTypeId = KnownListIds.TradeUnionCorporationType, Confederation = _disk };
        var independent = new TradeUnion { Name = "Bağımsız Sendika", CorporationTypeId = KnownListIds.TradeUnionCorporationType };
        var platform = new Corporation { Name = "Bir Platform", CorporationTypeId = 4 };
        Add(KnownListIds.WorkplaceCategory, [D(2026, 1)], r =>
        {
            r.ResistanceCorporations.Add(new ResistanceCorporation { Corporation = union, Role = OrganizationRole.Organizing });
            r.ResistanceResult = ResistanceResult.FullGain;
        });
        Add(KnownListIds.WorkplaceCategory, [D(2026, 1)], r => r.ResistanceCorporations.Add(new ResistanceCorporation { Corporation = independent, Role = OrganizationRole.Supporting }));
        Add(KnownListIds.WorkplaceCategory, [D(2026, 1)], r =>
        {
            r.ResistanceCorporations.Add(new ResistanceCorporation { Corporation = platform, Role = OrganizationRole.Organizing });
            r.ResistanceResult = ResistanceResult.ZeroGain;
        });

        var analysis = await Analyse();

        Assert.Equal(
            [("DİSK", 1), (CaseAnalysisHandler.UnionWithoutConfederation, 1), (CaseAnalysisHandler.NoUnion, 1)],
            analysis.Confederations.Select(c => (c.Name, c.Count)).OrderBy(c => c.Name, StringComparer.Ordinal));
        Assert.Equal(
            [new AnalysisResults("Sendika kaydı var", 1, 0, 0, 1), new AnalysisResults(CaseAnalysisHandler.NoUnion, 0, 0, 1, 0)],
            analysis.ResultsByUnion);
    }

    [Fact]
    public async Task HowLongCasesLastedAndWhatIsMissing()
    {
        Add(KnownListIds.WorkplaceCategory, D(2026, 1, 1));
        Add(KnownListIds.WorkplaceCategory, D(2026, 1, 1), D(2026, 1, 5));
        Add(KnownListIds.WorkplaceCategory, D(2026, 1, 1), D(2026, 6, 1));
        var noPlace = Add(KnownListIds.GeneralCategory, D(2026, 2, 1));
        noPlace.Protestos[0].Locations.Clear();
        _db.SaveChanges();

        var analysis = await Analyse();

        Assert.Equal([2, 1, 0, 0, 1, 0], analysis.Durations.Select(d => d.Count));
        Assert.Equal(new AnalysisCompleteness(2026, 4, 4, 4, 6, 1), analysis.Completeness.Single());
    }

    [Fact]
    public async Task ProvincesCountEachProtestInEachOfItsProvincesAndEachCaseOnce()
    {
        var bursa = new City { Id = 16, Name = "Bursa" };
        Add(KnownListIds.WorkplaceCategory, D(2026, 1, 1), D(2026, 1, 2)); // two protests in Kocaeli
        Add(KnownListIds.GeneralCategory, [D(2026, 5, 1)], r => r.Protestos[0].Locations.Add(new ProtestoLocation { City = bursa, CityId = 16 }));

        var provinces = (await Analyse()).Provinces;

        Assert.Equal([new AnalysisProvince(41, "Kocaeli", 3, 2), new AnalysisProvince(16, "Bursa", 1, 1)], provinces);
    }

    [Fact]
    public void TheQueriesTranslateToSqlServer()
    {
        using var sqlServer = new RSDBContext(new DbContextOptionsBuilder<RSDBContext>().UseSqlServer("Server=none;Database=none").Options);
        var cases = new CaseFilter(CategoryId: 1, Year: 2025).Apply(sqlServer.Resistance);

        Assert.Contains("[ResistanceCorporation]", CaseAnalysisHandler.Cases(cases).ToQueryString());
        Assert.Contains("[ConfederationId]", CaseAnalysisHandler.Unions(sqlServer).ToQueryString());
        Assert.Contains("[ProtestoLocation]", CaseAnalysisHandler.Protests(cases, new CaseFilter(Year: 2025)).ToQueryString());
    }
}
