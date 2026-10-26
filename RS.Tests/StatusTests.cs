using RS.MVC.Features.Dashboard;
using RS.MVC.Models;

namespace RS.Tests;

public class StatusTests
{
    private readonly FakePipeline _pipeline = new();

    [Fact]
    public async Task GroupsDaysUnderTheirSourceNewestFirstWithNames()
    {
        var at = new DateTimeOffset(2026, 10, 20, 8, 0, 0, TimeSpan.FromHours(3));
        _pipeline.Metrics = new PipelineMetrics(
            new DateOnly(2026, 10, 19), new DateOnly(2026, 10, 20),
            [new PipelineSourceMetrics("evrensel", at, "Collector", 5, 1, 3), new PipelineSourceMetrics("kizilbayrak", null, null, 0, 0, 0)],
            [
                new PipelineDailyMetrics(new DateOnly(2026, 10, 19), "evrensel", 2, 2, 0, 1, 0),
                new PipelineDailyMetrics(new DateOnly(2026, 10, 20), "evrensel", 3, 2, 1, 0, 1),
                new PipelineDailyMetrics(new DateOnly(2026, 10, 20), "kizilbayrak", 0, 0, 0, 0, 0),
            ],
            [new PipelineFailure("b", "evrensel", "https://evrensel.net/haber/2/y", null!, "Fetched", at, "HTTP 404")]);

        var page = await new GetStatusHandler(_pipeline).Handle(99);

        Assert.Equal([31], _pipeline.MetricsRequests); // at most 31 days
        Assert.Equal(["Evrensel", "Kızılbayrak"], page.Sources.Select(s => s.SourceName));
        var evrensel = page.Sources[0];
        Assert.Equal([20, 19], evrensel.Days.Select(d => d.Day.Day));
        Assert.Equal((5, 1, 3, "Collector"), (evrensel.Received, evrensel.Failed, evrensel.Open, evrensel.LastReceivedOrigin));
        Assert.Equal(("Evrensel", "Fetched", "HTTP 404"), (page.RecentFailures[0].SourceName, page.RecentFailures[0].Stage, page.RecentFailures[0].Reason));
    }

    [Fact]
    public async Task CollectingNowReportsEachFeedWithItsName()
    {
        _pipeline.Collected = new PipelineCollectResponse([
            new PipelineFeedRun("evrensel", 100, 97, 3, 0, null!),
            new PipelineFeedRun("kizilbayrak", 0, 0, 0, 0, "Feed answered HTTP 503."),
        ]);

        var feeds = await new CollectFeedsHandler(_pipeline).Handle();

        Assert.Equal(new CollectedFeed("evrensel", "Evrensel", 100, 97, 3, 0, null!), feeds[0]);
        Assert.Equal(("Kızılbayrak", "Feed answered HTTP 503."), (feeds[1].SourceName, feeds[1].Error));
    }

    [Fact]
    public async Task TheAisWorkComesWithEachSourceAndTheAgreement()
    {
        var at = new DateTimeOffset(2026, 10, 20, 8, 0, 0, TimeSpan.FromHours(3));
        _pipeline.Metrics = new PipelineMetrics(
            new DateOnly(2026, 10, 20), new DateOnly(2026, 10, 20),
            [new PipelineSourceMetrics("evrensel", at, "Collector", 5, 0, 10, 4, 3, 1)],
            [new PipelineDailyMetrics(new DateOnly(2026, 10, 20), "evrensel", 5, 5, 0, 0, 0, 2, 1, 2, 2, new PipelineArticleStates(1, 0, 2, 1, 1, 0, 0))],
            [],
            new PipelineAiMetrics(true, [new PipelineVerdictAgreement("NotRelevant", 1, 30)]));

        var page = await new GetStatusHandler(_pipeline).Handle(1);
        var queued = await new QueueForAiHandler(_pipeline).Handle("evrensel");

        var evrensel = Assert.Single(page.Sources);
        Assert.Equal((10, 4, 3, 1), (evrensel.Open, evrensel.OpenWithDraft, evrensel.OpenNotRelevant, evrensel.OpenNotAssessed));
        Assert.Equal((2, 1, 2, 2), Assert.Single(evrensel.Days) is var d ? (d.AssessedRelevant, d.AssessedUncertain, d.AssessedNotRelevant, d.Drafted) : default);
        Assert.Equal(new PipelineArticleStates(1, 0, 2, 1, 1, 0, 0), evrensel.Days[0].Now); // where that day's arrivals stand now
        Assert.Equal(new StatusAgreement("NotRelevant", 1, 30), Assert.Single(page.Ai.Agreement));
        Assert.True(page.Ai.Enabled);
        Assert.Equal(new QueuedForAi("evrensel", "Evrensel", 3, 1, false, true, 10), queued);
        Assert.Equal(["evrensel"], _pipeline.QueuedForAi);
    }
}

