namespace Xamarin.UITest.Queries.Tokens
{
    internal class HiddenToken : IQueryToken
    {
        readonly string _rawQuery;

        public HiddenToken(string rawQuery)
        {
            _rawQuery = rawQuery;
        }

        public string ToQueryString(QueryPlatform queryPlatform)
        {
            return _rawQuery;
        }

        public string ToCodeString()
        {
            return string.Empty;
        }
    }
}