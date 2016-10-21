using System;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var client = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock"))
                .CreateClient();

            var containers = client.Containers.ListContainersAsync(
                new ContainersListParameters(){
                    Limit = 10,
                }).Result;

            foreach (var container in containers)
            {
                System.Console.WriteLine(string.Join(",", container.Names));
            }
        }
    }
}
