using DTOql.ASP;
using DTOql.Extensions;
using DTOql.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DTOql.Models
{
    public class DynamicProperty
    {
        public string PropertyName { get; set; }
        public string SourcePropertyPath { get; set; }
        public string Allias { get; set; }
        public bool Execlude { get; set; }
        public bool IsDistinguishProperty { get; set; }
        public bool IsIntermediateProperty { get; set; }
        public bool? IsResponseMode { get; set; }

        public string ToAnonymousProperty()
        {
            if (Execlude)
                return string.Empty;

            string property = SourcePropertyPath.HasValueOrDefault(PropertyName);
            if (Allias.HasValue())
            {
                property += $" as {Allias} ";
            }
            else
            {
                property += $" as {property} ";
            }

            return property;
        }
    }
}



