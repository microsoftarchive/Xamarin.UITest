using System;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;
using NUnit.Framework;
using Should;
using Xamarin.UITest.Shared.Android;
using Xamarin.UITest.Shared.Resources;

namespace Xamarin.UITest.Tests.Utils
{
    [TestFixture]
    public class AndroidXmltreeParserTests
    {
        [Test]
        public void CanLoadTasky()
        {
            var loader = new EmbeddedResourceLoader();

            var input = loader.GetEmbeddedResourceString(Assembly.GetExecutingAssembly(), "tasky-xmltree.txt");

            var document = new AndroidXmltreeParser().GetXml(input);

            Console.WriteLine(document.ToString());

            document.Root.ShouldNotBeNull();
            document.Root.Name.ShouldEqual("manifest");
            document.Descendants().Count().ShouldEqual(10);

            var namespaces = document.Root
                .CreateNavigator()
                .GetNamespacesInScope(XmlNamespaceScope.All);

            namespaces.Count.ShouldEqual(2);
            namespaces.ContainsKey("xml").ShouldBeTrue();
            namespaces.ContainsKey("android").ShouldBeTrue();
            namespaces["android"].ShouldEqual("http://schemas.android.com/apk/res/android");
        }

        [Test]
        public void CanLoadFlipboard()
        {
            var loader = new EmbeddedResourceLoader();

            var input = loader.GetEmbeddedResourceString(Assembly.GetExecutingAssembly(), "flipboard-xmltree.txt");

            var document = new AndroidXmltreeParser().GetXml(input);

            Console.WriteLine(document.ToString());

            document.Root.ShouldNotBeNull();
            document.Root.Name.ShouldEqual("manifest");
            document.Descendants().Count().ShouldEqual(162);

            var namespaces = document.Root
                .CreateNavigator()
                .GetNamespacesInScope(XmlNamespaceScope.All);

            namespaces.Count.ShouldEqual(2);
            namespaces.ContainsKey("xml").ShouldBeTrue();
            namespaces.ContainsKey("android").ShouldBeTrue();
            namespaces["android"].ShouldEqual("http://schemas.android.com/apk/res/android");
        }
    }
}
