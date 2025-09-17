using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Xamarin.UITest.Shared.Dependencies
{
    public class DirectoryWalker
    {
        readonly int _maxDepth;

        public DirectoryWalker(int maxDepth)
        {
            _maxDepth = maxDepth;
        }

        public IEnumerable<WalkerFileMatch> GetMatches(string topLevelPath, params Regex[] patterns)
        {
            return RecursiveSearch(topLevelPath, string.Empty, _maxDepth, patterns);
        }

        IEnumerable<WalkerFileMatch> RecursiveSearch(string topLevelPath, string currentPathRelative, int maxDepth, Regex[] patterns)
        {
            var currentDirectory = new DirectoryInfo(Path.Combine(topLevelPath, currentPathRelative));

            if (maxDepth > 0)
            {
                foreach (var directoryName in currentDirectory.EnumerateDirectories())
                {
                    foreach (var result in RecursiveSearch(topLevelPath, Path.Combine(currentPathRelative, directoryName.Name), maxDepth - 1, patterns))
                    {
                        yield return result;
                    }
                }
            }

            foreach (var file in currentDirectory.EnumerateFiles())
            {
                var matchingPattern = patterns.FirstOrDefault(x => x.IsMatch(file.Name));

                if (matchingPattern != null)
                {
                    yield return new WalkerFileMatch(Path.Combine(currentPathRelative, file.Name), file.FullName, matchingPattern);
                }
            }
        }
    }
}