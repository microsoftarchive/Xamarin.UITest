using Xamarin.UITest.Shared.Android.Adb;
using Xamarin.UITest.Shared.Execution;

namespace Xamarin.UITest.Shared.Android.Queries
{
    public class QueryAdbInstalledPackageLsLong : IQuery<string, AdbProcessRunner>
    {
        readonly AdbArguments _adbArguments;
        readonly InstalledPackage _package;

        public QueryAdbInstalledPackageLsLong(string deviceSerial, InstalledPackage package)
        {
            _package = package;
            _adbArguments = new AdbArguments(deviceSerial);
        }

        public string Execute(AdbProcessRunner processRunner)
        {
            var args = _adbArguments.ShellList(_package.ApkPath, true);
            var result = processRunner.Run(args);
            var output = result.Trim();
            return output;
        }
    }
}