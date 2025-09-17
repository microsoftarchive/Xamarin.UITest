using System;
using System.Net;
using System.Threading.Tasks;
using System.Linq.Expressions;
using NUnit.Framework;
using NSubstitute;
using Xamarin.UITest.XDB.Entities;
using Xamarin.UITest.XDB.Services;
using Xamarin.UITest.XDB.Exceptions;

namespace Xamarin.UITest.Tests.XDB.iOSDeviceAgentTests
{
    public class iOSDeviceAgentServiceShutdownTests : iOSDeviceAgentServiceTestBase
    {
        bool _pingReturnError;
        IHttpService _httpService;

        [Test]
        public void ShutdownAsyncCompletesWithoutError()
        {
            _pingReturnError = true;

            SetupHttpService();

            var deviceAgentService = InitialiseIOSDeviceAgentService(httpService: _httpService);
            deviceAgentService.ShutdownAsync(DefaultDeviceAddress, true).Wait();

            _httpService.Received(1).GetAsync<iOSDeviceAgentService.StatusResult>(_defaultPingUrl,
                                                                                 Arg.Any<TimeSpan?>(),
                                                                                 Arg.Any<int>(),
                                                                                 Arg.Any<TimeSpan?>(),
                                                                                 Arg.Any<bool>(),
                                                                                 Arg.Any<bool>());
            _httpService.Received(1).PostAsync<iOSDeviceAgentService.ShutdownResult>(_defaultShutdownUrl,
                                                                                     Arg.Any<TimeSpan?>(),
                                                                                     Arg.Any<int>(),
                                                                                     Arg.Any<TimeSpan?>(),
                                                                                     Arg.Any<bool>(),
                                                                                     Arg.Any<bool>());
            _httpService.Received(1).DeleteAsync<iOSDeviceAgentService.StatusResult>(_defaultSessionUrl,
                                                                                     Arg.Any<TimeSpan?>(),
                                                                                     Arg.Any<int>(),
                                                                                     Arg.Any<TimeSpan?>(),
                                                                                     Arg.Any<bool>(),
                                                                                     Arg.Any<bool>());
        }

        [Test]
        public void ExceptionThrownDueToErrorWhenStatusResultHasError()
        {
            const string errorMessage = "error";
            SetupHttpService(statusResultError: errorMessage);

            var deviceAgentService = InitialiseIOSDeviceAgentService(httpService: _httpService);

            var exception = Assert.ThrowsAsync<DeviceAgentException>(async () =>
            {
                await deviceAgentService.ShutdownAsync(DefaultDeviceAddress, true);
            });

            _httpService.Received(1).DeleteAsync<iOSDeviceAgentService.StatusResult>(_defaultSessionUrl,
                                                                         Arg.Any<TimeSpan?>(),
                                                                         Arg.Any<int>(),
                                                                         Arg.Any<TimeSpan?>(),
                                                                         Arg.Any<bool>(),
                                                                         Arg.Any<bool>());
            _httpService.Received(0).PostAsync<iOSDeviceAgentService.ShutdownResult>(_defaultShutdownUrl,
                                                                         Arg.Any<TimeSpan?>(),
                                                                         Arg.Any<int>(),
                                                                         Arg.Any<TimeSpan?>(),
                                                                         Arg.Any<bool>(),
                                                                         Arg.Any<bool>());

            Assert.IsTrue(exception.Message.Equals($"DeviceAgent delete session failed: {errorMessage}"));
        }

        [Test]
        public void NoExceptionWhenStatusResultHasErrorAsErrorIfUnavailableIsFalse()
        {
            _pingReturnError = true;

            const string errorMessage = "error";
            SetupHttpService(statusResultError: errorMessage);

            var deviceAgentService = InitialiseIOSDeviceAgentService(httpService: _httpService);

            deviceAgentService.ShutdownAsync(DefaultDeviceAddress, false).Wait();

            _httpService.Received(1).GetAsync<iOSDeviceAgentService.StatusResult>(_defaultPingUrl,
                                                                     Arg.Any<TimeSpan?>(),
                                                                     Arg.Any<int>(),
                                                                     Arg.Any<TimeSpan?>(),
                                                                     Arg.Any<bool>(),
                                                                     Arg.Any<bool>());
            _httpService.Received(1).DeleteAsync<iOSDeviceAgentService.StatusResult>(_defaultSessionUrl,
                                                                                     Arg.Any<TimeSpan?>(),
                                                                                     Arg.Any<int>(),
                                                                                     Arg.Any<TimeSpan?>(),
                                                                                     Arg.Any<bool>(),
                                                                                     Arg.Any<bool>());
            _httpService.Received(1).PostAsync<iOSDeviceAgentService.ShutdownResult>(_defaultShutdownUrl,
                                                             Arg.Any<TimeSpan?>(),
                                                             Arg.Any<int>(),
                                                             Arg.Any<TimeSpan?>(),
                                                             Arg.Any<bool>(),
                                                             Arg.Any<bool>());
        }

