using Xamarin.UITest.Shared;
using Xamarin.UITest.Shared.Android;
using Xamarin.UITest.Shared.Android.Commands;
using Xamarin.UITest.Shared.Artifacts;
using Xamarin.UITest.Shared.Screenshots;
using Xamarin.UITest.Configuration;
using Xamarin.UITest.Utils;

namespace Xamarin.UITest.Android
{
    internal class AndroidDeps
    {
        public AndroidDeps(
            AndroidGestures gestures, 
            IScreenshotTaker screenshotTaker, 
            WaitForHelper waitForHelper, 
            AndroidConfig config, 
            ArtifactFolder artifactFolder, 
            AndroidAppLifeCycle appLifeCycle, 
            IAndroidAppConfiguration appConfiguration, 
            SharedTestServer testServer, 
            CommandAdbStartMonkey monkeyStarter, 
            string deviceSerial)
        {
            Gestures = gestures;
            ScreenshotTaker = screenshotTaker;
            WaitForHelper = waitForHelper;
            Config = config;
            ArtifactFolder = artifactFolder;
            AppLifeCycle = appLifeCycle;
            AppConfiguration = appConfiguration;
            TestServer = testServer;
            MonkeyStarter = monkeyStarter;

            Device = new AndroidDevice(appConfiguration.DeviceUri, gestures, deviceSerial);
        }

        public readonly AndroidGestures Gestures;
        public readonly AndroidConfig Config;
        public readonly IScreenshotTaker ScreenshotTaker;
        public readonly WaitForHelper WaitForHelper;
        public readonly ArtifactFolder ArtifactFolder;
        public readonly AndroidAppLifeCycle AppLifeCycle;
        public readonly IAndroidAppConfiguration AppConfiguration;
        public readonly AndroidDevice Device;
        public readonly SharedTestServer TestServer;
        public readonly CommandAdbStartMonkey MonkeyStarter;
    }
}