using System;
using System.Text.RegularExpressions;
using Xamarin.UITest.Shared.Extensions;

namespace Xamarin.UITest.Shared.Queries
{
    /// <summary>
    /// Required so the calabash-android server is able to parse valid XPath locators which contain single quotes.
    /// This workaround enables the use of [@id='id'] and [text()='text'].
    /// </summary>
    internal class XPathSingleQuoteWorkAroundString(string unescapedString) : QuoteUnescapedString(unescapedString.Replace("'", "\""))
    {
        public override string ToString()
        {
            return UnescapedString;
        }
    }
}