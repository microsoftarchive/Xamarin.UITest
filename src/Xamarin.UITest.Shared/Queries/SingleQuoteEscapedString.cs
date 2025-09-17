using Xamarin.UITest.Shared.Extensions;

namespace Xamarin.UITest.Shared.Queries
{
    internal class SingleQuoteEscapedString(string unescapedString) : QuoteUnescapedString(unescapedString)
    {
        public override string ToString()
        {
            return UnescapedString.EscapeSingleQuotes();
        }
    }
}