using Xamarin.UITest.Shared.Extensions;
using Xamarin.UITest.Shared.Queries;

namespace Xamarin.UITest.Queries.Tokens
{
    internal class ButtonToken : IQueryToken
    {
        readonly SingleQuoteEscapedString _marked;

        public ButtonToken(SingleQuoteEscapedString marked)
        {
            _marked = marked;
        }

        public string ToQueryString(QueryPlatform queryPlatform)
        {
            if (queryPlatform == QueryPlatform.Android)
            {
                if (!_marked.IsNullOrWhiteSpace())
                {
                    return string.Format("android.widget.Button marked:'{0}'", _marked);
                }

                return "android.widget.Button";
            }

            if (!_marked.IsNullOrWhiteSpace())
            {
                return string.Format("button marked:'{0}'", _marked);
            }

            return "button";
        }

        public string ToCodeString()
        {
            if (!_marked.IsNullOrWhiteSpace())
            {
                return string.Format("Button(\"{0}\")", _marked.UnescapedString);
            }

            return "Button()";
        }
    }
}