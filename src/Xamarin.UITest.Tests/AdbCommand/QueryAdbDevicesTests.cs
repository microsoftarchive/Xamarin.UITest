using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Xamarin.UITest.Shared.Android.Adb;
using Xamarin.UITest.Shared.Android.Queries;
using Xamarin.UITest.Shared.Dependencies;
using Xamarin.UITest.Shared.Processes;

namespace Xamarin.UITest.Tests.AdbCommand
{
    [TestFixture]
    public class QueryAdbDevicesTests
    {
        [Test]
        public void NoConnectedDeviceAndDamonStarted()
        {
            var expected = new string[0];
            var adbOutput = "List of devices attached" +
                "\n* daemon not running. starting it now on port 5037 *" +
                "\n* daemon started successfully *";

            var mockProcessRunner = Substitute.For<IProcessRunner>();
            var processOutput = new ProcessOutput[] { new ProcessOutput(adbOutput)};
            var processResult = new ProcessResult(processOutput, 0, 0, true);
            mockProcessRunner.Run(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int[]>()).Returns(processResult);

            var mockSdkTools = Substitute.For<IAndroidSdkTools>();
            mockSdkTools.GetAdbPath().Returns("adb");

            var adbProcessRunner = new AdbProcessRunner(mockProcessRunner, mockSdkTools);
            var actual = new QueryAdbDevices().Execute(adbProcessRunner, mockSdkTools);

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void NoConnectedDevice()
        {
            var expected = new string[0];
            var adbOutput = "List of devices attached\n\n";

            var mockProcessRunner = Substitute.For<IProcessRunner>();
            var processOutput = new ProcessOutput[] { new ProcessOutput(adbOutput) };
            var processResult = new ProcessResult(processOutput, 0, 0, true);
            mockProcessRunner.Run(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int[]>()).Returns(processResult);

            var mockSdkTools = Substitute.For<IAndroidSdkTools>();
            mockSdkTools.GetAdbPath().Returns("adb");

            var adbProcessRunner = new AdbProcessRunner(mockProcessRunner, mockSdkTools);
            var actual = new QueryAdbDevices().Execute(adbProcessRunner, mockSdkTools);

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void SingleConnectedDevice()
        {
            string[] expected = { "1234" };
            var adbOutput = $"List of devices attached{Environment.NewLine}1234\tdevice";

            var mockProcessRunner = Substitute.For<IProcessRunner>();
            var processOutput = new ProcessOutput[] { new ProcessOutput(adbOutput) };
            var processResult = new ProcessResult(processOutput, 0, 0, true);
            mockProcessRunner.Run(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int[]>()).Returns(processResult);

            var mockSdkTools = Substitute.For<IAndroidSdkTools>();
            mockSdkTools.GetAdbPath().Returns("adb");

            var adbProcessRunner = new AdbProcessRunner(mockProcessRunner, mockSdkTools);
            var actual = new QueryAdbDevices().Execute(adbProcessRunner, mockSdkTools);

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void DaemonStillNotRunningShouldRetry()
        { 
            string[] expected = { "1234" };

            var adbErrorOutput = "List of devices attached" + Environment.NewLine +
                "* daemon not running. starting it now on port 5037 *" + Environment.NewLine +
                "* daemon started successfully *" + Environment.NewLine +
                "** daemon still not running" + Environment.NewLine +
                "error: cannot connect to daemon: Connection refused";
            var adbCorrectOutput = $"List of devices attached{Environment.NewLine}1234\tdevice";

            var errorProcess = SetupProcessResult(adbErrorOutput, 1);
            var correctOutput = SetupProcessResult(adbCorrectOutput);

            var mockProcessRunner = Substitute.For<IProcessRunner>();
            mockProcessRunner.Run(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int[]>())
                             .Returns(errorProcess,correctOutput);

            var mockSdkTools = Substitute.For<IAndroidSdkTools>();
            mockSdkTools.GetAdbPath().Returns("adb");

            var adbProcessRunner = new AdbProcessRunner(mockProcessRunner, mockSdkTools);
            var actual = new QueryAdbDevices().Execute(adbProcessRunner, mockSdkTools);

            CollectionAssert.AreEqual(expected, actual);
            mockProcessRunner.Received(2).Run("adb", "devices", Arg.Is<int[]>(e => e.Single() == 1));
        }

        [Test]
        public void AddressAlreadyInUseShouldRetry()
        { 
            string[] expected = { "1234" };

            var adbErrorOutput = "error: could not install *smartsocket* listener: Address already in use" + 
                Environment.NewLine +
                "ADB server didn't ACK " + Environment.NewLine + 
                "* failed to start daemon * " + Environment.NewLine +
                "error: cannot connect to daemon" + Environment.NewLine +
                "List of devices attached" + Environment.NewLine +
                "adb server version (32) doesn't match this client (36); killing...";
            var adbCorrectOutput = $"List of devices attached{Environment.NewLine}1234\tdevice";

            var errorProcess = SetupProcessResult(adbErrorOutput, 1);
            var correctOutput = SetupProcessResult(adbCorrectOutput);

            var mockProcessRunner = Substitute.For<IProcessRunner>();
            mockProcessRunner.Run(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int[]>())
                             .Returns(errorProcess, correctOutput);

            var mockSdkTools = Substitute.For<IAndroidSdkTools>();
            mockSdkTools.GetAdbPath().Returns("adb");

            var adbProcessRunner = new AdbProcessRunner(mockProcessRunner, mockSdkTools);
            var actual = new QueryAdbDevices().Execute(adbProcessRunner, mockSdkTools);

            CollectionAssert.AreEqual(expected, actual);
            mockProcessRunner.Received(2).Run("adb", "devices", Arg.Is<int[]>(e => e.Single() == 1));
        }

        ProcessResult SetupProcessResult(string output, int exitCode = 0)
        { 
            var processOutput = new ProcessOutput[] { new ProcessOutput(output) };
            return new ProcessResult(processOutput, exitCode, 0, true);
        }
    }
}
