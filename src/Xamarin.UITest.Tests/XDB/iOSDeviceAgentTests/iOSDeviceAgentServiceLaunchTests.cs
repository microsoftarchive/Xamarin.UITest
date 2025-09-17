using System;
using NUnit.Framework;
using NSubstitute;
using Xamarin.UITest.XDB.Services;
using Xamarin.UITest.XDB.Entities;
using System.Threading.Tasks;
using System.Threading;
using Xamarin.UITest.XDB.Services.Processes;
using Xamarin.UITest.XDB.Exceptions;
using Xamarin.UITest.Shared.Resources;
using Xamarin.UITest.Shared.Artifacts;

namespace Xamarin.UITest.Tests.XDB.iOSDeviceAgentTests
{
    public class iOSDeviceAgentServiceLaunchTests : iOSDeviceAgentServiceTestBase
    {
        IHttpService _httpService;
        IHttpResult<iOSDeviceAgentService.ShutdownResult> _shutdownResult;
        IHttpResult<iOSDeviceAgentService.StatusResult> _statusResult;

        public iOSDeviceAgentServiceLaunchTests()
        {
            _shutdownResult = new HttpResult<iOSDeviceAgentService.ShutdownResult>
            {
                Content = new iOSDeviceAgentService.ShutdownResult()
            };

            _statusResult = new HttpResult<iOSDeviceAgentService.StatusResult>
            {
                Content = new iOSDeviceAgentService.StatusResult()
            };
        }

        [SetUp]
        public void SetUp()
        {
            _httpService = Substitute.For<IHttpService>();
            _httpService.PostAsync<iOSDeviceAgentService.ShutdownResult>(
                Arg.Any<string>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<int>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<bool>(),
                Arg.Any<bool>()
            ).Returns(Task.FromResult(_shutdownResult));

            _httpService.DeleteAsync<iOSDeviceAgentService.StatusResult>(
                _defaultSessionUrl,
                Arg.Any<TimeSpan?>(),
                Arg.Any<int>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<bool>(),
                Arg.Any<bool>()
            ).Returns(Task.FromResult(_statusResult));

            _httpService.GetAsync<iOSDeviceAgentService.StatusResult>(
                _defaultPingUrl,
                Arg.Any<TimeSpan?>(),
                Arg.Any<int>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<bool>(),
                Arg.Any<bool>()
            ).Returns(Task.FromResult(_statusResult));
        }

        [Ignore("Legacy ignore from NUnit 3 upgrade")]
        public async void PingTaskCompletesFirstIndicatingSuccessfulLaunch(string deviceId)
        {
            bool pingReturnError = true;

            _httpService.GetAsync<iOSDeviceAgentService.StatusResult>(
                _defaultPingUrl,
                Arg.Any<TimeSpan?>(),
                Arg.Any<int>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<bool>(),
                Arg.Any<bool>()
            ).Returns(Task.FromResult(_statusResult))
             .AndDoes(x =>
             {
                 if (pingReturnError)
                 {
                     pingReturnError = false;
                     throw new DeviceAgentException();
                 }
             });

            var deviceControl = CreateIDBService(deviceId);

            var iOSDeviceAgent = InitialiseIOSDeviceAgentService(deviceControl, _httpService);

            await iOSDeviceAgent.LaunchTestAsync(deviceId, DefaultDeviceAddress);
        }

        [Ignore("Legacy ignore from NUnit 3 upgrade")]
        public async void PingTaskCompletesFirstIndicatingSuccessfulLaunchUsingCodeSignIdentity(string deviceId)
        {
            var deviceControl = CreateIDBService(deviceId);

            var pingReturnError = true;

            _httpService.GetAsync<iOSDeviceAgentService.StatusResult>(
                _defaultPingUrl,
                Arg.Any<TimeSpan?>(),
                Arg.Any<int>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<bool>(),
                Arg.Any<bool>()
            ).Returns(Task.FromResult(_statusResult))
             .AndDoes(x =>
             {
                 if (pingReturnError)
                 {
                     pingReturnError = false;
                     throw new DeviceAgentException();
                 }
             });

            var iOSDeviceAgent = InitialiseIOSDeviceAgentService(deviceControl, _httpService);

            await iOSDeviceAgent.LaunchTestAsync(deviceId, DefaultDeviceAddress);
        }

