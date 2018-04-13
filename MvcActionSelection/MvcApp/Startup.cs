using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

    public class InternalHeaderAppenderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHeaderMessageProvider _headerMessageProvider;

        public InternalHeaderAppenderMiddleware(RequestDelegate next, IHeaderMessageProvider headerMessageProvider)
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

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSingleton<IMessageProvider, DefaultMessageProvider>();
            services.AddSingleton<IHeaderMessageProvider, InternalHeaderMessageProvider>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMiddleware<DefaultHeaderAppenderMiddleware>();
            app.UseMiddleware<InternalHeaderAppenderMiddleware>();
            app.UseMvc();
        }
    }
}