        [Test]
        public void ExceptionThrownDueToErrorWhenShutdownResultHasError()
        {
            const string errorMessage = "error";
            SetupHttpService(shutdownResultError: errorMessage);

            var deviceAgentService = InitialiseIOSDeviceAgentService(httpService: _httpService);

            var exception = Assert.ThrowsAsync<DeviceAgentException>(async () =>
            {
                await deviceAgentService.ShutdownAsync(DefaultDeviceAddress, true);
            });

            _httpService.Received(1).DeleteAsync<iOSDeviceAgentService.StatusResult>(_defaultSessionUrl,
                                                                         Arg.Any<TimeSpan?>(),
                                                                         Arg.Any<int>(),
                                                                         Arg.Any<TimeSpan?>(),
                                                                         Arg.Any<bool>(),
                                                                         Arg.Any<bool>());
            _httpService.Received(1).PostAsync<iOSDeviceAgentService.ShutdownResult>(_defaultShutdownUrl,
                                                 Arg.Any<TimeSpan?>(),
                                                 Arg.Any<int>(),
                                                 Arg.Any<TimeSpan?>(),
                                                 Arg.Any<bool>(),
                                                 Arg.Any<bool>());

            Assert.IsTrue(exception.Message.Equals($"DeviceAgent shutdown failed: {errorMessage}"));
        }

        [Test]
        public void WebExceptionThrownWhenCallingSessionUrl()
        {
            const string exceptionMessage = "Connection Failure";

            SetupHttpService();

            _httpService.DeleteAsync<iOSDeviceAgentService.StatusResult>(_defaultSessionUrl,
                                                                         Arg.Any<TimeSpan?>(),
                                                                         Arg.Any<int>(),
                                                                         Arg.Any<TimeSpan?>(),
                                                                         Arg.Any<bool>(),
                                                                         Arg.Any<bool>())
                        .Returns<IHttpResult<iOSDeviceAgentService.StatusResult>>(x => { throw new WebException(exceptionMessage); });

            var deviceAgentService = InitialiseIOSDeviceAgentService(httpService: _httpService);

            var exception = Assert.ThrowsAsync<DeviceAgentException>(async () =>
            {
                await deviceAgentService.ShutdownAsync(DefaultDeviceAddress, true);
            });

            _httpService.Received(1).DeleteAsync<iOSDeviceAgentService.StatusResult>(_defaultSessionUrl,
                                                                                     Arg.Any<TimeSpan?>(),
                                                                                     Arg.Any<int>(),
                                                                                     Arg.Any<TimeSpan?>(),
                                                                                     Arg.Any<bool>(),
                                                                                     Arg.Any<bool>());
            _httpService.Received(0).PostAsync<iOSDeviceAgentService.ShutdownResult>(_defaultShutdownUrl,
                                                                                     Arg.Any<TimeSpan?>(),
                                                                                     Arg.Any<int>(),
                                                                                     Arg.Any<TimeSpan?>(),
                                                                                     Arg.Any<bool>(),
                                                                                     Arg.Any<bool>());

            Assert.IsTrue(exception.Message.Equals($"Unable to end session: {exceptionMessage}"));
        }

