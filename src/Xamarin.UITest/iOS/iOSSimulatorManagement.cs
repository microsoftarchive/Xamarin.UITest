using System;
using System.IO;
using System.Text.RegularExpressions;
using Xamarin.UITest.Shared.Hashes;
using Xamarin.UITest.Shared.Logging;
using Xamarin.UITest.XDB;
using Xamarin.UITest.XDB.Services.OSX;
using Xamarin.UITest.XDB.Services.OSX.IDB;

namespace Xamarin.UITest.Shared.iOS
{

    // TODO: Consider removing of this class. We have IDBService instead.
    internal class iOSSimulatorManagement
    {
        readonly HashHelper _hashHelper = new HashHelper();
        readonly string _simulatorRootPath;

        internal iOSSimulatorManagement()
        {
            _simulatorRootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Developer", "CoreSimulator", "Devices");
        }

        internal void PrepareSimulator(string deviceId, string targetApplication, bool clearData)
        {
            var UDID = new UDID(ExtractUUID(deviceId));

            var appIdentifier = targetApplication;
            var isAppBundleDirectory = Directory.Exists(targetApplication);

            if (isAppBundleDirectory)
            {
                appIdentifier = ExtractAppIdentifier(targetApplication);
            }

            ISimulatorAppLocator appLocator = new iOS8AndiOS9SimulatorAppLocator(_simulatorRootPath);

            var installedApp = appLocator.GetInstalledApp(UDID.ToString(), appIdentifier);

            IIDBService idbService = XdbServices.GetRequiredService<IIDBService>();
            if (!idbService.IsAppInstalled(UDID: UDID, bundleId: appIdentifier))

            {
                Log.Info("Sim check: App not installed.");
                return;
            }

            if (isAppBundleDirectory)
            {

                var installedAppHash = _hashHelper.GetSha256Hash(new DirectoryInfo(installedApp.AppBundlePath));
                var targetAppHash = _hashHelper.GetSha256Hash(new DirectoryInfo(targetApplication));

                if (targetAppHash == installedAppHash)
                {
                    Log.Debug("Sim check: App is up to date.");

                    if (clearData)
                    {
                        idbService.ClearXCAppData(UDID: UDID, bundleId: installedApp.AppIdentifier);
                    }
                }
                else
                {
                    Log.Debug(string.Format("Sim check: Not up to date. Target hash: {0} - Installed hash: {1}", targetAppHash, installedAppHash));
                    Log.Debug(string.Format("Deleting: {0}", installedApp.AppPath));
                    idbService.UninstallApp(UDID: UDID, bundleId: installedApp.AppIdentifier);
                }
            }
            else
            {
                Log.Debug("Sim check: App is installed.");
                if (clearData)
                {
                    idbService.ClearXCAppData(UDID: UDID, bundleId: installedApp.AppIdentifier);
                }
            }

            EnableSoftKeyboard(UDID.ToString());
        }

        private void EnableSoftKeyboard(string deviceId)
        {
            var devicePath = Path.Combine(_simulatorRootPath, deviceId);
            var plistPath = Path.Combine(devicePath, "data/Library/Preferences/com.apple.Preferences.plist");
            PListHelper.SetOrAddPListValueInFile(plistPath, "AutomaticMinimizationEnabled", "integer", "0");
        }

        private string ExtractAppIdentifier(string appBundlePath)
        {
            var pListPath = Path.Combine(appBundlePath, "Info.plist");
            return PListHelper.ReadPListValueFromFile(pListPath, "CFBundleIdentifier");
        }

        private string ExtractUUID(string deviceId)
        {
            var match = Regex.Match(deviceId, "[0-9a-fA-F]+\\-[0-9a-fA-F]+\\-[0-9a-fA-F]+\\-[0-9a-fA-F]+\\-[0-9a-fA-F]+");

            if (match.Success)
            {
                return match.Groups[0].Value;
            }

            return deviceId;
        }
    }
}