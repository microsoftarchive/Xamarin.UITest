using Xamarin.UITest.iOS.ApplicationSigning.Entities;
using Xamarin.UITest.XDB.Entities.iOSDevice;
using Xamarin.UITest.XDB.Services.OSX;

namespace Xamarin.UITest.XDB.Services
{
    internal interface IEnvironmentService
    {
        string AppBundlePath { get; set;}
        string AppBundleId { get; set; }
        IiOSDevice IOSDevice { get; }
        ProvisioningProfile ProvisioningProfile { get; set; }
        CodesignIdentity CodesignIdentity { get; set; }
        void SetIOSDevice(UDID deviceId);

        string IDBPathOverride { get; set; }

        string DeviceAgentBundleId { get; set; }

        string DeviceAgentPathOverride { get; set; }

        bool ShouldInstallDeviceAgent { get; set; }

        string GetTempFolderForDeviceAgentResigning();
    }
}