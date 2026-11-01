using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RS.COMMON.Constants;
using RS.COMMON.Entities;
using RS.COMMON.Entities.LookupEntity;
using RS.EF;
using RS.MVC.Features.Cases;
using RS.MVC.Features.Review;
using RS.MVC.Models;
using static RS.COMMON.Constants.Enums;

namespace RS.Tests;

/// <summary>R37, part A: the employer, how it works in the case, the organization above it and the workplace.</summary>
public class EmployerWorkplaceTests
{
    private readonly RSDBContext _db;
    private readonly Resistance _case;

    public EmployerWorkplaceTests()
    {
        _db = new(new TestUser().Track(new DbContextOptionsBuilder<RSDBContext>())
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options);
        _db.City.AddRange(new City { Id = 34, Name = "İstanbul" }, new City { Id = 16, Name = "Bursa" });
        _db.District.Add(new District { Id = 340, Name = "Fatih", CityId = 34 });
        _db.Company.AddRange(
            new Company { Id = 10, Name = "Efsane Kadro Yemekçilik" },
            new Company { Id = 11, Name = "İstanbul Üniversitesi" },
            new Company { Id = 12, Name = "Ford Otosan" });
        _db.Workplace.Add(new Workplace { Id = 1, Name = "İstanbul Tıp Fakültesi Hastanesi (Çapa)", CityId = 34, OwnerCompanyId = 11, Approved = true });
        _case = new Resistance { Name = "Vaka", Description = "x", CategoryId = KnownListIds.WorkplaceCategory, CompanyId = 12, ResistanceResult = ResistanceResult.Unknown };
        _db.Resistance.Add(_case);
        _db.SaveChanges();
    }

    private static CaseInput Case(Action<CaseInput> change)
    {
        var c = ReviewValidationTests.CompleteNewCase().Case!;
        change(c);
        return c;
    }

    private async Task<Resistance> Save(CaseInput input)
    {
        var result = await new UpdateCaseHandler(_db, new CaseWriter(_db), new ListValueResolver(_db)).Handle(new UpdateCaseCommand(_case.Id, input, "ayse"));
        Assert.Equal(CaseCommandStatus.Ok, result.Status);
        return _db.Resistance.Single(r => r.Id == _case.Id);
    }

    [Theory]
    [InlineData(true, EmployerRole.Subcontractor, 11)]
    [InlineData(false, EmployerRole.Direct, null)]
    public async Task TheOldFormsTaseronMuStillDecidesTheRole(bool isOutsource, EmployerRole role, int? mainCompanyId)
    {
        // Until the new form ships it sends no role, only “Taşeron mu?”.
        var saved = await Save(Case(c => { c.IsOutsource = isOutsource; c.MainCompanyId = 11; }));

        Assert.Equal((role, mainCompanyId), (saved.EmployerRole, saved.MainCompanyId));
    }

    [Theory]
    [InlineData(EmployerRole.Contractor, true, EmployerRole.Contractor)] // the loaded role agrees with “Evet”: kept
    [InlineData(EmployerRole.Contractor, false, EmployerRole.Direct)] // “Hayır” on a migrated guess: the answer decides
    [InlineData(EmployerRole.Direct, true, EmployerRole.Subcontractor)] // “Evet” on a direct one: Taşeron
    [InlineData(EmployerRole.Unknown, false, EmployerRole.Unknown)]
    public void TheOldFormsAnswerWinsWhenItDisagreesWithTheLoadedRole(EmployerRole loaded, bool isOutsource, EmployerRole expected) =>
        Assert.Equal(expected, EmployerRoles.Effective(new CaseInput { EmployerRole = loaded, IsOutsource = isOutsource }));

