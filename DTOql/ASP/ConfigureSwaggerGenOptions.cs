using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;

namespace DTOql.ASP
{
    public class ConfigureSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IServiceProvider _serviceProvider;
        private IHttpContextAccessor _httpContextAccessor;

        public ConfigureSwaggerGenOptions(IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor)
        {
            _serviceProvider = serviceProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        public void Configure(SwaggerGenOptions options)
        {
            options.SchemaFilter<CustomSwaggerSchemaFilter>();
        }
    }
}
