using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using RS.COMMON.Entities;

namespace RS.MVC.Features.Activity
{
    /// <summary>
    /// The activity log's central part (F4b), run on every save: a new log line without a person gets the signed-in
    /// user, and the request remembers whether it changed data and whether it wrote a line (for <see cref="ActivityGuard"/>).
    /// </summary>
    public class ActivityInterceptor(IHttpContextAccessor http) : SaveChangesInterceptor
    {
        public const string ChangedData = "activity:changed";
        public const string WroteLine = "activity:logged";

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            Track(eventData.Context);
            return result;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            Track(eventData.Context);
            return ValueTask.FromResult(result);
        }

        private void Track(DbContext db)
        {
            if (db == null)
                return;
            var request = http.HttpContext;
            var changes = db.ChangeTracker.Entries().Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted).ToList();
            foreach (var line in changes.Select(e => e.Entity).OfType<ActivityEntry>().Where(a => a.Username == null))
                line.Username = request?.User?.Identity?.Name;
            if (request == null)
                return;
            if (changes.Any(e => e.Entity is not ActivityEntry))
                request.Items[ChangedData] = true;
            if (changes.Any(e => e.Entity is ActivityEntry))
                request.Items[WroteLine] = true;
        }
    }

    /// <summary>On an endpoint that changes data but deliberately writes no log line; the reason is for the reader.</summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class NotLoggedAttribute(string reason) : Attribute
    {
        public string Reason { get; } = reason;
    }

    /// <summary>
    /// Catches a change without a log line: a request that saved data but wrote no activity line is reported in the
    /// app's log (Azure: Log stream), unless the endpoint is marked <see cref="NotLoggedAttribute"/>.
    /// </summary>
    public class ActivityGuard(ILogger<ActivityGuard> logger) : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await next();
            var items = context.HttpContext.Items;
            if (items.ContainsKey(ActivityInterceptor.ChangedData) && !items.ContainsKey(ActivityInterceptor.WroteLine)
                && !context.ActionDescriptor.EndpointMetadata.OfType<NotLoggedAttribute>().Any())
            {
                logger.LogWarning("{Method} {Path} changed data but wrote no activity line (F4b): add one, or mark the endpoint [NotLogged]",
                    context.HttpContext.Request.Method, context.HttpContext.Request.Path);
            }
        }
    }
}
