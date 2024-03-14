﻿using DTOql.ASP;
using DTOql.Extensions;
using DTOql.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DTOql.Interfaces
{
    public interface IDynamicProperty
    {
        IEnumerable<DynamicProperty> GetDynamicProperties()
        {
            return GetType().ExtractDynamicProperties(GetOverrideProperties().ToArray()).Properties;
        }
      
        IEnumerable<DynamicProperty> GetOverrideProperties();

        void RegisterLogic(params Type[] logicExecuterTypes)
        {
            using (var scope = ServiceProviderWrapper.ServiceProvider.CreateScope())
            {
                logicExecuterTypes.ForEach(x =>
                {
                    var parameters = x.GetConstructors().Single().GetParameters();
                    var paramtersInstances = parameters
                                             .Select(x => scope.ServiceProvider.GetRequiredService(x.ParameterType))
                                             .ToArray();
                    var executerInstance = Activator.CreateInstance(x, paramtersInstances);

                    var executerHolder = ServiceProviderWrapper.ServiceProvider.GetRequiredService<LogicExecuterHolder>();
                    executerHolder.Add(x, executerInstance, this);
                });
            }

        }

    }
}



