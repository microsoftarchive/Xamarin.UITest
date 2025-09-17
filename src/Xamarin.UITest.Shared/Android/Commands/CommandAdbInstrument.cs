using System;
using Xamarin.UITest.Shared.Android.Adb;
using Xamarin.UITest.Shared.Execution;

namespace Xamarin.UITest.Shared.Android.Commands
{
    public class CommandAdbInstrument : ICommand<AdbProcessRunner>
    {
        readonly AdbArguments _adbArguments;
        readonly string _appPackageName;
        readonly string _testServerPackageName;
        readonly int _testServerPort;
        readonly string _launchableActivity;

        public CommandAdbInstrument(
            string deviceSerial,
            ApkFile appApkFile,
            ApkFile testServerApkFile,
            int testServerPort,
            string launchableActivity)
            : this(appApkFile, testServerApkFile, testServerPort, launchableActivity)
        {
            _adbArguments = new AdbArguments(deviceSerial);
        }

        public CommandAdbInstrument(
            ApkFile appApkFile,
            ApkFile testServerApkFile,
            int testServerPort,
            string launchableActivity)
            : this(appApkFile.PackageName, testServerApkFile.PackageName, testServerPort, launchableActivity)
        {
            _adbArguments = new AdbArguments(null);
        }

        public CommandAdbInstrument(
            string deviceSerial,
            string appPackageName,
            string testServerPackageName,
            int testServerPort,
            string launchableActivity)
            : this(appPackageName, testServerPackageName, testServerPort, launchableActivity)
        {
            _adbArguments = new AdbArguments(deviceSerial);
        }

        public CommandAdbInstrument(
            string appPackageName,
            string testServerPackageName,
            int testServerPort,
            string launchableActivity)
        {
            _appPackageName = appPackageName;
            _testServerPackageName = testServerPackageName;
            _testServerPort = testServerPort;
            _launchableActivity = launchableActivity;
        }

        public void Execute(AdbProcessRunner processRunner)
        {
            var amArgs = new ActivityManagerIntentArguments()
                .AddData("target_package", _appPackageName)
                .AddData("main_activity", _launchableActivity)
                .AddData("debug", "false")
                .AddData("test_server_port", _testServerPort.ToString())
                .AddData("class", "sh.calaba.instrumentationbackend.InstrumentationBackend");

            var svr = $"{_testServerPackageName}/sh.calaba.instrumentationbackend.CalabashInstrumentationTestRunner";
            var args = _adbArguments.ActivityManagerInstrument(svr, amArgs);
            var output = processRunner.Run(args);

            if (output.Contains("INSTRUMENTATION_FAILED"))
            {
                throw new Exception(string.Format("Unable to start test server, server is most likely not installed. Try uninstalling application and retry. Failed with: {0}", output));
            }

            if (output.Contains("does not have a signature matching the target"))
            {
                throw new Exception("Unable to start test server. The keystore information supplied does not match the currently installed app.");
            }
        }
    }
}