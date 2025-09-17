using NSubstitute;
using NUnit.Framework;
using System;
using System.IO;
using Xamarin.UITest.XDB.Entities;
using Xamarin.UITest.XDB.Exceptions;
using Xamarin.UITest.XDB.Services;
using Xamarin.UITest.XDB.Services.OSX;
using Xamarin.UITest.XDB.Services.OSX.IDB;
using Xamarin.UITest.XDB.Services.Processes;

namespace Xamarin.UITest.Tests.XDB
{
    [TestFixture]
    public class IDBServiceTests
    {
        private readonly string PathToBundle = "path/to/name.app";
        private readonly UDID PhysicalDeviceUDID = new UDID("1234");
        private readonly UDID SimulatorUDID = new UDID("159be932-a1f5-4ae9-98bd-bbc356482160");

        private readonly bool IsCurrentOSWindows;

        private readonly IDependenciesDeploymentService DependenciesDeploymentService;
        private readonly ILoggerService LoggerService;
        private readonly IProcessService ProcessService;
        private readonly ProcessResult ProcessResult;

        public IDBServiceTests()
        {
            ProcessResult = new ProcessResult(0, "Success", "", "");
            DependenciesDeploymentService = Substitute.For<IDependenciesDeploymentService>();
            DependenciesDeploymentService.HashId.Returns("foo");
            DependenciesDeploymentService.PathToDeviceTestRunner.Returns("foo");
            DependenciesDeploymentService.PathToSimTestRunner.Returns("foo");

            ProcessService = Substitute.For<IProcessService>();

            LoggerService = Substitute.For<ILoggerService>();

            IsCurrentOSWindows = UITest.Shared.Processes.Platform.Instance.IsWindows;
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            ProcessService.Run(Arg.Any<string>(), Arg.Any<string>())
                           .Returns(ProcessResult);
        }

        [Test]
        public void LaunchSimulatorForPhysicalDeviceUDIDThrowsExceptionTest()
        {
            if (IsCurrentOSWindows) return;
            IDBService idbService = Substitute.For<IDBService>(
                new EnvironmentService(),
                DependenciesDeploymentService,
                LoggerService,
                ProcessService
            );
            Assert.Throws<IDBException>(() => idbService.LaunchSimulator(UDID: PhysicalDeviceUDID, 1));
        }

        [Test]
        public void InstallAppUsingPhysicalDeviceUDIDTest()
        {
            if (IsCurrentOSWindows) return;

            string expectedArgs = $"install --udid {PhysicalDeviceUDID} \"{PathToBundle}\"";
            string actualArgs = string.Empty;

            ProcessService.Run(Arg.Any<string>(), Arg.Is(expectedArgs))
                           .Returns(ProcessResult)
                           .AndDoes(x => { actualArgs = (string)x.Args()[1]; });

            IDBService idbService = Substitute.For<IDBService>(
                new EnvironmentService(),
                DependenciesDeploymentService,
                LoggerService,
                ProcessService
            );
            idbService.InstallApp(UDID: PhysicalDeviceUDID, pathToBundle: PathToBundle);

            Assert.IsTrue(expectedArgs.Equals(actualArgs));
        }

        [Test]
        public void InstallAppUsingSimulatorUDIDTest()
        {
            if (IsCurrentOSWindows) return;

            string expectedArgs = $"install --udid {SimulatorUDID} \"{PathToBundle}\"";
            string actualArgs = string.Empty;

            ProcessService.Run(Arg.Any<string>(), Arg.Is(expectedArgs))
                .Returns(ProcessResult)
                .AndDoes(x => { actualArgs = x.Args()[1].ToString(); });

            IDBService idbService = Substitute.For<IDBService>(
                new EnvironmentService(),
                DependenciesDeploymentService,
                LoggerService,
                ProcessService
            );
            idbService.InstallApp(UDID: SimulatorUDID, pathToBundle: PathToBundle);

            Assert.IsTrue(expectedArgs.Equals(actualArgs));
        }

