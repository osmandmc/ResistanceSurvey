using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RS.MVC.Features.Activity;
using RS.MVC.Features.History;

namespace RS.Tests;

/// <summary>
/// The signed-in person as the activity log and the change history see it (F4b, R39): a request whose user can change
/// between calls. Setting the name starts a new request.
/// </summary>
public sealed class TestUser
{
    public HttpContextAccessor Http { get; } = new() { HttpContext = new DefaultHttpContext() };

    public string? Name
    {
        set => Http.HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(value == null ? new ClaimsIdentity() : new ClaimsIdentity([new Claim(ClaimTypes.Name, value)], "test")) };
    }

    /// <summary>Adds the app's save hooks, so log lines and recorded changes get this user.</summary>
    public DbContextOptionsBuilder<T> Track<T>(DbContextOptionsBuilder<T> options) where T : DbContext =>
        options.AddInterceptors(new ActivityInterceptor(Http), new ChangeHistoryInterceptor(Http));
}
