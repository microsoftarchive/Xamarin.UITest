using System;

namespace Xamarin.UITest.Queries.Tokens
{
    internal class QueryToken : IQueryToken
    {
        readonly AppQuery _query;
        readonly string _codeString;

        public QueryToken(AppQuery query, string codeString)
        {
            _query = query;
            _codeString = codeString;
        }

        public string ToQueryString(QueryPlatform queryPlatform)
        {
            return _query.ToString();
        }

        public string ToCodeString()
        {
            return _codeString;
        }
    }
}