using System;
using NUnit.Framework;
using Xamarin.UITest.Shared.Resources;
using Should;
using System.Reflection;
using Xamarin.UITest.Shared.NUnitXml.Transformations;
using System.Xml.Linq;
using System.Linq;
using Xamarin.UITest.Shared.NUnitXml;

namespace Xamarin.UITest.Tests.NUnitXml
{
    [TestFixture]
    public class NUnitXmlTests
    {
        readonly EmbeddedResourceLoader _resourceLoader = new EmbeddedResourceLoader();

        [Test]
        public void RemoveIgnoredTest()
        {
            XDocument xdoc = GetiPadMiniXDocument();

            new RemoveIgnoredTransformation().Apply(xdoc);

            xdoc.Descendants("test-case")
                .Count(x => x.Attribute("result").Value == "Ignored")
                .ShouldEqual(0);

            xdoc.Descendants("test-results")
                .Count(x => x.Attribute("ignored").Value != "0")
                .ShouldEqual(0);

            Console.WriteLine(xdoc);
        }

        [Test]
        public void RemoveIgnoredCountsDownNotRunCorrectlyTest()
        {
            XDocument xdoc = GetiPadMiniXDocument();

            xdoc.Descendants("test-results")
                .First()
                .Attribute("not-run")
                .SetValue("42");

            new RemoveIgnoredTransformation().Apply(xdoc);

            xdoc.Descendants("test-results")
                .Count(x => x.Attribute("not-run").Value == "41")
                .ShouldEqual(1);

            Console.WriteLine(xdoc);
        }

        [Test]
        public void RemoveEmptyTestSuitesTest()
        {
            XDocument xdoc = GetiPadMiniXDocument();

            new RemoveEmptySuitesTransformation().Apply(xdoc);

            xdoc.Descendants("test-suite")
                .Count(x => !x.Descendants().Any())
                .ShouldEqual(0);

            Console.WriteLine(xdoc);
        }

        [Test]
        public void RemoveTestSuitesContainingOnlyEmptyResultsTagTest()
        {
            XDocument xdoc = GetiPadMiniXDocument();

            new RemoveIgnoredTransformation().Apply(xdoc);
            new RemoveEmptySuitesTransformation().Apply(xdoc);

            xdoc.Descendants("test-suite")
                .Count(x => x.Descendants().Count() <= 1)
                .ShouldEqual(0);

            Console.WriteLine(xdoc);
        }

        [Test]
        public void AppendToTestNamesTests()
        {
            XDocument xdoc = GetiPadMiniXDocument();

            new AppendToTestNameTransformation("_xamarin_rocks").Apply(xdoc);
            new RemoveEmptySuitesTransformation().Apply(xdoc);

            xdoc.Descendants("test-case")
                .Count(x => x.Attribute("name").Value == "Xamarin.UITest.Tests.Samples.NUnit.LPSimpleExampleiOSTests.BasicTest_xamarin_rocks")
                .ShouldEqual(1);

            Console.WriteLine(xdoc);
        }

        [Test]
        public void CombineTwoResultFilesTest()
        {
            XDocument xdoc1 = GetiPadMiniXDocument();
            XDocument xdoc2 = GetiPhone4sXDocument();

            var combiner = new NUnitXmlCombiner();

            var combinedDoc = combiner.Combine(xdoc1, xdoc2);

            var xdoc1SuiteCount = xdoc1.Descendants("test-suite").Count();
            var xdoc2SuiteCount = xdoc2.Descendants("test-suite").Count();

            combinedDoc.Descendants("test-suite").Count()
                .ShouldEqual(xdoc1SuiteCount + xdoc2SuiteCount);

            combinedDoc.Descendants("test-results")
                .First()
                .Attribute("total").Value
                .ShouldEqual("2");

            Console.WriteLine(combinedDoc);
        }

        XDocument GetiPadMiniXDocument()
        {
            var xml = _resourceLoader.GetEmbeddedResourceString(Assembly.GetExecutingAssembly(), "apple_ipad_mini_6.0.2_nunit_report_cd3c95ba-fc32-4842-990f-8b648bab6d83.xml");
            return XDocument.Parse(xml);
        }

        XDocument GetiPhone4sXDocument()
        {
            var xml = _resourceLoader.GetEmbeddedResourceString(Assembly.GetExecutingAssembly(), "apple_iphone_4s_6.1.3_nunit_report_c90dba3b-4360-442a-b5f7-1fb2adc363b9.xml");
            return XDocument.Parse(xml);
        }
    }
}

