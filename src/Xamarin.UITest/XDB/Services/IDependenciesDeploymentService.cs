namespace Xamarin.UITest.XDB.Services
{
    interface IDependenciesDeploymentService
    {
        string DeviceAgentBundleVersion { get; }

        string HashId { get; }
        
        void Install(string directory);

        string PathToDeviceTestRunner { get; }

        string PathToSimTestRunner { get; }
    }
}