using DTOql.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Text.Json;

namespace DTOql.ASP
{

    public class ConfigureMvcOptions : IConfigureOptions<MvcOptions>
    {
        private readonly IServiceProvider _serviceProvider;
        private IHttpContextAccessor _httpContextAccessor;

        public ConfigureMvcOptions(IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor)
        {
            _serviceProvider = serviceProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        public void Configure(MvcOptions options)
        {            
            options.ModelBinderProviders.Insert(0, new CustomModelBinderProvider(_httpContextAccessor));
            options.Filters.Add(new ModelStateBindingErrorActionFilter());
        }
    }
}
