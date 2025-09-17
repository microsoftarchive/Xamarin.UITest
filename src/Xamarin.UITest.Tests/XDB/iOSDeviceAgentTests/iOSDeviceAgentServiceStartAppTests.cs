using NUnit.Framework;
using NSubstitute;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xamarin.UITest.XDB.Services;
using Xamarin.UITest.XDB.Entities;
using Xamarin.UITest.Tests.Extensions;

namespace Xamarin.UITest.Tests.XDB.iOSDeviceAgentTests
{
    public class iOSDeviceAgentServiceStartAppTests : iOSDeviceAgentServiceTestBase
    {
        readonly string[] _defaultLaunchArgs = { "arg1", "arg2" };

        readonly IDictionary<string, string> _defaultEnvVars = new Dictionary<string, string>
            {
                { "Env1" , "Value1" },
                { "Env2", "Value2" }
            };

        private IHttpService _httpService;

        [Test]
        public async Task StartWithoutLunchArgsOrEnvVars()
        {
            var deviceControl = CreateIDBService(DefaultDeviceId);

            SetupHttpWithAssertionCallback(new
            {
                bundleID = DefaultBundleId,
                launchArgs = new string[] { },
                environment = new Dictionary<string, string> { }
            });

            var deviceAgentService = InitialiseIOSDeviceAgentService(deviceControl, _httpService);

            await deviceAgentService.StartAppAsync(DefaultDeviceAddress, DefaultBundleId);
            _httpService.Received(1);
        }

        [Test]
        public async Task StartWithLaunchArgsAsIEumerable()
        {
            var deviceControl = CreateIDBService(DefaultDeviceId);

            SetupHttpWithAssertionCallback(new
            {
                bundleID = DefaultBundleId,
                launchArgs = _defaultLaunchArgs,
                environment = new Dictionary<string, string> { }
            });

            var deviceAgentService = InitialiseIOSDeviceAgentService(deviceControl, _httpService);

            await deviceAgentService.StartAppAsync(
                DefaultDeviceAddress,
                DefaultBundleId,
                launchArgs: _defaultLaunchArgs
            );
        }

        [Test]
        public async Task StartWithEnvironmentVariablesAsIDictionary()
        {
            var deviceControl = CreateIDBService(DefaultDeviceId);

            SetupHttpWithAssertionCallback(new
            {
                bundleID = DefaultBundleId,
                launchArgs = new string[] { },
                environment = _defaultEnvVars
            });

            var deviceAgentService = InitialiseIOSDeviceAgentService(deviceControl, _httpService);

            await deviceAgentService.StartAppAsync(
                DefaultDeviceAddress,
                DefaultBundleId,
                environmentVars: _defaultEnvVars
            );

            _httpService.Received(1);
        }

        [Test]
        public async Task StartWithLaunchArgsAndEnvVars()
        {
            var deviceControl = CreateIDBService(DefaultDeviceId);

            SetupHttpWithAssertionCallback(new
            {
                bundleID = DefaultBundleId,
                launchArgs = _defaultLaunchArgs,
                environment = _defaultEnvVars
            });

            var deviceAgentService = InitialiseIOSDeviceAgentService(deviceControl, _httpService);

            await deviceAgentService.StartAppAsync(
                DefaultDeviceAddress,
                DefaultBundleId,
                _defaultLaunchArgs,
                _defaultEnvVars
            );

            _httpService.Received(1);
        }

        //wip
        [Ignore("Ignored as WiP")]
        public async Task ExceptionThrownAsAppIsNotInstalled()
        {
            var deviceControl = CreateIDBService(DefaultDeviceId, isInstalledExitCode: 2);

            SetupHttpWithAssertionCallback(new
            {
                bundleID = DefaultBundleId,
                launchArgs = _defaultLaunchArgs,
                environment = _defaultEnvVars
            }, "error");

            var deviceAgentService = InitialiseIOSDeviceAgentService(deviceControl, _httpService);

            await deviceAgentService.StartAppAsync(
                DefaultDeviceAddress,
                DefaultBundleId,
                _defaultLaunchArgs,
                _defaultEnvVars);

            Assert.Throws<Exception>(delegate
            {
                _httpService.Received(1);
            });
        }

        [Test]
        public async Task StartAppUsingJsonContainingLaunchArgsAndEnvVars()
        {
            string jsonDefaultEnvVars = "{ \"Env1\": \"Value1\" , \"Env2\": \"Value2\" }";
            string jsonDefaultLaunchArgs = "[ \"arg1\" , \"arg2\" ]";

            var deviceControl = CreateIDBService(DefaultDeviceId);

            SetupHttpWithAssertionCallback(new
            {
                bundleID = DefaultBundleId,
                launchArgs = _defaultLaunchArgs,
                environment = _defaultEnvVars
            });

            var deviceAgentService = InitialiseIOSDeviceAgentService(deviceControl, _httpService);

            await deviceAgentService.StartAppAsync(
                DefaultDeviceAddress,
                DefaultBundleId,
                jsonDefaultLaunchArgs,
                jsonDefaultEnvVars
            );

            _httpService.Received(1);
        }

        void SetupHttpWithAssertionCallback(object arguments, string error = null)
        {
            _httpService = Substitute.For<IHttpService>();

            IHttpResult<iOSDeviceAgentService.StatusResult> statusResult = new HttpResult<iOSDeviceAgentService.StatusResult>
            {
                Content = new iOSDeviceAgentService.StatusResult { Error = "" }
            };

            _httpService.PostAsJsonAsync<iOSDeviceAgentService.StatusResult>(
                _defaultSessionUrl,
                Arg.Any<object>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<int>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<bool>(),
                Arg.Any<bool>()
            ).Returns(Task.FromResult(statusResult))
            .AndDoes(x => { AssertStartAppHasCorrectArguements(arguments, x.Args()[1]); });
        }

        void AssertStartAppHasCorrectArguements(object expected, object actual)
        {
            const string bundleKey = "bundleID";
            const string launchArgsKey = "launchArgs";
            const string envVarsKey = "environment";

            var expectedBundleId = expected.GetPropertyValue<string>(bundleKey).Equals(DefaultBundleId);
            var expectedLaunchArgs = expected.GetPropertyValue<IEnumerable<string>>(launchArgsKey);
            var expectedEnvVars = expected.GetPropertyValue<Dictionary<string, string>>(envVarsKey);

            var actualBundleId = actual.GetPropertyValue<string>(bundleKey).Equals(DefaultBundleId);
            var actualLaunchArgs = actual.GetPropertyValue<IEnumerable<string>>(launchArgsKey);
            var actualEnvVars = actual.GetPropertyValue<Dictionary<string, string>>(envVarsKey);

            Assert.IsTrue(expectedBundleId.Equals(actualBundleId));
            Assert.AreEqual(expectedLaunchArgs, actualLaunchArgs);
            Assert.AreEqual(expectedEnvVars, actualEnvVars);
        }
    }
}