    [Fact]
    public async Task TheRoleKeepsTheOrganizationAboveOnlyWhenItHasOne()
    {
        var subcontract = await Save(Case(c => { c.EmployerRole = EmployerRole.Subcontractor; c.IsOutsource = true; c.MainCompanyId = 11; c.WorkplaceId = 1; }));
        Assert.Equal((EmployerRole.Subcontractor, 11, 1), (subcontract.EmployerRole, subcontract.MainCompanyId, subcontract.WorkplaceId));

        // A direct employer has nothing above it, whatever the form still holds.
        var direct = await Save(Case(c => { c.EmployerRole = EmployerRole.Direct; c.MainCompanyId = 11; c.IsOutsource = false; }));
        Assert.Equal((EmployerRole.Direct, (int?)null, (int?)null), (direct.EmployerRole, direct.MainCompanyId, direct.WorkplaceId));
    }

    [Fact]
    public async Task AnUnknownEmployerLeavesTheCompanyEmpty()
    {
        var saved = await Save(Case(c =>
        {
            c.EmployerUnknown = true;
            c.CompanyId = 10; // left from before: not kept
            c.EmployerRole = EmployerRole.Subcontractor;
            c.IsOutsource = true;
            c.MainCompanyId = 11;
            c.WorkplaceId = 1;
        }));

        Assert.Equal((true, (int?)null, 11, 1), (saved.EmployerUnknown, saved.CompanyId, saved.MainCompanyId, saved.WorkplaceId));
    }

    [Fact]
    public async Task GenelCasesHaveNoEmployerRoleOrWorkplace()
    {
        var saved = await Save(Case(c =>
        {
            c.CategoryId = KnownListIds.GeneralCategory;
            c.EmployerRole = EmployerRole.Subcontractor;
            c.MainCompanyId = 11;
            c.WorkplaceId = 1;
            c.EmployerUnknown = true;
        }));

        Assert.Equal((EmployerRole.Unknown, false, (int?)null, (int?)null, (int?)null),
            (saved.EmployerRole, saved.EmployerUnknown, saved.CompanyId, saved.MainCompanyId, saved.WorkplaceId));
    }

    [Fact]
    public async Task AnUnknownOrDeletedWorkplaceIsNotSaved()
    {
        _db.Workplace.Add(new Workplace { Id = 2, Name = "Silinmiş", Deleted = true });
        await _db.SaveChangesAsync();

        Assert.Null((await Save(Case(c => c.WorkplaceId = 99))).WorkplaceId);
        Assert.Null((await Save(Case(c => c.WorkplaceId = 2))).WorkplaceId);
    }

    [Theory]
    [InlineData(EmployerRole.Subcontractor, "Lütfen asıl işvereni seçiniz.")]
    [InlineData(EmployerRole.Contractor, "Lütfen iş sahibini seçiniz.")]
    [InlineData(EmployerRole.Operator, "Lütfen varlığın sahibini seçiniz.")]
    public void EachRoleAboveTheEmployerNeedsItsOrganization(EmployerRole role, string message)
    {
        var errors = ReviewValidation.ForCase(Case(c => { c.EmployerRole = role; c.IsOutsource = true; c.MainCompanyId = null; }));

        Assert.Equal(message, errors["case.mainCompanyId"]);
        Assert.Empty(ReviewValidation.ForCase(Case(c => { c.EmployerRole = EmployerRole.Direct; c.MainCompanyId = null; })));
        Assert.Empty(ReviewValidation.ForCase(Case(c => { c.EmployerRole = EmployerRole.Unknown; c.MainCompanyId = null; })));
    }

    [Fact]
    public void AnEmployerIsNeededUnlessItIsUnknown()
    {
        Assert.Contains("İşveren bilinmiyor", ReviewValidation.ForCase(Case(c => c.CompanyId = null))["case.companyId"]);
        Assert.Empty(ReviewValidation.ForCase(Case(c => { c.CompanyId = null; c.EmployerUnknown = true; })));
    }

