using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RS.COMMON.Constants;
using RS.COMMON.Entities;
using RS.COMMON.Entities.LookupEntity;
using RS.EF;
using RS.MVC.Features.Activity;
using RS.MVC.Features.Cases;
using RS.MVC.Features.Review;
using RS.MVC.Models;
using static RS.COMMON.Constants.Enums;

namespace RS.Tests;

public class CaseDetailTests
{
    private readonly TestUser _user = new();
    private readonly RSDBContext _db;

    private readonly Resistance _case;

    public CaseDetailTests()
    {
        _db = new(_user.Track(new DbContextOptionsBuilder<RSDBContext>())
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options);
        _db.City.Add(new City { Id = 16, Name = "Bursa" });
        _db.ResistanceReason.Add(new ResistanceReason { Id = 3, Name = "Ücret alacağı" });
        _db.Company.AddRange(new Company { Id = 10, Name = "Örnek Tekstil" }, new Company { Id = 11, Name = "Örnek Holding" });
        _case = new Resistance
        {
            Name = "Tekstil direnişi",
            Description = "Ücretler ödenmedi",
            CategoryId = 1,
            ResistanceTargetType = ResistanceTargetType.Company,
            CompanyId = 10,
            ResistanceResult = ResistanceResult.Unknown,
            Note = "eski not",
            ResistanceResistanceReasons = [new ResistanceResistanceReason { ResistanceReasonId = 3 }],
            ResistanceCorporations = [new ResistanceCorporation { CorporationId = 4 }],
            ResistanceEmploymentTypes = [new ResistanceEmploymentType { EmploymentTypeId = 5 }],
            Protestos =
            [
                new Protesto { StartDate = new DateTime(2026, 9, 10), GenderId = 1, Locations = [new ProtestoLocation { CityId = 16, Place = "fabrika önü" }], ProtestoProtestoTypes = [new ProtestoProtestoType { ProtestoTypeId = 1 }] },
                new Protesto { StartDate = new DateTime(2026, 8, 1), GenderId = 1, Locations = [] },
                new Protesto { StartDate = new DateTime(2026, 7, 1), GenderId = 1, Deleted = true },
            ],
            ResistanceNews = [new ResistanceNews { News = new News { Header = "İlk haber", Link = "https://x/1", Date = new DateTime(2026, 9, 11), Status = Status.Linked } }],
        };
        _db.Resistance.Add(_case);
        _db.SaveChanges();
    }

    private static CaseInput ValidCase(Action<CaseInput>? change = null)
    {
        var c = ReviewValidationTests.CompleteNewCase().Case!;
        change?.Invoke(c);
        return c;
    }

    [Fact]
    public async Task TheDetailHasTheFormValuesProtestsAndArticles()
    {
        var detail = await new GetCaseHandler(_db).Handle(new GetCaseQuery(_case.Id));

        Assert.NotNull(detail);
        Assert.Equal(("Tekstil direnişi", 10, "Örnek Tekstil"), (detail.Case.Name, detail.Case.CompanyId, detail.CompanyName));
        Assert.Equal([3], detail.Case.ReasonIds);
        Assert.Equal([new DateTime(2026, 8, 1), new DateTime(2026, 9, 10)], detail.Protests.Select(p => p.Protest.StartDate!.Value));
        Assert.Equal(["Bursa"], detail.Protests[1].CityNames);
        Assert.Equal("İlk haber", Assert.Single(detail.Articles).Title);
        Assert.Null(await new GetCaseHandler(_db).Handle(new GetCaseQuery(999)));
    }

    [Fact]
    public async Task EditingReplacesTheListsKeepsTheNoteAndRecordsTheSubcontractor()
    {
        var input = ValidCase(c =>
        {
            c.Name = "Yeni ad";
            c.CompanyId = 10;
            c.IsOutsource = true;
            c.MainCompanyId = 11;
            c.ReasonIds = [];
            c.NewReasonNames = ["Mobbing"];
            c.CorporationIds = [7, 8];
            c.EmploymentTypeIds = [9];
        });

        var result = await new UpdateCaseHandler(_db, new CaseWriter(_db), new ListValueResolver(_db)).Handle(new UpdateCaseCommand(_case.Id, input, "ayse"));

        Assert.Equal(CaseCommandStatus.Ok, result.Status);
        var r = _db.Resistance.Single(x => x.Id == _case.Id);
        Assert.Equal(("Yeni ad", 11, "ayse", "eski not"), (r.Name, r.MainCompanyId, r.Updater, r.Note));
        Assert.Equal(["Mobbing"], _db.ResistanceResistanceReason.Where(x => x.ResistanceId == r.Id).Select(x => x.ResistanceReason.Name));
        Assert.Equal([7, 8], _db.ResistanceCorporation.Where(x => x.ResistanceId == r.Id).Select(x => x.CorporationId).OrderBy(x => x));
        Assert.Equal([9], _db.ResistanceEmploymentType.Where(x => x.ResistanceId == r.Id).Select(x => x.EmploymentTypeId));
        Assert.Single(_db.CompanyOutsourceCompany, c => c.CompanyId == 11 && c.OutsourceCompanyId == 10);
        Assert.Single(_db.ResistanceNews, rn => rn.ResistanceId == r.Id); // articles are not touched by editing
    }

