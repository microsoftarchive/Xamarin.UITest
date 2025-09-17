using System;

namespace Xamarin.UITest.Shared.Json
{
    public class StringToken
    {
        public StringToken(string value, bool newLine, ConsoleColor? color = null, int indentChange = 0)
        {
            Value = value;
            NewLine = newLine;
            Color = color;
            IndentChange = indentChange;
        }

        public bool NewLine { get; private set; }

        public int IndentChange { get; private set; }

        public string Value { get; private set; }

        public ConsoleColor? Color { get; private set; }
    }
}