    [Fact]
    public async Task TheCasePageGetsTheRoleAndThePlace()
    {
        await Save(Case(c => { c.EmployerRole = EmployerRole.Subcontractor; c.IsOutsource = true; c.MainCompanyId = 11; c.WorkplaceId = 1; c.CompanyId = 10; }));

        var detail = (await new GetCaseHandler(_db).Handle(new GetCaseQuery(_case.Id)))!;

        Assert.Equal((EmployerRole.Subcontractor, (EmployerRole?)EmployerRole.Subcontractor, 1, "İstanbul Tıp Fakültesi Hastanesi (Çapa)", true),
            (detail.Case.EmployerRole!.Value, detail.Case.EmployerRole, detail.Case.WorkplaceId!.Value, detail.WorkplaceName, detail.Case.IsOutsource));
    }

    [Fact]
    public async Task AWorkplaceIsAddedOnceForTheSameNameInTheSameProvince()
    {
        var handler = new CreateWorkplaceHandler(_db);

        var again = await handler.Handle(new CreateWorkplaceCommand("istanbul tıp fakültesi hastanesi (ÇAPA)", 34, null, null));
        var elsewhere = await handler.Handle(new CreateWorkplaceCommand("İstanbul Tıp Fakültesi Hastanesi (Çapa)", 16, null, null));

        Assert.Equal((true, 1), (again.Existed, again.Workplace.Id));
        Assert.Equal((false, false, "Bursa"), (elsewhere.Existed, elsewhere.Workplace.Approved, elsewhere.Workplace.CityName));
        Assert.Equal("İlçe bu ile ait değil.", (await handler.Handle(new CreateWorkplaceCommand("Bir yer", 16, 340, null))).Error);
        Assert.Equal("Lütfen işyerinin adını giriniz.", (await handler.Handle(new CreateWorkplaceCommand("  ", 34, null, null))).Error);
        Assert.Equal("definition.workplace-create", _db.ActivityEntry.Single().Action);
    }

    [Fact]
    public async Task AFormSuggestsTheCompanysKnownPlaces()
    {
        _db.Workplace.Add(new Workplace { Id = 3, Name = "Gölcük fabrikası", CityId = 16 });
        _db.Resistance.Add(new Resistance { Name = "Ford", Description = "x", CategoryId = KnownListIds.WorkplaceCategory, CompanyId = 12, WorkplaceId = 3 });
        await _db.SaveChangesAsync();
        var search = new SearchWorkplacesHandler(_db);

        // Ford Otosan: the place of its cases. İstanbul Üniversitesi: the place that belongs to it.
        Assert.Equal(["Gölcük fabrikası"], (await search.Handle(new SearchWorkplacesQuery(null, 12))).Select(w => w.Name));
        var capa = Assert.Single(await search.Handle(new SearchWorkplacesQuery(null, 11)));
        Assert.Equal((11, "İstanbul Üniversitesi"), (capa.OwnerCompanyId!.Value, capa.OwnerName));
        // By name (the database ignores case with its Turkish collation; the in-memory one does not).
        Assert.Equal(["Gölcük fabrikası"], (await search.Handle(new SearchWorkplacesQuery("Gölcük"))).Select(w => w.Name));
        Assert.Empty(await search.Handle(new SearchWorkplacesQuery(null)));
    }

    [Fact]
    public void TheWorkplaceSuggestionsTranslateToSqlServer()
    {
        using var sqlServer = new RSDBContext(new DbContextOptionsBuilder<RSDBContext>().UseSqlServer("Server=none;Database=none").Options);
        var places = sqlServer.Workplace.Where(w => !w.Deleted)
            .Where(w => w.OwnerCompanyId == 12 || sqlServer.Resistance.Any(r => !r.Deleted && r.WorkplaceId == w.Id && (r.CompanyId == 12 || r.MainCompanyId == 12)))
            .OrderByDescending(w => sqlServer.Resistance.Count(r => !r.Deleted && r.WorkplaceId == w.Id && (r.CompanyId == 12 || r.MainCompanyId == 12)));

        Assert.Contains("[Workplace]", places.ToQueryString());
    }
}
