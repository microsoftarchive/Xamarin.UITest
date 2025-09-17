using System;
using System.IO;
using NUnit.Framework;
using Xamarin.UITest;

namespace IntegrationTests.Shared
{
    public static class TestApps
    {
        public static AppInformation AndroidTestApp
        {
            get
            {
                return new AppInformation()
                {
                    FilePath = GetAppPath("AndroidTestApp.apk"),
                    Identifier = ""
                };
            }
        }

        public static AppInformation iOSTestApp
        {
            get
            {
                return new AppInformation()
                {
                    FilePath = GetAppPath("TestApp-sim.app.zip"),
                    Identifier = "sh.calaba.TestApp"
                };
            }
        }

        public static AppInformation iOSDeviceAgent
        {
            get
            {
                return new AppInformation()
                {
                    FilePath = GetAppPath("DeviceAgent-sim.app.zip"),
                    Identifier = "com.apple.test.DeviceAgent-Runner"
                };
            }
        }

        public static AppInformation iOSCalWebView
        {
            get
            {
                return new AppInformation()
                {
                    FilePath = GetAppPath("CalWebView-sim.app.zip"),
                    Identifier = "sh.calaba.CalWebView-cal"
                };
            }
        }

        public static AppInformation iOSIntegration
        {
            get
            {
                return new AppInformation()
                {
                    FilePath = GetAppPath("iOSIntegration-sim.app.zip"),
                    Identifier = "com.xamarin.samples.taskyprotouch"
                };
            }
        }

        public static AppInformation iOSCalSmoke
        {
            get
            {
                return new AppInformation()
                {
                    FilePath = GetAppPath("CalSmoke-sim.app.zip"),
                    Identifier = "sh.calaba.TestApp"
                };
            }
        }

        static string GetAppPath(string appName)
        {
            return Path.Combine(GetTestAppsPath(), appName);
        }

        static string GetTestAppsPath()
        {
            var buildScriptLocation = new DirectoryInfo(
                Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "binaries"));

            if (buildScriptLocation.Exists)
            {
                return buildScriptLocation.FullName;
            }

            return Path.Combine(GetSrcPath(), "..", "binaries", "TestApps");
        }

        static string GetSrcPath(DirectoryInfo directory = null)
        {
            const string srcDirName = "src";

            directory = directory ?? new DirectoryInfo(TestContext.CurrentContext.TestDirectory);

            if (directory.Name == srcDirName)
            {
                return directory.FullName;
            }

            if (directory.Name == "Xamarin.UITest")
            {
                throw new Exception($"Cannot find {srcDirName} directory");
            }

            return GetSrcPath(directory.Parent);
        }
    }
}
