using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xamarin.UITest.Android;
using Xamarin.UITest.iOS;
using Xamarin.UITest.Shared.Logging;

namespace Xamarin.UITest.Events
{
    internal static class EventManager
    {
        public static void AfterAppStarted(IApp app)
        {
            TriggerAfterAppStarted(app);
        }

        static void TriggerAfterAppStarted(IApp app)
        {
            foreach (var methodInfo in GetStaticMethodsWithAttribute(typeof(AfterAppStartedAttribute)))
            {
                var parameterInfos = methodInfo.GetParameters();

                if (parameterInfos.Length == 0)
                {
                    methodInfo.Invoke(null, new object[] { });
                }
                else if (parameterInfos.Length == 1)
                {
                    var parameterInfo = parameterInfos.First();

                    if (parameterInfo.ParameterType == typeof(IApp))
                    {
                        methodInfo.Invoke(null, new object[] { app });
                    }
                    else if (parameterInfo.ParameterType == typeof(AndroidApp))
                    {
                        if (app is AndroidApp)
                        {
                            methodInfo.Invoke(null, new object[] { app as AndroidApp });
                        }
                    }
                    else if (parameterInfo.ParameterType == typeof(iOSApp))
                    {
                        if (app is iOSApp)
                        {
                            methodInfo.Invoke(null, new object[] { app as iOSApp });
                        }
                    }
                }
                else
                {
                    throw new Exception(string.Format("Invalid signature found for method: {0} in type: {1}{2}Valid options are: Zero arguments or one argument of one of these types: {3}, {4} or {5}.",
                        methodInfo.Name,
                        methodInfo.DeclaringType.FullName,
                        Environment.NewLine,
                        typeof(IApp).FullName,
                        typeof(AndroidApp).FullName,
                        typeof(iOSApp).FullName));
                }
            }
        }

        static IEnumerable<MethodInfo> GetStaticMethodsWithAttribute(Type attributeType)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var list = new List<MethodInfo>();

            foreach (var assembly in assemblies)
            {
                try
                {
                    var methodInfos = (from type in assembly.GetTypes()
                                       from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                                       from attributes in method.GetCustomAttributes(attributeType)
                                       select method);

                    list.AddRange(methodInfos);
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Log.Debug("Unable to scan assembly: " + assembly, ex);
                }
                catch (Exception ex)
                {
                    Log.Info("Unable to scan assembly: " + assembly, ex);
                }
            }

            return list;
        }
    }
}