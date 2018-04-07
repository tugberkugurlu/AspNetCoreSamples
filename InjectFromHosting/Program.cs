using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InjectFromHosting
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureServices(services =>
                {
                    // services.AddSingleton<IMessenger, ConsoleMessenger>();
                    services.AddSingleton<IMessenger, LoggerBasedMessenger>();
                })
                .Build();
        }

        private class ConsoleMessenger : IMessenger
        {
            public void SayHello()
            {
                Console.WriteLine("Hello!");
            }
        }

        private class LoggerBasedMessenger : IMessenger
        {
            private readonly ILogger<LoggerBasedMessenger> _logger;

            public LoggerBasedMessenger(ILogger<LoggerBasedMessenger> logger)
            {
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

            public void SayHello()
            {
                _logger.LogInformation("Hello!");
            }
        }
    }
}
