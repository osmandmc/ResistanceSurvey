using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RS.COMMON;
using RS.DAL;
using RS.EF;
using RS.MVC.Applications;
using RS.MVC.Utilities;
using Microsoft.EntityFrameworkCore;

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
            services.AddTransient<IResistanceApplication, ResistanceApplication>();
            // services.AddScoped<IStorageUtilities, StorageUtilities>();
            // services.AddDbContext<RSDBContext>(options => options.UseMySQL(Configuration.GetConnectionString("RSConnectionString")));
            services.AddDbContext<RSDBContext>(options => options.UseSqlServer(Configuration.GetConnectionString("RSConnectionString")));
            services.AddScoped<IStorageUtilities>(s => new StorageUtilities(Configuration.GetConnectionString("RSConnectionString")));
            services.AddMvc();
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

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Resistance}/{action=Index}/{id?}");
            });
        }
    }
}
