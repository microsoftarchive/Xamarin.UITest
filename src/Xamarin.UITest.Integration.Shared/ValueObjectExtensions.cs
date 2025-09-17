using System;
using System.Linq;
using DocoptNet;

namespace Xamarin.UITest.Integration.Shared
{
    public static class ValueObjectExtensions
    {
        public static string[] ToStringArray(this ValueObject obj)
        {
            if (obj != null && !obj.IsList)
            {
                throw new Exception("Tried to parse non-list value as list");
            }

            return obj.AsList.ToArray().Select(o => o.ToString()).ToArray();
        }
    }
}