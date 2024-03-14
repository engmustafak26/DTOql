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
        Dictionary<Type, (object Executer, List<object> AppliedInstances)> Executers { set; get; }
        private readonly IMemoryCache _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public LogicExecuterHolder(IMemoryCache cache, IHttpContextAccessor httpContextAccessor)
        {
            Executers = new();
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
        }

        public void Add(Type type, object executer, object instance)
        {

            var context = _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<AppExecutionContext>();
            _cache.TryGetValue(context.RequestId, out var value);
            if (value != null)
            {
                Executers = value as Dictionary<Type, (object Executer, List<object> AppliedInstances)>;
            }
            else
            {
                Executers = new();
            }


            Executers.TryGetValue(type, out var executerWithInstances);
            if (executerWithInstances == default)
            {
                Executers.Add(type, (executer, new List<object>() { instance }));
            }
            else
            {
                executerWithInstances.AppliedInstances.Add(instance);
            }


            _cache.Set(context.RequestId, Executers, TimeSpan.FromSeconds(3000));

        }
        public async Task ExecuteLogicDisplayers()
        {

            var context = _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<AppExecutionContext>();
            _cache.TryGetValue(context.RequestId, out var value);
            Executers = value as Dictionary<Type, (object Executer, List<object> AppliedInstances)> ?? new();

            var dtoLogicDisplayer = Executers.Values.Where(x => x.Executer
                                                                 .GetType()
                                                                 .GetInterfaces()
                                                                 .Any(y => y.GetGenericTypeDefinition() == typeof(ILogicDisplayer<>)))
                                                                 .ToArray();
            foreach (var item in dtoLogicDisplayer)
            {
                foreach (var modelItem in item.AppliedInstances)
                {
                    dynamic returnTask = item.Executer.GetType().GetMethod("ExecuteAsync").Invoke(item.Executer, new object[] { modelItem });
                    await returnTask;
                }

            }
        }
        public async Task ExecuteDtoLogicExecuters()
        {

            var context = _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<AppExecutionContext>();
            _cache.TryGetValue(context.RequestId, out var value);
            Executers = value as Dictionary<Type, (object Executer, List<object> AppliedInstances)> ?? new();

            var dtoLogicExecuters = Executers.Values.Where(x => x.Executer
                                                                 .GetType()
                                                                 .GetInterfaces()
                                                                 .Any(y => y.GetGenericTypeDefinition() == typeof(IDtoLogicExecuter<>)))
                                                                 .ToArray();
            foreach (var item in dtoLogicExecuters)
            {
                foreach (var modelItem in item.AppliedInstances)
                {
                    dynamic returnTask = item.Executer.GetType().GetMethod("ExecuteAsync").Invoke(item.Executer, new object[] { modelItem });
                    await returnTask;
                }

            }
        }

    }
}
