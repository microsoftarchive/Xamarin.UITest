using System;

namespace Xamarin.UITest.Android
{
    /// <summary>
    /// Runtime information and control of device.
    /// </summary>
    public class AndroidDevice : IDevice
    {
        readonly Uri _deviceUri;
        readonly string _deviceIdentifier;
        readonly AndroidGestures _gestures;

        internal AndroidDevice(
            Uri deviceUri, 
            AndroidGestures gestures, 
            string deviceIdentifier) 
        {
            _deviceUri = deviceUri;
            _gestures = gestures;
            _deviceIdentifier = deviceIdentifier;
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
            _gestures.SetGpsCoordinates(latitude, longitude);
        }
    }
}