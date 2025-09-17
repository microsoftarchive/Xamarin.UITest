using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Xamarin.UITest.Shared.Extensions
{
    public static class TypeExtension
    {
        public static Boolean IsAnonymousType(this Type type)
        {
            var hasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any();
            var nameContainsAnonymousType = type.FullName.Contains("AnonymousType") || type.FullName.Contains("AnonType");

            var isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;

            return isAnonymousType;
        }
    }
}