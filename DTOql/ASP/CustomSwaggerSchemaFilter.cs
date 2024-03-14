using DTOql.Extensions;
using DTOql.Interfaces;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DTOql.ASP
{
    public class CustomSwaggerSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {

            if (typeof(ISearch).IsAssignableFrom(context.Type))
            {
                var allowedTypesSet = new HashSet<string>()
                {
                        "number",
                        "integer",
                        "boolean",
                        "string",
                        "array"
                };
                var keys = schema.Properties
                                 .Where(x => allowedTypesSet.Contains(x.Value.Type))
                                 .Select(x => new { x.Key, x.Value.Type, x.Value.Format })
                                 .ToArray();


                keys.ForEach(x =>
                {
                    var swaggerProp = schema.Properties[x.Key];
                    schema.Properties.Remove(x.Key);
                    var custom = new KeyValuePair<string, OpenApiSchema>(x.Key, new OpenApiSchema()
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>()
                    });

                    string propertyType = x.Type switch
                    {
                        "number" => "number",
                        "array" => "array",
                        "integer" => "number",
                        "boolean" => "boolean",
                        "string" => x.Format.IsEqual("date") ? "date" : "string",
                        _ => throw new ArgumentOutOfRangeException(x.Type)
                    };
                    custom.Value.Properties.Add(new KeyValuePair<string, OpenApiSchema>("value", swaggerProp));
                    custom.Value.Properties.Add(new KeyValuePair<string, OpenApiSchema>("type", new OpenApiSchema() { Type = x.Type.IsEqual("array") ? "array" : "string", Example = new OpenApiString(propertyType) }));
                    custom.Value.Properties.Add(new KeyValuePair<string, OpenApiSchema>("condition", new OpenApiSchema() { Type = "string" }));
                    custom.Value.Properties.Add(new KeyValuePair<string, OpenApiSchema>("group", new OpenApiSchema() { Type = "number" }));
                    custom.Value.Properties.Add(new KeyValuePair<string, OpenApiSchema>("isOR", new OpenApiSchema() { Type = "boolean" }));

                    schema.Properties.Add(custom);
                });


                var model = Activator.CreateInstance(context.Type) as ISearch;
                var names = model.GetDynamicProperties()
                                 .Where(x => !x.IsDistinguishProperty)
                                 .Select(x => x.PropertyName)
                                 .ToArray();
                schema
                      .Properties
                      .Keys
                      .Where(x => !names.Any(y => y.IsEqual(x)))
                      .ForEach(x => schema.Properties.Remove(x));



            }
            else if (typeof(IDynamicProperty).IsAssignableFrom(context.Type))
            {

                var model = Activator.CreateInstance(context.Type) as IDynamicProperty;
                var names = model.GetDynamicProperties()
                                 .Where(x => !x.IsDistinguishProperty)
                                 .Where(x => x.IsResponseMode == null || x.IsResponseMode == false)
                                 .Select(x => x.PropertyName)
                                 .ToArray();
                schema
                      .Properties
                      .Keys
                      .Where(x => !names.Any(y => y.IsEqual(x)))
                      .ForEach(x => schema.Properties.Remove(x));



            }

        }
    }
}



