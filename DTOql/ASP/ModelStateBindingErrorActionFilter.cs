using DTOql.Extensions;
using DTOql.Interfaces;
using DTOql.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

public class ModelStateBindingErrorActionFilter : IActionFilter
{

    public void OnActionExecuted(ActionExecutedContext context)
    {

    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ActionArguments.Any())
        {
            var request = context.ActionArguments.ElementAt(0).Value;
            if (typeof(ISearch).IsAssignableFrom(request.GetType()))
            {
                var search = (ISearch)request;
                var includedNames = search.GetDynamicProperties().Select(x => x.PropertyName);
                context.ModelState.Where(x => !includedNames.Any(n => n.IsEqual(x.Key)))
                                  .ForEach(x => context.ModelState.Remove(x.Key));
            }

        }

    }

}