using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using RS.COMMON.Entities;
using RS.COMMON.Entities.LookupEntity;
using RS.EF;
using RS.MVC.Applications;
using RS.MVC.Features.Review;

namespace RS.Tests;

/// <summary>R30: the pipeline's drafts offer the app's current lists; R45: sent when a save changed them.</summary>
[Collection("ListsPublisher")] // it remembers the last send for the whole app
public class ListsSyncTests
{
    private readonly string _dbName = Guid.NewGuid().ToString();
    private readonly RSDBContext _db;
    private readonly FakePipeline _pipeline = new();

    public ListsSyncTests()
    {
        _db = new(new DbContextOptionsBuilder<RSDBContext>().UseInMemoryDatabase(_dbName).Options);
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
    public async Task OnlyASaveThatChangesAListAsksForASend()
    {
        var (services, sync) = Services();
        await using var db = services.GetRequiredService<RSDBContext>();

        db.Company.Add(new Company { Name = "Yeni şirket" });
        await db.SaveChangesAsync();
        var afterCompany = sync.ChangeWaiting;
        var reason = await db.ResistanceReason.FirstAsync(r => r.Name == "Düşük ücret");
        reason.Name = "Düşük ücretler";
        await db.SaveChangesAsync();

        Assert.False(afterCompany);
        Assert.True(sync.ChangeWaiting);
    }

    [Fact]
    public async Task ListsAreSentAtStartupAndOnceForChangesCloseTogether()
    {
        var (services, sync) = Services();
        await sync.StartAsync(default);
        try
        {
            await Until(() => _pipeline.SentLists.Count == 1); // at startup
            await using (var db = services.CreateScope().ServiceProvider.GetRequiredService<RSDBContext>())
            {
                db.ProtestoType.Add(new ProtestoType { Name = "Grev" });
                await db.SaveChangesAsync();
                db.City.Add(new City { Id = 16, Name = "Bursa" });
                await db.SaveChangesAsync();
            }
            await Until(() => _pipeline.SentLists.Count == 2);
            await Task.Delay(300); // nothing more follows

            Assert.Equal(2, _pipeline.SentLists.Count);
            Assert.Equal(["Grev"], _pipeline.SentLists[1]["protestTypes"]);
            Assert.Equal(["Ankara", "Bursa", "İstanbul"], _pipeline.SentLists[1]["cities"]);
        }
        finally
        {
            await sync.StopAsync(default);
        }
    }

    /// <summary>The app's wiring for these tests: the save hook, the sender (short waits) and the publisher.</summary>
    private (ServiceProvider Services, ListsSync Sync) Services()
    {
        var services = new ServiceCollection()
            .AddSingleton<IPipelineClient>(_pipeline)
            .AddSingleton<ListsChangeInterceptor>()
            .AddScoped<ListsPublisher>()
            .AddSingleton(sp => new ListsSync(sp.GetRequiredService<IServiceScopeFactory>(), NullLogger<ListsSync>.Instance)
                { StartWait = TimeSpan.Zero, ChangeWait = TimeSpan.FromMilliseconds(100) })
            .AddDbContext<RSDBContext>((sp, options) =>
                options.UseInMemoryDatabase(_dbName).AddInterceptors(sp.GetRequiredService<ListsChangeInterceptor>()))
            .BuildServiceProvider();
        return (services, services.GetRequiredService<ListsSync>());
    }

    private static async Task Until(Func<bool> done)
    {
        for (var i = 0; i < 100 && !done(); i++)
            await Task.Delay(50);
        Assert.True(done());
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
