using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ApiClient
{   
    public static class Program 
    {
        public static int Main(string[] args)
        {
            var config = ConfigBuilder.Build();
            var settings = config.Get<ApiClientSettings>();
            
            if(settings?.ApiBaseUrl == null)
            {
                Console.Error.WriteLine("Error: Specify ApiBaseUrl through ApiClient_ApiBaseUrl environment variable.");
                return -1;
            }
            
            Console.WriteLine("Started the api-client, giving time for APIs to init");
            Task.Delay(5000).Wait();
            
            using(var client = new HttpClient { BaseAddress = new Uri(settings.ApiBaseUrl) })
            {
                while (true)
                {
                    try
                    {
                        Console.WriteLine($"Fething at {DateTime.UtcNow}");
                        
                        var response = client.GetAsync("cars").Result;
                        var cars = JsonConvert.DeserializeObject<IEnumerable<Car>>(response.Content.ReadAsStringAsync().Result);
                        foreach (var car in cars)
                        {
                            Console.WriteLine(car.Make + ", " + car.Model);
                        }
                        
                        Console.Write(Environment.NewLine);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error: {ex.Message}");
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