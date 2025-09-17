namespace Xamarin.UITest.Queries.Tokens
{
    internal class AllToken : IQueryToken
    {
        internal readonly string _className;

        public AllToken(string className)
        {
            _className = className;
        }

        public string ToQueryString(QueryPlatform queryPlatform)
        {
            if (!string.IsNullOrWhiteSpace(_className))
            {
                return string.Format("all {0}", _className);
            }

            return "all *";
        }

        public string ToCodeString()
        {
            if (!string.IsNullOrWhiteSpace(_className))
            {
                return string.Format("All(\"{0}\")", _className);
            }

            return "All()";
        }
    }
}