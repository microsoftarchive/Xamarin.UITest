using System;
using NSubstitute;
using NUnit.Framework;
using Xamarin.UITest.XDB.Exceptions;
using Xamarin.UITest.XDB.Services.OSX;
using Xamarin.UITest.XDB.Services.OSX.IDB;

namespace Xamarin.UITest.Tests.XDB.iOSDeviceAgentTests
{
    public class iOSDeviceAgentServiceInstallTests : iOSDeviceAgentServiceTestBase
    {
        [Test]
        public void SimulatorSucessfulInstall()
        {
            var deviceControl = CreateIDBService(DefaultSimulatorGuidId);

            deviceControl.InstallApp(UDID: new UDID(DefaultSimulatorGuidId), pathToBundle: AppBundlePath);
            deviceControl.Received(1).InstallApp(UDID: new UDID(DefaultSimulatorGuidId), pathToBundle: AppBundlePath);
            deviceControl.Received(1).IDBCommandProvider.LaunchSimulator(UDID: new UDID(DefaultSimulatorGuidId));
        }

        [Test]
        public void SimluatorFailedToLaunchDuringInstall()
        {
            var deviceId = Guid.NewGuid().ToString();
            var deviceControl = CreateIDBService(
                deviceId,
                installAppExitCode: 0,
                launchSimulatorExitCode: 1
            );

            var exception = Assert.Throws<IDBException>(() => deviceControl.InstallApp(UDID: new UDID(deviceId), pathToBundle: AppBundlePath));

            Assert.True(exception.Message.StartsWith("Failed to launch simulator"));
            deviceControl.Received(1).IDBCommandProvider.LaunchSimulator(UDID: new UDID(deviceId));
        }

        [Test]
        public void PhysicalDeviceSucessfullInstall()
        {
            var deviceControl = CreateIDBService(DefaultDeviceId, installAppExitCode: 0);
            var iOSDeviceAgent = InitialiseIOSDeviceAgentService(deviceControl);

            deviceControl.InstallApp(UDID: new UDID(DefaultDeviceId), pathToBundle: AppBundlePath);

            deviceControl.Received(1).IDBCommandProvider.InstallApp(UDID: new UDID(DefaultDeviceId), pathToBundle: AppBundlePath);
            deviceControl.Received(0).IDBCommandProvider.LaunchSimulator(UDID: new UDID(DefaultDeviceId));
        }

        [TestCase(DefaultDeviceId)]
        [TestCase(DefaultSimulatorGuidId)]
        public void ExceptionThrownWhileInstallingApp(string deviceId)
        {
            var deviceControl = CreateIDBService(deviceId);
            deviceControl.IDBCommandProvider.InstallApp(UDID: new UDID(deviceId), pathToBundle: AppBundlePath)
                         .Returns(x => { throw new DeviceAgentException(); });

            Assert.Throws<IDBException>(() =>
                deviceControl.InstallApp(UDID: new UDID(deviceId), pathToBundle: AppBundlePath)
            );
        }

        [Test]
        public void ExceptionThrownWhileStartingSimulatorDuringInstall()
        {
            var deviceControl = Substitute.For<IDBService>();
            deviceControl = CreateIDBService(DefaultSimulatorGuidId);
            deviceControl.IDBCommandProvider.LaunchSimulator(UDID: new UDID(DefaultSimulatorGuidId)).Returns(x => { throw new IDBException(); });

            Assert.Throws<DeviceAgentException>(() =>
                deviceControl.InstallApp(
                    UDID: new UDID(DefaultSimulatorGuidId),
                    pathToBundle: AppBundlePath)
            );
        }
    }
}
