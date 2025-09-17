using System.Collections.Generic;
using System.Text.RegularExpressions;
using Xamarin.UITest.Shared.Android.Adb;
using Xamarin.UITest.Shared.Execution;

namespace Xamarin.UITest.Shared.Android.Queries
{
    public class QueryAdbInstalledPackageSha256 : IQuery<string, AdbProcessRunner>
    {
        readonly IEnumerable<string> _sha256Arguments;

        public QueryAdbInstalledPackageSha256(string deviceSerial, InstalledPackage package, int sdkLevel)
        {
            _sha256Arguments = new AdbArguments(deviceSerial).Sha256(package.ApkPath, sdkLevel);
        }

        public string Execute(AdbProcessRunner processRunner)
        {
            foreach (var sha256Arg in _sha256Arguments)
            {
                var sha256SumResult = processRunner.Run(sha256Arg);

                var hashMatch = Regex.Match(sha256SumResult, @"^(?<hash>[a-f0-9]+)\s+");

                if (hashMatch.Success)
                {
                    return hashMatch.Groups["hash"].Value;
                }
            }

            return null;
        }
    }
}
