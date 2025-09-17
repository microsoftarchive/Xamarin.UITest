using System;
using NSubstitute;
using NUnit.Framework;
using Xamarin.UITest.iOS;
using Xamarin.UITest.Shared.Processes;

namespace Xamarin.UITest.Tests.iOS
{
    [TestFixture]
    public class OToolTests
    {
        IProcessRunner _mockProcessRunner;

        [SetUp]
        public void Before()
        {
            _mockProcessRunner = Substitute.For<IProcessRunner>();

            _mockProcessRunner.RunCommand(
                "xcrun",
                "otool-classic -hv -arch all \"path/to/file.app\"",
                CheckExitCode.AllowAnything
            )
            .Returns(CreateProcessResult(0, "valid"));
            
            _mockProcessRunner.RunCommand(
                "xcrun",
                "otool -hv -arch all \"path/to/file.app\"",
                CheckExitCode.AllowAnything
            )
            .Returns(CreateProcessResult(0, "valid"));
        }

        [Test]
        public void OToolClassicUsedWhenXcodeVersionEqualAndGreaterThan8()
        {
            var otool = new OTool(new Version(8, 0, 0, 0), _mockProcessRunner);

            var actual = otool.CheckForExecutable("path/to/file.app");

            EnsureReceived("otool-classic", "path/to/file.app"); 
            Assert.AreEqual(LinkStatus.ExecutableExists, actual);
        }

        [Test]
        public void OToolUsedWhenXcodeVersionLessThan8()
        {
            var otool = new OTool(new Version(7, 0, 0, 0), _mockProcessRunner);

            var actual = otool.CheckForExecutable("path/to/file.app");

            EnsureReceived("otool", "path/to/file.app");
            Assert.AreEqual(LinkStatus.ExecutableExists, actual);
        }

        [TestCase(7, "otool")]
        [TestCase(8, "otool-classic")]
        public void ReturnsNoExecutableLinkStatus(int xcodeVersion, string otoolCommand)
        {
            _mockProcessRunner.RunCommand(
                "xcrun",
                $"{otoolCommand} -hv -arch all \"path/to/invalid-file.dll\"",
                CheckExitCode.AllowAnything
            )
            .Returns(CreateProcessResult(0, "information: is not an object file"));

            var otool = new OTool(new Version(xcodeVersion, 0, 0, 0), _mockProcessRunner);
            var actual = otool.CheckForExecutable("path/to/invalid-file.dll");

            EnsureReceived(otoolCommand, "path/to/invalid-file.dll");
            Assert.AreEqual(LinkStatus.NoExecutable, actual);
        }

        [TestCase(7,"otool")]
        [TestCase(8,"otool-classic")]
        public void ReturnsCheckFailedOnNonZeroStatusCode(int xcodeVersion, string otoolCommand)
        {
            _mockProcessRunner.RunCommand(
                "xcrun",
                $"{otoolCommand} -hv -arch all \"path/to/invalid-file.dll\"",
                CheckExitCode.AllowAnything
            )
            .Returns(CreateProcessResult(1, "error"));

            var otool = new OTool(new Version(xcodeVersion, 0, 0, 0), _mockProcessRunner);

            var actual = otool.CheckForExecutable("path/to/invalid-file.dll");

            EnsureReceived(otoolCommand, "path/to/invalid-file.dll");
            Assert.AreEqual(LinkStatus.CheckFailed, actual);
        }

        ProcessResult CreateProcessResult(int exitCode, string output)
        {
            return new ProcessResult(new[] { new ProcessOutput(output) }, exitCode, 1, true);
        }

        void EnsureReceived(string otoolCommand, string path)
        { 
            _mockProcessRunner.Received().RunCommand(
                "xcrun",
                $"{otoolCommand} -hv -arch all \"{path}\"",
                CheckExitCode.AllowAnything
            );
        }
    }
}

