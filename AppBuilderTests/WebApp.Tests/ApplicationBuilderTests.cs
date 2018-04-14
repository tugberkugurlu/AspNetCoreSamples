using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace WebApp.Tests
{
    public class TrackingApplicationBuilder : IApplicationBuilder
    {
        private readonly IApplicationBuilder _appBuilder;
        private readonly IList<Type> _registeredMiddlewares;
        private readonly IServiceCollection _serviceCollection;

        public TrackingApplicationBuilder()
        {
            // from: https://github.com/WebApiContrib/WebAPIContrib.Core/blob/c89570ade7d4fc9792cc2e08fc38ab7688907911/src/WebApiContrib.Core/ParallelApplicationPipelinesExtensions.cs#L20-L26
            var webHost = new WebHostBuilder()
                .UseStartup<EmptyStartup>()
                .ConfigureServices(services => 
                {
                    services.AddSingleton<IServer, DummyServer>();
                })
                .Build();

            var appBuilderFactory = webHost.Services.GetService<IApplicationBuilderFactory>();
            _appBuilder = appBuilderFactory.CreateBuilder(webHost.ServerFeatures);

            // This reflection magic is to be able to get the common services for free so that we don't rebuild them from scratch
            // we need this as we cannot go back from IServiceProvider to IServiceCollection
            // see: https://github.com/aspnet/Home/issues/3057
            // see: https://github.com/aspnet/Hosting/blob/rel/2.0.0/src/Microsoft.AspNetCore.Hosting/WebHostBuilder.cs#L169
            // see: https://github.com/aspnet/Hosting/blob/rel/2.0.0/src/Microsoft.AspNetCore.Hosting/WebHostBuilder.cs#L175-L180

            // see: https://github.com/aspnet/Hosting/blob/rel/2.0.0/src/Microsoft.AspNetCore.Hosting/Internal/WebHost.cs#L26
            var internalWebHostType = typeof(WebHostBuilder)
                .Assembly
                .GetType("Microsoft.AspNetCore.Hosting.Internal.WebHost");

            // see: https://github.com/aspnet/Hosting/blob/rel/2.0.0/src/Microsoft.AspNetCore.Hosting/Internal/WebHost.cs#L30
            var applicationServicesCollection = (IServiceCollection)internalWebHostType
                .GetField("_applicationServiceCollection", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(webHost);

            _serviceCollection = applicationServicesCollection;

            _registeredMiddlewares = new List<Type>();;
        }

        public IEnumerable<Type> RegisteredMiddlewares => _registeredMiddlewares;
        public IServiceCollection ApplicationServicesCollection => _serviceCollection;

        public IServiceProvider ApplicationServices 
        { 
            get 
            {
                return _serviceCollection.BuildServiceProvider();
            } 

            set 
            {
                throw new NotImplementedException();
            }
        }

        public IFeatureCollection ServerFeatures => _appBuilder.ServerFeatures;

        public IDictionary<string, object> Properties => _appBuilder.Properties;

        public RequestDelegate Build()
        {
            return _appBuilder.Build();
        }

        public IApplicationBuilder New()
        {
            return _appBuilder.New();
        }

        public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
        {
            try 
            {
                var appliedMiddleware = middleware.Target
                    .GetType()
                    .GetField("middleware")
                    .GetValue(middleware.Target);

                if(appliedMiddleware is Type) 
                {
                    _registeredMiddlewares.Add((Type)appliedMiddleware);
                }
                else 
                {
                    var middlewareType = appliedMiddleware.GetType();
                    _registeredMiddlewares.Add(middlewareType);
                }
            }
            catch(Exception ex) 
            {
                Console.WriteLine(ex.Message);
            }

            return _appBuilder.Use(middleware);
        }

        private class EmptyStartup
        {
            public void ConfigureServices(IServiceCollection services) {}

            public void Configure(IApplicationBuilder app) {}
        }

        private class DummyServer : IServer
        {
            public IFeatureCollection Features { get; } = new FeatureCollection();
            public Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken) => Task.CompletedTask;
            public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
            public void Dispose() { }
        }

        private class DummyServiceProvider : IServiceProvider
        {
            private Dictionary<Type, object> _services = new Dictionary<Type, object>();

            public void AddService(Type type, object value) => _services[type] = value;

            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(IServiceProvider))
                {
                    return this;
                }

                if (_services.TryGetValue(serviceType, out object value))
                {
                    return value;
                }
                return null;
            }
        }
    }

    public class ApplicationBuilderTests
    {   
        [Fact]
        public void DefaultHeaderAppenderMiddleware_ShouldBeRegisteredBeforeProductHeaderAppenderMiddleware()
        {
            var trackingAppBuilder = new TrackingApplicationBuilder();
            var startup = new Startup();
            startup.ConfigureServices(trackingAppBuilder.ApplicationServicesCollection);
            startup.Configure(trackingAppBuilder);

            bool defaultHeaderAppenderMiddlewareFound = false;
            bool productHeaderAppenderMiddlewareFound = false;

            foreach (var registeredMiddleware in trackingAppBuilder.RegisteredMiddlewares)
            {
                productHeaderAppenderMiddlewareFound = registeredMiddleware == typeof(ProductHeaderAppenderMiddleware);
                defaultHeaderAppenderMiddlewareFound = registeredMiddleware == typeof(DefaultHeaderAppenderMiddleware);

                if(productHeaderAppenderMiddlewareFound) 
                {
                    Assert.False(true, $"{nameof(DefaultHeaderAppenderMiddleware)} should have been registered before {nameof(ProductHeaderAppenderMiddleware)}");
                    break;
                }

                if(defaultHeaderAppenderMiddlewareFound)
                {
                    Assert.True(true, $"{nameof(DefaultHeaderAppenderMiddleware)} has been registered before {nameof(ProductHeaderAppenderMiddleware)}");
                    break;
                }
            }

            if(!defaultHeaderAppenderMiddlewareFound) 
            {
                Assert.False(true, $"{nameof(DefaultHeaderAppenderMiddleware)} should have been registered");
            }
        }
    }
}
