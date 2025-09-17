using System.Collections.Generic;
using Xamarin.UITest.Shared.Extensions;
using Xamarin.UITest.Utils;

namespace Xamarin.UITest.iOS
{
    internal class iOSVersionInfo
	{
        public iOSVersionInfo(Dictionary<string, object> dictionary)
        {
            DeviceFamily = dictionary.TryGetString("device_family", string.Empty);
            Is4Inch = dictionary.TryGetBool("4inch");
            Simulator = dictionary.TryGetString("simulator", string.Empty);
            iOSVersion = new VersionNumber(dictionary.TryGetString("ios_version", "0.0"));
        } 

        public string DeviceFamily { get; private set; }
        public string Simulator { get; private set; }
        public bool Is4Inch { get; private set; }
        public VersionNumber iOSVersion { get; private set; }
	}
}