        [TestCase(DefaultDeviceId)]
        [TestCase(DefaultSimulatorGuidId)]
        public void StartTestTaskFaultedIndicatingFailure(string deviceId)
        {
            bool pingReturnError = true;

            _httpService.GetAsync<iOSDeviceAgentService.StatusResult>(
                _defaultPingUrl,
                Arg.Any<TimeSpan?>(),
                Arg.Any<int>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<bool>(),
                Arg.Any<bool>()
            ).Returns(Task.Run(() =>
            {
                return new Task<IHttpResult<iOSDeviceAgentService.StatusResult>>(() =>
                {
                    Thread.Sleep(300);
                    return _statusResult;
                });
            }))
            .AndDoes(x =>
            {
                if (pingReturnError)
                {
                    pingReturnError = false;
                    throw new DeviceAgentException();
                }
            });

            var deviceControl = CreateIDBService(deviceId);

            var iOSDeviceAgent = InitialiseIOSDeviceAgentService(deviceControl, _httpService);

            var actual = Assert.ThrowsAsync<DeviceAgentException>(async () =>
               {
                   await iOSDeviceAgent.LaunchTestAsync(deviceId, DefaultDeviceAddress);
               });

            Assert.True(actual.Message.StartsWith("Failed to launch DeviceAgent", StringComparison.InvariantCulture));
        }

        [TestCase(DefaultDeviceId)]
        [TestCase(DefaultSimulatorGuidId)]
        public void StartTestTaskCompletesFirstIndicatingFailedLaunch(string deviceId)
        {
            bool pingReturnError = true;

            _httpService.GetAsync<iOSDeviceAgentService.StatusResult>(
                _defaultPingUrl,
                Arg.Any<TimeSpan?>(),
                Arg.Any<int>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<bool>(),
                Arg.Any<bool>()
            ).Returns(Task.Run(() =>
            {
                var tcs = new TaskCompletionSource<IHttpResult<iOSDeviceAgentService.StatusResult>>();
                return tcs.Task;
            }))
            .AndDoes(x =>
            {
                if (pingReturnError)
                {
                    pingReturnError = false;
                    throw new DeviceAgentException();
                }
            });

            var deviceControl = CreateIDBService(deviceId);
            var iOSDeviceAgent = InitialiseIOSDeviceAgentService(deviceControl, _httpService);

            var actual = Assert.ThrowsAsync<DeviceAgentException>(async () =>
            {
                await iOSDeviceAgent.LaunchTestAsync(deviceId, DefaultDeviceAddress);
            });

            Assert.IsTrue(actual.Message.Equals("DeviceAgent is not running"));
        }

        [TestCase(DefaultDeviceId)]
        [TestCase(DefaultSimulatorGuidId)]
        public void PingTaskFaultedIndicatingFailedLaunch(string deviceId)
        {
            _httpService.GetAsync<iOSDeviceAgentService.StatusResult>(
                _defaultPingUrl,
                Arg.Any<TimeSpan?>(),
                Arg.Any<int>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<bool>(),
                Arg.Any<bool>()
            ).Returns<IHttpResult<iOSDeviceAgentService.StatusResult>>(x =>
            {
                throw new Exception();
            });

            var deviceControl = CreateIDBService(deviceId);

            var iOSDeviceAgent = InitialiseIOSDeviceAgentService(deviceControl, _httpService);

            var actual = Assert.ThrowsAsync<DeviceAgentException>(async () =>
            {
                await iOSDeviceAgent.LaunchTestAsync(deviceId, DefaultDeviceAddress);
            });

            Assert.IsTrue(actual.Message.Equals("Unable to contact DeviceAgent"));
        }
    }
}
