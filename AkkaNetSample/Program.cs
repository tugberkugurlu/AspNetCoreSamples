using System;
using System.Threading;
using Akka.Actor;

namespace AkkaNetSample
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var sys = ActorSystem.Create("test")) 
            {
                var firstRef = sys.ActorOf(Props.Create<PrintMyActorRefActor>(), "first-actor");
                Console.WriteLine($"First: {firstRef}");
                firstRef.Tell("printit", ActorRefs.NoSender);
                Thread.Sleep(5000);

                firstRef.GracefulStop(TimeSpan.FromSeconds(5)).Wait();
            }
        }
    }

    public class PrintMyActorRefActor : UntypedActor
    {
        protected override void OnReceive(object message)
        {
            switch (message) 
            {
                case "printit":
                    var secondRef = Context.ActorOf(Props.Empty, "second-actor");
                    Console.WriteLine($"Second: {secondRef}");
                    break;
            }
        }

        protected override void PreStart() 
        {
            Console.WriteLine($"PreStart: {Context.Self}");
        }

        protected override void PostStop() 
        {
            Console.WriteLine($"PostStop: {Context.Self}");
        }
    }
}
