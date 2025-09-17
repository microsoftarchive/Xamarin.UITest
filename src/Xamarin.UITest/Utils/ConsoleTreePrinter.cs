using System;
using System.Linq;

namespace Xamarin.UITest.Utils
{
    internal class ConsoleTreePrinter : ITreePrinter
    {
        public void PrintTreeElement(TreeElement element, int indent = 0)
        {
            Write(new string(' ', indent * 2));

            if (!element.Visible)
            {
                Write("[");
                Write(element.SimplifiedType, ConsoleColor.Gray);
                Write("]");
            }
            else
            {
                Write(string.Format("[{0}]", element.SimplifiedType));
            }

            if (!string.IsNullOrWhiteSpace(element.Id))
            {
                Write(" id: ");
                Write("\"" + element.Id + "\"", ConsoleColor.Yellow);
            }

            if(!string.IsNullOrWhiteSpace(element.Id) && !string.IsNullOrWhiteSpace(element.Label))
            {
                Write(", ");
            }

            if(!string.IsNullOrWhiteSpace(element.Label))
            {
                Write(" label: ");
                Write("\"" + element.Label + "\"", ConsoleColor.Cyan);
            }
                
            if (!string.IsNullOrWhiteSpace(element.Label) && !string.IsNullOrWhiteSpace(element.Text))
            {
                Write(", ");
            }

            if (!string.IsNullOrWhiteSpace(element.Text))
            {
                Write(" text: ");
                Write("\"" + element.Text + "\"", ConsoleColor.Green);
            }

            if (!element.Visible)
            {
                Write(" (center not on screen)");
            }

            WriteLine(string.Empty);

            foreach (var child in element.Children)
            {
                PrintTreeElement(child, indent + 1);
            }
        }

        static void Write(string str = null, ConsoleColor? color = null)
        {
            if (color != null)
            {
                Console.ForegroundColor = color.Value;
            }

            Console.Write(str);
            Console.ResetColor();
        }

        static void WriteLine(string str, ConsoleColor? color = null)
        {
            if (color != null)
            {
                Console.ForegroundColor = color.Value;
            }

            Console.WriteLine(str);
            Console.ResetColor();
        }
    }
}
