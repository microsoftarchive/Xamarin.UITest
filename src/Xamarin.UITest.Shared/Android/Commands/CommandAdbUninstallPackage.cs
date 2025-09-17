using System;
using System.Linq;
using Xamarin.UITest.Shared.Android.Adb;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Logging;

namespace Xamarin.UITest.Shared.Android.Commands
{
    public class CommandAdbUninstallPackage : ICommand<AdbProcessRunner>
    {
        readonly AdbArguments _adbArguments;
        readonly ApkFile _apkFile;

        public CommandAdbUninstallPackage(ApkFile apkFile) : this(null, apkFile)
        { }

        public CommandAdbUninstallPackage(string deviceSerial, ApkFile apkFile)
        {
            _adbArguments = new AdbArguments(deviceSerial);
            _apkFile = apkFile;
        }

        public void Execute(AdbProcessRunner processRunner)
        {
            processRunner.Run(_adbArguments.Uninstall(_apkFile.PackageName));
        }
    }
}