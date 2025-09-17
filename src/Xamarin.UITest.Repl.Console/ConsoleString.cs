using System;

namespace Xamarin.UITest.Repl
{
    public class ConsoleString
    {
        public string Message { get; set; }
        public ConsoleColor? Color { get; set; }

        public ConsoleString(string message, ConsoleColor? color = null)
        {
            Message = message;
            Color = color;
        }
    }
}