using System;

namespace Xamarin.UITest.Utils
{
    internal class TreePrintHelper
    {
        readonly IGestures _gestures;

        public TreePrintHelper(IGestures gestures)
        {
            _gestures = gestures;
        }

        public void PrintTree(ITreePrinter treePrinter)
        {
            TreeElement[] elements = null;

            for (var i = 0; i < 5; i++)
            {
                elements = _gestures.Dump();

                if (elements != null)
                {
                    break;
                }
            }

            if (elements == null)
            {
                throw new Exception("Unable to print tree or nothing was visible.");
            }

            foreach (var element in elements)
            {
                element.Condense();
                treePrinter.PrintTreeElement(element);
            }
        }

        public void PrintTreeWithDeviceAgent(ITreePrinter treePrinter)
        {
            TreeElement[] elements = _gestures.DumpWithDeviceAgent() ?? throw new Exception("Unable to print tree or nothing was visible.");
            foreach (var element in elements)
            {
                element.Condense();
                treePrinter.PrintTreeElement(element);
            }
        }
    }
}