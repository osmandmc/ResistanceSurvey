using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RS.MVC.Infrastructure
{
    /// <summary>
    /// Errors no controller handled, for the JSON API (/api/*): a JSON answer with a message the screens show (F6).
    /// The pipeline unreachable → 502 with the cause; anything else → 500 with an error code that is also in the log,
    /// so a user's report can be found. Other paths fall through to the error page. A request the browser cancelled
    /// never gets here: ASP.NET Core answers it with 499 and logs it only at Debug.
    /// </summary>
    public class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger, IHostEnvironment environment) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            // By now the middleware has pointed Request.Path at the error page; the original path is in this feature.
            var path = context.Features.Get<IExceptionHandlerPathFeature>()?.Path ?? context.Request.Path;
            if (!path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
                return false;

            int status;
            string message;
            // The pipeline is the only service the app calls over HTTP.
            if (exception is HttpRequestException or TaskCanceledException)
            {
                var cause = exception switch
                {
                    HttpRequestException { StatusCode: { } code } => $"HTTP {(int)code}",
                    TaskCanceledException => "zaman aşımı",
                    _ => "bağlantı hatası",
                };
                logger.LogWarning(exception, "Pipeline unavailable for {Path}: {Cause}", path, cause);
                status = StatusCodes.Status502BadGateway;
                message = $"Haber işleme hattına ulaşılamadı ({cause}).";
            }
            else
            {
                var errorCode = ErrorCode(context);
                logger.LogError(exception, "Unexpected error {ErrorCode} for {Method} {Path}", errorCode, context.Request.Method, path);
                status = StatusCodes.Status500InternalServerError;
                message = $"Beklenmeyen bir hata oluştu. Hata kodu: {errorCode}";
                if (environment.IsDevelopment())
                    message += $" ({exception.GetType().Name}: {exception.Message})";
            }

            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(new { error = message }, cancellationToken);
            return true;
        }

        /// <summary>Short enough to read out or paste: the first 8 characters of the request's trace id.</summary>
        public static string ErrorCode(HttpContext context)
        {
            var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
            return traceId.Length > 8 ? traceId[..8] : traceId;
        }
    }
}
