using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace IdentitySample
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var assemblyName = typeof(Program).GetTypeInfo().Assembly.GetName();
            var machineName = Environment.GetEnvironmentVariable("HOSTNAME");
            var waitTime = TimeSpan.FromSeconds(10);

            Console.WriteLine($"Hello from {assemblyName} on {machineName}!");
            Console.WriteLine("Waiting for {waitTime} to give dependencies a chance to start up...");

            Thread.Sleep(waitTime);

            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables("ASPNETCORE_")
                .Build();

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseConfiguration(config)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}