using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;

namespace Xamarin.UITest.Shared.Extensions
{
    public static class ObjectExtensions
    {
        public static string Stringify(this object info)
        {
            return info.InternalStringify();
        }

        static string InternalStringify(this object info, int depth = 0)
        {
            if (depth >= 10)
            {
                return "...";
            }

            if (info == null || info is Exception)
            {
                return string.Empty;
            }

            if (info is string)
            {
                return string.Format("\"{0}\"", info);
            }

            if (info is IEnumerable)
            {
                return string.Format("[ {0} ]", string.Join(", ", ((IEnumerable)info).Cast<object>().Select(x => x.InternalStringify(depth + 1))));
            }

            if (info.GetType().IsValueType)
            {
                return info.ToString();
            }

            var propertyCollection = TypeDescriptor.GetProperties(info);

            var pairs = propertyCollection.OfType<PropertyDescriptor>()
                .Select(x => string.Format("{0}: {1}", x.Name, (x.GetValue(info) != null ? x.GetValue(info).InternalStringify(depth + 1) : "null")))
                .ToArray();

            if (!pairs.Any())
            {
                return string.Empty;
            }

            return string.Format("{{ {0} }}", string.Join(", ", pairs));
        }

    }
}