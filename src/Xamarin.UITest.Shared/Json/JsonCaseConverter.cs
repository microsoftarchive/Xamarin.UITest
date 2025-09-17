using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Xamarin.UITest.Shared.Json
{
    public class JsonCaseConverter
    {
        static readonly Regex SnakeCaseRegex = new Regex("_.", RegexOptions.IgnoreCase);

        public string ConvertedCaseJObject(string json)
        {
            var jsonObject = JObject.Parse(json);
            var convertedCaseJObject = new JObject(jsonObject.Children().Select(MapToken));
            return convertedCaseJObject.ToString();
        }

        object MapToken(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Property:
                    var property = (JProperty)token;
                    var camelCaseName = SnakeCaseRegex.Replace(property.Name, match => match.Value.Substring(1, 1).ToUpperInvariant());
                    var pascalCaseName = camelCaseName.Substring(0, 1).ToUpperInvariant() + camelCaseName.Substring(1);
					return new JProperty(pascalCaseName, MapToken(property.Value));
				
				case JTokenType.Array:
					var array = (JArray)token;
					return new JArray(array.Select(x => MapToken(x)));

				case JTokenType.Object:
					var obj = (JObject)token;
					return new JObject(obj.Children().Select(MapToken));
            }

            return token;
        }
    }
}