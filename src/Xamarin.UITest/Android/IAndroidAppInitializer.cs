using Xamarin.UITest.Configuration;
using Xamarin.UITest.Shared.Artifacts;

namespace Xamarin.UITest.Android
{
    internal interface IAndroidAppInitializer
    {
        AndroidDeps PrepareEnvironment();
        void VerifyConfiguration();
        TestApkFiles PrepareApkFiles(IAndroidAppConfiguration appConfiguration, ArtifactFolder artifactFolder);
    }
}