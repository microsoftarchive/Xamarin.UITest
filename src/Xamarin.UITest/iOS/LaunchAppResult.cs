using Xamarin.UITest.Shared.Artifacts;
using Xamarin.UITest.Shared.iOS;

namespace Xamarin.UITest.iOS
{
    class LaunchAppResult
    {
        public LaunchAppResult()
        {
        }

        public LaunchAppResult(
            ArtifactFolder artifactFolder,
            DeviceConnectionInfo deviceConnectionInfo,
            iOSCalabashDevice calabashDevice)
        {
            ArtifactFolder = artifactFolder;
            DeviceConnectionInfo = deviceConnectionInfo;
            CalabashDevice = calabashDevice;
        }
        public ArtifactFolder ArtifactFolder { get; set; }

        public DeviceConnectionInfo DeviceConnectionInfo { get; set; }

        public iOSCalabashDevice CalabashDevice { get; set; }
    }
}