    private Task<CaseCommandResult> Update(CaseInput input) =>
        new UpdateCaseHandler(_db, new CaseWriter(_db), new ListValueResolver(_db)).Handle(new UpdateCaseCommand(_case.Id, input, "ayse"));

    [Fact]
    public async Task AGenelCaseKeepsOnlyItsOrganizersAndReasons()
    {
        var input = ValidCase(c =>
        {
            c.CategoryId = KnownListIds.GeneralCategory;
            c.CorporationIds = [4, 7];
            c.Result = ResistanceResult.FullGain;
            c.TradeUnionId = 4;
        });

        Assert.Equal(CaseCommandStatus.Ok, (await Update(input)).Status);

        var r = _db.Resistance.Single(x => x.Id == _case.Id);
        Assert.Equal(((int?)null, (int?)null, (int?)null, ResistanceResult.Unknown, (int?)null), (r.CompanyId, r.MainCompanyId, r.EmployeeCountNumber, r.ResistanceResult, r.TradeUnionId));
        Assert.Equal([(4, OrganizationRole.Organizing), (7, OrganizationRole.Organizing)],
            _db.ResistanceCorporation.Where(x => x.ResistanceId == r.Id).OrderBy(x => x.CorporationId).Select(x => ValueTuple.Create(x.CorporationId, x.Role)));
        Assert.Empty(_db.ResistanceEmploymentType.Where(x => x.ResistanceId == r.Id));
    }

    [Fact]
    public async Task AnIsletmeCaseKeepsItsOrganizationsAsSupporting()
    {
        Assert.Equal(CaseCommandStatus.Ok, (await Update(ValidCase())).Status);

        Assert.All(_db.ResistanceCorporation.Where(x => x.ResistanceId == _case.Id), x => Assert.Equal(OrganizationRole.Supporting, x.Role));
        Assert.Equal(10, _db.Resistance.Single(x => x.Id == _case.Id).CompanyId);
    }

    [Fact]
    public async Task ADayanismaCaseLinksTheSupportedCaseAndShowsIt()
    {
        var supported = new Resistance { Name = "Migros depo işçileri", CategoryId = KnownListIds.WorkplaceCategory };
        _db.Resistance.Add(supported);
        await _db.SaveChangesAsync();
        var input = ValidCase(c =>
        {
            c.CategoryId = KnownListIds.SolidarityCategory;
            c.CompanyId = null;
            c.CorporationIds = [];
            c.SupportedCaseId = supported.Id;
        });

        Assert.Equal(CaseCommandStatus.Ok, (await Update(input)).Status);

        var detail = await new GetCaseHandler(_db).Handle(new GetCaseQuery(_case.Id));
        Assert.Equal(new CaseLink(supported.Id, "Migros depo işçileri"), detail!.SupportedCase);
        Assert.Equal(supported.Id, detail.Case.SupportedCaseId);
    }

    [Fact]
    public async Task ACaseCannotSupportItselfNorAMissingCase()
    {
        var self = await Update(ValidCase(c => { c.CategoryId = KnownListIds.SolidarityCategory; c.SupportedCaseId = _case.Id; }));
        var missing = await Update(ValidCase(c => { c.CategoryId = KnownListIds.SolidarityCategory; c.SupportedCaseId = 9999; }));

        Assert.Equal("Bir vaka kendisini destekleyemez.", self.Errors!["case.supportedCaseId"]);
        Assert.Equal("Desteklenen vaka bulunamadı.", missing.Errors!["case.supportedCaseId"]);
    }

    [Fact]
    public async Task EditingChecksRequiredFields()
    {
        var result = await new UpdateCaseHandler(_db, new CaseWriter(_db), new ListValueResolver(_db))
            .Handle(new UpdateCaseCommand(_case.Id, new CaseInput(), "ayse"));

        Assert.Equal(CaseCommandStatus.Invalid, result.Status);
        Assert.Contains("case.categoryId", result.Errors!.Keys);
        Assert.Equal("Tekstil direnişi", _db.Resistance.Single(x => x.Id == _case.Id).Name);
    }

