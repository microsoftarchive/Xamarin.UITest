using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xamarin.UITest.Shared.Extensions;

namespace Xamarin.UITest.Shared.Android
{
    public class AndroidXmltreeParser
    {
        public XDocument GetXml(string input)
        {
            var lines = input.Split(new [] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            var document = new XDocument();

            var stack = new Stack<ParsedElement>();

            var namespaces = new Dictionary<string, XNamespace>();

            foreach (var line in lines)
            {
                var trimmedLine = line.TrimStart();
                var indent = line.Length - trimmedLine.Length;

                if (trimmedLine.StartsWith("E"))
                {
                    while (stack.Any() && stack.Peek().Indent >= indent)
                    {
                        stack.Pop();
                    }

                    var match = Regex.Match(trimmedLine, @"^E:\s*((?<ns>[^:]+):)?(?<name>.*) \(line=\d+\)$");

                    if (!match.Success)
                    {
                        throw new Exception($"Invalid element: {line} in file: {Environment.NewLine}{input}");
                    }

                    var element = new XElement(GetXName(match, namespaces, line, input));

                    if (!stack.Any())
                    {
                        foreach (var pair in namespaces)
                        {
                            element.Add(new XAttribute(XNamespace.Xmlns + pair.Key, pair.Value));
                        }

                        document.Add(element);
                    }
                    else
                    {
                        stack.Peek().Element.Add(element);
                    }

                    stack.Push(new ParsedElement(element, indent));
                }
                else if (trimmedLine.StartsWith("A"))
                {
                    var match = Regex.Match(trimmedLine, @"^A:\s*((?<ns>[^:]+):)?(?<name>[^(]+)(\(.*\))?=(?<value>.*)$");

                    if (!match.Success)
                    {
                        throw new Exception($"Invalid attribute: {line} in file: {Environment.NewLine}{input}");
                    }

                    var value = match.Groups["value"].Value;

                    var strMatch = Regex.Match(value, @"\""(?<value>.*)\""\s*\(Raw:.*\)");

                    var xName = GetXName(match, namespaces, line, input);

                    stack.Peek()
                        .Element.Add(strMatch.Success
                            ? new XAttribute(xName, strMatch.Groups["value"].Value)
                            : new XAttribute(xName, value));
                }
                else if (trimmedLine.StartsWith("N"))
                {
                    var match = Regex.Match(trimmedLine, @"^N:\s*(?<ns>[^=]+)=(?<url>.*)$");

                    if (!match.Success)
                    {
                        throw new Exception($"Invalid namespace: {line} in file: {Environment.NewLine}{input}");
                    }

                    var namespaceName = match.Groups["ns"].Value;

                    if (!namespaces.ContainsKey(namespaceName))
                    {
                        namespaces.Add(namespaceName, XNamespace.Get(match.Groups["url"].Value));
                    }
                }
            }

            return document;
        }

        static XName GetXName(Match match, Dictionary<string, XNamespace> namespaces, string line, string input)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));
            if (namespaces == null) throw new ArgumentNullException(nameof(namespaces));
            if (line == null) throw new ArgumentNullException(nameof(line));
            if (input == null) throw new ArgumentNullException(nameof(input));

            var namespaceName = match.Groups["ns"].Value;

            if (!namespaceName.IsNullOrWhiteSpace() && !namespaces.ContainsKey(namespaceName))
            {
                throw new Exception($"Unknown xml namespace: {namespaceName} in file: {Environment.NewLine}{input}");
            }

            XName xName;

            try
            {
                xName = namespaceName.IsNullOrWhiteSpace()
                    ? XName.Get(match.Groups["name"].Value)
                    : XName.Get(match.Groups["name"].Value, namespaces[namespaceName].ToString());
            }
            catch
            {
                throw new Exception($"Invalid attribute: {line} in file: {Environment.NewLine}{input}");
            }

            return xName;
        }

        class ParsedElement
        {
            public XElement Element { get; }
            public int Indent { get; }

            public ParsedElement(XElement element, int indent)
            {
                Element = element;
                Indent = indent;
            }
        }
    }
}