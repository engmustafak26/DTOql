using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DTOql.Utilities
{
    public static class Builder
    {
        public static string _<T>(Expression<Func<T, object>> expr)
        {
            string bodyString = expr.Body.ToString();
            var tokens = bodyString.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            List<string> properties = new List<string>();


            for (int i = 1; i < tokens.Length; i++)
            {
                var token = tokens[i];

                if (token.Contains("select", StringComparison.InvariantCultureIgnoreCase)
                    && token.Contains("(", StringComparison.InvariantCultureIgnoreCase))
                {

                    properties[properties.Count - 1] += "[]";
                }
                else
                {
                    properties.Add(token);
                }
            }
            return string.Join(".", properties).Replace(")", "");
        }

    }
}
