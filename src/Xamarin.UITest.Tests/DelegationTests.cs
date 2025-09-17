using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Should;
using Xamarin.UITest.Android;
using Xamarin.UITest.iOS;
using Xamarin.UITest.Shared;

namespace Xamarin.UITest.Tests
{
    [TestFixture]
    public class DelegationTests
    {
        [Test]
        public void WaitForHelperDelegationAndroid()
        {
            var delegateeMethods = GetMethodSet<WaitForHelper>();
            var delegatorMethods = GetMethodSet<AndroidApp>();

            var missingMethods = delegateeMethods.Except(delegatorMethods);

            string.Join(Environment.NewLine, missingMethods).ShouldEqual(string.Empty);
        }

        [Test]
        public void WaitForHelperDelegationiOS()
        {
            var delegateeMethods = GetMethodSet<WaitForHelper>();
            var delegatorMethods = GetMethodSet<iOSApp>();

            var missingMethods = delegateeMethods.Except(delegatorMethods);

            string.Join(Environment.NewLine, missingMethods).ShouldEqual(string.Empty);
        }

        public ISet<string> GetMethodSet<T>()
        {
            var methodInfos = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            methodInfos = methodInfos
                .Where(x => x.GetCustomAttribute<NoDelegationAttribute>() == null)
                .ToArray();

            return new HashSet<string>(methodInfos.Select(GetMethodRepresentation));
        }

        static string GetMethodRepresentation(MethodInfo methodInfo)
        {
            return string.Format("{0} {1}({2})", methodInfo.ReturnType.FullName, methodInfo.Name, string.Join(", ", methodInfo.GetParameters().Select(x => x.ParameterType + " " + x.Name)));
        }
    }
}