using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using TurquoiseSoftware.DotNetTools.Swagger.TestApp;

namespace TurquoiseSoftware.DotNetTools.Swagger
{
    class Program
    {
        // see https://docs.microsoft.com/en-us/dotnet/core/tools/extensibility
        // see https://github.com/dotnet/cli/issues/5189
        // see https://github.com/NuGet/Home/issues/2469#issuecomment-349719803

        static void Main(string[] args)
        {
            using(var server = new TestServer(new WebHostBuilder().UseStartup<Startup>())) 
            using(var client = server.CreateClient())
            {
                System.Console.WriteLine(client
                    .GetAsync("/swagger/SwaggerTestApp/swagger.json")
                    .Result
                    .Content
                    .ReadAsStringAsync()
                    .Result);
            }
        }
    }
}
