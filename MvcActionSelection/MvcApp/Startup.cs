using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebApiContrib.Core;

namespace MvcApp
{
    public interface IMessageProvider
    {
        string GetMessage();
    }

    public class DefaultMessageProvider : IMessageProvider
    {
        public string GetMessage() => Guid.NewGuid().ToString();
    }

    public interface IHeaderMessageProvider
    {
        string GetHeaderMessage();
    }

    public class InternalHeaderMessageProvider : IHeaderMessageProvider
    {
        private readonly IMessageProvider _messageProvider;

        public InternalHeaderMessageProvider(IMessageProvider messageProvider)
        {
            _messageProvider = messageProvider ?? throw new ArgumentNullException(nameof(messageProvider));
        }

        public string GetHeaderMessage() => $"internal-{_messageProvider.GetMessage()}";
    }

    public class PublicHeaderMessageProvider : IHeaderMessageProvider
    {
        private readonly IMessageProvider _messageProvider;

        public PublicHeaderMessageProvider(IMessageProvider messageProvider)
        {
            _messageProvider = messageProvider ?? throw new ArgumentNullException(nameof(messageProvider));
        }

        public string GetHeaderMessage() => $"public-{_messageProvider.GetMessage()}";
    }

    public class DefaultHeaderAppenderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMessageProvider _messageProvider;

        public DefaultHeaderAppenderMiddleware(RequestDelegate next, IMessageProvider messageProvider)
        {
            _messageProvider = messageProvider ?? throw new ArgumentNullException(nameof(messageProvider));
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public Task Invoke(HttpContext context)
        {
            context.Response.Headers.Add("X-DEFAULT-HEADER", _messageProvider.GetMessage());

            return _next(context);
        }
    }

    public class ProductHeaderAppenderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHeaderMessageProvider _headerMessageProvider;

        public ProductHeaderAppenderMiddleware(RequestDelegate next, IHeaderMessageProvider headerMessageProvider)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _headerMessageProvider = headerMessageProvider ?? throw new ArgumentNullException(nameof(headerMessageProvider));
        }

        public Task Invoke(HttpContext context)
        {
            context.Response.Headers.Add("X-PRODUCT-HEADER", _headerMessageProvider.GetHeaderMessage());

            return _next(context);
        }
    }

    // https://www.strathweb.com/2017/04/running-multiple-independent-asp-net-core-pipelines-side-by-side-in-the-same-application/
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IMessageProvider, DefaultMessageProvider>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMiddleware<DefaultHeaderAppenderMiddleware>();
            
            app.UseBranchWithServices("/internal", 
                internalServices => 
                {
                    internalServices.AddMvc();
                    internalServices.AddSingleton<IMessageProvider, DefaultMessageProvider>();
                    internalServices.AddSingleton<IHeaderMessageProvider, InternalHeaderMessageProvider>();
                },

                internalApp => 
                {
                    internalApp.UseMiddleware<ProductHeaderAppenderMiddleware>();
                    internalApp.UseMvc();
                });

            app.UseBranchWithServices("/api", 
                publicServices => 
                {
                    publicServices.AddMvc();
                    publicServices.AddSingleton<IMessageProvider, DefaultMessageProvider>();
                    publicServices.AddSingleton<IHeaderMessageProvider, PublicHeaderMessageProvider>();
                },

                publicApp => 
                {
                    publicApp.UseMiddleware<ProductHeaderAppenderMiddleware>();
                    publicApp.UseMvc();
                });
        }
    }
}
