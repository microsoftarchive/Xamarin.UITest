using NUnit.Framework;
using Xamarin.UITest.Android;
using Xamarin.UITest.Configuration;

namespace IntegrationTests.Shared
{
    [Category("android")]
    public abstract class AndroidTestBase : TestBase
    {
        protected AndroidApp _app;
        protected AndroidAppConfigurator _appConfiguration;

        protected abstract AppInformation _appInformation { get; }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _appConfiguration = _testConfigurationManager.ConfigureAndroidApp(_appInformation);
        }

        [SetUp]
        public virtual void BeforeEach()
        {
            _app = _appConfiguration.StartApp(AppDataMode.Clear);
        }
    }
}