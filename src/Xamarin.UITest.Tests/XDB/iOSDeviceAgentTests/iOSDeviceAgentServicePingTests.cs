using System;
using System.Threading.Tasks;
using System.Linq.Expressions;
using NSubstitute;
using NUnit.Framework;
using Xamarin.UITest.XDB.Services;
using Xamarin.UITest.XDB.Entities;
using Xamarin.UITest.XDB.Exceptions;

namespace Xamarin.UITest.Tests.XDB.iOSDeviceAgentTests
{
    public class iOSDeviceAgentServicePingTests : iOSDeviceAgentServiceTestBase
    {
        IHttpService _httpService;
        iOSDeviceAgentService.StatusResult _statusResult;
        HttpResult<iOSDeviceAgentService.StatusResult> _httpResult;

        public iOSDeviceAgentServicePingTests()
        {
            _httpService = Substitute.For<IHttpService>();

            _statusResult = new iOSDeviceAgentService.StatusResult
            {
                Error = string.Empty
            };

            _httpResult = new HttpResult<iOSDeviceAgentService.StatusResult>
            {
                Content = _statusResult,
                StatusCode = System.Net.HttpStatusCode.OK
            };
        }

        [SetUp]
        public void SetUp()
        {
            _httpService = Substitute.For<IHttpService>();
        }

        [Test]
        public void SuccessfulPing()
        {
            _httpService.GetAsync<iOSDeviceAgentService.StatusResult>(
                _defaultPingUrl,
                Arg.Any<TimeSpan?>(),
                Arg.Any<int>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<bool>(),
                Arg.Any<bool>()
            ).Returns(Task.FromResult<IHttpResult<iOSDeviceAgentService.StatusResult>>(_httpResult));

            var iOSDeviceAgent = InitialiseIOSDeviceAgentService(httpService: _httpService);

            iOSDeviceAgent.PingAsync(DefaultDeviceAddress).Wait();

            _httpService.Received(1).GetAsync<iOSDeviceAgentService.StatusResult>(
                Arg.Is(_defaultPingUrl),
                Arg.Any<TimeSpan?>(),
                Arg.Any<int>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<bool>(),
                Arg.Any<bool>()
            );
        }

        [Test]
        public void GetAsyncThrowsExceptionCausingUnsucessfulPing()
        {
            _httpService.GetAsync<iOSDeviceAgentService.StatusResult>(
                Arg.Is(_defaultPingUrl),
                Arg.Any<TimeSpan?>(),
                Arg.Is(1),
                Arg.Any<TimeSpan?>(),
                Arg.Is(true),
                Arg.Is(true)
            ).Returns<IHttpResult<iOSDeviceAgentService.StatusResult>>(x => { throw new Exception(); });

            var iOSDeviceAgent = InitialiseIOSDeviceAgentService(httpService: _httpService);

            var exception = Assert.ThrowsAsync<DeviceAgentException>(async () => await iOSDeviceAgent.PingAsync(DefaultDeviceAddress));

            Assert.IsTrue(exception.Message.Equals($"Unable to contact DeviceAgent on {DefaultDeviceAddress}"));

            _httpService.Received(1).GetAsync<iOSDeviceAgentService.StatusResult>(
                Arg.Is(_defaultPingUrl),
                Arg.Any<TimeSpan?>(),
                Arg.Is(1),
                Arg.Any<TimeSpan?>(),
                Arg.Is(true),
                Arg.Is(true)
            );
        }

        [Test]
        public void PingResultValidationFailedDueToError()
        {
            var statusResult = new iOSDeviceAgentService.StatusResult
            {
                Error = "error"
            };

            var httpResult = new HttpResult<iOSDeviceAgentService.StatusResult>
            {
                Content = statusResult,
                StatusCode = System.Net.HttpStatusCode.OK
            };

            _httpService.GetAsync<iOSDeviceAgentService.StatusResult>(
                _defaultPingUrl,
                Arg.Any<TimeSpan?>(),
                Arg.Any<int>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<bool>(),
                Arg.Any<bool>()
            ).Returns(Task.FromResult<IHttpResult<iOSDeviceAgentService.StatusResult>>(httpResult));

            var iOSDeviceAgent = InitialiseIOSDeviceAgentService(httpService: _httpService);

            var exception = Assert.ThrowsAsync<DeviceAgentException>(async () => await iOSDeviceAgent.PingAsync(DefaultDeviceAddress));

            Assert.AreEqual($"DeviceAgent ping failed: error", exception.Message);

            _httpService.Received(1).GetAsync<iOSDeviceAgentService.StatusResult>(
                _defaultPingUrl,
                Arg.Any<TimeSpan?>(),
                Arg.Any<int>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<bool>(),
                Arg.Any<bool>()
            );
        }
    }
}
