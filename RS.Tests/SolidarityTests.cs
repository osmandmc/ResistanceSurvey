using Microsoft.EntityFrameworkCore;
using RS.COMMON.Constants;
using RS.COMMON.Entities;
using RS.COMMON.Entities.LookupEntity;
using RS.EF;
using RS.MVC.Features.Cases;
using RS.MVC.Features.Review;

namespace RS.Tests;

/// <summary>R31c: old Dayanışma cases linked to the struggles they supported.</summary>
public class SolidarityTests
{
    private readonly RSDBContext _db = new(new TestUser { Name = "ayse" }.Track(new DbContextOptionsBuilder<RSDBContext>())
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private readonly Company _migros = new() { Name = "Migros" };
    private readonly Company _pseudo = new() { Name = "Migros işçileriyle dayanışma eylemi" };

    private Resistance Case(int category, string? name, Company? company, DateTime protest, string? description = null)
    {
        var r = new Resistance
        {
            Name = name, CategoryId = category, Company = company, Description = description,
            Protestos = [new Protesto { StartDate = protest, Locations = [new ProtestoLocation { City = new City { Name = "Ankara" } }] }],
        };
        _db.Resistance.Add(r);
        _db.SaveChanges();
        return r;
    }

    /// <summary>As the migration leaves an old Dayanışma case: named after its old company, company emptied, recorded.</summary>
    private Resistance OldSolidarity(Company oldCompany, DateTime protest)
    {
        if (oldCompany.Id == 0)
        {
            _db.Company.Add(oldCompany);
            _db.SaveChanges();
        }
        var r = Case(KnownListIds.SolidarityCategory, oldCompany.Name, null, protest);
        _db.CaseMigrationRecord.Add(new CaseMigrationRecord { ResistanceId = r.Id, Step = "solidarity", OldCompanyId = oldCompany.Id, OldCompanyName = oldCompany.Name });
        _db.SaveChanges();
        return r;
    }

    [Fact]
    public async Task OldCasesAreGroupedByTheirOldCompanyWithTheStrugglesCaseSuggested()
    {
        var depot = Case(KnownListIds.WorkplaceCategory, "Migros depo işçileri direnişi", _migros, new DateTime(2026, 2, 1));
        Case(KnownListIds.WorkplaceCategory, "Migros 2019 grevi", _migros, new DateTime(2019, 5, 1));
        var a = OldSolidarity(_pseudo, new DateTime(2026, 2, 10));
        var b = OldSolidarity(_pseudo, new DateTime(2026, 2, 12));

        var page = await new ListSolidarityLinksHandler(_db).Handle();

        var group = Assert.Single(page.Groups);
        Assert.Equal(("Migros işçileriyle dayanışma eylemi", 2), (group.Name, group.CaseIds.Count));
        Assert.Equal([a.Id, b.Id], group.CaseIds.Order());
        Assert.Equal((depot.Id, "adında işyeri geçiyor"), (group.Suggestion!.CaseId, group.Suggestion.Reason)); // the 2026 case, not 2019
        Assert.False(group.OldCompanyUsedByIsletme);
    }

    [Fact]
    public async Task LinkingAGroupLinksEveryCaseAndLeavesTheList()
    {
        var depot = Case(KnownListIds.WorkplaceCategory, "Migros depo işçileri direnişi", _migros, new DateTime(2026, 2, 1));
        var a = OldSolidarity(_pseudo, new DateTime(2026, 2, 10));
        var b = OldSolidarity(_pseudo, new DateTime(2026, 2, 12));

        var error = await new ResolveSolidarityHandler(_db, new ListValueResolver(_db)).Handle(new ResolveSolidarityCommand([a.Id, b.Id], SolidarityAction.Link, depot.Id));

        Assert.Null(error);
        Assert.All(_db.Resistance.Where(r => r.Id == a.Id || r.Id == b.Id), r => Assert.Equal(depot.Id, r.SupportedResistanceId));
        Assert.Empty((await new ListSolidarityLinksHandler(_db).Handle()).Groups);
        Assert.Equal("“Migros işçileriyle dayanışma eylemi” (2 vaka) → “Migros depo işçileri direnişi”", _db.ActivityEntry.Single().Summary);
        var linked = await new CaseSolidarityHandler(_db).Handle(depot.Id);
        Assert.Equal([a.Id, b.Id], linked.Linked.Select(l => l.Id).Order());
    }

    [Fact]
    public async Task ANameThatWasTheOrganizerBecomesAnOrganizingOrganization()
    {
        var platform = new Company { Name = "Edremit Demokrasi Platformu" };
        var a = OldSolidarity(platform, new DateTime(2026, 4, 30));

        await new ResolveSolidarityHandler(_db, new ListValueResolver(_db)).Handle(new ResolveSolidarityCommand([a.Id], SolidarityAction.Organizer));

        var link = _db.ResistanceCorporation.Include(x => x.Corporation).Single(x => x.ResistanceId == a.Id);
        Assert.Equal(("Edremit Demokrasi Platformu", OrganizationRole.Organizing), (link.Corporation.Name, link.Role));
    }

    [Fact]
    public async Task AWorkplaceWhoseWorkersActedComesBack()
    {
        var ttk = new Company { Name = "Türkiye Taşkömürü Kurumu", Deleted = true };
        var a = OldSolidarity(ttk, new DateTime(2026, 3, 1));

        await new ResolveSolidarityHandler(_db, new ListValueResolver(_db)).Handle(new ResolveSolidarityCommand([a.Id], SolidarityAction.Workplace));

        Assert.Equal(ttk.Id, _db.Resistance.Single(r => r.Id == a.Id).CompanyId);
        Assert.False(_db.Company.Single(c => c.Id == ttk.Id).Deleted);
    }

    [Fact]
    public async Task ASameWorkplaceGroupIsSuggestedItsOwnCase()
    {
        var temelConta = new Company { Name = "Temel Conta" };
        var strike = Case(KnownListIds.WorkplaceCategory, "Temel Conta grevi", temelConta, new DateTime(2025, 6, 1));
        OldSolidarity(temelConta, new DateTime(2026, 1, 14));

        var group = Assert.Single((await new ListSolidarityLinksHandler(_db).Handle()).Groups);

        Assert.True(group.OldCompanyUsedByIsletme);
        Assert.Equal((strike.Id, "aynı işyeri"), (group.Suggestion!.CaseId, group.Suggestion.Reason));
    }

    [Fact]
    public async Task ACasePageSuggestsUnlinkedSolidarityNamingItsWorkplace()
    {
        var doruk = new Company { Name = "Doruk Madencilik" };
        var resistance = Case(KnownListIds.WorkplaceCategory, "Doruk direnişi", doruk, new DateTime(2026, 5, 1));
        var mentions = Case(KnownListIds.SolidarityCategory, "Dayanışma", null, new DateTime(2026, 5, 3), "Ankara'da direnen Doruk Madencilik işçileriyle dayanışma.");
        Case(KnownListIds.SolidarityCategory, "Başka dayanışma", null, new DateTime(2026, 5, 3), "Migros işçileriyle.");

        var solidarity = await new CaseSolidarityHandler(_db).Handle(resistance.Id);
        Assert.Equal([mentions.Id], solidarity.Suggested.Select(s => s.Id));

        Assert.Null(await new LinkSupportedCaseHandler(_db).Handle(new LinkSupportedCaseCommand(mentions.Id, resistance.Id)));
        Assert.Equal("Dayanışma vakası bulunamadı.", await new LinkSupportedCaseHandler(_db).Handle(new LinkSupportedCaseCommand(resistance.Id, mentions.Id)));
        Assert.Empty((await new CaseSolidarityHandler(_db).Handle(resistance.Id)).Suggested);
    }

    [Fact]
    public async Task IsletmeCasesWithAnOldTargetAreListedUntilChecked()
    {
        var union = new Corporation { Name = "DİSK Genel-İş" };
        _db.Add(union);
        var r = Case(KnownListIds.WorkplaceCategory, "Belediye", _migros, new DateTime(2026, 1, 1));
        _db.CaseMigrationRecord.Add(new CaseMigrationRecord { ResistanceId = r.Id, Step = "workplace-target", OldCorporationId = union.Id });
        _db.SaveChanges();

        Assert.Equal("DİSK Genel-İş", Assert.Single((await new ListSolidarityLinksHandler(_db).Handle()).TargetChecks).OldTargetName);
        Assert.True(await new ResolveTargetCheckHandler(_db).Handle(r.Id));
        Assert.Empty((await new ListSolidarityLinksHandler(_db).Handle()).TargetChecks);
    }
}
