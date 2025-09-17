using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using NSubstitute;
using Xamarin.UITest.XDB;
using Xamarin.UITest.XDB.Entities;
using Xamarin.UITest.XDB.Exceptions;
using Xamarin.UITest.XDB.Services;
using Xamarin.UITest.XDB.Services.OSX;
using Xamarin.UITest.XDB.Services.OSX.IDB;
using Xamarin.UITest.XDB.Services.Processes;
using Microsoft.Extensions.DependencyInjection;
using Xamarin.UITest.Shared.Resources;
using Xamarin.UITest.Shared.Artifacts;
using System.IO;

namespace Xamarin.UITest.Tests.XDB
{
    [TestFixture]
    public class iOSDeviceAgentServiceTests
    {
        bool _isWindows;
        IServiceCollection _services;
        IHttpService _httpService;
        IDBService IDBService;
        iOSDeviceAgentService deviceAgentService;
        bool _pingReturnError;

        public iOSDeviceAgentServiceTests()
        {
            _services = new ServiceCollection();
            ServiceHelper.RegisterServices((i, t) => _services.AddSingleton(i, t));

            IDBService = Substitute.For<IDBService>();
            deviceAgentService = Substitute.For<iOSDeviceAgentService>();
            _httpService = Substitute.For<IHttpService>();
            var pingResult = new HttpResult<iOSDeviceAgentService.StatusResult>
            {
                StatusCode = HttpStatusCode.OK,
                Content = new iOSDeviceAgentService.StatusResult()
            };

            _httpService.GetAsync<iOSDeviceAgentService.StatusResult>(
                        Arg.Any<String>(),
                        Arg.Any<TimeSpan?>(),
                        Arg.Any<int>(),
                        Arg.Any<TimeSpan?>(),
                        Arg.Any<bool>(),
                        Arg.Any<bool>())
                        .Returns(Task.FromResult((IHttpResult<iOSDeviceAgentService.StatusResult>)pingResult))
                        .AndDoes(x =>
                        {
                            if (_pingReturnError)
                            {
                                _pingReturnError = false;
                                throw new DeviceAgentException();
                            }
                        }
            );

            var deleteResult = new HttpResult<iOSDeviceAgentService.StatusResult>
            {
                StatusCode = HttpStatusCode.RequestTimeout,
            };

            _httpService.DeleteAsync<iOSDeviceAgentService.StatusResult>(
                        Arg.Any<String>(),
                        Arg.Any<TimeSpan?>(),
                        Arg.Any<int>(),
                        Arg.Any<TimeSpan?>(),
                        Arg.Any<bool>(),
                        Arg.Any<bool>())
                        .Returns(Task.FromResult((IHttpResult<iOSDeviceAgentService.StatusResult>)deleteResult));

            var shutdownResult = new HttpResult<iOSDeviceAgentService.ShutdownResult>
            {
                StatusCode = HttpStatusCode.RequestTimeout,
            };

            _httpService.PostAsync<iOSDeviceAgentService.ShutdownResult>(
                        Arg.Any<String>(),
                        Arg.Any<TimeSpan?>(),
                        Arg.Any<int>(),
                        Arg.Any<TimeSpan?>(),
                        Arg.Any<bool>(),
                        Arg.Any<bool>())
                        .Returns(Task.FromResult((IHttpResult<iOSDeviceAgentService.ShutdownResult>)shutdownResult));

            _services.AddSingleton(IDBService);
            _services.AddSingleton(_httpService);
            _services.AddSingleton(typeof(IEnvironmentService), new EnvironmentService());

            _isWindows = UITest.Shared.Processes.Platform.Instance.IsWindows;
        }

