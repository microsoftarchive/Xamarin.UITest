using System.Linq;
using System.Text.RegularExpressions;
using Xamarin.UITest.Shared.Android.Adb;
using Xamarin.UITest.Shared.Dependencies;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Extensions;
using Xamarin.UITest.Shared.Processes;

namespace Xamarin.UITest.Shared.Android.Queries
{
    public class QueryAdbInstalledPackageNames : IQuery<string[], AdbProcessRunner>
    {
        readonly AdbArguments _adbArguments;

        public QueryAdbInstalledPackageNames(string deviceSerial = null)
        {
            _adbArguments = new AdbArguments(deviceSerial);
        }

        public string[] Execute(AdbProcessRunner processRunner)
        {
            var packagePathResult = processRunner.Run(_adbArguments.PackageManagerList());

            var pathMatches = Regex.Matches(packagePathResult, "package:(?<package>.*)");

            return pathMatches.OfType<Match>()
                .Select(x => x.Groups["package"].Value.Trim())
                .ToArray();
        }
    }
}