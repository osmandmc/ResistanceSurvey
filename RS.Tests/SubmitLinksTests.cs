using RS.MVC.Features.Upload;

namespace RS.Tests;

public class SubmitLinksTests
{
    private readonly FakePipeline _pipeline = new();

    [Fact]
    public void AListIsSplitOnLinesSpacesCommasAndSemicolons()
    {
        var text = "https://evrensel.net/haber/1\r\nhttps://evrensel.net/haber/2, https://evrensel.net/haber/3;https://evrensel.net/haber/4\n\n" +
                   "  https://evrensel.net/haber/1  \t(https://evrensel.net/haber/5).";

        Assert.Equal(
            ["https://evrensel.net/haber/1", "https://evrensel.net/haber/2", "https://evrensel.net/haber/3", "https://evrensel.net/haber/4", "https://evrensel.net/haber/5"],
            SubmitLinksHandler.Split(text));
    }

    [Fact]
    public async Task EachLinkGetsItsOwnResultInTheOrderEntered()
    {
        _pipeline.KnownUrls.Add("https://evrensel.net/haber/2");

        var result = await new SubmitLinksHandler(_pipeline).Handle(new SubmitLinksCommand(
            "https://evrensel.net/haber/1\nhttps://evrensel.net/haber/2\nevrensel haberi\nftp://evrensel.net/x"));

        Assert.Null(result.Error);
        Assert.Equal(
            [("https://evrensel.net/haber/1", "Accepted"), ("https://evrensel.net/haber/2", "Duplicate"), ("evrensel", "Invalid"), ("haberi", "Invalid"), ("ftp://evrensel.net/x", "Invalid")],
            result.Links.Select(l => (l.Url, l.Status)));
        Assert.Equal(2, _pipeline.Submitted.Count); // only real links reach the pipeline
    }

    [Fact]
    public async Task EmptyOrTooManyIsRefusedWithoutSending()
    {
        var empty = await new SubmitLinksHandler(_pipeline).Handle(new SubmitLinksCommand("  \n "));
        var tooMany = await new SubmitLinksHandler(_pipeline).Handle(new SubmitLinksCommand(
            string.Join('\n', Enumerable.Range(1, SubmitLinksHandler.MaxLinks + 1).Select(i => $"https://evrensel.net/haber/{i}"))));

        Assert.NotNull(empty.Error);
        Assert.NotNull(tooMany.Error);
        Assert.Empty(_pipeline.Submitted);
    }
}
