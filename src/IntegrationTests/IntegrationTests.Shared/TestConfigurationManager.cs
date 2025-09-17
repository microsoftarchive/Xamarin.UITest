using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Configuration;

namespace IntegrationTests.Shared
{
    public class TestConfigurationManager
    {
        readonly JsonFileDeserializer _jsonFileDeserializer = new JsonFileDeserializer();
        readonly TestConfiguration _testConfiguration;

        public TestConfigurationManager()
        {
            var pathToConfig = FindConfigFolder();

            if (!string.IsNullOrEmpty(pathToConfig))
            {
                _testConfiguration = _jsonFileDeserializer.DeserializeConfigurationFile<TestConfiguration>(pathToConfig);
            }

            if (!string.IsNullOrEmpty(_testConfiguration?.XcodePath))
            {
                Environment.SetEnvironmentVariable("DEVELOPER_DIR", _testConfiguration.XcodePath);
            }
        }

        public iOSAppConfigurator ConfigureIOSApp(AppInformation appInformation)
        {
            var config = ConfigureApp.iOS;
            config.Debug();

            if (!string.IsNullOrEmpty(_testConfiguration.DeviceIdentifier))
            {
                config.DeviceIdentifier(_testConfiguration.DeviceIdentifier);
            }

            if (_testConfiguration.PhysicalDevice)
            {
                if (string.IsNullOrEmpty(appInformation.Identifier))
                {
                    throw new Exception($"App identifier is empty or null. It must be specified.");
                }

                config.InstalledApp(appInformation.Identifier);
            }
            else
            {
                if (string.IsNullOrEmpty(appInformation.FilePath))
                {
                    throw new Exception($"App FilePath is empty or null. It must be specified.");
                }

                var bundlePath = appInformation.FilePath;
                if (!_testConfiguration.Simulator)
                {
                    bundlePath = bundlePath.Replace("-sim.app", "-device.app");
                }

                config.AppBundleZip(bundlePath);
            }

            return config;
        }

        public AndroidAppConfigurator ConfigureAndroidApp(AppInformation appInformation)
        {
            var config = ConfigureApp.Android;

            if (!string.IsNullOrEmpty(_testConfiguration.DeviceIdentifier))
            {
                config.DeviceSerial(_testConfiguration.DeviceIdentifier);
            }

            if (string.IsNullOrEmpty(appInformation.FilePath))
            {
                throw new Exception($"App FilePath is empty or null. It must be specified.");
            }

            config.ApkFile(appInformation.FilePath);
            config.Debug();
            return config;
        }

        bool FileExists
        {
            get
            {
                return _testConfiguration != null;
            }
        }

        string FindConfigFolder(string path = "")
        {
            var currentPath = string.IsNullOrEmpty(path) ?
                             TestContext.CurrentContext.TestDirectory :
                             path;

            var directories = Directory.GetDirectories(currentPath);

            if (directories.Any(e => e.EndsWith("IntegrationTests")))
            {
                var intTestDirectory = Directory.GetFiles($"{currentPath}/IntegrationTests");

                return intTestDirectory.Any(e => e.EndsWith("test-config.json")) ?
                                       $"{currentPath}/IntegrationTests/test-config.json" :
                                       "";
            }

            return FindConfigFolder(Directory.GetParent(currentPath).FullName);
        }
    }
}
