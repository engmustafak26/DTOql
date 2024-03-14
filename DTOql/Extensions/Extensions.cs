using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DTOql.Extensions
{
    public static class Extensions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T element in source)
                action(element);
        }

        public static bool IsNullOrEmpty(this object source) =>
            ((source == null) || (source is string && String.IsNullOrEmpty((string)source)));

        public static bool HasNotValue(this string source) => string.IsNullOrWhiteSpace(source);
        public static bool ContainsIgnoreCase(this string source, string likeString) => Regex.IsMatch(source, Regex.Escape(likeString), RegexOptions.IgnoreCase);



        public static bool HasValue(this string source) => !string.IsNullOrWhiteSpace(source);
        public static string HasValueOrDefault(this string source, string defaultValue) => !string.IsNullOrWhiteSpace(source) ? source : defaultValue;
        public static bool IsEqual(this string source, string stringToCompare) => source?.Equals(stringToCompare, StringComparison.OrdinalIgnoreCase) == true;
        public static bool IsNotEqual(this string source, string stringToCompare) => !source.IsEqual(stringToCompare);

        public static Boolean And(this bool source, bool sourceOne) => source && sourceOne;

        public static Boolean OR(this bool source, bool sourceOne) => source || sourceOne;

    }
}