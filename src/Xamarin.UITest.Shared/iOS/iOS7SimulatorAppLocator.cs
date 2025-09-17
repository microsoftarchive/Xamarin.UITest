using System.IO;
using System.Linq;

namespace Xamarin.UITest.Shared.iOS
{
    public class iOS7SimulatorAppLocator : ISimulatorAppLocator
    {
        readonly string _simulatorRootPath;

        public iOS7SimulatorAppLocator(string simulatorRootPath)
        {
            _simulatorRootPath = simulatorRootPath;
        }

        public SimAppInfo GetInstalledApp(string deviceId, string appIdentifier)
        {
            foreach (var appDirectory in Directory.EnumerateDirectories(GetAppsRootPath(deviceId)))
            {
                var plistFile = Path.Combine(appDirectory, "iTunesMetadata.plist");

                if (!File.Exists(plistFile))
                {
                    continue;
                }

                using (var plistFileStream = File.OpenText(plistFile))
                {
                    if (!plistFileStream.ReadToEnd().Contains(appIdentifier))
                    {
                        continue;
                    }
                }

                var identifier = PListHelper.ReadPListValueFromFile(plistFile, "softwareVersionBundleId");

                if (identifier == appIdentifier)
                {
                    var appBundleDirectory = Directory.EnumerateDirectories(appDirectory, "*.app").Single();
                    var dataDirectory = Path.Combine(appDirectory, "Library");

                    return new SimAppInfo(identifier, appBundleDirectory, appDirectory, dataDirectory);
                }
            }

            return null;
        }

        string GetAppsRootPath(string deviceId)
        {
            return Path.Combine(_simulatorRootPath, deviceId, "data", "Applications");
        }
    }
}