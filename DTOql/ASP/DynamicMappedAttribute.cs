using DTOql.Extensions;
using DTOql.Interfaces;
using DTOql.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DTOql.ASP
{

    public class CustomModelBinderProvider : IModelBinderProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CustomModelBinderProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            var appContext = _httpContextAccessor?.HttpContext?.RequestServices?.GetRequiredService<AppExecutionContext>();
            if (appContext is not null)
            {
                appContext.GenerateRequestId();
            }

            if (typeof(ISearch).IsAssignableFrom(context.Metadata.ModelType))
                return new CustomModelBinder();


            return null;
        }
    }




    public class CustomModelBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            string valueFromBody = string.Empty;

            using (var sr = new StreamReader(bindingContext.HttpContext.Request.Body))
            {
                valueFromBody = await sr.ReadToEndAsync();
            }

            if (string.IsNullOrEmpty(valueFromBody))
            {
                return;
            }



            var propertiesInfoArray = bindingContext.ModelType.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToArray();
            Dictionary<string, SearchProperty> properties = new Dictionary<string, SearchProperty>();
            string modelString = string.Empty;
            var jObject = JObject.Parse(valueFromBody);
            foreach (JProperty property in jObject.Children())
            {
                try
                {
                    string propertyName = propertiesInfoArray.FirstOrDefault(x => x.Name.IsEqual(property.Name))?.Name;
                    var searchProperty = property.Value?.ToObject<SearchProperty>();
                    searchProperty?.EnsureValidity();
                    if (searchProperty != null && propertyName.HasValue())
                    {
                        properties.Add(propertyName, searchProperty);
                    }
                }
                catch
                {
                    modelString += property.ToString() + " ,";
                }
            }

            var instance = JsonConvert.DeserializeObject($"{{ {modelString} }}", bindingContext.ModelType, new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ObjectCreationHandling = ObjectCreationHandling.Replace,

                Error = HandleDeserializationError
            }) as ISearch;

            var dynamicProperties = instance.GetDynamicProperties().Select(x => x.PropertyName).ToArray();
            bindingContext.ModelType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                                           .ForEach(x =>
                                                           {
                                                               if (!dynamicProperties.Contains(x.Name))
                                                               {


                                                               }
                                                               else
                                                               {
                                                                   var key = properties.Keys.FirstOrDefault(k => k.IsEqual(x.Name));
                                                                   if (key != null)
                                                                   {
                                                                       if (properties[key].value is Newtonsoft.Json.Linq.JArray arr)
                                                                       {
                                                                           properties[key].value = arr.ToObject(x.PropertyType);
                                                                       }
                                                                       try
                                                                       {
                                                                           x.SetValue(instance, Convert.ChangeType(properties[key].value, x.PropertyType));
                                                                       }
                                                                       catch (Exception ex)
                                                                       {
                                                                           x.SetValue(instance, properties[key].value);

                                                                       }

                                                                   }
                                                                   else
                                                                   {
                                                                       if (x.GetValue(instance) is null)
                                                                       {

                                                                           if (x.PropertyType.IsString())
                                                                           {
                                                                               x.SetValue(instance, " ");

                                                                           }
                                                                           else
                                                                           {
                                                                               x.SetValue(instance, default);
                                                                           }
                                                                       }
                                                                   }
                                                               }
                                                           });

            instance.SetDynamicProperties(properties);

            bindingContext.Result = ModelBindingResult.Success(instance);


        }

        public void HandleDeserializationError(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs errorArgs)
        {
            var currentError = errorArgs.ErrorContext.Error.Message;
            errorArgs.ErrorContext.Handled = true;
        }
    }

}