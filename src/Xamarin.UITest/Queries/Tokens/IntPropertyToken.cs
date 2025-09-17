namespace Xamarin.UITest.Queries.Tokens
{
    internal class IntPropertyToken : IQueryToken
    {
        readonly string _propertyName;
        readonly int _value;

        public IntPropertyToken(string propertyName, int value)
        {
            _propertyName = propertyName;
            _value = value;
        }

        public string ToQueryString(QueryPlatform queryPlatform)
        {
            return string.Format("{0}:{1}", _propertyName, _value);
        }

        public string ToCodeString()
        {
            return string.Format("Property(\"{0}\", {1})", _propertyName, _value);
        }
    }
}