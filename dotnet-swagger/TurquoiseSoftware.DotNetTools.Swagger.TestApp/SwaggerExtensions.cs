using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TurquoiseSoftware.DotNetTools.Swagger.TestApp
{
    internal static class SwaggerExtensions 
    {
        private const string SwaggerAppName = "SwaggerTestApp";
        private const string SwaggerAppTitle = "Swagger TestApp";

        public static IServiceCollection AddSwaggerForTestApp(this IServiceCollection services) => 
            services.AddSwaggerGen(CreatedSwaggerConfiguror());

        public static IApplicationBuilder UseSwaggerForTestApp(this IApplicationBuilder app) => 
            app.UseSwagger().UseSwaggerUI(c => 
            {
                c.SwaggerEndpoint($"/swagger/{SwaggerAppName}/swagger.json", SwaggerAppTitle);
            });

        private static Action<SwaggerGenOptions> CreatedSwaggerConfiguror()
        {
            return c =>
            {
                c.SwaggerDoc(SwaggerAppName, new Info
                {
                    Title = SwaggerAppTitle,
                    Description = @"lorem foo impsum",

                    Contact = new Contact
                    {
                        Name = "Foo bar",
                        Email = "foo@bar.com",
                        Url = "http://tugberkugurlu.com"
                    }
                });
            };
        }
    }
}