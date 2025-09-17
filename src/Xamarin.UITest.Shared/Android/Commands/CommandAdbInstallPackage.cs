using System;
using System.Linq;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Android.Queries;
using Xamarin.UITest.Shared.Android.Adb;

namespace Xamarin.UITest.Shared.Android.Commands
{
    public class CommandAdbInstallPackage : ICommand<AdbProcessRunner, IExecutor>
    {
        readonly AdbArguments _adbArguments;
        readonly IApkFileInformation _apkFile;
        readonly string _deviceSerial;

        public CommandAdbInstallPackage(IApkFileInformation apkFile) : this(string.Empty, apkFile)
        { }

        public CommandAdbInstallPackage(string deviceSerial, IApkFileInformation apkFile)
        {
            _apkFile = apkFile;
            _adbArguments = new AdbArguments(deviceSerial);
            _deviceSerial = deviceSerial;  
        }

        public void Execute(AdbProcessRunner processRunner, IExecutor executor)
        {
            int deviceSdkVersion = executor.Execute(new QueryAdbSdkVersion(_deviceSerial));
            ExecuteInner((args) => processRunner.Run(args), deviceSdkVersion);
        }

        internal void ExecuteInner(Func<string, string> adbShell, int deviceSdkVersion)
        {
            var result = adbShell(_adbArguments.Install(_apkFile.ApkPath, deviceSdkVersion));

            var packagesCommandOuput = adbShell(_adbArguments.PackageManagerList());
            var packages = packagesCommandOuput.Split('\n').Select(s => s.Trim());

            if (!packages.Any(s => s.EndsWith("package:" + _apkFile.PackageName, StringComparison.InvariantCulture)))
            {
                throw new Exception($"App installation failed with output: {result}. Expected Package Name: {_apkFile.PackageName}. Adb Packages Output: {packagesCommandOuput}");
            }

            //set additional permissions that couldn't be set
            if (deviceSdkVersion >= 23)
            {
                adbShell(_adbArguments.EnableMockLocation(_apkFile.PackageName));
            }

            if (deviceSdkVersion >= 30)
            {
                adbShell(_adbArguments.EnableManageExternalStorage(_apkFile.PackageName));
            }
        }
    }
}