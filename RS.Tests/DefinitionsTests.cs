using Microsoft.EntityFrameworkCore;
using RS.COMMON.Entities;
using RS.COMMON.Entities.LookupEntity;
using RS.EF;
using RS.MVC.Features.Definitions;

namespace RS.Tests;

public class DefinitionsTests
{
    private readonly RSDBContext _db = new(new DbContextOptionsBuilder<RSDBContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private readonly Category _category = new() { Name = "Grev" };

    private Resistance Case(bool deleted = false, Company? company = null, Company? mainCompany = null)
    {
        var resistance = new Resistance { Name = "Vaka", Category = _category, Deleted = deleted, Company = company, MainCompany = mainCompany };
        _db.Resistance.Add(resistance);
        return resistance;
    }

    private Task<DefinitionPage> List(DefinitionKind kind, string? text = null, bool pending = false, int? typeId = null) =>
        new ListDefinitionsHandler(_db).Handle(new ListDefinitionsQuery(kind, text!, pending, typeId));

    private Task<MergeResult> Merge(DefinitionKind kind, int from, int into) =>
        new MergeDefinitionsHandler(_db).Handle(new MergeDefinitionsCommand(kind, from, into));

    [Fact]
    public void TheListFiltersTranslateToSqlServer()
    {
        // No database needed: EF builds the SQL and fails here if the interface property cannot be translated.
        using var sqlServer = new RSDBContext(new DbContextOptionsBuilder<RSDBContext>().UseSqlServer("Server=none;Database=none").Options);

        var sql = ListDefinitionsHandler.Filter(sqlServer.Corporation, "işçi", pendingOnly: true).ToQueryString();

        Assert.Contains("[Approved]", sql);
        Assert.Contains("LIKE", sql);
    }

    [Fact]
    public async Task CorporationsCountLiveCasesInEveryRole()
    {
        var union = new TradeUnion { Name = "DİSK Genel-İş", CorporationTypeId = 1, Approved = true };
        var typed = new Corporation { Name = "Yeni Platform", Approved = false };
        _db.AddRange(union, typed);
        var supporting = Case();
        supporting.ResistanceCorporations = [new ResistanceCorporation { Corporation = union }];
        var asUnion = Case();
        asUnion.TradeUnion = union;
        Case(deleted: true).ResistanceCorporations = [new ResistanceCorporation { Corporation = union }];
        await _db.SaveChangesAsync();

        var page = await List(DefinitionKind.Corporations);

        Assert.Equal(1, page.PendingCount);
        var item = page.Items.Single(i => i.Name == "DİSK Genel-İş");
        Assert.Equal((2, true, 1), (item.Usage, item.Approved!.Value, item.TypeId!.Value)); // the deleted case does not count
        Assert.Equal(["Yeni Platform"], (await List(DefinitionKind.Corporations, pending: true)).Items.Select(i => i.Name));
        Assert.Equal(["DİSK Genel-İş"], (await List(DefinitionKind.Corporations, typeId: 1)).Items.Select(i => i.Name));
    }

    [Fact]
    public async Task RenamingToAnExistingNameSuggestsMerging()
    {
        _db.ResistanceReason.AddRange(new ResistanceReason { Id = 1, Name = "Ücret" }, new ResistanceReason { Id = 2, Name = "Ucret" });
        await _db.SaveChangesAsync();

        var result = await new UpdateDefinitionHandler(_db).Handle(new UpdateDefinitionCommand(DefinitionKind.Reasons, 2, " Ücret "));

        Assert.Equal((ChangeStatus.Duplicate, 1), (result.Status, result.ExistingId!.Value));
    }

    [Fact]
    public async Task OnlyUnusedValuesCanBeDeleted()
    {
        var unused = new ResistanceReason { Name = "Kullanılmayan" };
        var used = new ResistanceReason { Name = "Ücret" };
        var onlyDeleted = new ResistanceReason { Name = "Eski" };
        _db.AddRange(unused, used, onlyDeleted);
        Case().ResistanceResistanceReasons = [new ResistanceResistanceReason { ResistanceReason = used }];
        Case(deleted: true).ResistanceResistanceReasons = [new ResistanceResistanceReason { ResistanceReason = onlyDeleted }];
        await _db.SaveChangesAsync();
        var delete = new DeleteDefinitionHandler(_db);

        Assert.Equal(ChangeStatus.Ok, (await delete.Handle(new(DefinitionKind.Reasons, unused.Id))).Status);
        Assert.Equal((ChangeStatus.InUse, 1), ((await delete.Handle(new(DefinitionKind.Reasons, used.Id))) is var r ? (r.Status, r.Usage) : default));
        // A deleted case still points at it: removing the value would break that case.
        Assert.Equal((ChangeStatus.InUse, 0), ((await delete.Handle(new(DefinitionKind.Reasons, onlyDeleted.Id))) is var d ? (d.Status, d.Usage) : default));
        Assert.Equal(["Eski", "Ücret"], _db.ResistanceReason.Select(x => x.Name).OrderBy(n => n));
    }

    [Fact]
    public async Task MergingIntoAnUnusedValueMovesTheLinks()
    {
        // The old page deleted the case links when the target was not used yet.
        var typo = new Corporation { Name = "kurumsuz" };
        var kept = new Corporation { Name = "Kurumsuz", Approved = true };
        _db.AddRange(typo, kept);
        var resistance = Case();
        resistance.ResistanceCorporations = [new ResistanceCorporation { Corporation = typo }];
        resistance.TradeUnion = typo;
        await _db.SaveChangesAsync();

        var result = await Merge(DefinitionKind.Corporations, typo.Id, kept.Id);

        Assert.Equal((MergeStatus.Ok, 1), (result.Status, result.Affected));
        var link = Assert.Single(_db.ResistanceCorporation);
        Assert.Equal(kept.Id, link.CorporationId);
        Assert.Equal(kept.Id, resistance.TradeUnionId!.Value);
        Assert.Equal(["Kurumsuz"], _db.Corporation.Select(c => c.Name));
    }

    [Fact]
    public async Task ACaseThatHadBothValuesKeepsOneLink()
    {
        var a = new ResistanceReason { Name = "Ücret" };
        var b = new ResistanceReason { Name = "Ücret gaspı" };
        _db.AddRange(a, b);
        var resistance = Case();
        resistance.ResistanceResistanceReasons = [new ResistanceResistanceReason { ResistanceReason = a }, new ResistanceResistanceReason { ResistanceReason = b }];
        await _db.SaveChangesAsync();

        await Merge(DefinitionKind.Reasons, b.Id, a.Id);

        Assert.Equal([a.Id], _db.ResistanceResistanceReason.Where(x => x.ResistanceId == resistance.Id).Select(x => x.ResistanceReasonId));
    }

    [Fact]
    public async Task MergingCompaniesMovesEveryReference()
    {
        var duplicate = new Company { Name = "Örnek Tekstil A.Ş." };
        var kept = new Company { Name = "Örnek Tekstil" };
        var subcontractor = new Company { Name = "Taşeron Ltd." };
        _db.AddRange(duplicate, kept, subcontractor);
        var asCompany = Case(company: duplicate);
        var asMain = Case(company: subcontractor, mainCompany: duplicate);
        await _db.SaveChangesAsync();
        _db.CompanyOutsourceCompany.AddRange(
            new CompanyOutsourceCompany { CompanyId = duplicate.Id, OutsourceCompanyId = subcontractor.Id },
            new CompanyOutsourceCompany { CompanyId = kept.Id, OutsourceCompanyId = subcontractor.Id },
            new CompanyOutsourceCompany { CompanyId = kept.Id, OutsourceCompanyId = duplicate.Id });
        await _db.SaveChangesAsync();

        var result = await Merge(DefinitionKind.Companies, duplicate.Id, kept.Id);

        Assert.Equal((MergeStatus.Ok, 2), (result.Status, result.Affected));
        Assert.Equal((kept.Id, kept.Id), (asCompany.CompanyId!.Value, asMain.MainCompanyId!.Value));
        // The duplicate link and the link to itself are gone; one subcontractor link remains.
        var link = Assert.Single(_db.CompanyOutsourceCompany);
        Assert.Equal((kept.Id, subcontractor.Id), (link.CompanyId, link.OutsourceCompanyId));
        Assert.True(duplicate.Deleted);
        Assert.DoesNotContain(duplicate.Id, (await List(DefinitionKind.Companies)).Items.Select(i => i.Id));
    }

    [Fact]
    public async Task ProtestTypesCountAndMergeByProtest()
    {
        var a = new ProtestoType { Name = "İş bırakma", Approved = true };
        var b = new ProtestoType { Name = "iş bırakma eylemi" };
        _db.AddRange(a, b);
        var resistance = Case();
        resistance.Protestos =
        [
            new Protesto { ProtestoProtestoTypes = [new ProtestoProtestoType { ProtestoType = b }] },
            new Protesto { ProtestoProtestoTypes = [new ProtestoProtestoType { ProtestoType = a }, new ProtestoProtestoType { ProtestoType = b }] },
        ];
        await _db.SaveChangesAsync();

        Assert.Equal(2, (await List(DefinitionKind.ProtestTypes)).Items.Single(i => i.Id == b.Id).Usage);
        await Merge(DefinitionKind.ProtestTypes, b.Id, a.Id);

        Assert.Equal(2, _db.ProtestoProtestoType.Count());
        Assert.All(_db.ProtestoProtestoType, x => Assert.Equal(a.Id, x.ProtestoTypeId));
    }

    [Fact]
    public async Task AValueCannotBeMergedIntoItself() =>
        Assert.Equal(MergeStatus.SameValue, (await Merge(DefinitionKind.Reasons, 1, 1)).Status);

    [Theory]
    [InlineData(DefinitionKind.Companies, "Lindsay Sulama San. Tic. A.Ş.", "LİNDSAY SULAMA")]
    [InlineData(DefinitionKind.Companies, "Birleşik Kamu-İş", "Birleşik Kamu İş")]
    [InlineData(DefinitionKind.Corporations, "TÜRKİYE KAMU-SEN TÜRK-SAĞLIK-SEN", "Türkiye Kamu-Sen Türk Sağlık Sen")]
    [InlineData(DefinitionKind.Reasons, "Ücret gaspı", "ucret gaspi")]
    public void LikelyDuplicatesShareAKey(DefinitionKind kind, string a, string b) =>
        Assert.Equal(DuplicateKey.For(kind, a), DuplicateKey.For(kind, b));

    [Fact]
    public void LegalWordsAreIgnoredOnlyForCompanies() =>
        Assert.NotEqual(DuplicateKey.For(DefinitionKind.Corporations, "Örnek Dernek A.Ş."), DuplicateKey.For(DefinitionKind.Corporations, "Örnek Dernek"));

    [Fact]
    public async Task DuplicatesComeInGroupsWithTheValueToKeepFirst()
    {
        var typo = new Corporation { Name = "kurumsuz" };
        var kept = new Corporation { Name = "Kurumsuz", Approved = true };
        _db.AddRange(typo, kept, new Corporation { Name = "Başka Kurum", Approved = true });
        Case().ResistanceCorporations = [new ResistanceCorporation { Corporation = typo }, new ResistanceCorporation { Corporation = kept }];
        await _db.SaveChangesAsync();

        var group = Assert.Single(await new FindDuplicatesHandler(_db).Handle(DefinitionKind.Corporations));

        Assert.Equal(["Kurumsuz", "kurumsuz"], group.Items.Select(i => i.Name)); // approved first
        Assert.Equal([1, 1], group.Items.Select(i => i.Usage));
    }

    [Fact]
    public async Task NotDuplicateGroupsAreNotSuggestedAgain()
    {
        var a = new Company { Name = "1 Mayıs: Trabzon" };
        var b = new Company { Name = "1 Mayıs-Trabzon" };
        _db.AddRange(a, b);
        await _db.SaveChangesAsync();
        var find = new FindDuplicatesHandler(_db);
        Assert.Single(await find.Handle(DefinitionKind.Companies));

        await new DismissDuplicatesHandler(_db).Handle(new DismissDuplicatesCommand(DefinitionKind.Companies, [b.Id, a.Id]));
        await new DismissDuplicatesHandler(_db).Handle(new DismissDuplicatesCommand(DefinitionKind.Companies, [a.Id, b.Id])); // again: no second row

        Assert.Empty(await find.Handle(DefinitionKind.Companies));
        Assert.Single(_db.DefinitionNotDuplicate);
        // A third look-alike brings the group back, with a pair nobody has judged yet.
        _db.Company.Add(new Company { Name = "1 MAYIS TRABZON" });
        await _db.SaveChangesAsync();
        Assert.Equal(3, Assert.Single(await find.Handle(DefinitionKind.Companies)).Items.Count);
    }

    [Fact]
    public async Task AGroupIsMergedIntoTheChosenValue()
    {
        var a = new Company { Name = "AKSA" };
        var b = new Company { Name = "Aksa" };
        var c = new Company { Name = "Aksa A.Ş." };
        _db.AddRange(a, b, c);
        var first = Case(company: b);
        var second = Case(company: c);
        await _db.SaveChangesAsync();
        var handler = new MergeGroupHandler(_db, new MergeDefinitionsHandler(_db));

        var result = await handler.Handle(new MergeGroupCommand(DefinitionKind.Companies, a.Id, [b.Id, c.Id, a.Id]));

        Assert.Equal((MergeStatus.Ok, 2), (result.Status, result.Affected));
        Assert.Equal((a.Id, a.Id), (first.CompanyId!.Value, second.CompanyId!.Value));
        Assert.Equal([a.Id], _db.Company.Where(x => !x.Deleted).Select(x => x.Id));
    }

    [Fact]
    public async Task ACompanyShowsItsCasesAndRelations()
    {
        var main = new Company { Name = "Ana Firma" };
        var company = new Company { Name = "Örnek Tekstil" };
        var sub = new Company { Name = "Taşeron Ltd." };
        var gone = new Company { Name = "Eski Taşeron", Deleted = true };
        _db.AddRange(main, company, sub, gone);
        var own = Case(company: company);
        var ofSubcontractor = Case(company: sub, mainCompany: company);
        Case(deleted: true, company: company);
        await _db.SaveChangesAsync();
        _db.CompanyOutsourceCompany.AddRange(
            new CompanyOutsourceCompany { CompanyId = main.Id, OutsourceCompanyId = company.Id },
            new CompanyOutsourceCompany { CompanyId = company.Id, OutsourceCompanyId = sub.Id },
            new CompanyOutsourceCompany { CompanyId = company.Id, OutsourceCompanyId = gone.Id });
        await _db.SaveChangesAsync();

        var detail = await new GetCompanyHandler(_db).Handle(company.Id);

        Assert.NotNull(detail);
        Assert.Equal([(own.Id, false), (ofSubcontractor.Id, true)], detail.Cases.Select(c => (c.Id, c.AsMainCompany)).OrderBy(c => c.Item1));
        Assert.Equal(["Ana Firma"], detail.MainCompanies.Select(c => c.Name));
        Assert.Equal(["Taşeron Ltd."], detail.Subcontractors.Select(c => c.Name)); // the deleted one is left out
        Assert.Null(await new GetCompanyHandler(_db).Handle(gone.Id));
    }

    [Fact]
    public async Task SubcontractorLinksAreAddedOnceAndRemoved()
    {
        var main = new Company { Name = "Ana Firma" };
        var sub = new Company { Name = "Taşeron Ltd." };
        _db.AddRange(main, sub);
        await _db.SaveChangesAsync();
        var add = new AddCompanyLinkHandler(_db);

        Assert.Equal(LinkStatus.Ok, await add.Handle(new CompanyLinkCommand(main.Id, sub.Id)));
        Assert.Equal(LinkStatus.Ok, await add.Handle(new CompanyLinkCommand(main.Id, sub.Id)));
        Assert.Equal(LinkStatus.SameCompany, await add.Handle(new CompanyLinkCommand(main.Id, main.Id)));
        Assert.Equal(LinkStatus.NotFound, await add.Handle(new CompanyLinkCommand(main.Id, 999)));
        Assert.Single(_db.CompanyOutsourceCompany);

        Assert.Equal(LinkStatus.Ok, await new RemoveCompanyLinkHandler(_db).Handle(new CompanyLinkCommand(main.Id, sub.Id)));
        Assert.Empty(_db.CompanyOutsourceCompany);
    }
}
