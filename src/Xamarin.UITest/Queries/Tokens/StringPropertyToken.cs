using Xamarin.UITest.Shared.Queries;

namespace Xamarin.UITest.Queries.Tokens
{
    internal class StringPropertyToken<TEscapedString> : IQueryToken where TEscapedString : QuoteUnescapedString
    {
        readonly string _propertyName;
        readonly TEscapedString _value;

        public StringPropertyToken(string propertyName, TEscapedString value)
        {
            _propertyName = propertyName;
            _value = value;
        }

        public string ToQueryString(QueryPlatform queryPlatform)
        {
            return string.Format("{0}:'{1}'", _propertyName, _value);
        }

        public string ToCodeString()
        {
            return string.Format("Property(\"{0}\", \"{1}\")", _propertyName, _value.UnescapedString);
        }
    }
}