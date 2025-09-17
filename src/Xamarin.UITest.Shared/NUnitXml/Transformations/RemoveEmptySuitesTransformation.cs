using System.Xml.Linq;
using System.Linq;

namespace Xamarin.UITest.Shared.NUnitXml.Transformations
{
    public class RemoveEmptySuitesTransformation
    {
        public void Apply(XDocument xdoc)
        {
            foreach (var testSuite in xdoc.Descendants("test-suite").ToArray())
            {
                if (testSuite.Descendants().Count() <= 1)
                {
                    testSuite.Remove();
                }
            }
        }
    }
}
