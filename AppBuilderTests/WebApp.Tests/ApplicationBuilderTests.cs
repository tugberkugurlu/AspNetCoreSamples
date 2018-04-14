using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Hosting.Builder;
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

        public TrackingApplicationBuilder()
        {
            _appBuilder = new ApplicationBuilder(new DummyServiceProvider());
            _registeredMiddlewares = new List<Type>();;
            ApplicationServices = _appBuilder.ApplicationServices;
        }

        public IEnumerable<Type> RegisteredMiddlewares => _registeredMiddlewares;

        public IServiceProvider ApplicationServices { get; set; }

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
            new Startup().Configure(trackingAppBuilder);

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
