using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using RS.EF;
using RS.MVC.Applications;
using RS.MVC.Models;
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
