using System;
using NUnit.Framework;
using NSubstitute;
using Xamarin.UITest.XDB.Exceptions;
using Xamarin.UITest.XDB.Services.OSX;
using Xamarin.UITest.XDB.Services.Processes;

namespace Xamarin.UITest.Tests.XDB
{
    [TestFixture]
    public class XcodeServiceTests
    {
        const string ExpectedProcessOutputWithValidVersion = "Xcode 7.3.1\nBuild version 7D1014";
        const string NoVersionNumberInOutput = "Xcode \nBuild version 7D1014";
        const string NoDeveloperToolsInstalledOutput = "xcode-select: note: no developer tools were found at " +
            "'/Applications/Xcode.app' requesting install. " +
            "Choose an option in the dialog to download the command line developer tools.";
        const string ExternalExceptionMessage = "xcodebuild output not as expected";

        [Test]
        public void ValidVersionNumber()
        {
            Version expected = new Version(7, 3, 1);
            var xcodeService = new XcodeService(SetupMockAndGetProcessService(ExpectedProcessOutputWithValidVersion));

            Version actual = xcodeService.GetCurrentVersion();

            Assert.IsTrue(expected.Equals(actual));
        }

        [Test]
        public void NoDeveloperToolsInstalled()
        {
            var exception = Assert.Throws<ExternalProcessException>(() =>
                new XcodeService(SetupMockAndGetProcessService(NoDeveloperToolsInstalledOutput)).GetCurrentVersion());

            Assert.IsTrue(ExternalExceptionMessage.Equals(exception.Message));
        }

        [Test]
        public void RegexResultHasTwoGroupsButNoVersionNumber()
        {
            var exception = Assert.Throws<ExternalProcessException>(() =>
                new XcodeService(SetupMockAndGetProcessService(NoVersionNumberInOutput)).GetCurrentVersion());

            Assert.IsTrue(ExternalExceptionMessage.Equals(exception.Message));
        }

        IProcessService SetupMockAndGetProcessService(string processResponseMessage)
        {
            var processService = Substitute.For<IProcessService>();

            processService.Run(
                Arg.Any<string>(),
                Arg.Any<string>()
            ).Returns(new ProcessResult(0, processResponseMessage, "", ""));

            return processService;
        }
    }
}