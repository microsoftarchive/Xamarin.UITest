using System.Collections.Generic;
using Xamarin.UITest.Shared.Extensions;
using Xamarin.UITest.Shared.Logging;

namespace Xamarin.UITest.Utils
{
    internal class LoggerTreePrinter : ITreePrinter
    {
        public void PrintTreeElement(TreeElement element, int indent = 0)
        {
            var output = new string(' ', indent * 2);

            output += string.Format("[{0}]", element.SimplifiedType);

            var extras = new List<string>();

            if (!element.Id.IsNullOrWhiteSpace())
            {
                extras.Add(string.Format("id: \"{0}\"", element.Id));
            }

            if (!element.Label.IsNullOrWhiteSpace())
            {
                extras.Add(string.Format("label: \"{0}\"", element.Label));
            }

            if (!element.Text.IsNullOrWhiteSpace())
            {
                extras.Add(string.Format("text: \"{0}\"", element.Text));
            }

            output += string.Format(" {0}", string.Join(", ", extras));

            if (!element.Visible)
            {
                output += " (center not on screen)";
            }

            Log.Info(output);

            foreach (var child in element.Children)
            {
                PrintTreeElement(child, indent + 1);
            }
        }
    }
}
