using System;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using LoremNET;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.PlatformAbstractions;

namespace rabbitsample
{
    public static class Program 
    {
        // EasyNetQ Quick Start: https://github.com/EasyNetQ/EasyNetQ/wiki/Quick-Start
        // Management Console: http://localhost:15672/

        // Exchange: where the message are being published
        // Queue: where the exchgaes are being directed to

        // What I need:
        //
        // (YES) A message broker where I publish messages
        // (YES) A message broker where I subscribe to messages
        // When a consumer is processing a message, it can get a failure and process can shut down. 
        //      In those cases, message shouldn't be lost.
        // When a consumer is processing a message, 
        //      it can get a temporary failure. In those cases, the message
        //      should be retriable.
        
        // More resources:
        // - Docker RabbitMQ cluster: https://github.com/bijukunjummen/docker-rabbitmq-cluster
        
        public static void Main(string[] args)
        {
            var config = ConfigBuilder.Build();
            var settings = new RabbitMQSettings();
            ConfigurationBinder.Bind(config.GetSection("RabbitMQ"), settings);

            Console.WriteLine($"Starting the pub/sub sample on '{settings.Host}' rabbitmq instance.");
            
            var pubTask = new Publisher(settings.Host).Start();
            var subTask = new Subscriber(settings.Host).Start();
            Task.WhenAll(pubTask, subTask).Wait();
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
    
    public class TextMessage
    {
        public string Text { get; set; }
    }

    public class Publisher
    {
        private readonly string _host;
        
        public Publisher(string host)
        {
            if (host == null) throw new ArgumentNullException(nameof(host));
            _host = host;
        }
        
        public Task Start()
        {
            return Task.Run(() =>
            {
                using (var bus = RabbitHutch.CreateBus($"host={_host}"))
                {   
                    while (true)
                    {
                        var input = Lorem.Sentence(50);
                        bus.Publish(new TextMessage
                        {
                            Text = input
                        });

                        Thread.Sleep(1000);
                    }
                }
            });
        }
    }

    public class Subscriber
    {
        private readonly string _host;
        
        public Subscriber(string host)
        {
            if (host == null) throw new ArgumentNullException(nameof(host));
            _host = host;
        }
        
        public Task Start()
        {
            return Task.Run(() =>
            {
                using (var bus = RabbitHutch.CreateBus($"host={_host}"))
                {
                    bus.Subscribe<TextMessage>("test", HandleTextMessage);
                    Thread.Sleep(-1);
                }
            });
        }

        private static void HandleTextMessage(TextMessage textMessage)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[Subscriber] Got message: {0}", textMessage.Text);
            Console.ResetColor();
        }
    }
}