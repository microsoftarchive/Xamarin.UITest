using System;
using Xamarin.UITest.Shared.Android.Adb;
using Xamarin.UITest.Shared.Execution;

namespace Xamarin.UITest.Shared.Android.Commands
{
    public class CommandAdbWakeUp : ICommand<AdbProcessRunner>
    {
        readonly AdbArguments _adbArguments;
        readonly ApkFile _testServerApkFile;

        public CommandAdbWakeUp(string deviceSerial, ApkFile testServerApkFile)
        {
            _adbArguments = new AdbArguments(deviceSerial);
            _testServerApkFile = testServerApkFile;
        }

        public CommandAdbWakeUp(ApkFile testServerApkFile) : this(null, testServerApkFile)
        { }

        public void Execute(AdbProcessRunner processRunner)
        {
            var amArgs = new ActivityManagerIntentArguments()
                .AddAction("android.intent.action.MAIN")
                .AddComponent($"{_testServerApkFile.PackageName}/sh.calaba.instrumentationbackend.WakeUp");
            
            var output = processRunner.Run(_adbArguments.ActivityManagerStart(amArgs));

            if (output.Contains("Error"))
            {
                throw new Exception(string.Format("Unable to wake device, test server is most likely not installed. Try uninstalling application and retry. Failed with: {0}", output));
            }
        }
    }
}