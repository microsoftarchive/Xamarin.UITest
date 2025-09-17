using System;
using NSubstitute;
using NUnit.Framework;
using Should;
using Xamarin.UITest.Configuration;
using Xamarin.UITest.iOS;
using Xamarin.UITest.Shared.Execution;

namespace Xamarin.UITest.Tests.Configuration
{
    [TestFixture]
    public class iOSConfigTests
    {
        IExecutor _executor;

        [SetUp]
        public void BeforeEachTest()
        {
            _executor = Substitute.For<IExecutor>();
        }

        [Test]
        public void LaunchApp_NoAppSpecified()
        {
            var config = ConfigureApp
                .iOS
                .GetConfiguration(StartAction.LaunchApp);

            var exception = Assert.Throws<Exception>(() => new iOSApp(config));

            if (exception.Message == "iOS tests are not supported on Windows.")
            {
                return;
            }

            exception.Message.ShouldContain("Must have either installed app or app bundle.");
        }


        [Test]
        public void DefaultLogDirectoryTest()
        {
            var config = ConfigureApp.iOS.GetConfiguration(StartAction.LaunchApp);

            config.LogDirectory.ShouldBeNull();
        }

        [Test]
        public void OverriddenLogDirectoryTest()
        {
            var config = ConfigureApp.iOS.LogDirectory("DirectoryName").GetConfiguration(StartAction.LaunchApp);

            config.LogDirectory.ShouldEqual("DirectoryName");
        }
    }
}