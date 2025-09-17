using System.Linq;
using System.Text.RegularExpressions;
using Xamarin.UITest.Shared.Dependencies;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Processes;

namespace Xamarin.UITest.Shared.Android.Queries
{
    internal class QueryKeyStoreMd5Fingerprints : IQuery<string[], IProcessRunner, IJdkTools>
    {
        static readonly Regex FingerprintRegex = new Regex(@"MD5.*:\s*(?<fp>([a-f\d]{2}:){15}[a-f\d]{2})", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        readonly string _keyStorePath;
        readonly string _alias;
        readonly string _password;

        public QueryKeyStoreMd5Fingerprints(string keyStorePath, string alias, string password)
        {
            _keyStorePath = keyStorePath;
            _alias = alias;
            _password = password;
        }

        public string[] Execute(IProcessRunner processRunner, IJdkTools jdkTools)
        {
            var arguments = string.Format("-J-Duser.language=en -list -v -alias {0} -keystore \"{1}\" -storepass {2}", _alias, _keyStorePath, _password);

            var result = processRunner.Run(jdkTools.GetKeyToolPath(), arguments);

            return ExtractMd5Fingerprints(result);
        }

        public static string[] ExtractMd5Fingerprints(ProcessResult processResult)
        {
            MatchCollection matches = FingerprintRegex.Matches(processResult.Output);

            return matches
                .OfType<Match>()
                .Select(x => x.Groups["fp"].Value)
                .ToArray();
        }
    }
}