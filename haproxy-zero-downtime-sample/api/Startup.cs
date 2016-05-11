using System;
using System.Collections;
using System.Text;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;

namespace HelloWorldWeb 
{
    public class Startup 
    {   
        public Startup(ILoggerFactory loggerFactory) 
        {
            loggerFactory.MinimumLevel = LogLevel.Debug;
            loggerFactory.AddConsole();
        }
        
        public void Configure(IApplicationBuilder app)
        {
            app.Run(async ctx => 
            {
                ctx.Response.StatusCode = 200;
                ctx.Response.Headers["Content-Type"] = "text/html";
                StringBuilder builder = new StringBuilder();
                builder.AppendLine($"<div>Hello from {GetMachineName()}! Here is my context:</div>");
                builder.AppendLine("<hr/>");
                foreach(DictionaryEntry envVar in System.Environment.GetEnvironmentVariables())
                {
                    builder.AppendLine(envVar.Key  + ":" + envVar.Value);
                }
                
                await ctx.Response.WriteAsync($"<pre>{builder.ToString()}</pre>");
            });
        }
        
        private static string GetMachineName() 
        {
#if DNX451
            var machineName = Environment.MachineName;
#else  
            var machineName = Environment.GetEnvironmentVariable("HOSTNAME");
            if(machineName == null) 
            {
                machineName = Environment.GetEnvironmentVariable("COMPUTERNAME");
            }
#endif
            
            return machineName;
        }
        
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }    
}