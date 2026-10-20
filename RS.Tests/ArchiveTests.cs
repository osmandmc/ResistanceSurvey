using Microsoft.EntityFrameworkCore;
using RS.COMMON.Entities;
using RS.COMMON.Entities.LookupEntity;
using RS.EF;
using RS.MVC.Features.Archive;
using RS.MVC.Models;
using static RS.COMMON.Constants.Enums;

namespace RS.Tests;

public class ArchiveTests
{
    private static readonly DateTimeOffset Nov3 = new(2026, 11, 3, 10, 42, 0, TimeSpan.FromHours(3));

    private readonly RSDBContext _db = new(new DbContextOptionsBuilder<RSDBContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private readonly FakePipeline _pipeline = new();
    private readonly GetArchiveHandler _archive;

    public ArchiveTests() => _archive = new GetArchiveHandler(_db, _pipeline);

    private static PipelineStageResult Ok(int minutes) => new("Succeeded", Nov3.AddMinutes(minutes));

    private void Add(string id, IReadOnlyDictionary<string, PipelineStageResult> stages, PipelineOutcome? outcome = null)
    {
        _pipeline.Add(id, $"https://evrensel.net/haber/{id}", $"Haber {id}", Nov3);
        _pipeline.States[id] = (stages, outcome!);
    }

    private Task<ArchivePage> List(ArchiveFilter filter = ArchiveFilter.All, string? month = null, string? search = null) =>
        _archive.Handle(new GetArchiveQuery(month!, null!, search!, filter, null!));

    [Fact]
    public async Task StateComesFromTheStagesUntilSomeoneDecides()
    {
        Add("processing", new Dictionary<string, PipelineStageResult> { ["Submitted"] = Ok(0) });
        Add("failed", new Dictionary<string, PipelineStageResult>
        {
            ["Submitted"] = Ok(0),
            ["Fetched"] = new("Failed", Nov3.AddMinutes(1), Reason: "HTTP 404"),
        });
        Add("open", new Dictionary<string, PipelineStageResult> { ["Submitted"] = Ok(0), ["Fetched"] = Ok(1), ["Parsed"] = Ok(2) });

        var items = (await List()).Items.ToDictionary(i => i.ArticleId);

        Assert.Equal(ArchiveState.Processing, items["processing"].State);
        Assert.Equal((ArchiveState.Failed, "HTTP 404"), (items["failed"].State, items["failed"].Reason));
        Assert.Equal(ArchiveState.Open, items["open"].State);
        Assert.Equal(["Submitted", "Fetched", "Parsed"], items["open"].Timeline.Select(e => e.Step));
    }

    [Fact]
    public async Task TheAppsDecisionWinsAndNamesTheCase()
    {
        var parsed = new Dictionary<string, PipelineStageResult> { ["Parsed"] = Ok(2), ["Submitted"] = Ok(0) };
        // The pipeline has not heard about the link yet (outcome call failed); the app knows.
        Add("linked", parsed);
        Add("dismissed", parsed, new PipelineOutcome("Dismissed", Nov3.AddHours(1), Reason: "Spor haberi"));
        var category = new Category { Name = "Grev" };
        var resistance = new Resistance { Name = "Korteks grevi", Category = category };
        var deleted = new Resistance { Name = "Silinmiş", Category = category, Deleted = true };
        var news = new News { ArticleId = "linked", Link = "https://evrensel.net/haber/linked", Status = Status.Linked, Source = 1 };
        _db.AddRange(resistance, deleted, news, new News { ArticleId = "dismissed", Status = Status.Passive, Source = 1 });
        await _db.SaveChangesAsync();
        _db.ResistanceNews.AddRange(
            new ResistanceNews { NewsId = news.Id, ResistanceId = resistance.Id },
            new ResistanceNews { NewsId = news.Id, ResistanceId = deleted.Id });
        await _db.SaveChangesAsync();

        var items = (await List()).Items.ToDictionary(i => i.ArticleId);

        Assert.Equal(ArchiveState.Linked, items["linked"].State);
        Assert.Equal([new ArchiveCase(resistance.Id, "Korteks grevi")], items["linked"].Cases);
        Assert.Equal((ArchiveState.Dismissed, "Spor haberi"), (items["dismissed"].State, items["dismissed"].Reason));
        Assert.Equal(["Submitted", "Parsed", "Outcome"], items["dismissed"].Timeline.Select(e => e.Step));
    }

    [Theory]
    [InlineData(ArchiveFilter.All, "all", "any")]
    [InlineData(ArchiveFilter.Open, "parsed", "open")]
    [InlineData(ArchiveFilter.Handled, "all", "handled")]
    [InlineData(ArchiveFilter.Failed, "failed", "any")]
    public async Task FilterBecomesThePipelinesStatusAndOutcome(ArchiveFilter filter, string status, string outcome)
    {
        await List(filter, search: "  işçi ");

        var query = Assert.Single(_pipeline.Queries);
        Assert.Equal((status, outcome, "işçi"), (query.Status, query.Outcome, query.Search));
        Assert.Null(query.From); // no month: the whole archive
    }

    [Fact]
    public async Task MonthLimitsTheDates()
    {
        await List(month: "2026-11");

        var query = Assert.Single(_pipeline.Queries);
        Assert.Equal((new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30)), (query.From, query.To));
    }

    [Fact]
    public async Task TooShortSearchIsRefused() =>
        await Assert.ThrowsAsync<ArgumentException>(() => List(search: " a "));
}
