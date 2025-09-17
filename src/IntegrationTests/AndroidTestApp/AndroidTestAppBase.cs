using System;
using Xamarin.UITest.Queries;
using Xamarin.UITest.Configuration;
using IntegrationTests.Shared;

public abstract class AndroidTestAppBase : AndroidTestBase
{
    protected override AppInformation _appInformation { get; } = TestApps.AndroidTestApp;

    public override void BeforeEach()
    {
        _appConfiguration = ReConfigureApp(_appConfiguration);
        _app = _appConfiguration.StartApp();
        OnAppStarted();
    }

    protected virtual AndroidAppConfigurator ReConfigureApp(AndroidAppConfigurator app)
    {
        return app;
    }

    protected virtual void OnAppStarted()
    {
    }

    protected void SelectTestActivity(Func<AppQuery, AppQuery> testButton)
    {
        _app.ScrollDownTo(testButton, timeout: TimeSpan.FromMinutes(2));
        _app.Tap(testButton);
        _app.WaitForNoElement(testButton, timeout: TimeSpan.FromSeconds(15));
    }
}