    [Fact]
    public async Task AddingAnEarlierProtestMovesTheCaseStart()
    {
        var protest = ReviewValidationTests.CompleteProtest();
        protest.StartDate = new DateTime(2026, 6, 15);

        var result = await new SaveProtestHandler(_db, new CaseWriter(_db)).Handle(new SaveProtestCommand(_case.Id, null, protest, "ayse"));

        Assert.Equal(CaseCommandStatus.Ok, result.Status);
        var saved = _db.Protesto.Include(p => p.ProtestoProtestoTypes).Include(p => p.Locations).Single(p => p.Id == result.Id);
        Assert.Equal((2, 1, "ayse"), (saved.ProtestoProtestoTypes.Count, saved.Locations.Count, saved.Creator));
        Assert.Equal(new DateTime(2026, 6, 15), _db.Resistance.Single(x => x.Id == _case.Id).StartDate);
    }

    [Fact]
    public async Task EditingAProtestReplacesItsListsAndDeletingOneMovesTheStartBack()
    {
        var first = _case.Protestos.Single(p => p.StartDate == new DateTime(2026, 9, 10));
        var protest = ReviewValidationTests.CompleteProtest();
        protest.StartDate = new DateTime(2026, 9, 12);
        protest.ProtestTypeIds = [4];
        protest.Locations = [];

        await new SaveProtestHandler(_db, new CaseWriter(_db)).Handle(new SaveProtestCommand(_case.Id, first.Id, protest, "ayse"));
        await new DeleteProtestHandler(_db, new CaseWriter(_db)).Handle(new DeleteProtestCommand(_case.Id, _case.Protestos.Single(p => p.StartDate == new DateTime(2026, 8, 1)).Id, "ayse"));

        Assert.Equal([4], _db.ProtestoProtestoType.Where(t => t.ProtestoId == first.Id).Select(t => t.ProtestoTypeId));
        Assert.Empty(_db.ProtestoLocation.Where(l => l.ProtestoId == first.Id));
        Assert.Equal(new DateTime(2026, 9, 12), _db.Resistance.Single(x => x.Id == _case.Id).StartDate); // earliest remaining
    }

    [Fact]
    public async Task TheCaseHistoryNamesWhoDidWhatNewestFirst()
    {
        var protest = ReviewValidationTests.CompleteProtest();
        protest.StartDate = new DateTime(2026, 9, 12);
        var first = _case.Protestos.Single(p => p.StartDate == new DateTime(2026, 9, 10));

        _user.Name = "ayse";
        await new UpdateCaseHandler(_db, new CaseWriter(_db), new ListValueResolver(_db)).Handle(new UpdateCaseCommand(_case.Id, ValidCase(), "ayse"));
        _user.Name = "mehmet";
        await new SaveProtestHandler(_db, new CaseWriter(_db)).Handle(new SaveProtestCommand(_case.Id, first.Id, protest, "mehmet"));
        _user.Name = "ayse";
        await new DeleteCaseHandler(_db).Handle(new DeleteCaseCommand(_case.Id, "ayse"));

        var history = await new CaseActivityHandler(_db).Handle(_case.Id);
        Assert.Equal(
            [("ayse", Actions.CaseDelete), ("mehmet", Actions.ProtestUpdate), ("ayse", Actions.CaseUpdate)],
            history.Select(h => (h.Username, h.Action)));
        Assert.Equal("12.09.2026 eylemi", history[1].Summary);
        Assert.Equal(DateTimeKind.Utc, history[0].At.Kind);
    }

    [Fact]
    public async Task ADeletedCaseIsGoneFromTheDetail()
    {
        await new DeleteCaseHandler(_db).Handle(new DeleteCaseCommand(_case.Id, "ayse"));

        Assert.Null(await new GetCaseHandler(_db).Handle(new GetCaseQuery(_case.Id)));
        Assert.True(_db.Resistance.Single(x => x.Id == _case.Id).Deleted); // marked, not removed
    }

    [Fact]
    public async Task AttachingAndDetachingArticles()
    {
        var news = new News { Header = "İkinci haber", Link = "https://x/2", Date = new DateTime(2026, 10, 1), Status = Status.Pending };
        _db.News.Add(news);
        await _db.SaveChangesAsync();

        var found = await new SearchNewsHandler(_db).Handle(new SearchNewsQuery("İkinci", 2026, 10));
        await new AttachArticleHandler(_db).Handle(new AttachArticleCommand(_case.Id, Assert.Single(found).Id));

        Assert.Equal(Status.Linked, _db.News.Single(n => n.Id == news.Id).Status);
        Assert.Equal(2, _db.ResistanceNews.Count(rn => rn.ResistanceId == _case.Id));

        await new DetachArticleHandler(_db).Handle(new DetachArticleCommand(_case.Id, news.Id));

        Assert.Single(_db.ResistanceNews, rn => rn.ResistanceId == _case.Id);
        Assert.Single(_db.News, n => n.Id == news.Id); // the article itself stays
    }
}
