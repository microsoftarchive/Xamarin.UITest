namespace Xamarin.UITest.Queries.Tokens
{
    internal class ValueToken<T> : IQueryToken
    {
        public string ToQueryString(QueryPlatform queryPlatform)
        {
            return string.Empty;
        }

        public string ToCodeString()
        {
            return string.Format("Value<{0}>()", typeof(T).Name);
        }
    }
}