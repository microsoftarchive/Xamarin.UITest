using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xamarin.UITest.Shared.Logging;

namespace Xamarin.UITest.Shared.AssemblyAnalysis
{
    public class TypeNameConverter
    {
        readonly Assembly Assembly;

        public TypeNameConverter(Assembly assembly)
        {
            Assembly = assembly;
        }

        public IEnumerable<string> ConvertTypeNamesToFullNames(IEnumerable<string> typeNames)
        {
            if (typeNames == null || !typeNames.Any()) return typeNames;

            var matches = typeNames.ToList();
            IEnumerable<Type> types = null;

            try
            {
                types = Assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types.Where(t => t != null);
            }

            if (types == null || !types.Any()) return typeNames;

            var classNames = types.Select(e => e.Name);
            var qualifiedClassNames = types.Select(e => e.FullName);

            foreach (string typeName in typeNames)
            {
                if (classNames.Any(e => e.Equals(typeName)))
                {
                    var convertedClassNames = qualifiedClassNames.Where(e => e.EndsWith($".{typeName}"));

                    if (convertedClassNames.Count() > 1)
                    {
                        var matched = string.Join("\n", convertedClassNames);
                        Log.Info($"INFO: More than one occurance of class name in assembly '{typeName}': {matched}");
                    }

                    matches.Remove(typeName);
                    matches.AddRange(convertedClassNames);
                }
            }

            return matches;
        }
    }
}
