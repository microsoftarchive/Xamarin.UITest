using IntegrationTests.Shared;
using NUnit.Framework;
using Xamarin.UITest.Configuration;
using Xamarin.UITest.iOS;

[Category("ios")]
public class iOSDeviceAgentNullEnvVarTests : TestBase
{
    protected iOSApp _app;
    protected iOSAppConfigurator _appConfiguration;

    public iOSDeviceAgentNullEnvVarTests()
    {
        _appConfiguration = _testConfigurationManager.ConfigureIOSApp(TestApps.iOSDeviceAgent);
    }

    [SetUp]
    public virtual void BeforeEach()
    {
        _app = _appConfiguration
            .AutEnvironmentVars(null)
            .AutArguments(null)
            .StartApp(AppDataMode.Clear);
    }

    [Test]
    public void AutLaunchedWithoutEnvVars()
    {
        _app.Tap(c => c.Marked("Misc"));
        _app.Tap(c => c.Marked("environment row"));
        _app.WaitForElement(c => c.Marked("environment page"));
        _app.WaitForNoElement(c => c.Text("Of course not!"));
    }

    [Test]
    public void AutLaunchedWithoutArguments()
    {
        _app.Tap(c => c.Marked("Misc"));
        _app.Tap(c => c.Marked("arguments row"));
        _app.WaitForElement(c => c.Marked("arguments page"));
        _app.WaitForNoElement(c => c.Text("The Calabus Driver is on the job!"));
    }
}
