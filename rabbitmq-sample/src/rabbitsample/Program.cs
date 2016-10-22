using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LoremNET;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace rabbitsample
{
    public static class Program 
    {
        public const string ExchangeName = "activity.textmessages";
        public const string QueueName = "textmessages.worker.1";
        public const string QueueName2 = "textmessages.worker.2";
        public const string RoutingKey = "textmessage.received";

        // EasyNetQ Quick Start: https://github.com/EasyNetQ/EasyNetQ/wiki/Quick-Start
        // Management Console: http://localhost:15672/
        // Naming Convention: https://derickbailey.com/2015/09/02/rabbitmq-best-practices-for-designing-exchanges-queues-and-bindings/

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
            Console.WriteLine("Waiting for 5s to let the RabbitMQ start up");
            Thread.Sleep(5000);

            var config = ConfigBuilder.Build();
            var settings = new RabbitMQSettings();
            ConfigurationBinder.Bind(config.GetSection("RabbitMQ"), settings);

            Console.WriteLine($"Starting the pub/sub sample on '{settings.Host}' rabbitmq instance.");
            
            var factory = new ConnectionFactory 
            {
                HostName = settings.Host
            };

            var conn = factory.CreateConnection();
            var channel = conn.CreateModel();

            channel.ExchangeDeclare(ExchangeName, ExchangeType.Fanout, false, false);
            channel.QueueDeclare(QueueName, false, false, false, null);
            channel.QueueDeclare(QueueName2, false, false, false, null);
            channel.QueueBind(QueueName, ExchangeName, RoutingKey, null);
            channel.QueueBind(QueueName2, ExchangeName, RoutingKey, null);

            var pubTask = new Publisher(conn).Start();
            var subTask = new Subscriber(conn, QueueName).Start();
            var subTask2 = new Subscriber(conn, QueueName2).Start();
            Task.WhenAll(pubTask, subTask, subTask2).Wait();
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
        private readonly IModel _channel;
        
        public Publisher(IConnection conn)
        {
            if (conn == null) throw new ArgumentNullException(nameof(conn));
            _channel = conn.CreateModel();
        }
        
        public Task Start()
        {
            return Task.Run(() =>
            {
                while (true)
                {
                    var input = Lorem.Sentence(50);
                    var textMessage = new TextMessage  { Text = input };
                    var message = JsonConvert.SerializeObject(textMessage);
                    var messageBodyBytes = Encoding.UTF8.GetBytes(message);
                    
                    _channel.BasicPublish(Program.ExchangeName, Program.RoutingKey, null, messageBodyBytes);
                    Thread.Sleep(5000);
                }
            });
        }
    }

    public class Subscriber
    {
        private readonly IModel _channel;
        private readonly string _queueName;

        public Subscriber(IConnection conn, string queueName)
        {
            if (conn == null) throw new ArgumentNullException(nameof(conn));
            if (queueName == null) throw new ArgumentNullException(nameof(queueName));

            _channel = conn.CreateModel();
            _queueName = queueName;
        }
        
        public Task Start()
        {
            return Task.Run(() =>
            {
                while(true) 
                {
                    var result = _channel.BasicGet(_queueName, false);

                    if(result != null) 
                    {
                        try
                        {
                            var bodyBytes = result.Body;
                            var jsonBody = Encoding.UTF8.GetString(bodyBytes);
                            var message = JsonConvert.DeserializeObject<TextMessage>(jsonBody);
                            HandleTextMessage(message, _queueName);

                            _channel.BasicAck(result.DeliveryTag, false);
                        }
                        catch (Exception ex) 
                        {
                            Console.WriteLine("Exception while handling message: {0}", ex.ToString());
                        }
                    } 
                    else 
                    {
                        Console.WriteLine("No message to process, wait for 2s");
                        Thread.Sleep(2000);
                    }
                }
            });
        }

        private static void HandleTextMessage(TextMessage textMessage, string queueName)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[Subscriber-{0}] Got message: {1}", queueName, textMessage.Text);
            Console.ResetColor();
        }
    }
}