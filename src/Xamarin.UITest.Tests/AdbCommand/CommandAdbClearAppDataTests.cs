using NSubstitute;
using NUnit.Framework;
using Xamarin.UITest.Shared.Android.Adb;
using Xamarin.UITest.Shared.Android.Commands;
using Xamarin.UITest.Shared.Dependencies;
using Xamarin.UITest.Shared.Processes;

namespace Xamarin.UITest.Tests.AdbCommand
{
    [TestFixture]
    public class CommandAdbClearAppDataTests
    {
        AdbProcessRunner _adbProcessRunner;
        IProcessRunner _mockProcessRunner;
        IAndroidSdkTools _mockSdkTools;
        CommandAdbClearAppData _commandAdbClearAppData;

        public void SetupMock(string deviceSerial, string package)
        {
            var deviceSerialOrEmtpy = string.IsNullOrWhiteSpace(deviceSerial) ? "" : $"-s {deviceSerial} ";

            _mockProcessRunner = Substitute.For<IProcessRunner>();

            SetupMockForCommandWithResponse(
                $"{deviceSerialOrEmtpy}shell pm list packages",
                "package:packageName\npackage:tspName"
            );

            string report = "INSTRUMENTATION_STATUS: ClearAppData3-status=SUCCESSFUL\n INSTRUMENTATION_STATUS_CODE: -1\n";
            SetupMockForCommandWithResponse(
                $"{deviceSerialOrEmtpy}shell am instrument -w tspName/sh.calaba.instrumentationbackend.ClearAppData3",
                report
            );

            _mockSdkTools = Substitute.For<IAndroidSdkTools>();
            _mockSdkTools.GetAdbPath().Returns("adb");
            _adbProcessRunner = new AdbProcessRunner(_mockProcessRunner, _mockSdkTools);
        }

        [Test]
        public void EnsuresStatusAndResultOfAppDataClearRouteLocally()
        {
            SetupMock("1234", "tspName");

            _commandAdbClearAppData = new CommandAdbClearAppData("1234", "packageName", "tspName");
            _commandAdbClearAppData.Execute(_adbProcessRunner);

            _mockProcessRunner.Received().Run(
                "adb",
                "-s 1234 shell am instrument -w tspName/sh.calaba.instrumentationbackend.ClearAppData3",
                Arg.Any<int[]>()
            );
        }

        void SetupMockForCommandWithResponse(string command, string response)
        {
            var processOutput = new ProcessResult(new[] { new ProcessOutput(response) }, 0, 1, true);
            _mockProcessRunner.Run("adb", command, Arg.Any<int[]>()).Returns(processOutput);
        }
    }
}

