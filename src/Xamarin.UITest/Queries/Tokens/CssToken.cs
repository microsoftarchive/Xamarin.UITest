using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.UITest.Shared.Queries;

namespace Xamarin.UITest.Queries.Tokens
{
    internal class CssToken : IQueryToken
    {
        private readonly string _cssSelector;

        public CssToken(string cssSelector)
        {
            _cssSelector = cssSelector;
        }

        public string ToQueryString(QueryPlatform queryPlatform)
        {
            return string.Format("css:'{0}'", new SingleQuoteEscapedString(_cssSelector));
        }

        public string ToCodeString()
        {
            return string.Format("Css(\"{0}\")", _cssSelector);
        }

        public string CssSelector
        {
            get { return _cssSelector; }
        }
    }
}
