using System;
using Xamarin.UITest.Shared.iOS;

namespace Xamarin.UITest.iOS
{
    /// <summary>
    /// Represents runtime information about the currently running app / device.
    /// </summary>
    public class iOSConfig
    {
        readonly Uri _deviceUri;
        readonly iOSCalabashDevice _device;

        internal iOSConfig(Uri deviceUri, iOSCalabashDevice device)
        {
            _deviceUri = deviceUri;
            _device = device;
        }

        /// <summary>
        /// The uri of the device.
        /// </summary>
        public Uri DeviceUri
        {
            get { return _deviceUri; }
        }

        /// <summary>
        /// Whether the current test is running on a phone.
        /// </summary>
        public bool IsPhone
        {
            get { return !_device.IsIPad; }
        }

        /// <summary>
        /// Whether the current test is running on a tablet.
        /// </summary>
        public bool IsTablet
        {
            get { return _device.IsIPad; }
        }
    }
}