using Microsoft.EntityFrameworkCore;
using RS.COMMON.Constants;
using RS.COMMON.Entities.LookupEntity;
using RS.EF;
using RS.MVC.Features.Definitions;
using RS.MVC.Features.Review;

namespace RS.Tests;

/// <summary>R34: unions and confederations as classes of the organization table; "Kurum Ekle".</summary>
public class OrganizationTests
{
    private readonly RSDBContext _db = new(new TestUser { Name = "ayse" }.Track(new DbContextOptionsBuilder<RSDBContext>())
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private readonly Confederation _disk = new() { Name = "DİSK", CorporationTypeId = KnownListIds.ConfederationCorporationType, Approved = true };

    public OrganizationTests()
    {
        _db.CorporationType.AddRange(
            new CorporationType { Id = KnownListIds.TradeUnionCorporationType, Name = "Sendika" },
            new CorporationType { Id = 4, Name = "Dayanışma Platformu" },
            new CorporationType { Id = KnownListIds.ConfederationCorporationType, Name = "Konfederasyon" });
        _db.Corporation.Add(_disk);
        _db.SaveChanges();
    }

    private Task<CreateCorporationResult> Create(string name, int? typeId, int? confederationId = null, bool approved = false) =>
        new CreateCorporationHandler(_db).Handle(new CreateCorporationCommand(name, typeId, confederationId, approved));

    [Theory]
    [InlineData(KnownListIds.TradeUnionCorporationType, OrganizationKind.TradeUnion)]
    [InlineData(KnownListIds.ConfederationCorporationType, OrganizationKind.Confederation)]
    [InlineData(4, OrganizationKind.Organization)]
    [InlineData(null, OrganizationKind.Organization)]
    public void TheTypeDecidesTheClass(int? typeId, OrganizationKind kind) => Assert.Equal(kind, Organizations.KindOf(typeId));

    [Fact]
    public async Task KurumEkleCreatesAUnionWithItsConfederation()
    {
        var result = await Create(" DİSK  Genel-İş ", KnownListIds.TradeUnionCorporationType, _disk.Id);

        Assert.Equal((false, "DİSK Genel-İş", true), (result.Existed, result.Corporation.Name, result.Corporation.IsTradeUnion));
        var union = Assert.IsType<TradeUnion>(_db.Corporation.Single(c => c.Id == result.Corporation.Id));
        Assert.Equal((_disk.Id, false), (union.ConfederationId, union.Approved)); // from a case: waits for approval
        Assert.Equal("definition.corporation-create", _db.ActivityEntry.Single().Action);
    }

    [Fact]
    public async Task AnExistingNameIsReturnedInsteadOfADuplicate()
    {
        var first = await Create("Edremit Demokrasi Platformu", 4);

        var again = await Create("EDREMİT DEMOKRASİ PLATFORMU", 4);

        Assert.Equal((true, first.Corporation.Id), (again.Existed, again.Corporation.Id));
        Assert.IsNotType<TradeUnion>(_db.Corporation.Single(c => c.Id == first.Corporation.Id));
    }

    [Fact]
    public async Task AConfederationIdThatIsNotOneIsIgnoredAndAnUnknownTypeRefused()
    {
        var platform = await Create("Bir Platform", 4, approved: true);

        var union = await Create("Bağımsız Sendika", KnownListIds.TradeUnionCorporationType, platform.Corporation.Id);

        Assert.Null(Assert.IsType<TradeUnion>(_db.Corporation.Single(c => c.Id == union.Corporation.Id)).ConfederationId);
        Assert.Equal("Bilinmeyen kurum türü.", (await Create("X", 99)).Error);
        Assert.Equal("Lütfen kurumun adını giriniz.", (await Create("  ", 4)).Error);
    }

    [Fact]
    public async Task ConfederationsCountAsUnionsInTheForm()
    {
        await Create("DİSK Genel-İş", KnownListIds.TradeUnionCorporationType, _disk.Id);
        await Create("Bir Platform", 4);

        var options = await new GetCaseFormOptionsHandler(_db).Handle(new GetCaseFormOptionsQuery());

        Assert.Equal([("Bir Platform", false), ("DİSK", true), ("DİSK Genel-İş", true)], options.Corporations.Select(c => (c.Name, c.IsTradeUnion)));
        Assert.Equal(["DİSK"], options.Confederations!.Select(c => c.Name));
        Assert.Equal(3, options.CorporationTypes!.Count);
    }

    [Fact]
    public async Task TanimlarShowsAndChangesAUnionsConfederation()
    {
        var kesk = new Confederation { Name = "KESK", CorporationTypeId = KnownListIds.ConfederationCorporationType, Approved = true };
        _db.Add(kesk);
        var union = new TradeUnion { Name = "KESK SES", CorporationTypeId = KnownListIds.TradeUnionCorporationType, Approved = true };
        _db.Add(union);
        await _db.SaveChangesAsync();

        await new UpdateDefinitionHandler(_db).Handle(new UpdateDefinitionCommand(
            DefinitionKind.Corporations, union.Id, "KESK SES", KnownListIds.TradeUnionCorporationType, ConfederationId: kesk.Id));

        var page = await new ListDefinitionsHandler(_db).Handle(new ListDefinitionsQuery(DefinitionKind.Corporations));
        Assert.Equal(kesk.Id, page.Items.Single(i => i.Name == "KESK SES").ConfederationId);
    }

    [Fact]
    public async Task AConfederationWithUnionsIsNotDeletedAndAMergeMovesItsUnions()
    {
        var typo = new Confederation { Name = "Disk", CorporationTypeId = KnownListIds.ConfederationCorporationType };
        _db.Add(typo);
        var union = new TradeUnion { Name = "DİSK BMİS", CorporationTypeId = KnownListIds.TradeUnionCorporationType, Confederation = typo };
        _db.Add(union);
        await _db.SaveChangesAsync();

        var deleted = await new DeleteDefinitionHandler(_db).Handle(new DeleteDefinitionCommand(DefinitionKind.Corporations, typo.Id));
        Assert.Equal(ChangeStatus.InUse, deleted.Status);

        await new MergeDefinitionsHandler(_db).Handle(new MergeDefinitionsCommand(DefinitionKind.Corporations, typo.Id, _disk.Id));
        Assert.Equal(_disk.Id, _db.Set<TradeUnion>().Single(u => u.Id == union.Id).ConfederationId);
    }

    [Fact]
    public void TheUnionQueriesTranslateToSqlServer()
    {
        using var sqlServer = new RSDBContext(new DbContextOptionsBuilder<RSDBContext>().UseSqlServer("Server=none;Database=none").Options);

        var options = sqlServer.Corporation.Select(e => new { e.Id, IsUnion = e is TradeUnion || e is Confederation }).ToQueryString();
        var confederation = sqlServer.Corporation.Select(c => c is TradeUnion ? ((TradeUnion)c).ConfederationId : null).ToQueryString();

        Assert.Contains("[Kind]", options);
        Assert.Contains("[ConfederationId]", confederation);
    }
}