        private void ValidateArgs(string command, string args)
        {
            Assert.IsTrue(command.Contains("xcrun"));

            var argsArray = args.Split(' ');

            Assert.AreEqual(8, argsArray.Length);
            Assert.IsTrue(argsArray[0].Equals("xcodebuild"));
            Assert.IsTrue(argsArray[1].Equals("test-without-building"));
            Assert.IsTrue(argsArray[2].Equals("-xctestrun"));
            Assert.IsTrue(Path.GetFileName(argsArray[3]).Equals("DeviceAgent-device.xctestrun"));
            Assert.IsTrue(argsArray[4].Equals("-destination"));
            Assert.IsTrue(argsArray[5].StartsWith("'id="));
            Assert.IsTrue(argsArray[6].Equals("-derivedDataPath"));
            Assert.IsTrue(Path.GetFileName(argsArray[7]).Equals("DerivedData"));
        }

        [SetUp]
        public void SetUp()
        {
            // Looks like Nsubstitute doesnt replace the "AndDoes" so this is a work around
            IDBService = Substitute.For<IDBService>();
            _services.AddSingleton(IDBService);

            deviceAgentService.StartTest(UDID: new UDID(Arg.Any<string>()))
                     .Returns(x => { Thread.Sleep(500); return new ProcessResult(0, "", "", ""); });
        }

        [Test]
        public void LaunchDeviceIdAndCodesignIdentityTest()
        {
            if (_isWindows) { return; }

            _pingReturnError = true;

            var deviceId = "deviceId";
            var deviceAgentService = Substitute.For<iOSDeviceAgentService>();
            deviceAgentService.StartTest(UDID: new UDID(Arg.Any<string>()))
                                 .Returns(x => { Thread.Sleep(500); return new ProcessResult(0, "", "", ""); })
                                 .AndDoes(x => { Assert.IsTrue(deviceId.Equals(x.Arg<string>())); });

            IDBService.IDBCommandProvider.InstallDeviceAgent(UDID: new UDID(Arg.Any<string>()))
                                 .Returns(new ProcessResult(0, "", "", ""))
                                 .AndDoes(x => { Assert.IsTrue(deviceId.Equals(x.Arg<string>())); });

            deviceAgentService.LaunchTestAsync(deviceId, "127.0.0.1").Wait();
        }

        [Test]
        public void LaunchSimulatorTest()
        {
            if (_isWindows) { return; }

            _pingReturnError = true;

            var deviceId = Guid.NewGuid().ToString();
            bool simulatorLaunched = false;

            IDBService.IDBCommandProvider.LaunchSimulator(UDID: new UDID(Arg.Any<string>()))
                                 .Returns(new ProcessResult(0, "", "", ""))
                                 .AndDoes(x => simulatorLaunched = true);

            IDBService.IDBCommandProvider.InstallDeviceAgent(UDID: new UDID(Arg.Any<string>()))
                                 .Returns(new ProcessResult(0, "", "", ""))
                                 .AndDoes(x => { Assert.IsTrue(deviceId.Equals(x.Arg<string>())); });

            var deviceAgentService = _services.BuildServiceProvider().GetRequiredService<IiOSDeviceAgentService>();

            deviceAgentService.LaunchTestAsync(deviceId, "127.0.0.1").Wait();
            Assert.True(simulatorLaunched);
        }

        [Test]
        public void LaunchPhysicalDeviceTest()
        {
            if (_isWindows) { return; }

            _pingReturnError = true;

            var deviceId = "not_a_sim";
            bool simulatorLaunched = false;

            IDBService.IDBCommandProvider.LaunchSimulator(UDID: new UDID(Arg.Any<string>()))
                     .Returns(new ProcessResult(0, "", "", ""))
                     .AndDoes(x => simulatorLaunched = true);

            IDBService.IDBCommandProvider.InstallDeviceAgent(UDID: new UDID(Arg.Any<string>()))
                     .Returns(new ProcessResult(0, "", "", ""))
                     .AndDoes(x => { Assert.IsTrue(deviceId.Equals(x.Arg<string>())); });

            var deviceAgentService = _services.BuildServiceProvider().GetRequiredService<IiOSDeviceAgentService>();

            deviceAgentService.LaunchTestAsync(deviceId, "127.0.0.1").Wait();

            Assert.False(simulatorLaunched);
        }

        // TODO: Tests for InstallApp, StartAppAsync, ShutdownAsync, etc
    }
}