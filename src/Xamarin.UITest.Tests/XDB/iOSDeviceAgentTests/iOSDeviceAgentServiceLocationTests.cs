using System;
using NSubstitute;
using NUnit.Framework;
using Xamarin.UITest.XDB.Entities;
using Xamarin.UITest.XDB.Services.Processes;
using Xamarin.UITest.XDB.Exceptions;
using Xamarin.UITest.XDB.Services.OSX;

namespace Xamarin.UITest.Tests.XDB.iOSDeviceAgentTests
{
    public class iOSDeviceAgentServiceLocationTests : iOSDeviceAgentServiceTestBase
    {
        readonly LatLong _defaultLatLong = new LatLong(56.1629, 10.2039);

        [Ignore("Unstable tests")]
        public void LocationSetSuccessfully()
        {
            var deviceControl = CreateIDBService(DefaultDeviceId);
            deviceControl.IDBCommandProvider.SetLocation(UDID: new UDID(DefaultDeviceId), _defaultLatLong)
                         .Returns(new ProcessResult(0, "", "", ""));

            deviceControl.SetLocation(UDID: new UDID(DefaultDeviceId), latLong: _defaultLatLong);

            deviceControl.IDBCommandProvider.SetLocation(UDID: new UDID(DefaultDeviceId), _defaultLatLong).Received(1);
        }

        [Ignore("Unstable tests")]
        public void ExceptionThrownSettingLocation()
        {
            var deviceControl = CreateIDBService(DefaultDeviceId);
            deviceControl.IDBCommandProvider.SetLocation(UDID: new UDID(DefaultDeviceId), _defaultLatLong)
                         .Returns(new ProcessResult(1, "", "", ""));

            var exception = Assert.Throws<IDBException>(() =>
                deviceControl.SetLocation(UDID: new UDID(DefaultDeviceId), _defaultLatLong));

            Assert.IsTrue(exception.Message.Equals("Unable to set location"));

            deviceControl.IDBCommandProvider.SetLocation(UDID: new UDID(DefaultDeviceId), _defaultLatLong).Received(1);
        }

        [Ignore("Not implemented")]
        public void StopSimulatingLocationSuccessful()
        {
            throw new NotImplementedException();
        }

        [Ignore("Not implemented")]
        public void ExceptionThrownStoppingLocationSimulation()
        {
            throw new NotImplementedException();
        }
    }
}