        [Test]
        public void WebExceptionThrownWhenCallingShutdownUrl()
        {
            const string exceptionMessage = "Connection Failure";
            SetupHttpService();

            _httpService.PostAsync<iOSDeviceAgentService.ShutdownResult>(_defaultShutdownUrl,
                                                                         Arg.Any<TimeSpan?>(),
                                                                         Arg.Any<int>(),
                                                                         Arg.Any<TimeSpan?>(),
                                                                         Arg.Any<bool>(),
                                                                         Arg.Any<bool>())
                        .Returns<IHttpResult<iOSDeviceAgentService.ShutdownResult>>(x => { throw new WebException(exceptionMessage); });

            var deviceAgentService = InitialiseIOSDeviceAgentService(httpService: _httpService);

            var exception = Assert.ThrowsAsync<DeviceAgentException>(async () =>
            {
                await deviceAgentService.ShutdownAsync(DefaultDeviceAddress, true);
            });

            _httpService.Received(1).DeleteAsync<iOSDeviceAgentService.StatusResult>(_defaultSessionUrl,
                                                                                     Arg.Any<TimeSpan?>(),
                                                                                     Arg.Any<int>(),
                                                                                     Arg.Any<TimeSpan?>(),
                                                                                     Arg.Any<bool>(),
                                                                                     Arg.Any<bool>());
            _httpService.Received(1).PostAsync<iOSDeviceAgentService.ShutdownResult>(_defaultShutdownUrl,
                                                                                     Arg.Any<TimeSpan?>(),
                                                                                     Arg.Any<int>(),
                                                                                     Arg.Any<TimeSpan?>(),
                                                                                     Arg.Any<bool>(),
                                                                                     Arg.Any<bool>());

            Assert.IsTrue(exception.Message.Equals($"Unable to shutdown DeviceAgent: {exceptionMessage}"));
        }

        void SetupHttpService(
          string statusResultError = "",
          string statusResultContent = "dead",
          string shutdownResultError = "",
          string shutdownResultContent = "Goodbye.",
          string pingResultError = "",
          string pingResultContent = "honk!")
        {
            _httpService = Substitute.For<IHttpService>();
            _httpService.DeleteAsync<iOSDeviceAgentService.StatusResult>(_defaultSessionUrl,
                                                                         Arg.Any<TimeSpan?>(),
                                                                         Arg.Any<int>(),
                                                                         Arg.Any<TimeSpan?>(),
                                                                         Arg.Any<bool>(),
                                                                         Arg.Any<bool>())
                        .Returns(x =>
                        {
                            var statusResult = new iOSDeviceAgentService.StatusResult();
                            statusResult.Status = statusResultContent;
                            statusResult.Error = statusResultError;
                            var httpResult = new HttpResult<iOSDeviceAgentService.StatusResult>
                            {
                                Content = statusResult,
                                StatusCode = HttpStatusCode.OK
                            };
                            return Task.FromResult<IHttpResult<iOSDeviceAgentService.StatusResult>>(httpResult);
                        });

            _httpService.PostAsync<iOSDeviceAgentService.ShutdownResult>(_defaultShutdownUrl,
                                                                         Arg.Any<TimeSpan?>(),
                                                                         Arg.Any<int>(),
                                                                         Arg.Any<TimeSpan?>(),
                                                                         Arg.Any<bool>(),
                                                                         Arg.Any<bool>())
                        .Returns(x =>
                        {
                            var shutdownResult = new iOSDeviceAgentService.ShutdownResult();
                            shutdownResult.Message = shutdownResultContent;
                            shutdownResult.Error = shutdownResultError;
                            var httpResult = new HttpResult<iOSDeviceAgentService.ShutdownResult>
                            {
                                Content = shutdownResult,
                                StatusCode = HttpStatusCode.OK
                            };
                            return Task.FromResult<IHttpResult<iOSDeviceAgentService.ShutdownResult>>(httpResult);
                        });

            _httpService.GetAsync<iOSDeviceAgentService.StatusResult>(_defaultPingUrl,
                                                                      Arg.Any<TimeSpan?>(),
                                                                      Arg.Any<int>(),
                                                                      Arg.Any<TimeSpan?>(),
                                                                      Arg.Any<bool>(),
                                                                      Arg.Any<bool>())
                        .Returns(x =>
                        {
                            var statusResult = new iOSDeviceAgentService.StatusResult();
                            statusResult.Status = pingResultContent;
                            statusResult.Error = pingResultError;
                            var httpResult = new HttpResult<iOSDeviceAgentService.StatusResult>
                            {
                                Content = statusResult,
                                StatusCode = HttpStatusCode.OK
                            };
                            return Task.FromResult<IHttpResult<iOSDeviceAgentService.StatusResult>>(httpResult);
                        }).AndDoes(x =>
                        {
                            if (_pingReturnError)
                            {
                                _pingReturnError = true;
                                throw new DeviceAgentException();
                            }
                        });
        }
    }
}
