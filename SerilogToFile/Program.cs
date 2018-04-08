using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace SerilogToFile
{
    public static class Program
    {
        private static IConfiguration _configuration = new ConfigurationBuilder()
            .ConfigureAppConfig()
            .Build();

        public static int Main(string[] args)
        {
            var logsFilePath = GetLogsFilePath();
            var serilogger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(new CompactJsonFormatter(), logsFilePath, rollingInterval: RollingInterval.Minute)
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .CreateLogger();

            Console.WriteLine($"Logs are being written to {logsFilePath}");

            // Log.Logger = serilogger;

            try
            {
                serilogger.Information("Starting web host");
                BuildWebHost(args, serilogger).Run();
                return 0;
            }
            catch (Exception ex)
            {
                serilogger.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                // Log.CloseAndFlush();
            }
        }

        private static string GetLogsFilePath()
        {
#if DEBUG
            var logsFolder = Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), "Logs");
#else
            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var logsFolder = Path.Combine(appDataFolder, "Local", "AspNetCoreSamples", "Logs", "SerilogToFileSample");
#endif
            return Path.Combine(logsFolder, "logs-.txt");
        }

        public static IWebHost BuildWebHost(string[] args, Serilog.ILogger serilogger) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((_, builder) => builder.ConfigureAppConfig())
                .UseSerilog(serilogger, true)
                .UseStartup<Startup>()
                .Build();

        private static IConfigurationBuilder ConfigureAppConfig(this IConfigurationBuilder builder) =>
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables("SerilogToFileSample");
    }
}
