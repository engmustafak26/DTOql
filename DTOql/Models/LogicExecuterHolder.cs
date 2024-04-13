using DTOql.ASP;
using DTOql.Continuations;
using DTOql.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOql.Models
{
    internal class LogicExecuterHolder
    {
        HashSet<Type> Executers { set; get; } = new HashSet<Type>();
        private readonly IMemoryCache _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public LogicExecuterHolder(IMemoryCache cache, IHttpContextAccessor httpContextAccessor)
        {
            Executers = new();
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
        }

        public void Add(Type type)
        {

            Executers.Add(type);

        }
        public async Task ExecuteLogicDisplayers(object dto)
        {

            Type dtoType = dto.GetType();
            var dtoLogicDisplayer = Executers.Where(x => x.GetInterfaces()
                                                            .Any(y => y.GetGenericTypeDefinition() == typeof(ILogicDisplayer<>)
                                                                      && y.GetGenericArguments().Contains(dtoType)))
                                                            .ToArray();
            var services = ServiceProviderWrapper.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext.RequestServices;

            foreach (var item in dtoLogicDisplayer)
            {

                using (var scope = ServiceProviderWrapper.ServiceProvider.CreateScope())
                {

                    var parameters = item.GetConstructors().Single().GetParameters();
                    var paramtersInstances = parameters
                                             .Select(x => services.GetRequiredService(x.ParameterType))
                                             .ToArray();
                    var executerInstance = Activator.CreateInstance(item, paramtersInstances);

                    dynamic returnTask = executerInstance.GetType().GetMethod("ExecuteAsync").Invoke(executerInstance, new object[] { dto });
                    await returnTask;

                }

            }
        }



        public async Task ExecuteDtoSearchInterceptors(object dto)
        {
            Type dtoType = dto.GetType();
            var context = _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<AppExecutionContext>();
            _cache.TryGetValue(context.RequestId, out var value);

            var dtoLogicExecuters = Executers.Where(x => x.GetInterfaces()
                                                                 .Any(y => y.GetGenericTypeDefinition() == typeof(IDtoSearchInterceptor<>)
                                                                           && y.GetGenericArguments().Contains(dtoType)))
                                                                 .ToArray();

            var services = ServiceProviderWrapper.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext.RequestServices;

            foreach (var item in dtoLogicExecuters)
            {
              
                    using (var scope = ServiceProviderWrapper.ServiceProvider.CreateScope())
                    {

                        var parameters = item.GetConstructors().Single().GetParameters();
                        var paramtersInstances = parameters
                                                 .Select(x => services.GetRequiredService(x.ParameterType))
                                                 .ToArray();
                        var executerInstance = Activator.CreateInstance(item, paramtersInstances);

                        dynamic returnTask = executerInstance.GetType().GetMethod("ExecuteAsync").Invoke(executerInstance, new object[] { dto });
                        await returnTask;

                    }


                

            }
        }
        public async Task<DTOqlBaseResponseDto<object>> ExecuteDtoLogicExecuters(object dto)
        {
            Type dtoType = dto.GetType();

            var context = _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<AppExecutionContext>();
            

            var dtoLogicExecuters = Executers.Where(x => x.GetInterfaces()
                                                                 .Any(y => y.GetGenericTypeDefinition() == typeof(IDtoLogicExecuter<>)
                                                                           && y.GetGenericArguments().Contains(dtoType)))                                                                 .ToArray();
            var services = ServiceProviderWrapper.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext.RequestServices;

            foreach (var item in dtoLogicExecuters)
            {
               
                    using (var scope = ServiceProviderWrapper.ServiceProvider.CreateScope())
                    {

                        var parameters = item.GetConstructors().Single().GetParameters();
                        var paramtersInstances = parameters
                                                 .Select(x => services.GetRequiredService(x.ParameterType))
                                                 .ToArray();
                        var executerInstance = Activator.CreateInstance(item, paramtersInstances);

                        dynamic returnTask = executerInstance.GetType().GetMethod("ExecuteAsync").Invoke(executerInstance, new object[] { dto });
                        await returnTask;

                        if (!returnTask.Result.IsSuccess)
                        {
                            return new DTOqlBaseResponseDto<object>().Error(returnTask.Result);
                        }

                    }


                

            }

            return DTOqlBaseResponseDto<object>.Success(new());
        }

    }
}
