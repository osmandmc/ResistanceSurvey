using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RS.MVC.Features.Activity;

namespace RS.Tests;

/// <summary>The signed-in person as the activity log sees it (F4b): a request whose user can change between calls.</summary>
public sealed class TestUser
{
    public HttpContextAccessor Http { get; } = new() { HttpContext = new DefaultHttpContext() };

    public string? Name
    {
        set => Http.HttpContext!.User = new ClaimsPrincipal(value == null ? new ClaimsIdentity() : new ClaimsIdentity([new Claim(ClaimTypes.Name, value)], "test"));
    }

    /// <summary>Adds the app's save hook, so log lines get this user.</summary>
    public DbContextOptionsBuilder<T> Track<T>(DbContextOptionsBuilder<T> options) where T : DbContext =>
        options.AddInterceptors(new ActivityInterceptor(Http));
}
