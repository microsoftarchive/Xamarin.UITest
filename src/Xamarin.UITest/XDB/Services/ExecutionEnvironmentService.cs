using System;
using System.IO;
using Xamarin.UITest.iOS.ApplicationSigning.Entities;
using Xamarin.UITest.XDB.Entities.iOSDevice;
using Xamarin.UITest.XDB.Services.OSX;

namespace Xamarin.UITest.XDB.Services
{
    internal class EnvironmentService : IEnvironmentService
    {
        private string DeviceAgentResigningTempFolderPath = null;
        public string AppBundlePath { get; set; } = null;
        public string AppBundleId { get; set; } = null;
        public IiOSDevice IOSDevice { get; set; } = null;
        public CodesignIdentity CodesignIdentity { get; set; } = null;
        public ProvisioningProfile ProvisioningProfile { get; set; } = null;
        public string IDBPathOverride { get; set; } = null;
        public string DeviceAgentBundleId { get; set; } = null;
        public string DeviceAgentPathOverride { get; set; } = null;
        public bool ShouldInstallDeviceAgent { get; set; } = true;

        public void SetIOSDevice(UDID deviceUDID)
        {
            if (deviceUDID.IsPhysicalDevice)
            {
                IOSDevice = new iOSPhysicalDevice(deviceUDID: deviceUDID);
            }
            else
            {
                IOSDevice = new iOSSimulator(deviceUDID: deviceUDID);
            }
        }

        public string GetTempFolderForDeviceAgentResigning()
        {
            if (DeviceAgentResigningTempFolderPath == null)
            {
                IDependenciesDeploymentService dependenciesDeploymentService = XdbServices.GetRequiredService<IDependenciesDeploymentService>();
                DeviceAgentResigningTempFolderPath = Path.Combine(
                    Path.GetTempPath(),
                    "xdb",
                    "DeviceAgentResigning",
                    Guid.NewGuid().ToString());
                Directory.CreateDirectory(path: DeviceAgentResigningTempFolderPath);
            }
            return DeviceAgentResigningTempFolderPath;
        }
    }
}