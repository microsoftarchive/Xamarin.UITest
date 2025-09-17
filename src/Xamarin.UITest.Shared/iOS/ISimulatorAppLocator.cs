namespace Xamarin.UITest.Shared.iOS
{
    public interface ISimulatorAppLocator
    {
        SimAppInfo GetInstalledApp(string deviceId, string appIdentifier);
    }
}