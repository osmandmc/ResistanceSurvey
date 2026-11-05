using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RS.COMMON.Entities;
using RS.COMMON.Entities.LookupEntity;
using RS.EF;
using RS.MVC.Features.Activity;
using RS.MVC.Features.Cases;
using RS.MVC.Features.History;
using RS.MVC.Features.Review;
using RS.MVC.Models;
using static RS.COMMON.Constants.Enums;

namespace RS.Tests;

/// <summary>R39: every changed field is recorded with its old and new value, who and how, and shown in the case's Geçmiş.</summary>
public class ChangeHistoryTests
{
    private readonly TestUser _user = new();
    private readonly RSDBContext _db;
    private readonly Resistance _case;
    private readonly Protesto _protest;

    public ChangeHistoryTests()
    {
        _db = new(_user.Track(new DbContextOptionsBuilder<RSDBContext>())
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options);
        _db.City.Add(new City { Id = 16, Name = "Bursa" });
        _db.District.Add(new District { Id = 160, Name = "Nilüfer" });
        _db.ResistanceReason.AddRange(new ResistanceReason { Id = 3, Name = "Ücret alacağı" }, new ResistanceReason { Id = 7, Name = "İşten çıkarma" });
        _db.ProtestoType.AddRange(new ProtestoType { Id = 1, Name = "İş bırakma" }, new ProtestoType { Id = 2, Name = "Basın açıklaması" });
        _db.Company.AddRange(new Company { Id = 10, Name = "Proda Mühendislik" }, new Company { Id = 11, Name = "Ahen Maden" });
        _protest = new Protesto
        {
            StartDate = new DateTime(2026, 10, 12), EndDate = new DateTime(2026, 10, 12), GenderId = 1, SimpleProtestoDescription = "Vardiya sonunda 15 dakika iş bırakma",
            ProtestoProtestoTypes = [new ProtestoProtestoType { ProtestoTypeId = 1 }, new ProtestoProtestoType { ProtestoTypeId = 2 }],
            ProtestoProtestoPlaces = [new ProtestoProtestoPlace { ProtestoPlaceId = 6 }],
            ProtestoInterventionTypes = [new ProtestoInterventionType { InterventionTypeId = ReviewValidation.NoInterventionTypeId }],
            Locations = [new ProtestoLocation { CityId = 16, DistrictId = 160, Place = "Fabrika önü" }],
        };
        // The case as the complete form saves it, so saving that form again changes nothing.
        _case = new Resistance
        {
            Name = "Örnek Tekstil ücret direnişi", Description = "120 işçi ödenmeyen ücretler için iş bıraktı.", CategoryId = 1, CompanyId = 10,
            EmployerRole = EmployerRole.Direct, EmployeeCountNumber = 120, FiredEmployeeCountByProtesto = 0, StartDate = new DateTime(2026, 10, 12), ResistanceResult = ResistanceResult.Unknown,
            ResistanceResistanceReasons = [new ResistanceResistanceReason { ResistanceReasonId = 3 }],
            ResistanceCorporations = [new ResistanceCorporation { CorporationId = 4 }],
            ResistanceEmploymentTypes = [new ResistanceEmploymentType { EmploymentTypeId = 5 }],
            Protestos = [_protest],
        };
        _db.Resistance.Add(_case);
        _db.SaveChanges();
    }

    private static CaseInput Case(Action<CaseInput>? change = null)
    {
        var c = ReviewValidationTests.CompleteNewCase().Case!;
        change?.Invoke(c);
        return c;
    }

    private Task<CaseCommandResult> UpdateCase(CaseInput c) =>
        new UpdateCaseHandler(_db, new CaseWriter(_db), new ListValueResolver(_db)).Handle(new UpdateCaseCommand(_case.Id, c, "ayse"));

    [Fact]
    public void ANewCaseWritesNoChangesItsValuesAreTheFirstOnes() => Assert.Empty(_db.FieldChange);

    [Fact]
    public async Task SavingTheSameFormAgainRecordsNothing()
    {
        _user.Name = "ayse";
        await UpdateCase(Case());

        // The lists were removed and added again with the same values.
        Assert.Empty(_db.FieldChange);
    }

    [Fact]
    public async Task EditingACaseRecordsEachChangedFieldWithItsOldAndNewValue()
    {
        _user.Name = "ayse";
        await UpdateCase(Case(c => { c.CompanyId = 11; c.EmployeeCount = 88; c.ReasonIds = [3, 7]; }));

        var line = _db.ActivityEntry.Single(a => a.Action == Actions.CaseUpdate);
        Assert.Equal(
            [
                ("CompanyId", "10", "11"),
                ("EmployeeCountNumber", "120", "88"),
                ("Reasons", null, "7"),
            ],
            _db.FieldChange.OrderBy(c => c.Field).Select(c => new ValueTuple<string, string?, string?>(c.Field, c.OldValue, c.NewValue)));
        Assert.All(_db.FieldChange, c =>
        {
            Assert.Equal(("Resistance", _case.Id, _case.Id, "ayse", ChangeSources.Person, line.Id), (c.Entity, c.RecordId, c.CaseId, c.Username, c.Source, c.ActivityEntryId));
        });
    }

