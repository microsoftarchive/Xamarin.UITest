using System.Text.RegularExpressions;
using Xamarin.UITest.Shared.Execution;
using System.Collections.Generic;
using Xamarin.UITest.Shared.Android.Adb;

namespace Xamarin.UITest.Shared.Android.Queries
{
    public class QueryAdbInstalledPackages : IQuery<InstalledPackage[], AdbProcessRunner>
    {
        AdbArguments _adbArguments;
        PackageManagerCommandOptions _packageManagerArgs;

        public QueryAdbInstalledPackages(string deviceSerial)
        {
            _adbArguments = new AdbArguments(deviceSerial);
            _packageManagerArgs = new PackageManagerCommandOptions();
        }

        public InstalledPackage[] Execute(AdbProcessRunner processRunner)
        {
            var listPackageArgs = _packageManagerArgs.Packages(
                PackagesOption.SeeAssociatedFiles, 
                PackagesOption.ShowEnabledOnly
            );

            var packagePathResult = processRunner.Run(
                _adbArguments.PackageManagerList(listPackageArgs)
            );

            if (packagePathResult.StartsWith("Error: Unknown option: -e"))
            {
                listPackageArgs = _packageManagerArgs.Packages(PackagesOption.SeeAssociatedFiles);
                packagePathResult = processRunner.Run(
                    _adbArguments.PackageManagerList(listPackageArgs)
                );
            }

            var pathMatches = Regex.Matches(packagePathResult, "package:(?<path>.*)=(?<package>.*)");

            var packages = new List<InstalledPackage>();

            foreach (Match pathMatch in pathMatches)
            {
                string apkPath = pathMatch.Groups["path"].Value.Trim();
                string package = pathMatch.Groups["package"].Value.Trim();

                packages.Add(new InstalledPackage(package, apkPath));
            }

            return packages.ToArray();
        }
    }
}