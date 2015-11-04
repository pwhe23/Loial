using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Data.Entity;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;

namespace Loial
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            // Setup configuration sources.
            var builder = new ConfigurationBuilder(appEnv.ApplicationBasePath)
                .AddJsonFile("config.json", optional: true)
                .AddJsonFile($"config.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {

            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            services.AddMvc();

            var provider = services.BuildServiceProvider();

            var cs = provider.GetService<IOptions<AppSettings>>().Options.LoialDb;
            services.AddEntityFramework()
                .AddSqlServer()
                .AddDbContext<LoialDb>(options => options.UseSqlServer(cs));

            services.AddScoped<Processor>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.MinimumLevel = LogLevel.Information;
            loggerFactory.AddConsole();

            //if (env.IsDevelopment())
            //{
                app.UseErrorPage();
            //}
            //else
            //{
                // Add Error handling middleware which catches all application specific errors and
                // sends the request to the following path or controller action.
                //app.UseErrorHandler("/Home/Error");
            //}

            //app.UseIISPlatformHandler();
            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    };
}
