using Xamarin.UITest.iOS.ApplicationSigning.Entities;
using Xamarin.UITest.XDB.Entities;

namespace Xamarin.UITest.XDB.Services.OSX.IDB
{
    internal interface IIDBService
    {
        string GetTempFolderPath();

        string GetDeviceAgentBundlePathForPhysicalDevice();

        void ClearXCAppData(UDID UDID, string bundleId);

        void InstallApp(UDID UDID, string pathToBundle);

        bool IsAppInstalled(UDID UDID, string bundleId);

        void InstallDeviceAgent(UDID UDID);

        void SetLocation(UDID UDID, LatLong latLong);

        void StopSimulatingLocation(UDID UDID);

        void UninstallApp(UDID UDID, string bundleId);
    }
}