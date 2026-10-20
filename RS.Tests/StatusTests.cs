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
}
