using System.Text.RegularExpressions;

namespace Xamarin.UITest.Shared.Dependencies
{
    public class WalkerFileMatch
    {
        public string RelativePath { get; private set; }
        public string AbsolutePath { get; private set; }
        public Regex Pattern { get; private set; }

        public WalkerFileMatch(string relativePath, string absolutePath, Regex pattern)
        {
            RelativePath = relativePath;
            AbsolutePath = absolutePath;
            Pattern = pattern;
        }
    }
}