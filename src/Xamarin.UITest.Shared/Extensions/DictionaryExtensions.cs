using System;
using System.Collections.Generic;

namespace Xamarin.UITest.Shared.Extensions
{
    public static class DictionaryExtensions
    {
        public static string TryGetString<TK, TV>(this IDictionary<TK, TV> dictionary, TK key, string defaultValue = null)
        {
            TV value;

            if (dictionary.TryGetValue(key, out value))
            {
                return value as string ?? defaultValue;
            }

            return defaultValue;
        }

        public static bool TryGetBool<TK, TV>(this IDictionary<TK, TV> dictionary, TK key, bool defaultValue = false)
        {
            TV value;

            if (dictionary.TryGetValue(key, out value))
            {
                bool output;
                if(Boolean.TryParse(value.ToString(), out output))
                {
                    return output;
                }

                return defaultValue;
            }

            return defaultValue;
        }
    }
}

