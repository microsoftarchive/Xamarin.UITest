namespace Xamarin.UITest.Queries.Tokens
{
    internal class DirectionalToken : IQueryToken
    {
        readonly string _className;
        readonly string _modifier;
        readonly string _codeName;

        public DirectionalToken(string className, string modifier, string codeName)
        {
            _className = className;
            _modifier = modifier;
            _codeName = codeName;
        }

        public string ToQueryString(QueryPlatform queryPlatform)
        {
            if (!string.IsNullOrWhiteSpace(_className))
            {
                return string.Format("{1} {0}", _className, _modifier);
            }

            return string.Format("{0} *", _modifier);
        }

        public string ToCodeString()
        {
            if (!string.IsNullOrWhiteSpace(_className))
            {
                return string.Format("{1}(\"{0}\")", _className, _codeName);
            }

            return string.Format("{0}()", _codeName);
        }
    }
}