        [Test]
        public void SetLocationForPhysicalDeviceTest()
        {
            if (IsCurrentOSWindows) return;

            LatLong latLong = new LatLong(56.1629, 10.2039);
            string expectedArgs = $"set-location --udid {PhysicalDeviceUDID} {latLong.Latitude} {latLong.Longitude}";
            string actualArgs = string.Empty;

            ProcessService.Run(Arg.Any<string>(), Arg.Any<string>())
               .Returns(ProcessResult)
               .AndDoes(x => { actualArgs = (string)x.Args()[1]; });

            IDBService idbService = Substitute.For<IDBService>(
                new EnvironmentService(),
                DependenciesDeploymentService,
                LoggerService,
                ProcessService
            );

            idbService.SetLocation(UDID: PhysicalDeviceUDID, latLong: latLong);

            Assert.IsTrue(expectedArgs.Equals(actualArgs));
        }

        [Test]
        public void StopSimulatingLocationForPhysicalDeviceTest()
        {
            if (IsCurrentOSWindows) return;

            LatLong defaultLocation = new(lattitude: -122.147911, longitude: 37.485023);
            string expectedArgs = $"set-location --udid {PhysicalDeviceUDID} {defaultLocation.Latitude} {defaultLocation.Longitude}";
            string actualArgs = string.Empty;

            ProcessService.Run(Arg.Any<string>(), Arg.Any<string>())
               .Returns(ProcessResult)
               .AndDoes(x => { actualArgs = (string)x.Args()[1]; });

            IDBService idbService = Substitute.For<IDBService>(
                new EnvironmentService(),
                DependenciesDeploymentService,
                LoggerService,
                ProcessService
            );

            idbService.SetLocation(UDID: PhysicalDeviceUDID, latLong: defaultLocation);

            Assert.IsTrue(expectedArgs.Equals(actualArgs));
        }

        [Test]
        public void SetLocationForSimulatorTest()
        {
            if (IsCurrentOSWindows) return;

            LatLong latLong = new LatLong(56.1629, 10.2039);
            string expectedArgs = $"set-location --udid {PhysicalDeviceUDID} {latLong.Latitude} {latLong.Longitude}";
            string actualArgs = string.Empty;

            ProcessService.Run(Arg.Any<string>(), Arg.Any<string>())
               .Returns(ProcessResult)
               .AndDoes(x => { actualArgs = (string)x.Args()[1]; });

            IDBService idbService = Substitute.For<IDBService>(
                new EnvironmentService(),
                DependenciesDeploymentService,
                LoggerService,
                ProcessService
            );

            idbService.SetLocation(UDID: PhysicalDeviceUDID, latLong: latLong);

            Assert.IsTrue(expectedArgs.Equals(actualArgs));
        }

        [Test]
        public void StopSimulatingLocationForSimulatorTest()
        {
            if (IsCurrentOSWindows) return;

            LatLong defaultLocation = new(lattitude: -122.147911, longitude: 37.485023);
            string expectedArgs = $"set-location --udid {SimulatorUDID} {defaultLocation.Latitude} {defaultLocation.Longitude}";
            string actualArgs = string.Empty;

            ProcessService.Run(Arg.Any<string>(), Arg.Any<string>())
               .Returns(ProcessResult)
               .AndDoes(x => { actualArgs = (string)x.Args()[1]; });

            IDBService idbService = Substitute.For<IDBService>(
                new EnvironmentService(),
                DependenciesDeploymentService,
                LoggerService,
                ProcessService
            );

            idbService.SetLocation(UDID: SimulatorUDID, latLong: defaultLocation);

            Assert.IsTrue(expectedArgs.Equals(actualArgs));
        }

        [Test]
        public void ClearXCAppDataOnPhysicalDeviceTest()
        {
            if (IsCurrentOSWindows) return;

            IDBService idbService = Substitute.For<IDBService>(
                new EnvironmentService(),
                DependenciesDeploymentService,
                LoggerService,
                ProcessService
            );

            idbService.ClearXCAppData(UDID: PhysicalDeviceUDID, bundleId: Arg.Any<string>());
        }

        [Test]
        public void ClearXCAppDataOnPhysicalDeviceFailsWhenAppIsNotInstalledTest()
        {
            if (IsCurrentOSWindows) return;

            IDBService idbService = Substitute.For<IDBService>(
                new EnvironmentService(),
                DependenciesDeploymentService,
                LoggerService,
                ProcessService
            );

            idbService.ClearXCAppData(UDID: PhysicalDeviceUDID, bundleId: Arg.Any<string>());
        }
    }
}
