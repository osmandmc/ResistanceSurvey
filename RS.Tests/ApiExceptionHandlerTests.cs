using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging.Abstractions;
using RS.MVC.Infrastructure;

namespace RS.Tests;

public class ApiExceptionHandlerTests
{
    private static async Task<(bool Handled, int Status, string? Error)> Handle(Exception exception, string path = "/api/inbox", string environment = "Production")
    {
        // As the middleware leaves it: Request.Path already points at the error page, the original path is in the feature.
        var context = new DefaultHttpContext();
        context.Request.Path = "/Home/Error";
        context.Features.Set<IExceptionHandlerPathFeature>(new ExceptionHandlerFeature { Path = path, Error = exception });
        context.Response.Body = new MemoryStream();
        var handler = new ApiExceptionHandler(NullLogger<ApiExceptionHandler>.Instance, new HostingEnvironment { EnvironmentName = environment });

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        context.Response.Body.Position = 0;
        var body = new StreamReader(context.Response.Body).ReadToEnd();
        var error = body.Length == 0 ? null : JsonDocument.Parse(body).RootElement.GetProperty("error").GetString();
        return (handled, context.Response.StatusCode, error);
    }

    [Theory]
    [InlineData(HttpStatusCode.ServiceUnavailable, "Haber işleme hattına ulaşılamadı (HTTP 503).")]
    [InlineData(null, "Haber işleme hattına ulaşılamadı (bağlantı hatası).")]
    public async Task PipelineFailuresAre502WithTheCause(HttpStatusCode? status, string message) =>
        Assert.Equal((true, 502, message), await Handle(new HttpRequestException("x", null, status)));

    [Fact]
    public async Task APipelineTimeoutSaysSo() =>
        Assert.Equal((true, 502, "Haber işleme hattına ulaşılamadı (zaman aşımı)."), await Handle(new TaskCanceledException()));

    [Fact]
    public async Task AnythingElseIs500WithAnErrorCodeButNoDetails()
    {
        var (handled, status, error) = await Handle(new InvalidOperationException("secret detail"));

        Assert.Equal((true, 500), (handled, status));
        Assert.StartsWith("Beklenmeyen bir hata oluştu. Hata kodu: ", error);
        Assert.DoesNotContain("secret detail", error);
    }

    [Fact]
    public async Task DevelopmentAlsoShowsTheException()
    {
        var (_, _, error) = await Handle(new InvalidOperationException("detail"), environment: Environments.Development);

        Assert.EndsWith("(InvalidOperationException: detail)", error);
    }

    [Fact]
    public async Task OldPagesAreLeftToTheErrorPage() =>
        Assert.Equal((false, 200, null), await Handle(new InvalidOperationException(), path: "/Resistance/Index"));
}
