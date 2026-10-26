using Microsoft.EntityFrameworkCore;
using RS.COMMON.Entities.LookupEntity;
using RS.EF;
using RS.MVC.Features.Review;

namespace RS.Tests;

/// <summary>R30: the pipeline's drafts offer the app's current lists.</summary>
[Collection("ListsPublisher")] // it remembers the last send for the whole app
public class ListsSyncTests
{
    private readonly RSDBContext _db = new(new DbContextOptionsBuilder<RSDBContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    private readonly FakePipeline _pipeline = new();

    public ListsSyncTests()
    {
        ListsPublisher.Forget();
        _db.ResistanceReason.AddRange(new ResistanceReason { Name = " TİS " }, new ResistanceReason { Name = "Düşük ücret" }, new ResistanceReason { Name = "" });
        _db.City.AddRange(new City { Id = 34, Name = "İstanbul" }, new City { Id = 6, Name = "Ankara" });
        _db.SaveChanges();
    }

    [Fact]
    public async Task ListsAreSentOnlyWhenTheyChangedUnlessAsked()
    {
        var publisher = new ListsPublisher(_db, _pipeline);

        var first = await publisher.PublishAsync();
        var again = await publisher.PublishAsync();
        _db.ResistanceReason.Add(new ResistanceReason { Name = "Yeni neden" });
        await _db.SaveChangesAsync();
        var changed = await publisher.PublishAsync();
        var forced = await publisher.PublishAsync(force: true);

        Assert.NotNull(first);
        Assert.Null(again);
        Assert.NotNull(changed);
        Assert.NotNull(forced);
        Assert.Equal(3, _pipeline.SentLists.Count);
        var sent = _pipeline.SentLists[0];
        Assert.Equal(["categories", "reasons", "employmentTypes", "tradeUnionAuthorities", "protestTypes", "protestPlaces", "interventionTypes", "cities"], sent.Keys);
        Assert.Equal(["Düşük ücret", "TİS"], sent["reasons"]); // trimmed, no blanks, Turkish order
        Assert.Equal(["Ankara", "İstanbul"], sent["cities"]);
        Assert.Contains("Yeni neden", _pipeline.SentLists[1]["reasons"]);
    }

    [Fact]
    public async Task ThePromptToCopyOffersTheSameLists()
    {
        _pipeline.Add("a1", "https://www.evrensel.net/haber/1/x", "Başlık", DateTimeOffset.Now, "Metin.");

        var prompt = await new GetAiPromptHandler(_pipeline, _db).Handle(new GetAiPromptQuery("a1"));

        Assert.Contains("### Direniş nedenleri\n- Düşük ücret\n- TİS\n", prompt.Prompt.Text);
    }
}

[CollectionDefinition("ListsPublisher", DisableParallelization = true)]
public sealed class ListsPublisherCollection;
