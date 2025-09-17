using System;
using System.IO;
using NSubstitute;
using NUnit.Framework;
using Should;
using Xamarin.UITest.Android;
using Xamarin.UITest.Configuration;
using Xamarin.UITest.Shared.Android;
using Xamarin.UITest.Shared.Android.Queries;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Logging;

namespace Xamarin.UITest.Tests.Configuration
{
    [TestFixture]
    public class AndroidConfigTests
    {
        IAndroidFactory _factory;
        IExecutor _executor;

        [SetUp]
        public void BeforeEachTest()
        {
            _factory = Substitute.For<IAndroidFactory>();
            _executor = Substitute.For<IExecutor>();

            _factory.BuildExecutor(null)
                .ReturnsForAnyArgs(_executor);
        }

        [Test]
        public void LaunchApp_NoApkFileConfigured()
        {
            _executor.Execute(Arg.Any<QueryAdbDevices>())
                .Returns(new[] { "device-serial" });

            var config = ConfigureApp
                .Android
                .GetConfiguration(StartAction.LaunchApp);

            var exception = Assert.Throws<Exception>(() => new AndroidApp(config));

            exception.Message.ShouldEqual("ApkFile or InstalledApp has not been configured.");
        }        
        
        [Test]
        public void LaunchApp_ApkFileDoesNotExist()
        {
            _executor.Execute(Arg.Any<QueryAdbDevices>())
                .Returns(new[] { "device-serial" });

            var config = ConfigureApp
                .Android
                .ApkFile("I don't exist at all")
                .GetConfiguration(StartAction.LaunchApp);

            var exception = Assert.Throws<Exception>(() => new AndroidApp(config));

            exception.Message.ShouldContain("ApkFile does not exist:");
        }

        [Test]
        public void ConnectApp_NoApkFileConfigured()
        {
            _executor.Execute(Arg.Any<QueryAdbDevices>())
                .Returns(new[] { "device-serial" });

            var config = ConfigureApp
                .Android
                .DevicePort(1337)
                .GetConfiguration(StartAction.ConnectToApp);

            new AndroidApp(config, _executor);
        }

        [Test]
        public void NoDevicesConnected()
        {
            _executor.Execute(Arg.Any<QueryAdbDevices>())
                .Returns(new string[0]);

            var config = ConfigureApp
                .Android
                .InstalledApp("my.installed.app")
                .GetConfiguration(StartAction.LaunchApp);

            var exception = Assert.Throws<Exception>(() => new AndroidApp(config, _executor));

            exception.Message.ShouldEqual("No devices connected.");
        }

        [Test]
        public void MoreThanOneDevicesConnected()
        {
            _executor.Execute(Arg.Any<QueryAdbDevices>())
                .Returns(new[] { "device-1", "device-2" });

            var config = ConfigureApp
                .Android
                .InstalledApp("my.installed.app")
                .GetConfiguration(StartAction.LaunchApp);

            var exception = Assert.Throws<Exception>(() => new AndroidApp(config, _executor));

            exception.Message.ShouldEqual("Found 2 connected Android devices. Either only have 1 connected or select one using DeviceSerial during configuration. Devices found: device-1, device-2");
        }

        [Test]
        public void DefaultLogDirectoryTest()
        {
            var config = ConfigureApp.Android.GetConfiguration(StartAction.LaunchApp);

            config.LogDirectory.ShouldBeNull();
        }

        [Test]
        public void OverriddenLogDirectoryTest()
        {
            var config = ConfigureApp.Android.LogDirectory("DirectoryName").GetConfiguration(StartAction.LaunchApp);

            config.LogDirectory.ShouldEqual("DirectoryName");
        }

        [Test]
        public void FileLogConsumerDirectoryNullTest()
        {
            Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);

            var consumer = new FileLogConsumer();

            consumer.LogPath.ShouldStartWith(Path.GetTempPath());
        }

        [Test]
        public void FileLogConsumerDirectoryNotNullTest()
        {
            var consumer = new FileLogConsumer("DirectoryName");

            consumer.LogPath.ShouldStartWith("DirectoryName");
        }
    }
}