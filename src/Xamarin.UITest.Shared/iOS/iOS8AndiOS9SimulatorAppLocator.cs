using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Xamarin.UITest.Shared.iOS
{
    public class iOS8AndiOS9SimulatorAppLocator : ISimulatorAppLocator
    {
        readonly string _simulatorRootPath;


        public iOS8AndiOS9SimulatorAppLocator(string simulatorRootPath)
        {
            _simulatorRootPath = simulatorRootPath;
        }

        public SimAppInfo GetInstalledApp(string deviceId, string appIdentifier)
        {
            foreach (var appDirectory in EnumerateDirectories(GetAppsRootPath(deviceId)))
            {
                var appBundleDirectory = Directory.EnumerateDirectories(appDirectory, "*.app").Single();
                var plistFile = Path.Combine(appBundleDirectory, "Info.plist");

                using (var plistFileStream = File.OpenText(plistFile))
                {
                    if (!plistFileStream.ReadToEnd().Contains(appIdentifier))
                    {
                        continue;
                    }
                }

                if (PListHelper.ReadPListValueFromFile(plistFile, "CFBundleIdentifier") != appIdentifier)
                {
                    continue;
                }

                var dataFolder = GetDataFolder(deviceId, appIdentifier);

                if (dataFolder == null)
                {
                    return null;
                }

                return new SimAppInfo(appIdentifier, appBundleDirectory, appDirectory, dataFolder.DataPath);
            }

            return null;
        }

        SimAppDataInfo GetDataFolder(string deviceId, string appIdentifier)
        {
            foreach (var dataDirectory in EnumerateDirectories(GetDataRootPath(deviceId)))
            {
                var plistFile = Path.Combine(dataDirectory, ".com.apple.mobile_container_manager.metadata.plist");

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

                if (PListHelper.ReadPListValueFromFile(plistFile, "MCMMetadataIdentifier") == appIdentifier)
                {
                    return new SimAppDataInfo(appIdentifier, dataDirectory);
                }
            }

            return null;
        }

        IEnumerable<string> EnumerateDirectories(string dir) 
        {
            if(Directory.Exists(dir)) 
            {
                return Directory.EnumerateDirectories(dir);
            }
            return Enumerable.Empty<string>();
        }

        string GetAppsRootPath(string deviceId)
        {
            return Path.Combine(_simulatorRootPath, deviceId, "data", "Containers", "Bundle", "Application");
        }

        string GetDataRootPath(string deviceId)
        {
            return Path.Combine(_simulatorRootPath, deviceId, "data", "Containers", "Data", "Application");
        }
    }
}