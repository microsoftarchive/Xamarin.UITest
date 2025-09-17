namespace Xamarin.UITest.Queries.Tokens
{
    internal class PropertyValueToken<T> : IQueryToken
	{
        readonly string _propertyName;

        public PropertyValueToken(string propertyName)
        {
            _propertyName = propertyName;
        }

        public string ToQueryString(QueryPlatform queryPlatform)
        {
            return string.Empty;
        }

        public string ToCodeString()
        {
            return string.Format("Property(\"{0}\").Value<{1}>()", _propertyName, typeof(T).Name);
        }
	}
}