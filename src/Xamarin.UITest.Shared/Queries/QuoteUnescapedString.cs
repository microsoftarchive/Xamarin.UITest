namespace Xamarin.UITest.Shared.Queries
{
    internal class QuoteUnescapedString(string unescapedString)
    {
        public readonly string UnescapedString = unescapedString;

        public bool IsNullOrWhiteSpace()
        {
            return string.IsNullOrWhiteSpace(UnescapedString);
        }
    }
}

