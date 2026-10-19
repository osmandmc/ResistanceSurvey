using Microsoft.AspNetCore.Builder;
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
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Load Configuration
var configuration = builder.Configuration;

// Add Services
builder.Services.AddScoped<IResistanceApplication, ResistanceApplication>();
builder.Services.AddScoped<INewsApplication, NewsApplication>();
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

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
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

// Catch-all Route for Vue 3
app.MapFallbackToController("IndexVue", "Resistance"); // Ensure only one catch-all route


app.Run();
