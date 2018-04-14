using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace WebApp
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

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IMessageProvider, DefaultMessageProvider>();
            services.AddSingleton<IHeaderMessageProvider, PublicHeaderMessageProvider>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();
            app.UseCors(options => options.AllowAnyOrigin());
            app.UseMiddleware<DefaultHeaderAppenderMiddleware>();
            app.UseMiddleware<ProductHeaderAppenderMiddleware>();

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
