using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.PlatformAbstractions;

namespace rabbitsample
{
    public static class Program 
    {   
        public static void Main(string[] args)
        {
            var config = ConfigBuilder.Build();

            var settings = config.GetValue<RabbitMQSettings>("RabbitMQ");
            if(settings == null) 
            {
                System.Console.WriteLine("'settings' is null");
            }

            var cildrenNodes = config.GetChildren();
            foreach (var item in cildrenNodes)
            {
                System.Console.WriteLine($"'{item.Key}' value is null? {item.Value == null}");
            }
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