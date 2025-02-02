using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RS.EF;
using RS.MVC.Applications;
using Microsoft.EntityFrameworkCore;
using RS.MVC.Models;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace RS.MVC
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IResistanceApplication, ResistanceApplication>();
            services.AddScoped<INewsApplication, NewsApplication>();
            services.AddScoped<IUserApplication, UserApplication>();
            
            // Add DbContext and configure connection string
            services.AddDbContext<RSDBContext>(options => 
                options.UseSqlServer(Configuration.GetConnectionString("RSConnectionString")));
            
            // Add MVC with views
            services.AddControllersWithViews()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                });
            // Configure localization
            services.Configure<RequestLocalizationOptions>(opts =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("tr-TR"),
                };

                opts.DefaultRequestCulture = new RequestCulture("tr-TR");
                opts.SupportedCultures = supportedCultures;
                opts.SupportedUICultures = supportedCultures;
            });

            // Add Session
            services.AddSession();

            // Bind AppSettings from configuration
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            // JWT Authentication Configuration
            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStaticFiles();
            app.UseRouting();

            // Add Authentication
            app.UseAuthentication();
            app.UseAuthorization();

            // Define the default route
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                
                // Catch-all route for Vue paths under /Resistance/IndexVue
                endpoints.MapFallbackToController("IndexVue", "Resistance");

            });
        }
    }
}
