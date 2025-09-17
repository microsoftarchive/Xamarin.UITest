using Xamarin.UITest.Shared.Extensions;
using Xamarin.UITest.Shared.iOS;
using Xamarin.UITest.Utils;

namespace Xamarin.UITest.iOS
{
    internal class iOSCalabashDevice
	{
        readonly iOSVersionInfo _versionInfo;
        readonly VersionNumber _calabashServerVersion;

        public iOSCalabashDevice(iOSVersionInfo versionInfo, VersionNumber calabashServerVersion)
        {
            _versionInfo = versionInfo;
            _calabashServerVersion = calabashServerVersion;
        }

        public bool IsIPad
        {
            get { return _versionInfo.DeviceFamily == "iPad"; }
        }

        public bool IsIPhone4In
        {
            get { return _versionInfo.Is4Inch; }
        }

        public bool IsSimulator
        {
            get { return !_versionInfo.Simulator.IsNullOrWhiteSpace(); }
        }

        public iOSResolution GetScreenSize()
        {
            if (IsIPad)
            {
                return new iOSResolution(768, 1024);
            }

            if (IsIPhone4In)
            {
                return new iOSResolution(320, 568);
            }

            return new iOSResolution(320, 480);
        }

        public VersionNumber iOSVersion
        {
            get { return _versionInfo.iOSVersion; }
        }

        public VersionNumber CalabashServerVersion
        {
            get { return _calabashServerVersion; }
        }
	}
}
