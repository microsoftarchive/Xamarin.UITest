using System;
using System.IO;
using System.Linq;
using System.Threading;
using Xamarin.UITest.Shared.Logging;

namespace Xamarin.UITest.Shared.Artifacts
{
    public class ArtifactCleaner
    {
        const int CleanUpIntervalDays = 1;
        const int ExpireDays = 14;

        public static void PotentialCleanUp()
        {
            var attempts = 5;

            while (attempts > 0)
            {
                attempts--;

                try
                {
                    var rootArtifactFolder = Path.Combine(Path.GetTempPath(), "uitest");

                    if (!Directory.Exists(rootArtifactFolder))
                    {
                        return;
                    }

                    var cleanUpTokenPath = Path.Combine(rootArtifactFolder, "last-clean-up");

                    var cleanUpTokenAge = DateTime.Now - File.GetCreationTime(cleanUpTokenPath);

                    if (File.Exists(cleanUpTokenPath) && cleanUpTokenAge < TimeSpan.FromDays(CleanUpIntervalDays))
                    {
                        return;
                    }

                    File.Delete(cleanUpTokenPath);
                    File.WriteAllText(cleanUpTokenPath, DateTime.Now.ToString());

                    var artifactFolders = Directory.EnumerateDirectories(
                        rootArtifactFolder,
                        "a-*",
                        SearchOption.TopDirectoryOnly);

                    var expiredFolders = artifactFolders
                        .Where(x => DateTime.Now - Directory.GetLastAccessTime(x) > TimeSpan.FromDays(ExpireDays))
                        .ToArray();

                    foreach (var expiredFolder in expiredFolders)
                    {
                        Directory.Delete(expiredFolder, true);
                    }

                    var logFiles = Directory.EnumerateFiles(rootArtifactFolder, "log*", SearchOption.TopDirectoryOnly);

                    var expiredFiles = logFiles
                        .Where(x => DateTime.Now - File.GetLastAccessTime(x) > TimeSpan.FromDays(ExpireDays))
                        .ToArray();

                    foreach (var expiredFile in expiredFiles)
                    {
                        File.Delete(expiredFile);
                    }

                    return;
                }
                catch
                {
                    if (attempts == 0)
                    {
                        Log.Info("Cleaning artifacts failed after 5 attempts.");
                        throw;
                    }

                    // Pause for 0.1 - 2 seconds
                    var waitSeconds = new Random().Next(10, 200) / 100.0;

                    Log.Info("Cleaning artifacts failed, files may be in use by another test run. Retrying..");

                    Thread.Sleep(TimeSpan.FromSeconds(waitSeconds));
                }
            }
        }
    }
}