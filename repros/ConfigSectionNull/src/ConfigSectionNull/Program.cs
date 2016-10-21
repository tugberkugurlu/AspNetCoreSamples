using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.PlatformAbstractions;

namespace rabbitsample
{
    public static class Program 
    {
        public static void Main(string[] args)
        {
            var config = ConfigBuilder.Build();
            var settings = new RabbitMQSettings();
            ConfigurationBinder.Bind(config.GetSection("RabbitMQ"), settings);

            Console.WriteLine(settings.Host);
        }
    }
    
    public class RabbitMQSettings 
    {
        public string Host { get; set; }
    }
    
    public static class ConfigBuilder
    {   
        public static IConfiguration Build()
        {
            return new ConfigurationBuilder()
                .SetBasePath(PlatformServices.Default.Application.ApplicationBasePath)
                .AddJsonFile("config.json")
                .AddEnvironmentVariables("rabbitsample_")
                .Build();
        }
    }
}