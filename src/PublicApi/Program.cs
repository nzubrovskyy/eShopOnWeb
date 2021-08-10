using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.Extensions.Logging.AzureAppServices;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Debug;
using System;
using System.Threading.Tasks;

namespace Microsoft.eShopWeb.PublicApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args)
                        .Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();
                try
                {
                    var catalogContext = services.GetRequiredService<CatalogContext>();
                    await CatalogContextSeed.SeedAsync(catalogContext, loggerFactory);

                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    await AppIdentityDbContextSeed.SeedAsync(userManager, roleManager);
                }
                catch (Exception ex)
                {
                    var logger = loggerFactory.CreateLogger<Program>();
                    logger.LogError(ex, "An error occurred seeding the DB.");
                }
            }

            host.Run();
        }


        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                //.ConfigureLogging(logging =>
                //{
                //    logging.ClearProviders();
                //    logging.AddConsole();
                //    logging.AddDebug();
                //    logging.AddAzureWebAppDiagnostics();
                //})
                //.ConfigureServices(services =>
                //{
                //    services.Configure<AzureFileLoggerOptions>(options =>
                //    {
                //        options.FileName = "my-azure-diagnostics-";
                //        options.FileSizeLimit = 50 * 1024;
                //        options.RetainedFileCountLimit = 5;
                //    });
                //});
                .ConfigureLogging((context, builder) =>
                {
                    // Providing an instrumentation key is required if you're using the
                    // standalone Microsoft.Extensions.Logging.ApplicationInsights package,
                    // or when you need to capture logs during application startup, for example
                    // in the Program.cs or Startup.cs itself.
                    builder.AddApplicationInsights(
                        context.Configuration["APPINSIGHTS_CONNECTIONSTRING"]);

                    //builder.AddFilter("System", LogLevel.Debug)
                    //    .AddFilter<DebugLoggerProvider>("Microsoft", LogLevel.Information)
                    //    .AddFilter<ConsoleLoggerProvider>("Microsoft", LogLevel.Trace);

                    // Capture all log-level entries from Program
                    builder.AddFilter<ApplicationInsightsLoggerProvider>(
                            typeof(Program).FullName, LogLevel.Error);

                    // Capture all log-level entries from Startup
                    builder.AddFilter<ApplicationInsightsLoggerProvider>(
                        typeof(Startup).FullName, LogLevel.Error);
                });
    }
}
