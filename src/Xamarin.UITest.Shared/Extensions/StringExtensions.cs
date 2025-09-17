using System;

namespace Xamarin.UITest.Shared.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        public static bool StartsWithIgnoreCase(this string str, string value)
        {
            if (str == null)
            {
                return false;
            }

            return str.StartsWith(value, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool EndsWithIgnoreCase(this string str, string value)
        {
            if (str == null)
            {
                return false;
            }

            return str.EndsWith(value, StringComparison.InvariantCultureIgnoreCase);
        }

        public static string EscapeSingleQuotes(this string str)
        {
            if (str.IsNullOrWhiteSpace())
            {
                return str;
            }

            return str.Replace("'", @"\'");
        }

        public static string EscapeBackslashes(this string str)
        {
            if (str.IsNullOrWhiteSpace())
            {
                return str;
            }

            return str.Replace(@"\", @"\\");
        }
    }
}
