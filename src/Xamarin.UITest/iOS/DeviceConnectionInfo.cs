using System;
namespace Xamarin.UITest.iOS
{
    internal class DeviceConnectionInfo
    {
        public DeviceConnectionInfo(
            string deviceIdentifier,
            ICalabashConnection connection,
            bool useXDB,
            string deviceAddress = null)
        {
            if (useXDB && deviceAddress == null)
            {
                throw new ArgumentException("deviceAddress is required when useXDB is true");
            }

            Connection = connection;
            DeviceAddress = deviceAddress;
            DeviceIdentifier = deviceIdentifier;
            UseXDB = useXDB;
        }

        public string DeviceAddress { get; }

        public string DeviceIdentifier { get; }

        public ICalabashConnection Connection { get; }

        public bool UseXDB { get; }
    }
}