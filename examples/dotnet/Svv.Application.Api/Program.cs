using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace Svv.Application.Api
{
    public class Program
    {
        public static IConfiguration Configuration;
        public static IConfiguration GetConfiguration(string[] args)
        {
            if (Configuration != null)
            {
                return Configuration;
            }

            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile(
                    $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json",
                    true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            return Configuration;
        }
        public static int Main(string[] args)
        {
            try
            {
                var host = CreateWebHostBuilder(args).Build();

                host.Run();

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.Write($"Host terminated unexpectedly: {ex.Message}");
                return 1;
            }
        }


        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return new WebHostBuilder()
                .UseKestrel()
                .ConfigureKestrel((context, options) =>
                {
                    // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel?view=aspnetcore-2.2
                    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseConfiguration(GetConfiguration(args))
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.AddEventSourceLogger();
                })
                .UseStartup<Startup>();
        }
    }
}
