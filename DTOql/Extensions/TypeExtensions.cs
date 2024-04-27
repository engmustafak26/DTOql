using DTOql.Interfaces;
using DTOql.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DTOql.Extensions
{
    public static class TypeExtensions
    {
        private static readonly Dictionary<Type, Type[]> _parentSubClassesDic = new Dictionary<Type, Type[]>();

        private static readonly HashSet<Type> NumericTypes = new HashSet<Type>
    {
        typeof(int),  typeof(double),  typeof(decimal),
        typeof(long), typeof(short),   typeof(sbyte),
        typeof(byte), typeof(ulong),   typeof(ushort),
        typeof(uint), typeof(float)
    };

        public static bool IsNumeric(this Type myType)
        {
            return NumericTypes.Contains(Nullable.GetUnderlyingType(myType) ?? myType) || myType.IsEnum;
        }
        public static bool IsString(this Type myType)
        {
            return (Nullable.GetUnderlyingType(myType) ?? myType) == typeof(string);
        }
        public static bool IsDate(this Type myType)
        {
            return (Nullable.GetUnderlyingType(myType) ?? myType) == typeof(DateTime)
                    || (Nullable.GetUnderlyingType(myType) ?? myType) == typeof(TimeSpan);
        }
        public static bool IsBoolean(this Type myType)
        {
            return (Nullable.GetUnderlyingType(myType) ?? myType) == typeof(bool);
        }

        public static Type[] GetSubClasses(this Type type, Assembly seachableAssembly)
        {
            if (!_parentSubClassesDic.ContainsKey(type))
            {
                _parentSubClassesDic.Add(type, seachableAssembly.GetTypes().Where(t => t.IsSubclassOf(type)).ToArray());
            }
            return _parentSubClassesDic[type];
        }


        //public static void SetServiceProviderToDynamicPropertiesRecursive(this object instance, IServiceProvider provider)
        //{
        //    if (instance is null)
        //        return;

        //    var type = instance.GetType();

        //    if (typeof(IDynamicProperty).IsAssignableFrom(type))
        //    {
        //        var dynamicProperty = (IDynamicProperty)instance;
        //        dynamicProperty.RegisterLogic(provider);
        //    }

        //    foreach (PropertyInfo property in type.GetProperties().Where(p => !p.GetIndexParameters().Any()))
        //    {
        //        if (property.PropertyType.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
        //        {
        //            foreach (var item in (IEnumerable)property.GetValue(instance, null))
        //            {
        //                if (typeof(IDynamicProperty).IsAssignableFrom(item.GetType()))
        //                {
        //                    var dynamicProperty = (IDynamicProperty)instance;
        //                    dynamicProperty.RegisterLogic(provider);
        //                }
        //            }
        //        }
        //        if (property.PropertyType.IsClass 
        //            &&!property.PropertyType.IsString()
        //            &&!property.PropertyType.IsNumeric()
        //            &&!property.PropertyType.IsDate()
        //            &&!property.PropertyType.IsBoolean())
        //        {
        //            if (typeof(IDynamicProperty).IsAssignableFrom(property.PropertyType))
        //            {
        //                var dynamicProperty = (IDynamicProperty)instance;
        //                dynamicProperty.RegisterLogic(provider);
        //            }
        //            SetServiceProviderToDynamicPropertiesRecursive(property.GetValue(instance, null), provider);
        //        }
        //    }
        //}
        public static TTarget GetInstance<TSource, TTarget>(this TSource source) where TSource : class where TTarget : class
        {


            string serializedObjString = JsonConvert.SerializeObject(source);
            var TargetObj = JsonConvert.DeserializeObject<TTarget>(serializedObjString);

            return TargetObj;

            var target = Activator.CreateInstance(typeof(TTarget)) as TTarget;



            source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ForEach(x =>
                {
                    try
                    {
                        var targetProperty = typeof(TTarget).GetProperty(x.Name);
                        if (targetProperty != null)
                        {
                            if (x.PropertyType.IsClass && !x.PropertyType.IsString() && !x.PropertyType.IsNumeric() && x.GetValue(source) != null)
                            {
                                targetProperty.SetValue(target, x.GetValue(source).GetInstance<dynamic, dynamic>());
                            }
                            else
                            {
                                targetProperty.SetValue(target, x.GetValue(source));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }

                });

            return target;
        }

        public static string GetAnonymousTypeAsString(this Type typeParameter, DynamicProperty[] overrideProperties = null, string prefix = "")
        {
            var type = typeParameter;
            if (type.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            {
                if (type.IsGenericType)
                {
                    type = typeParameter.GetGenericArguments()[0];
                }
                else if (type.IsArray)
                {
                    type = typeParameter.GetElementType();
                }
            }


            if (typeof(IDynamicProperty).IsAssignableFrom(type))
            {

                overrideProperties = (Activator.CreateInstance(type) as IDynamicProperty).GetOverrideProperties().ToArray();
            }

            (var dynamicProperties, var propertiesInfos) = type.ExtractDynamicProperties(overrideProperties);

            dynamicProperties = dynamicProperties
                                    .Where(p => p.PropertyName.IsNotEqual(nameof(IEntityState.EntityState)))
                                    .Where(x => x.IsResponseMode == null || x.IsResponseMode == true)
                                    .ToArray();
            var selectedProperties = propertiesInfos.Where(x => dynamicProperties.Any(y => y.PropertyName.IsEqual(x.Name)));
            var primitiveDynamicProperties = selectedProperties.Where(x => !x.PropertyType.IsClass ||
                                                                           x.PropertyType == typeof(string) ||
                                                                           x.PropertyType == typeof(DateTime))
                                                       .Select(x => dynamicProperties!.FirstOrDefault(y => y.PropertyName.IsEqual(x.Name)))
                                                       .Where(x => !x.IsDistinguishProperty && !x.IsIntermediateProperty)
                                                       .ToArray();

            string anonymousTypeString = string.Empty;
            anonymousTypeString += $"new {{ {string.Join($", ", primitiveDynamicProperties.Select(x => prefix + x?.ToAnonymousProperty()).ToArray())} ";


            selectedProperties.Where(x => x.PropertyType.IsClass && x.PropertyType != typeof(string))
                              .ForEach(x =>
                              {
                                  var dp = dynamicProperties!.FirstOrDefault(y => y.PropertyName.IsEqual(x.Name));
                                  if (x.PropertyType.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
                                  {
                                      anonymousTypeString += $" , {dp.SourcePropertyPath ?? dp.PropertyName}.Select({GetAnonymousTypeAsString(x.PropertyType)}).ToList() as {dp.Allias.HasValueOrDefault(dp.PropertyName)} ";
                                  }
                                  else
                                  {
                                      anonymousTypeString += $" , {GetAnonymousTypeAsString(x.PropertyType, prefix: dp.SourcePropertyPath + ".")} as {dp.Allias.HasValueOrDefault(dp.PropertyName)} ";

                                  }
                              });



            anonymousTypeString += "}";
            return anonymousTypeString;

        }

        public static (DynamicProperty[] Properties, PropertyInfo[] PropertiesInfos) ExtractDynamicProperties(this Type typeParameter, params DynamicProperty[] overrideProperties)
        {
            overrideProperties ??= new DynamicProperty[0];
            var type = typeParameter;
            if (type.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            {
                type = typeParameter.GetGenericArguments()[0];
            }

            var propertiesInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.CanWrite).ToArray();

            List<DynamicProperty> dynamicProperties = new List<DynamicProperty>(propertiesInfos.Select(x => new DynamicProperty
            {
                Execlude = false,
                PropertyName = x.Name,
                SourcePropertyPath = x.Name
            }));

            dynamicProperties.RemoveAll(x => Array.Exists(overrideProperties, y => y.PropertyName.IsEqual(x.PropertyName)));
            dynamicProperties.AddRange(overrideProperties.Where(x => (!x.Execlude) || x.IsIntermediateProperty || x.IsDistinguishProperty));
            return (dynamicProperties.ToArray(), propertiesInfos);
        }
    }
}
