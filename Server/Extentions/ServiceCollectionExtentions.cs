using BlazorAppExampleSink.Server.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAppExampleSink.Server.Extentions
{
    public static class ServiceCollectionExtentions
    {
        internal static IApplicationBuilder Initialize(this IApplicationBuilder app, Microsoft.Extensions.Configuration.IConfiguration _configuration)
        {
            using var serviceScope = app.ApplicationServices.CreateScope();

            var initializers = serviceScope.ServiceProvider.GetServices<IDatabaseSeeder>();

            foreach (var initializer in initializers)
            {
                initializer.Initialize();
            }

            return app;
        }

        public static IServiceCollection AddDatabase(this IServiceCollection services,
         IConfiguration configuration)
        {
            services
                .AddDbContext<MyContext>(options => options
                    .UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                    .UseLazyLoadingProxies())
                .AddTransient<IDatabaseSeeder, DatabaseSeeder>();
            return services;
        }
    }
}
