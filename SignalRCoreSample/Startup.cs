using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace SignalRCoreSample
{
    public class ChatHub : Hub { }

    public class MessageRequestModel 
    {
        [Required]
        public string Message { get; set; }
    }

    [Route("messages")]
    public class MessagesController : Controller
    {
        private readonly HubLifetimeManager<ChatHub> _chatHub;

        public MessagesController(HubLifetimeManager<ChatHub> chatHub)
        {
            _chatHub = chatHub;
        }

        [HttpPost]
        public IActionResult Post([FromBody]MessageRequestModel requestModel)
        {
            string timestamp = DateTime.Now.ToShortTimeString();

            _chatHub.SendAllAsync("Message_Received", new[] 
            { 
                new
                {
                    Message = requestModel.Message,
                    Timestamp = timestamp
                }
            });

            return Ok();
        }
    }

    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseSignalR(routes =>
            {
                routes.MapHub<ChatHub>("/hubs/chat");
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
