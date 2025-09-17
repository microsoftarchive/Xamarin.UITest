using System.IO;

namespace Xamarin.UITest.Shared.Screenshots
{
    internal class NullScreenshotTaker : IScreenshotTaker
    {
        public FileInfo Screenshot(string title)
        {
            return null;
        }
    }
}