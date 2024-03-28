using DTOql.Extensions;
using System;
using System.Collections;

namespace DTOql.Models
{
    public class SearchProperty
    {
        public object value { get; set; }
        public string type { get; set; }
        public string condition { get; set; }
        public bool? isOR { get; set; }
        public int? group { get; set; }
        public int? OrSubGroup { get; set; }

        public void EnsureValidity()
        {
            if (value is null && type is null && condition is null && isOR is null)
                throw new ArgumentNullException();
        }

        public virtual string GetLinqString(string fieldName)
        {
            if (this.value.GetType().IsDate())
            {
                type = "date";
            }
            else if (this.value.GetType().IsNumeric())
            {
                type = "number";
            }
            else if (this.value.GetType().IsString())
            {
                type = "string";
            }
            else if (this.value.GetType().IsBoolean())
            {
                type = "boolean";
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }

            string returnString = string.Empty;

            int indexOfArray = fieldName.IndexOf("[");
            string ArrayProperty = null;

            if (indexOfArray > -1)
            {
                ArrayProperty = fieldName.Substring(0, indexOfArray);
                fieldName = fieldName.Substring(indexOfArray + 3);
            }

            if (condition.IsEqual("<"))
            {
                if (type.IsEqual("number"))
                {
                    returnString = $" {fieldName} < {value.ToString()}";
                }
                else if (type.IsEqual("string"))
                {
                    returnString = $" {fieldName} < \"{value.ToString()}\"";
                }
                else if (type.IsEqual("date"))
                {
                    var val = (DateTime)value;
                    returnString = $" {fieldName} < DateTime.Parse(\"{val.ToString()}\").Date";
                }
            }
            else if (condition.IsEqual(">"))
            {
                if (type.IsEqual("number"))
                {
                    returnString = $" {fieldName} > {value.ToString()}";
                }
                else if (type.IsEqual("string"))
                {
                    returnString = $" {fieldName} > \"{value.ToString()}\"";
                }
                else if (type.IsEqual("date"))
                {
                    var val = (DateTime)value;
                    returnString = $" {fieldName} > DateTime.Parse(\"{val.ToString()}\").Date";
                }
            }
            else if (condition.IsEqual("=") || condition.IsEqual("=="))
            {
                if (type.IsEqual("number"))
                {
                    returnString = $" {fieldName} = {value.ToString()}";
                }
                else if (type.IsEqual("string"))
                {
                    returnString = $" {fieldName} = \"{value.ToString()}\"";
                }

                else if (type.IsEqual("date"))
                {
                    var val = (DateTime)value;
                    returnString = $" {fieldName} = DateTime.Parse(\"{val.ToString()}\").Date";
                }
                else if (type.IsEqual("boolean"))
                {
                    returnString = $" {fieldName} == {value.ToString().ToLower()}";
                }
                else if (type.IsEqual("array"))
                {

                    string val = string.Empty;
                    foreach (var item in value as IEnumerable)
                    {
                        val += fieldName + " == " + item + " ||";
                    }
                    returnString = val.Remove(val.Length - 3);

                }
            }
            else if (condition.IsEqual("<=") || condition.IsEqual("=<"))
            {
                if (type.IsEqual("number"))
                {
                    returnString = $" {fieldName} <= {value.ToString()}";
                }
                else if (type.IsEqual("string"))
                {
                    returnString = $" {fieldName} <= \"{value.ToString()}\"";
                }
                else if (type.IsEqual("date"))
                {
                    var val = (DateTime)value;
                    returnString = $"  {fieldName} <= DateTime.Parse(\"{val.ToString()}\").Date";
                }
            }
            else if (condition.IsEqual("=>") || condition.IsEqual(">="))
            {
                if (type.IsEqual("number"))
                {
                    returnString = $" {fieldName} >= {value.ToString()}";
                }
                else if (type.IsEqual("string"))
                {
                    returnString = $" {fieldName} >= \"{value.ToString()}\"";
                }
                else if (type.IsEqual("date"))
                {
                    var val = (DateTime)value;
                    returnString = $" {fieldName} >=  DateTime.Parse(\"{val.ToString()}\").Date";
                }
            }
            else if (condition.IsEqual("!") || condition.IsEqual("!="))
            {
                if (type.IsEqual("number") )
                {
                    returnString = $" {fieldName} != {value.ToString()}";
                }
                else if (type.IsEqual("string"))
                {
                    returnString = $" {fieldName} != \"{value.ToString()}\"";
                }
                else if (type.IsEqual("date"))
                {
                    var val = (DateTime)value;
                    returnString = $" {fieldName} != DateTime.Parse(\"{val.ToString()}\").Date";
                }
            }
            else if (condition.IsEqual("<>"))
            {
                if (type.IsEqual("number"))
                {
                    returnString = $" {fieldName} <> {value.ToString()}";
                }
                else if (type.IsEqual("string"))
                {
                    returnString = $" {fieldName} <> \"{value.ToString()}\"";
                }
                else if (type.IsEqual("date"))
                {
                    var val = (DateTime)value;
                    returnString = $" {fieldName} <> DateTime.Parse(\"{val.ToString()}\").Date";
                }
            }
            else if (condition.IsEqual("<>"))
            {
                if (type.IsEqual("number"))
                {
                    returnString = $" {fieldName} <> {value.ToString()}";
                }
                else if (type.IsEqual("string"))
                {
                    returnString = $" {fieldName} <> \"{value.ToString()}\"";
                }
                else if (type.IsEqual("date"))
                {
                    var val = (DateTime)value;
                    returnString = $" {fieldName} <> DateTime.Parse(\"{val.ToString()}\").Date";
                }
            }
            else if (condition.IsEqual("%%"))
            {
                if (type.IsEqual("string"))
                {
                    returnString = $" {fieldName}.Contains(\"{value.ToString()}\")";
                }

            }
            else if (condition.IsEqual("_%"))
            {
                if (type.IsEqual("string"))
                {
                    returnString = $" {fieldName}.EndsWith(\"{value.ToString()}\")";
                }
            }
            else if (condition.IsEqual("%_"))
            {
                if (type.IsEqual("string"))
                {
                    returnString = $" {fieldName}.StartsWith(\"{value.ToString()}\")";
                }
            }
            else if (condition.IsEqual("true") || condition.IsEqual("1"))
            {
                if (type.IsEqual("boolean"))
                {
                    returnString = $" {fieldName} == true";
                }
            }
            else if (condition.IsEqual("false") || condition.IsEqual("0"))
            {
                if (type.IsEqual("boolean"))
                {
                    returnString = $" {fieldName} == false";
                }

            }
            else
            {
                throw new ArgumentOutOfRangeException($"{condition} Has no map");
            }
            if (ArrayProperty.HasValue())
            {
                return $" {(isOR == true ? " || " : " && ")} {ArrayProperty}.Any({returnString})";
            }
            return $" {(isOR == true ? " || " : " && ")} " + returnString;
        }
    }
}



