using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleApplication
{
    public class PaginatedRequestCommand
    {
        [Range(0, int.MaxValue)]
        public int Skip { get; set; }

        [Range(1, 50)]
        public int Take { get; set; }
    }
    
    public class ValuesController : Controller 
    {
        [HttpGet("values")]
        public IActionResult Get([FromQuery]PaginatedRequestCommand requestCommand)
        {
            return TryValidateModel(requestCommand) ?
                Ok(new [] { "value1", "value2" }) :
                BadRequest(ModelState) as IActionResult;
        }
    }

    public class Startup 
    {
        public void ConfigureServices(IServiceCollection services) 
        {
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app) 
        {
            app.UseMvc();
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
