using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.UITest.Configuration;
using Xamarin.UITest.iOS;

namespace IntegrationTests.Shared
{
    [Category("ios")]
    public abstract class IOSTestBase : TestBase
    {
        protected iOSApp _app;
        protected iOSAppConfigurator _appConfiguration;

        protected abstract AppInformation _appInformation { get; }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _appConfiguration = _testConfigurationManager.ConfigureIOSApp(_appInformation);
        }

        [SetUp]
        public virtual void BeforeEach()
        {
            var envVars = new Dictionary<string, string>();
            envVars.Add("Android", "Of course not!");
            _app = _appConfiguration
                .AutEnvironmentVars(envVars)
                .AutArguments(new[] { "CALABUS_DRIVER" })
                //.CustomDeviceAgentPath("/Users/ilya.bausov/Xamarin.UITest/user-built-device-agent-support/DeviceAgent-Runner.app")
                //.SetDeviceAgentBundleId("com.IliaBausov.DeviceAgent.xctrunner")
                //.DisableDeviceAgentInstall()
                .StartApp(AppDataMode.DoNotClear);
        }
    }
}
