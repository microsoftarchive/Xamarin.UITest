namespace Xamarin.UITest.Queries.Tokens
{

    internal class BoolPropertyToken : IQueryToken
    {
        readonly string _propertyName;
        readonly bool _value;

        public BoolPropertyToken(string propertyName, bool value)
        {
            _propertyName = propertyName;
            _value = value;
        }

        public string ToQueryString(QueryPlatform queryPlatform)
        {
            if (queryPlatform == QueryPlatform.iOS)
            {
                return string.Format("{0}:{1}", _propertyName, (_value ? 1 : 0));
            }

            return string.Format("{0}:{1}", _propertyName, _value.ToString().ToLowerInvariant());
        }

        public string ToCodeString()
        {
            return string.Format("Property(\"{0}\", {1})", _propertyName, _value.ToString().ToLowerInvariant());
        }
    }
}