using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using RS.EF;
using RS.MVC.Applications;
using RS.MVC.Models;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Extensions.FileProviders;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Load Configuration
var configuration = builder.Configuration;

// Add Services
builder.Services.AddScoped<IResistanceApplication, ResistanceApplication>();
builder.Services.AddScoped<INewsApplication, NewsApplication>();
builder.Services.AddScoped<IInboxApplication, InboxApplication>();
builder.Services.AddScoped<IUserApplication, UserApplication>();

// Database Context
builder.Services.AddDbContext<RSDBContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("RSConnectionString")));

// Add MVC for both Razor Views & API Controllers
builder.Services.AddControllersWithViews();
   

// Enable CORS for Vue 3 Frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("VuePolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Vue dev server URL
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Localization Support
builder.Services.Configure<RequestLocalizationOptions>(opts =>
{
    var supportedCultures = new[] { new CultureInfo("tr-TR") };
    opts.DefaultRequestCulture = new RequestCulture("tr-TR");
    opts.SupportedCultures = supportedCultures;
    opts.SupportedUICultures = supportedCultures;
});

// News pipeline (empty CutoverMonth = off)
builder.Services.AddOptions<PipelineSettings>()
    .Bind(configuration.GetSection(PipelineSettings.SectionName))
    .Validate(s => s.IsValid(), "Pipeline: CutoverMonth must be empty or \"yyyy-MM\", and BaseUrl and FunctionKey must be set when it is.")
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
var missing = new[] { "ConnectionStrings:RSConnectionString", "AppSettings:Secret", "AppSettings:AdminPass", "AppSettings:UserPass" }
    .Where(k => string.IsNullOrWhiteSpace(configuration[k]))
    .ToList();
if (missing.Count > 0)
    throw new InvalidOperationException("Missing configuration: " + string.Join(", ", missing) + ". Locally use 'dotnet user-secrets'; see README.");

// JWT Authentication
var appSettingsSection = configuration.GetSection("AppSettings");
builder.Services.Configure<AppSettings>(appSettingsSection);
var appSettings = appSettingsSection.Get<AppSettings>();
var key = Encoding.ASCII.GetBytes(appSettings.Secret);

// Two ways to sign in: the new app (/app) uses an HttpOnly cookie; the existing Vue form sends a bearer token.
// "Auto" picks the bearer scheme when an Authorization header is present, otherwise the cookie.
builder.Services.AddAuthentication("Auto")
    .AddPolicyScheme("Auto", "Cookie or bearer token", options =>
    {
        options.ForwardDefaultSelector = context =>
            context.Request.Headers.Authorization.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? JwtBearerDefaults.AuthenticationScheme
                : CookieAuthenticationDefaults.AuthenticationScheme;
    })
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
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

// Build the App
var app = builder.Build();

// Test environment (Azure) only: bring its copy of the database up to date on startup.
// Production migrations stay a manual step. The test database pauses when idle, so retry while it wakes.
if (app.Environment.IsEnvironment("Test"))
{
    for (var attempt = 1; ; attempt++)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<RSDBContext>().Database.Migrate();
            break;
        }
        catch (Microsoft.Data.SqlClient.SqlException) when (attempt < 6)
        {
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(20));
        }
    }
}

// Middleware Setup
app.UseStaticFiles();

// The new front end (RS.Web) is built into wwwroot/rsweb and served at /app. wwwroot/app holds the old pages'
// scripts, which keep their /app/*.js addresses. Skipped when the front end has not been built.
var rsWebPath = Path.Combine(app.Environment.WebRootPath, "rsweb");
var rsWebFiles = Directory.Exists(rsWebPath) ? new PhysicalFileProvider(rsWebPath) : null;
if (rsWebFiles != null)
    app.UseStaticFiles(new StaticFileOptions { FileProvider = rsWebFiles, RequestPath = "/app" });

app.UseRouting();
app.UseCors("VuePolicy"); // Enable CORS for Vue 3
app.UseAuthentication();
app.UseAuthorization();

// Localization Middleware
// var localizationOptions = app.Services.GetRequiredService<RequestLocalizationOptions>();
// app.UseRequestLocalization(localizationOptions);

// Route Setup
app.MapControllerRoute(
    name: "news", // Explicit route for /News
    pattern: "News/{action=Index}/{id?}",
    defaults: new { controller = "News" });

app.MapControllerRoute(
    name: "default", // Default route for other Razor views
    pattern: "{controller=Home}/{action=Index}/{id?}");

// The new front end's own router handles the paths under /app.
if (rsWebFiles != null)
    app.MapFallbackToFile("app/{*path:nonfile}", "index.html", new StaticFileOptions { FileProvider = rsWebFiles });

// The old Vue app's own paths (e.g. /Resistance/IndexVue/edit/5) survive a page refresh.
// Only its paths: any other unknown address is a 404, not the old Vue page.
app.MapFallbackToController("Resistance/IndexVue/{*path:nonfile}", "IndexVue", "Resistance");


app.Run();
