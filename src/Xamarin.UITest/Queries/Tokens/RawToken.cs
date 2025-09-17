namespace Xamarin.UITest.Queries.Tokens
{
    internal class RawToken : IQueryToken
    {
        readonly string _rawQuery;
        readonly string _codeString;

        public RawToken(string rawQuery, string codeString = null)
        {
            _rawQuery = rawQuery;
            _codeString = codeString;
        }

        public string ToQueryString(QueryPlatform queryPlatform)
        {
            return _rawQuery;
        }

        public string ToCodeString()
        {
            if (string.IsNullOrWhiteSpace(_codeString))
            {
                return string.Format("Raw(\"{0}\")", _rawQuery);
            }

            return _codeString;
        }
    }
}