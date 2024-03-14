using DTOql.ASP;
using DTOql.DataAccess;
using DTOql.Interfaces;
using DTOql.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DTOql
{

    public static class DependancyInjector
    {
        public static IServiceCollection AddDTOql(this IServiceCollection services, Type dbContext, IConfiguration configuration)
        {
            services.AddHttpContextAccessor();
            services.AddMemoryCache();
            services.AddScoped<DbContext>(s => s.GetRequiredService(dbContext) as DbContext);
            services.ConfigureOptions<ConfigureMvcOptions>();
            services.ConfigureOptions<ConfigureApiBehaviourOptions>();
            services.ConfigureOptions<ConfigureSwaggerGenOptions>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(IService<>), typeof(Service<>));
            services.AddScoped(typeof(IUnitOfWork), typeof(DbUnitOfWork)); 
            services.AddScoped<AppExecutionContext,AppExecutionContext>();
            services.AddSingleton<LogicExecuterHolder, LogicExecuterHolder>();

            return services;
        }
    }
}
