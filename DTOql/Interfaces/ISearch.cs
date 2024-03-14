using DTOql.Extensions;
using DTOql.Models;
using System.Collections.Generic;
using System.Linq;

namespace DTOql.Interfaces
{
    public interface ISearch : IDynamicProperty
    {
        Dictionary<string, SearchProperty> SearchProperties { get; }
        PagingWithSortModel PaginationWithSort { get; set; }
        public void SetDynamicProperties(Dictionary<string, SearchProperty> properties) => properties.ForEach(x => SearchProperties.Add(x.Key, x.Value));
        public string GetSearchCriteria()
        {
            var distinguishProperties = GetDynamicProperties()
                                        .Where(x => x.IsDistinguishProperty)
                                        .Select(x =>
                                        {
                                            var prop = GetType().GetProperty(x.PropertyName);

                                            return new KeyValuePair<string, SearchProperty>(
                                                  x.PropertyName,
                                                 new SearchProperty
                                                 {
                                                     condition = "=",
                                                     value = prop.PropertyType.IsEnum ? (int)prop.GetValue(this) : prop.GetValue(this),
                                                     type = prop.PropertyType.IsNumeric() || prop.PropertyType.IsEnum ? "number" :
                                                         prop.PropertyType.IsBoolean() ? "boolean" :
                                                         prop.PropertyType.IsString() ? "string" : "date",
                                                 });
                                        })
                                        .ToArray();

            var properties = SearchProperties.ToDictionary(k => k.Key, v => v.Value);
            // distinguishProperties.ForEach(x => properties.Add(x.Key, x.Value));


            string distinguishPropertiesSearchString = string.Join(" ", distinguishProperties.Select(x =>
            {
                var dynamicProperty = GetDynamicProperties().FirstOrDefault(p => p.PropertyName.IsEqual(x.Key));
                var propertyName = dynamicProperty?.SourcePropertyPath?.HasValueOrDefault(dynamicProperty?.PropertyName) ?? x.Key;
                return x.Value.GetLinqString(propertyName);
            }));

            if (distinguishPropertiesSearchString.HasValue())
            {
                distinguishPropertiesSearchString = distinguishPropertiesSearchString.Remove(0, 4);
                distinguishPropertiesSearchString = $" && ({distinguishPropertiesSearchString})";
            }

            var groupedProperties = properties.GroupBy(x => x.Value.group);
            string searchString = " true ";

            foreach (var item in groupedProperties)
            {
                string searchablePropertiesSearchString = string.Join(" ", item.Select(x =>
                {

                    var subgroups = item.Where(y => y.Value.OrSubGroup != null).GroupBy(y => y.Value.OrSubGroup).ToArray();

                    if (subgroups.Any())
                    {
                        string subSearchString = "";
                        foreach (var subItem in subgroups)
                        {
                            string subPropertiesSearch = string.Join(" ", subItem.Select(y =>
                            {
                                var property = GetDynamicProperties().FirstOrDefault(p => p.PropertyName.IsEqual(y.Key));

                                if (property == null || property?.IsIntermediateProperty == true)
                                    return string.Empty;

                                var dynamicProperty = property;
                                var propertyName = dynamicProperty?.SourcePropertyPath?.HasValueOrDefault(dynamicProperty?.PropertyName) ?? y.Key;
                                return y.Value.GetLinqString(propertyName);
                            }));

                            if (subPropertiesSearch.StartsWith("  &&"))
                            {
                                subPropertiesSearch = subPropertiesSearch.Remove(0, 4);
                            }
                            else
                            {
                                subPropertiesSearch = "true ";
                            }

                            subSearchString += $" || ({subPropertiesSearch})";

                        }

                        return subSearchString;
                    }
                    else
                    {
                        var property = GetDynamicProperties().FirstOrDefault(p => p.PropertyName.IsEqual(x.Key));

                        if (property == null || property?.IsIntermediateProperty == true)
                            return string.Empty;

                        var dynamicProperty = property;
                        var propertyName = dynamicProperty?.SourcePropertyPath?.HasValueOrDefault(dynamicProperty?.PropertyName) ?? x.Key;
                        return x.Value.GetLinqString(propertyName);
                    }
                }));

                if (searchablePropertiesSearchString.HasValue())
                {
                    searchablePropertiesSearchString = searchablePropertiesSearchString.Remove(0, 4);
                    searchablePropertiesSearchString = $" ({searchablePropertiesSearchString}) ";
                }
                else
                {
                    searchablePropertiesSearchString = "true ";
                }

                searchString += " && " + searchablePropertiesSearchString;
            }


            return searchString + distinguishPropertiesSearchString;
        }
    }
}