    [Fact]
    public async Task AProtestsChangesGoWithItsLineThoughTheLineIsSavedLater()
    {
        var input = ReviewValidationTests.CompleteProtest();
        input.EmployeeCount = 40;
        input.ProtestTypeIds = [2];
        input.Locations = [new LocationInput { CityId = 16, DistrictId = 160, Place = "Valilik önü" }];

        _user.Name = "mehmet";
        var result = await new SaveProtestHandler(_db, new CaseWriter(_db)).Handle(new SaveProtestCommand(_case.Id, _protest.Id, input, "mehmet"));

        Assert.Equal(CaseCommandStatus.Ok, result.Status);
        var line = _db.ActivityEntry.Single(a => a.Action == Actions.ProtestUpdate);
        Assert.Equal(
            [
                ("EmployeeCountNumber", null, "40"),
                ("Locations", "16/160/Fabrika önü", null),
                ("Locations", null, "16/160/Valilik önü"),
                ("ProtestTypes", "1", null),
            ],
            _db.FieldChange.OrderBy(c => c.Field).ThenBy(c => c.NewValue).Select(c => new ValueTuple<string, string?, string?>(c.Field, c.OldValue, c.NewValue)));
        Assert.All(_db.FieldChange, c => Assert.Equal(("Protesto", _protest.Id, _case.Id, line.Id), (c.Entity, c.RecordId, c.CaseId, c.ActivityEntryId)));
    }

    [Fact]
    public async Task DeletingACaseIsRecordedAsAChange()
    {
        _user.Name = "ayse";
        await new DeleteCaseHandler(_db).Handle(new DeleteCaseCommand(_case.Id, "ayse"));

        var change = Assert.Single(_db.FieldChange);
        Assert.Equal(("Deleted", "false", "true"), (change.Field, change.OldValue, change.NewValue));
    }

    [Fact]
    public async Task TheCaseHistoryShowsTheChangesWithNames()
    {
        _user.Name = "ayse";
        await UpdateCase(Case(c => { c.CompanyId = 11; c.ReasonIds = [7]; }));

        var line = Assert.Single(await new CaseActivityHandler(_db).Handle(_case.Id));
        Assert.Equal(
            [
                ("İşveren", "Proda Mühendislik", "Ahen Maden", false),
                ("Vaka Nedeni", "Ücret alacağı", null, true),
                ("Vaka Nedeni", null, "İşten çıkarma", true),
            ],
            line.Changes!.Select(c => (c.Label, (string?)c.OldValue, (string?)c.NewValue, c.IsList)));
    }

    [Fact]
    public async Task AnImportsChangesAreALineOfTheirOwn()
    {
        // Nobody signed in, no activity line: e.g. a one-time import.
        _user.Name = null;
        _db.ChangeSource = ChangeSources.Import;
        _db.Protesto.Single().StrikeDuration = 2 * 86400 + 4 * 3600;
        await _db.SaveChangesAsync();

        var line = Assert.Single(await new CaseActivityHandler(_db).Handle(_case.Id));
        Assert.Equal((ActivityRow.ChangesOnly, "içe aktarma", (string?)null), (line.Action, line.Summary, line.Username));
        Assert.True(line.Id < 0);
        var change = Assert.Single(line.Changes!);
        Assert.Equal(("Eylemin Süresi", "Yok", "2 gün 4 saat"), (change.Label, change.OldValue, change.NewValue));
        Assert.Equal(ChangeSources.Import, _db.FieldChange.Single().Source);
    }

    [Fact]
    public async Task CompanyChangesAreRecordedWithoutACase()
    {
        _user.Name = "ayse";
        _db.Company.Single(c => c.Id == 10).Name = "Proda Mühendislik A.Ş.";
        await _db.SaveChangesAsync();

        var change = Assert.Single(_db.FieldChange);
        Assert.Equal(("Company", 10, (int?)null, "Name", "Proda Mühendislik", "Proda Mühendislik A.Ş."),
            (change.Entity, change.RecordId, change.CaseId, change.Field, change.OldValue, change.NewValue));
    }

    [Fact]
    public void TheCaseHistoryQueryTranslatesToSqlServer()
    {
        using var sqlServer = new RSDBContext(new DbContextOptionsBuilder<RSDBContext>().UseSqlServer("Server=none;Database=none").Options);
        List<long> lineIds = [1, 2];

        var sql = sqlServer.FieldChange.Where(c => c.CaseId == 5 || c.ActivityEntryId != null && lineIds.Contains(c.ActivityEntryId.Value))
            .OrderBy(c => c.Id).Take(10).ToQueryString();

        Assert.Contains("FieldChange", sql);
    }
}
