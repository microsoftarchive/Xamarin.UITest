namespace Xamarin.UITest.Queries.Tokens
{
    internal class WrappingToken : IQueryToken
    {
        readonly string _codeString;
        readonly IQueryToken _token;

        public WrappingToken(IQueryToken token, string codeString)
        {
            _codeString = codeString;
            _token = token;
        }

        public string ToQueryString(QueryPlatform queryPlatform)
        {
            return _token.ToQueryString(queryPlatform);
        }

        public string ToCodeString()
        {
            return _codeString;
        }
    }
}