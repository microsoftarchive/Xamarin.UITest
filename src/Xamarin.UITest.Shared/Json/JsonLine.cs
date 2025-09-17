using System.Linq;

namespace Xamarin.UITest.Shared.Json
{
    class JsonLine
    {
        readonly int _indentation;
        readonly StringToken[] _tokens;

        public JsonLine(StringToken[] tokens, int indendation = 0)
        {
            _tokens = tokens;
            _indentation = indendation;
        }

        public bool ContainsPropertyName()
        {
            return Tokens.Any(t => t.Value.Contains("=>"));
        }

        public string GetPropertyName()
        {
            var propertyName = "";
            if (ContainsPropertyName())
            {
                foreach (var token in Tokens)
                {
                    if (token.Value.Contains("=>"))
                    {
                        return propertyName;
                    }
                    propertyName += token.Value;
                }
            }
            return propertyName;
        }

        public int Indentation { get { return _indentation; } }

        public StringToken[] Tokens { get { return _tokens; } }
    }
}