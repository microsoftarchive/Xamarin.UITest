using System.Xml.Linq;
using Xamarin.UITest.Shared.Extensions;

namespace Xamarin.UITest.Shared.NUnitXml.Transformations
{
    public class AppendToTestNameTransformation
    {
        readonly string _textToAppend;

        public AppendToTestNameTransformation(string textToAppend)
        {
            _textToAppend = textToAppend;
        }

        public void Apply(XDocument xdoc)
        {
            foreach (var testCase in xdoc.Descendants("test-case"))
            {
                if (!testCase.Attribute("name").Value.IsNullOrWhiteSpace())
                {
                    testCase.Attribute("name").SetValue(testCase.Attribute("name").Value + _textToAppend);
                }
            }
        }
    }
}
