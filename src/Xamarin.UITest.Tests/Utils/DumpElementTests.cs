using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using NUnit.Framework;
using Should;
using Xamarin.UITest.Shared.Resources;
using Xamarin.UITest.Utils;

namespace Xamarin.UITest.Tests.Utils
{
    public class DumpElementTests
    {
        [Test]
        public void CanParseiOSOutput()
        {
            var iOSDump = new EmbeddedResourceLoader().GetEmbeddedResourceString(Assembly.GetExecutingAssembly(), "ios-dump-0.12.2.txt");

            var dumpElement = JsonConvert.DeserializeObject<DumpElement>(iOSDump);

            Console.WriteLine(JsonConvert.SerializeObject(dumpElement, Formatting.Indented));

            var allElements = dumpElement.GetAllElements();

            allElements.Count().ShouldEqual(51);
            allElements.Count(x => x.type == "UIButtonLabel").ShouldEqual(1);
        }
    }
}