using Microsoft.EntityFrameworkCore;
using RS.COMMON.Constants;
using RS.COMMON.Entities;
using RS.COMMON.Entities.LookupEntity;
using RS.EF;
using RS.MVC.Features.Definitions;
using RS.MVC.Features.Review;
using static RS.COMMON.Constants.Enums;

namespace RS.Tests;

/// <summary>R37, parts B and C: the company page's facts with other companies, its links from cases, and its new fields.</summary>
public class CompanyRelationTests
{
    private readonly RSDBContext _db = new(new TestUser { Name = "ayse" }.Track(new DbContextOptionsBuilder<RSDBContext>())
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private readonly Company _eti = new() { Name = "Eti Bakır" };
    private readonly Company _cengiz = new() { Name = "Cengiz Holding" };
    private readonly Company _efsane = new() { Name = "Efsane Kadro Yemekçilik" };
    private readonly Company _university = new() { Name = "İstanbul Üniversitesi" };

    public CompanyRelationTests()
    {
        _db.Category.Add(new Category { Id = KnownListIds.WorkplaceCategory, Name = "İşletme/Kurum" });
        _db.Company.AddRange(_eti, _cengiz, _efsane, _university, new Company { Id = 99, Name = "Silinmiş", Deleted = true });
        _db.SaveChanges();
    }

    private Task<RelationResult> Save(int companyId, CompanyRelationKind kind, int otherId, bool thisIsFirst = true,
        decimal? share = null, DateTime? start = null, DateTime? end = null, int? relationId = null) =>
        new SaveCompanyRelationHandler(_db).Handle(new SaveCompanyRelationCommand(
            companyId, relationId, kind, otherId, thisIsFirst, share, start, end, " https://kaynak.example/1 ", null, "ayse"));

    [Fact]
    public async Task AGroupFactShowsOnBothCompanies()
    {
        // Added on the holding's page: “Eti Bakır is one of this group's companies”.
        var added = await Save(_cengiz.Id, CompanyRelationKind.Group, _eti.Id, thisIsFirst: false, share: 100, start: new DateTime(2015, 1, 1));
        Assert.Equal(RelationStatus.Ok, added.Status);

        var onEti = Assert.Single((await new GetCompanyHandler(_db).Handle(_eti.Id))!.Relations);
        var onCengiz = Assert.Single((await new GetCompanyHandler(_db).Handle(_cengiz.Id))!.Relations);
        Assert.Equal((CompanyRelationKind.Group, true, "Cengiz Holding", 100m, "https://kaynak.example/1"),
            (onEti.Kind, onEti.Outgoing, onEti.OtherName, onEti.SharePercent!.Value, onEti.SourceUrl));
        Assert.Equal((false, "Eti Bakır"), (onCengiz.Outgoing, onCengiz.OtherName));
        Assert.Equal("definition.company-relation-add", _db.ActivityEntry.Single().Action);

        // The case form's line under İşveren reads the same facts.
        Assert.Equal("Cengiz Holding", Assert.Single(await new GetCompanyFactsHandler(_db).Handle(_eti.Id)).OtherName);
    }

    [Fact]
    public async Task AFactIsCheckedChangedAndRemoved()
    {
        Assert.Equal("Bir şirket kendisiyle ilişkilendirilemez.", (await Save(_eti.Id, CompanyRelationKind.Group, _eti.Id)).Error);
        Assert.Equal("Bitiş tarihi başlangıçtan önce olamaz.",
            (await Save(_eti.Id, CompanyRelationKind.Group, _cengiz.Id, start: new DateTime(2020, 1, 1), end: new DateTime(2019, 1, 1))).Error);
        Assert.Equal("Pay 0 ile 100 arasında olmalı.", (await Save(_eti.Id, CompanyRelationKind.Group, _cengiz.Id, share: 120)).Error);
        Assert.Equal(RelationStatus.NotFound, (await Save(_eti.Id, CompanyRelationKind.Group, 99)).Status); // a deleted company

        var id = (await Save(_university.Id, CompanyRelationKind.PublicBody, _cengiz.Id, share: 50)).Id!.Value;
        Assert.Null(_db.CompanyRelation.Single().SharePercent); // a share only for a group

        Assert.Equal(RelationStatus.Ok, (await Save(_university.Id, CompanyRelationKind.PublicBody, _cengiz.Id, end: new DateTime(2024, 6, 1), relationId: id)).Status);
        Assert.Equal(new DateTime(2024, 6, 1), _db.CompanyRelation.Single().EndDate);
        Assert.Equal(RelationStatus.NotFound, (await new RemoveCompanyRelationHandler(_db).Handle(new RemoveCompanyRelationCommand(_eti.Id, id))).Status);
        Assert.Equal(RelationStatus.Ok, (await new RemoveCompanyRelationHandler(_db).Handle(new RemoveCompanyRelationCommand(_cengiz.Id, id))).Status);
        Assert.Empty(_db.CompanyRelation);
    }

    [Fact]
    public async Task LinksThatDependOnTheJobComeFromTheCases()
    {
        // Çapa: Efsane Kadro subcontracts for İstanbul Üniversitesi (decision b: read from the case, not entered on the company).
        _db.Resistance.AddRange(
            new Resistance
            {
                Name = "Çapa", CategoryId = KnownListIds.WorkplaceCategory, CompanyId = _efsane.Id, MainCompanyId = _university.Id,
                EmployerRole = EmployerRole.Subcontractor, ResistanceResult = ResistanceResult.Unknown,
                Protestos = [new Protesto { StartDate = new DateTime(2026, 7, 9) }, new Protesto { StartDate = new DateTime(2026, 7, 20) }],
            },
            new Resistance { Name = "Doğrudan", CategoryId = KnownListIds.WorkplaceCategory, CompanyId = _university.Id, EmployerRole = EmployerRole.Direct, Protestos = [] },
            new Resistance { Name = "Silinmiş", CategoryId = KnownListIds.WorkplaceCategory, CompanyId = _efsane.Id, MainCompanyId = _university.Id, EmployerRole = EmployerRole.Subcontractor, Deleted = true, Protestos = [] });
        await _db.SaveChangesAsync();

        var asEmployer = Assert.Single((await new GetCompanyHandler(_db).Handle(_efsane.Id))!.JobLinks);
        var asAbove = Assert.Single((await new GetCompanyHandler(_db).Handle(_university.Id))!.JobLinks);

        Assert.Equal((EmployerRole.Subcontractor, true, "İstanbul Üniversitesi", 1, new DateTime(2026, 7, 9), new DateTime(2026, 7, 20)),
            (asEmployer.Role, asEmployer.AsEmployer, asEmployer.OtherName, asEmployer.Cases, asEmployer.First!.Value, asEmployer.Last!.Value));
        Assert.Equal((false, "Efsane Kadro Yemekçilik"), (asAbove.AsEmployer, asAbove.OtherName));
    }

    [Fact]
    public async Task ACompanyKeepsItsKindAndHizmetKolu()
    {
        _db.CompanyWorkLine.AddRange(new CompanyWorkLine { Id = 18, Name = "Sağlık ve sosyal hizmetler" },
            new CompanyWorkLine { Id = 24, Name = "MEMUR - Sağlık ve Sosyal Hizmetler", IsCivilService = true });
        await _db.SaveChangesAsync();

        await new UpdateDefinitionHandler(_db).Handle(new UpdateDefinitionCommand(DefinitionKind.Companies, _university.Id, "İstanbul Üniversitesi",
            TypeId: null, WorklineId: 18, InstitutionKind: InstitutionKind.PublicBody, ServiceBranchId: 24));
        var created = await new CreateCompanyHandler(_db).Handle(new CreateCompanyCommand("Karşıyaka Kent AŞ", null, null, null, InstitutionKind.MunicipalCompany));

        Assert.Equal((InstitutionKind.PublicBody, 18, 24), (_db.Company.Single(c => c.Id == _university.Id).InstitutionKind!.Value,
            _db.Company.Single(c => c.Id == _university.Id).CompanyWorkLineId!.Value, _db.Company.Single(c => c.Id == _university.Id).ServiceBranchId!.Value));
        Assert.Equal(InstitutionKind.MunicipalCompany, _db.Company.Single(c => c.Id == created.Company.Id).InstitutionKind);

        // The case form offers the two lists apart.
        var options = await new GetCaseFormOptionsHandler(_db).Handle(new GetCaseFormOptionsQuery());
        Assert.Equal(["Sağlık ve sosyal hizmetler"], options.CompanyWorklines.Select(o => o.Name));
        Assert.Equal(["MEMUR - Sağlık ve Sosyal Hizmetler"], options.ServiceBranches!.Select(o => o.Name));
    }

    [Fact]
    public async Task MergingACompanyMovesItsFacts()
    {
        var typo = new Company { Name = "Eti Bakir" };
        _db.Company.Add(typo);
        await _db.SaveChangesAsync();
        await Save(typo.Id, CompanyRelationKind.Group, _cengiz.Id);
        await Save(_eti.Id, CompanyRelationKind.Group, _cengiz.Id); // the same fact twice once merged
        await Save(typo.Id, CompanyRelationKind.Renamed, _eti.Id); // would link Eti Bakır to itself

        await new MergeDefinitionsHandler(_db).Handle(new MergeDefinitionsCommand(DefinitionKind.Companies, typo.Id, _eti.Id));

        var fact = Assert.Single(_db.CompanyRelation);
        Assert.Equal((_eti.Id, _cengiz.Id, CompanyRelationKind.Group), (fact.CompanyId, fact.RelatedCompanyId, fact.Kind));
    }

    [Fact]
    public async Task AHoldingSeesItsOwnItsGroupsAndItsCasesWorkplaces()
    {
        _db.City.AddRange(new City { Id = 34, Name = "İstanbul" }, new City { Id = 44, Name = "Malatya" });
        _db.District.Add(new District { Id = 341, Name = "Küçükçekmece", CityId = 34 });
        var cengizInsaat = new Company { Name = "Cengiz İnşaat" };
        _db.Company.Add(cengizInsaat);
        await _db.SaveChangesAsync();
        await Save(cengizInsaat.Id, CompanyRelationKind.Group, _cengiz.Id); // Cengiz İnşaat is part of Cengiz Holding
        _db.Workplace.AddRange(
            new Workplace { Id = 1, Name = "Cengiz Holding genel müdürlüğü", CityId = 34, OwnerCompanyId = _cengiz.Id, Approved = true },
            new Workplace { Id = 2, Name = "Eti Bakır Malatya işletmesi", CityId = 44, OwnerCompanyId = _eti.Id },
            new Workplace { Id = 3, Name = "Halkalı–İstanbul Havalimanı metro şantiyesi", CityId = 34, DistrictId = 341 },
            new Workplace { Id = 4, Name = "Başka bir şantiye", CityId = 34, OwnerCompanyId = cengizInsaat.Id });
        await Save(_eti.Id, CompanyRelationKind.Group, _cengiz.Id);
        // Case 3547's place: subcontracted work for Cengiz İnşaat, which names Cengiz Holding as before.
        _db.Resistance.Add(new Resistance { Name = "Metro", CategoryId = KnownListIds.WorkplaceCategory, MainCompanyId = _cengiz.Id, EmployerUnknown = true,
            EmployerRole = EmployerRole.Subcontractor, WorkplaceId = 3, Protestos = [] });
        await _db.SaveChangesAsync();

        var detail = (await new GetCompanyHandler(_db).Handle(_cengiz.Id))!;
        var places = detail.Workplaces!;
        // The subcontractor the news did not name still shows among the links from cases.
        Assert.Equal((EmployerRole.Subcontractor, false, 0, "İşveren bilinmiyor"),
            (detail.JobLinks[0].Role, detail.JobLinks[0].AsEmployer, detail.JobLinks[0].OtherId, detail.JobLinks[0].OtherName));

        Assert.Equal(
            [("Cengiz Holding genel müdürlüğü", "own"), ("Başka bir şantiye", "group"), ("Eti Bakır Malatya işletmesi", "group"), ("Halkalı–İstanbul Havalimanı metro şantiyesi", "cases")],
            places.Select(w => (w.Name, w.Source)));
        Assert.Equal(("İstanbul / Küçükçekmece", 1), (places[3].PlaceName, places[3].Cases));
    }

    [Fact]
    public async Task AWorkplacesProvinceDistrictAndOwnerAreEditedInTanimlar()
    {
        _db.City.AddRange(new City { Id = 34, Name = "İstanbul" }, new City { Id = 44, Name = "Malatya" });
        _db.District.Add(new District { Id = 341, Name = "Küçükçekmece", CityId = 34 });
        _db.Workplace.AddRange(new Workplace { Id = 3, Name = "Metro şantiyesi", CityId = 34 }, new Workplace { Id = 5, Name = "Metro şantiyesi", CityId = 44 });
        await _db.SaveChangesAsync();
        var update = new UpdateDefinitionHandler(_db);

        var saved = await update.Handle(new UpdateDefinitionCommand(DefinitionKind.Workplaces, 3, "Metro şantiyesi", CityId: 34, DistrictId: 341, OwnerCompanyId: _cengiz.Id));
        var badDistrict = await update.Handle(new UpdateDefinitionCommand(DefinitionKind.Workplaces, 5, "Metro şantiyesi", CityId: 44, DistrictId: 341, OwnerCompanyId: 99));

        Assert.Equal(ChangeStatus.Ok, saved.Status);
        Assert.Equal((341, _cengiz.Id), (_db.Workplace.Single(w => w.Id == 3).DistrictId!.Value, _db.Workplace.Single(w => w.Id == 3).OwnerCompanyId!.Value));
        // The same name in another province is another place; a district of another province and a deleted owner are dropped.
        Assert.Equal(ChangeStatus.Ok, badDistrict.Status);
        Assert.Equal(((int?)null, (int?)null), (_db.Workplace.Single(w => w.Id == 5).DistrictId, _db.Workplace.Single(w => w.Id == 5).OwnerCompanyId));
        Assert.Equal(ChangeStatus.Duplicate, (await update.Handle(new UpdateDefinitionCommand(DefinitionKind.Workplaces, 5, "Metro şantiyesi", CityId: 34))).Status);

        var row = (await new ListDefinitionsHandler(_db).Handle(new ListDefinitionsQuery(DefinitionKind.Workplaces))).Items.Single(i => i.Id == 3);
        Assert.Equal(("İstanbul / Küçükçekmece", "Cengiz Holding"), (row.PlaceName, row.OwnerName));
    }

    [Fact]
    public void TheFactsAndLinksTranslateToSqlServer()
    {
        using var sqlServer = new RSDBContext(new DbContextOptionsBuilder<RSDBContext>().UseSqlServer("Server=none;Database=none").Options);
        var facts = sqlServer.CompanyRelation
            .Where(r => (r.CompanyId == 1 && !r.RelatedCompany.Deleted) || (r.RelatedCompanyId == 1 && !r.Company.Deleted))
            .Select(r => new CompanyRelationItem(r.Id, r.Kind, r.CompanyId == 1, r.CompanyId == 1 ? r.RelatedCompanyId : r.CompanyId,
                r.CompanyId == 1 ? r.RelatedCompany.Name : r.Company.Name, r.SharePercent, r.StartDate, r.EndDate, r.SourceUrl, r.Note));

        Assert.Contains("[CompanyRelation]", facts.ToQueryString());
    }
}
