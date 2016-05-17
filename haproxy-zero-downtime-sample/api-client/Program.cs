using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ApiClient
{   
    public class Program 
    {
        public static int Main(string[] args)
        {
            var config = ConfigBuilder.Build();
            var settings = config.Get<ApiClientSettings>();
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<Program>();
            loggerFactory.MinimumLevel = LogLevel.Error;
            loggerFactory.AddConsole();
            
            if(settings?.ApiBaseUrl == null)
            {
                logger.LogError("Error: Specify ApiBaseUrl through ApiClient_ApiBaseUrl environment variable.");
                return -1;
            }
            
            logger.LogInformation("Started the api-client, giving time for APIs to init");
            Task.Delay(5000).Wait();
            
            using(var client = new HttpClient { BaseAddress = new Uri(settings.ApiBaseUrl) })
            {
                IList<string> machines = new List<string>();
                
                while (true)
                {
                    try
                    {   
                        var response = client.GetAsync("cars").Result;
                        response.EnsureSuccessStatusCode();
                        
                        var machineName = response.Headers.GetValues("MachineName").First();
                        if(!machines.Any(x => x.Equals(machineName, StringComparison.OrdinalIgnoreCase)))
                        {
                            logger.LogCritical($"'{DateTime.UtcNow}': first request from {machineName}");
                            machines.Add(machineName);
                        }
                        
                        var cars = JsonConvert.DeserializeObject<IEnumerable<Car>>(response.Content.ReadAsStringAsync().Result);
                        logger.LogInformation(string.Join(";", cars.Select(x => $"{x.Make}, {x.Model}")));
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Error: {ex.Message}");
                    }
                    
                    Task.Delay(100).Wait();
                }
            }
        }
    }
    
    public static class ConfigBuilder
    {   
        public static IConfiguration Build()
        {
            return new ConfigurationBuilder()
                .AddEnvironmentVariables("ApiClient_")
                .Build();
        }
    }
    
    public class ApiClientSettings
    {
        public string ApiBaseUrl { get; set; }
    }
    
    public class Car
    {
        public int Id { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
    }
}