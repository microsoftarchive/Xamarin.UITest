using System;
using NUnit.Framework;
using Xamarin.UITest.iOS;
using System.Linq;
using System.Collections.Generic;
using Xamarin.UITest.Utils;
using Xamarin.UITest.Shared.Http;
using System.Reflection;
using Xamarin.UITest.Shared.Resources;

namespace Xamarin.UITest.Tests.Utils
{
    [TestFixture] 
    public class iOSDumpTest
    {
        class NoWaitTimes : IWaitTimes
        {
            public TimeSpan WaitForTimeout { get; } = TimeSpan.MinValue;
            public TimeSpan GestureWaitTimeout { get; } = TimeSpan.MinValue;
            public TimeSpan GestureCompletionTimeout { get; } = TimeSpan.MinValue;
        }

        [Test]
        public void NewDumpWorks() {
            var calabashServerVersion = new VersionNumber("0.20.0");
            var iOSDump = new EmbeddedResourceLoader().GetEmbeddedResourceString(Assembly.GetExecutingAssembly(), "ios-dump-0.12.2.txt");
            var notusedDevice = new iOSCalabashDevice(new iOSVersionInfo(new Dictionary<string,object>()), calabashServerVersion);
            var connection = new MockCalabashConnection();
            var deviceConnectionInfo = new DeviceConnectionInfo("mock-device", connection, false);

            connection.DumpReply = new HttpResult(100, iOSDump);
            var gesture = new iOSGestures(
                deviceConnectionInfo,
                notusedDevice,
                null,
                new NoWaitTimes());

            var treeRoot = gesture.Dump().First();
            var elements = Flatten(treeRoot);
            var simpleTypes = elements.Select(e => e.SimplifiedType).ToArray();
            Assert.IsNotEmpty(simpleTypes);
        }

        IEnumerable<TreeElement> Flatten(TreeElement treeElement)
        {
            var children = treeElement.Children.SelectMany(c => Flatten(c)).ToList();
            children.Add(treeElement);
            return children;
        }
    }

    class MockCalabashConnection : ICalabashConnection 
    {
        internal HttpResult DumpReply { get; set;}

        public HttpResult Map(object arguments)
        {
            throw new NotImplementedException();
        }
        public HttpResult Location(object arguments)
        {
            throw new NotImplementedException();
        }
        public HttpResult UIA(string command)
        {
            throw new NotImplementedException();
        }
        public HttpResult Condition(object condition)
        {
            throw new NotImplementedException();
        }
        public HttpResult Backdoor(object condition)
        {
            throw new NotImplementedException();
        }
        public HttpResult Version()
        {
            throw new NotImplementedException();
        }
        public HttpResult Dump()
        {
            return DumpReply;
        }
        public HttpResult Suspend(double seconds)
        {
            throw new NotImplementedException();
        }
        public HttpResult Exit()
        {
            throw new NotImplementedException();
        }
        public HttpResult ClearText()
        {
            throw new NotImplementedException();
        }
    }
}


