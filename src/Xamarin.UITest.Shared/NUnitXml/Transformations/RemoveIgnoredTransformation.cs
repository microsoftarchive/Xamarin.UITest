using System;
using System.Linq;
using System.Xml.Linq;

namespace Xamarin.UITest.Shared.NUnitXml.Transformations
{
    public class RemoveIgnoredTransformation
    {
        public void Apply(XDocument xdoc)
        {
            foreach (var testResults in xdoc.Descendants("test-results").ToArray())
            {
                var ignoredValue = testResults.Attribute("ignored").Value;

                int ignored;
                if (Int32.TryParse(ignoredValue, out ignored))
                {
                    testResults.Attribute("ignored").SetValue("0");

                    var notRunValue = testResults.Attribute("not-run").Value;

                    int notRun;
                    if (Int32.TryParse(notRunValue, out notRun))
                    {
                        testResults.Attribute("not-run").SetValue(notRun - ignored);
                    }
                }
            }

            foreach (var testCase in xdoc.Descendants("test-case").ToArray())
            {
                if (testCase.Attribute("result").Value == "Ignored")
                {
                    testCase.Remove();
                }
            }
        }
    }
}

