using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RS.EF;
using RS.MVC.Applications;
using RS.MVC.Features;
using RS.MVC.Features.Articles;
using RS.MVC.Features.Review;
using RS.MVC.Models;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.IO;
using Microsoft.Extensions.FileProviders;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Load Configuration
var configuration = builder.Configuration;

// On Azure App Service, send log messages to its Log stream and log files (no effect elsewhere).
builder.Logging.AddAzureWebAppDiagnostics();

// Add Services

// Use cases of the new screens (commands and queries under Features/), and what they share.
builder.Services.AddFeatureHandlers();
builder.Services.AddScoped<ArticleNewsService>();
builder.Services.AddScoped<OutcomeSender>();
builder.Services.AddScoped<ArticleCaseLinker>();
builder.Services.AddScoped<ListValueResolver>();
builder.Services.AddScoped<RS.MVC.Features.Cases.CaseWriter>();
builder.Services.AddScoped<RS.MVC.Features.Review.ListsPublisher>();
builder.Services.AddScoped<RS.MVC.Features.Review.AiSuggestionRecorder>();

// Test environment (Azure) only: bring its copy of the database up to date in the background, after startup.
if (builder.Environment.IsEnvironment("Test"))
    builder.Services.AddHostedService<TestDatabaseMigrator>();
builder.Services.AddScoped<IUserApplication, UserApplication>();

// Database Context
builder.Services.AddDbContext<RSDBContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("RSConnectionString")));

// R30: the pipeline's drafts offer the app's current lists (sent at startup and when they change).
if (!string.IsNullOrWhiteSpace(configuration["Pipeline:BaseUrl"]))
    builder.Services.AddHostedService<RS.MVC.Features.Review.ListsSync>();

// API controllers, and the one view left: the error page.
builder.Services.AddControllersWithViews();

// News pipeline: every news item enters through it (R12b)
builder.Services.AddOptions<PipelineSettings>()
    .Bind(configuration.GetSection(PipelineSettings.SectionName))
    .Validate(s => s.IsValid(), "Pipeline: BaseUrl must be a full URL and FunctionKey must be set with it.")
    .ValidateOnStart();
builder.Services.AddHttpClient<IPipelineClient, PipelineClient>((sp, http) =>
{
    var settings = sp.GetRequiredService<IOptions<PipelineSettings>>().Value;
    if (Uri.TryCreate(settings.BaseUrl, UriKind.Absolute, out var baseUrl))
        http.BaseAddress = baseUrl;
    http.DefaultRequestHeaders.Add("x-functions-key", settings.FunctionKey ?? "");
    http.Timeout = TimeSpan.FromMinutes(2);
});

// Secrets are not in the repository: user-secrets locally, written by the deploy in production. Fail fast if missing.
var missing = new[] { "ConnectionStrings:RSConnectionString", "AppSettings:AdminPass", "AppSettings:UserPass" }
    .Where(k => string.IsNullOrWhiteSpace(configuration[k]))
    .ToList();
if (missing.Count > 0)
    throw new InvalidOperationException("Missing configuration: " + string.Join(", ", missing) + ". Locally use 'dotnet user-secrets'; see README.");

// F11: a local run must not write to a real database (it once used production's). In Development the database has to
// be on this machine, e.g. the copy made by tools/copy-test-db.sh; LocalDev:AllowRemoteDatabase=true overrides.
if (builder.Environment.IsDevelopment() && !configuration.GetValue<bool>("LocalDev:AllowRemoteDatabase")
    && !RS.MVC.Infrastructure.LocalDatabase.IsLocal(configuration.GetConnectionString("RSConnectionString")))
    throw new InvalidOperationException(
        "Development uses a database on another machine (possibly production). Make a local copy with tools/copy-test-db.sh; see README, \"Local database\".");

// Sign-in: an HttpOnly cookie set by /api/auth/login (the old pages' bearer token went with them, R12c).
builder.Services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = "rs.auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        // Local development runs over plain http; everywhere else the cookie is HTTPS-only.
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        // An API answers 401/403; the front end decides where to go (there is no server-side login page).
        options.Events.OnRedirectToLogin = context => { context.Response.StatusCode = StatusCodes.Status401Unauthorized; return Task.CompletedTask; };
        options.Events.OnRedirectToAccessDenied = context => { context.Response.StatusCode = StatusCodes.Status403Forbidden; return Task.CompletedTask; };
    });

// Errors no controller handled: JSON for /api (ApiExceptionHandler), the error page for the old pages (F6).
builder.Services.AddExceptionHandler<RS.MVC.Infrastructure.ApiExceptionHandler>();

// Build the App
var app = builder.Build();


// Middleware Setup
app.UseExceptionHandler("/Home/Error");
app.UseStaticFiles();

// The front end (RS.Web) is built into wwwroot/rsweb and served at /app. Skipped when it has not been built.
var rsWebPath = Path.Combine(app.Environment.WebRootPath, "rsweb");
var rsWebFiles = Directory.Exists(rsWebPath) ? new PhysicalFileProvider(rsWebPath) : null;
if (rsWebFiles != null)
    app.UseStaticFiles(new StaticFileOptions { FileProvider = rsWebFiles, RequestPath = "/app" });

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// API controllers use attribute routes; "/" and "/Home/Error" use the conventional one.
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}");

// The new front end's own router handles the paths under /app.
if (rsWebFiles != null)
    app.MapFallbackToFile("app/{*path:nonfile}", "index.html", new StaticFileOptions { FileProvider = rsWebFiles });


app.Run();
