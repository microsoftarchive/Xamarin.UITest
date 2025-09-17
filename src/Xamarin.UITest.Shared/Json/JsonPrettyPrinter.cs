using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Xamarin.UITest.Shared.Json
{
    public class JsonPrettyPrinter
    {
        public static void PrettyPrintObj(object obj, bool colorized = true, IndentFormat indentFormat = IndentFormat.Normal)
        {
            PrettyPrintJson(JsonConvert.SerializeObject(obj), colorized, indentFormat);
        }

        public static void PrettyPrintJson(string json, bool colorized = true, IndentFormat indentFormat = IndentFormat.Normal)
        {
            var jToken = JToken.Parse(json);

            var tokens = PrintToken(jToken);

            var lines = AggregateLines(tokens);

            int longestProperty = lines.Max(l => l.GetPropertyName().Length);

            Console.ForegroundColor = ConsoleColor.Yellow;

            foreach (var line in lines)
            {
                if (indentFormat != IndentFormat.None)
                {
                    Console.Write(new string(' ', line.Indentation * 4));
                }

                if (line.ContainsPropertyName() && indentFormat == IndentFormat.AlignArrows)
                {
                    Console.Write(new string(' ', longestProperty - line.GetPropertyName().Length));
                }

                foreach (var token in line.Tokens)
                {
                    if (colorized)
                    {
                        if (token.Color.HasValue)
                        {
                            Console.ForegroundColor = token.Color.Value;
                        }
                        else
                        {
                            Console.ResetColor();
                        }
                    }

                    Console.Write(token.Value);
                }

                Console.WriteLine();
            }
        }

        static JsonLine[] AggregateLines(IEnumerable<StringToken> tokens)
        {
            var lines = new List<JsonLine>();
            var currentIndent = 0;
            var lineIndent = 0;
            var lineTokens = new List<StringToken>();

            foreach (var token in tokens)
            {
                lineTokens.Add(token);

                if (token.NewLine)
                {
                    var line = new JsonLine(lineTokens.ToArray(), lineIndent);
                    lines.Add(line);

                    lineTokens.Clear();
                }

                currentIndent += token.IndentChange;

                if (token.NewLine)
                {
                    lineIndent = currentIndent;
                }
            }

            if (lineTokens.Any())
            {
                var line = new JsonLine(lineTokens.ToArray(), lineIndent);
                lines.Add(line);
            }

            return lines.ToArray();
        }

        public string PrintObject(object obj)
        {
            return PrintJson(JsonConvert.SerializeObject(obj));
        }

        public string PrintJson(string json)
        {
            var jToken = JToken.Parse(json);

            var tokens = PrintToken(jToken);

            var builder = new StringBuilder();

            int indent = 0;

            foreach (var token in tokens)
            {
                builder.Append(token.Value);

                indent += token.IndentChange;

                if (token.NewLine)
                {
                    builder.Append(Environment.NewLine);
                    builder.Append(new string(' ', indent * 2));
                }
            }

            var prettyJson = builder.ToString().TrimEnd('\r', '\n', '\t', ' ');

            return prettyJson;
        }

        static IEnumerable<StringToken> PrintToken(JToken token)
        {
            ConsoleColor? textColor = null;
            ConsoleColor? seperatorColor = null;
            var numberColor = ConsoleColor.Cyan;
            var stringColor = ConsoleColor.Yellow;
            var trueColor = ConsoleColor.Green;
            var falseColor = ConsoleColor.Red;
            var nullColor = ConsoleColor.Gray;
            var guidColor = ConsoleColor.DarkCyan;
            var dateColor = ConsoleColor.DarkGreen;

            switch (token.Type)
            {
                case JTokenType.None:
                    break;

                case JTokenType.Object:
                    yield return new StringToken("{", true, seperatorColor, 1);

                    var objectChildren = token.Children().ToArray();
                    for (var i = 0; i < objectChildren.Length; i++)
                    {
                        var child = objectChildren[i];

                        var tokens = PrintToken(child);

                        foreach (var stringToken in tokens)
                        {
                            yield return stringToken;
                        }

                        if (i != objectChildren.Length - 1)
                        {
                            yield return new StringToken(",", true, seperatorColor);
                        }
                    }

                    yield return new StringToken(string.Empty, true, seperatorColor, -1);
                    yield return new StringToken("}", false, seperatorColor);

                    break;

                case JTokenType.Array:
                    yield return new StringToken("[", true, seperatorColor, 1);

                    var children = token.Children().ToArray();
                    for(var i=0;i<children.Length;i++)
                    {
                        var child = children[i];

                        var tokens = PrintToken(child);
                        
                        yield return new StringToken("[", false, seperatorColor);
                        yield return new StringToken(i.ToString(), false, textColor);
                        yield return new StringToken("] ", false, seperatorColor);

                        foreach (var stringToken in tokens)
                        {
                            yield return stringToken;
                        }

                        if (i != children.Length - 1)
                        {
                            yield return new StringToken(",", true, seperatorColor);
                        }
                    }

                    yield return new StringToken(string.Empty, true, seperatorColor, -1);
                    yield return new StringToken("]", false, seperatorColor);

                    break;
          
                case JTokenType.Constructor:
                    break;
                case JTokenType.Property:
                    var jProperty = (JProperty)token;
                    yield return new StringToken(jProperty.Name, false, textColor);
                    yield return new StringToken(" => ", false, seperatorColor);

                    foreach (var valueToken in PrintToken(jProperty.Value))
                    {
                        yield return valueToken;
                    }
                    break;
                case JTokenType.Comment:
                    break;
                case JTokenType.Integer:
                    yield return new StringToken(token.Value<int>().ToString(), false, numberColor);
                    break;
                case JTokenType.Float:
                    yield return new StringToken(token.Value<float>().ToString(CultureInfo.InvariantCulture), false, numberColor);
                    break;
                case JTokenType.String:
                    yield return new StringToken("\"" + token.Value<string>() + "\"", false, stringColor);
                    break;
                case JTokenType.Boolean:
                    var value = token.Value<bool>();
                    if (value)
                    {
                        yield return new StringToken("true", false, trueColor);
                    }
                    else
                    {
                        yield return new StringToken("false", false, falseColor);
                    }
                    break;
                case JTokenType.Null:
                    yield return new StringToken("null", false, nullColor);
                    break;
                case JTokenType.Undefined:
                    yield return new StringToken("undefined", false, nullColor);
                    break;
                case JTokenType.Date:
                    yield return new StringToken(token.Value<DateTime>().ToString("yyyy-MM-dd HH:mm:ss"), false, dateColor);
                    break;
                case JTokenType.Raw:
                    break;
                case JTokenType.Bytes:
                    yield return new StringToken(token.Value<Byte[]>().ToString(), false, textColor);
                    break;
                case JTokenType.Guid:
                    yield return new StringToken(token.Value<Guid>().ToString(), false, guidColor);
                    break;
                case JTokenType.Uri:
                    yield return new StringToken(token.Value<Uri>().ToString(), false, stringColor);
                    break;
                case JTokenType.TimeSpan:
                    yield return new StringToken(token.Value<TimeSpan>().ToString(), false, dateColor);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}