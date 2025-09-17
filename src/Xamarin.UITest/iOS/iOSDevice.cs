using System;
using Xamarin.UITest.Shared.Dependencies;
using Xamarin.UITest.Shared.iOS;
using Xamarin.UITest.Utils;

namespace Xamarin.UITest.iOS
{
    /// <summary>
    /// Runtime information and control of device.
    /// </summary>
    public class iOSDevice : IDevice
    {
        readonly Uri _deviceUri;
        readonly string _deviceIdentifier;
        readonly ILocationSimulation _locationSimulation;
        readonly iOSCalabashDevice _device;

        internal iOSDevice(
            Uri deviceUri,
            iOSCalabashDevice device,
            string deviceIdentifier,
            ILocationSimulation locationSimulation)
        {
            _deviceUri = deviceUri;
            _device = device;
            _deviceIdentifier = deviceIdentifier;
            _locationSimulation = locationSimulation;
        }

        /// <summary>
        /// The uri of the device.
        /// </summary>
        public Uri DeviceUri
        {
            get { return _deviceUri; }
        }

        /// <summary>
        /// The identifier of the device.
        /// </summary>
        public string DeviceIdentifier
        {
            get { return _deviceIdentifier; }
        }

        /// <summary>
        /// Change GPS location of the device. 
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        public void SetLocation(double latitude, double longitude)
        {
            _locationSimulation.SetGpsCoordinates(latitude, longitude);
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

        /// <summary>
        /// Whether the current test is running on a simulator.
        /// </summary>
        /// <value><c>true</c> if is simulator; otherwise, <c>false</c>.</value>
        public bool IsSimulator
        {
            get { return _device.IsSimulator; }
        }

        /// <summary>
        /// What iOS version is running on the device/simulator
        /// </summary>
        public IVersionNumber OSVersion
        {
            get { return _device.iOSVersion; }
        }
    }
}
