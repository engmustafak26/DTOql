using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;

namespace DTOql.ASP
{
    public class ConfigureApiBehaviourOptions : IConfigureOptions<ApiBehaviorOptions>
    {
        private readonly IServiceProvider _serviceProvider;
        private IHttpContextAccessor _httpContextAccessor;

        public ConfigureApiBehaviourOptions(IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor)
        {
            _serviceProvider = serviceProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        public void Configure(ApiBehaviorOptions options)
        {
            options.SuppressModelStateInvalidFilter = true;
        }
    }
}
