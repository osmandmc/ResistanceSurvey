using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RS.EF;
using RS.MVC.Applications;
using Microsoft.EntityFrameworkCore;
using RS.MVC.Models;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

namespace RS.MVC
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IResistanceApplication, ResistanceApplication>();
            services.AddScoped<INewsApplication, NewsApplication>();
             services.AddScoped<IUserApplication, UserApplication>();
            services.AddDbContext<RSDBContext>(options => options.UseSqlServer(Configuration.GetConnectionString("RSConnectionString")));
            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.DateFormatString = "dd.MM.yyyy";
            });
            services.Configure<RequestLocalizationOptions>(opts =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("tr-TR"),
                };

                opts.DefaultRequestCulture = new RequestCulture("tr-TR");
                // Formatting numbers, dates, etc.
                opts.SupportedCultures = supportedCultures;
                // UI strings that we have localized.
                opts.SupportedUICultures = supportedCultures;
            });
            services.AddSession();
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            // configure jwt authentication
            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
            
            // configure DI for application services
           
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // if (env.IsDevelopment())
            // {
                app.UseDeveloperExceptionPage();
            // }
            // else
            // {
            //     app.UseExceptionHandler("/Home/Error");
            // }

            app.UseStaticFiles();
            // app.UseSession();
            //Add JWToken to all incoming HTTP Request Header
            // app.Use(async (context, next) =>
            // {
            //     var JWToken = context.Session.GetString("JWToken");
            //     if (!string.IsNullOrEmpty(JWToken))
            //     {
            //         context.Request.Headers.Add("Authorization", "Bearer " + JWToken);
            //     }
            //     await next();
            // });
            app.UseAuthentication();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
