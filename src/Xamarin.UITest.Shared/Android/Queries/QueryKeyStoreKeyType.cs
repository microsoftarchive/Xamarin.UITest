using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Processes;
using Xamarin.UITest.Shared.Dependencies;
using System.Text.RegularExpressions;
using System.Linq;
using System;

namespace Xamarin.UITest.Shared.Android.Queries
{
    internal class QueryKeyStoreKeyType : IQuery<KeyType, IProcessRunner, IJdkTools>
	{

        static readonly Regex regex = new Regex(@"Signature\salgorithm\sname:\s*(?<san>(\w*))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        readonly string _keyStorePath;
        readonly string _alias;
        readonly string _password;

        public QueryKeyStoreKeyType(string keyStorePath, string alias, string password)
        {
            _keyStorePath = keyStorePath;
            _alias = alias;
            _password = password;
        }

        public KeyType Execute(IProcessRunner processRunner, IJdkTools jdkTools)
        {
            var arguments = string.Format("-J-Duser.language=en -list -v -alias {0} -keystore \"{1}\" -storepass {2}", _alias, _keyStorePath, _password);

            var result = processRunner.Run(jdkTools.GetKeyToolPath(), arguments);

            return ExtractKeyType(result);
        }

        public static KeyType ExtractKeyType(ProcessResult processResult)
        {
            MatchCollection matches = regex.Matches(processResult.Output);

            var sigAlgo = matches.OfType<Match>().Select(x => x.Groups["san"].Value).Single().ToUpperInvariant();
            return KeyType.FromSigningAlgorithm(sigAlgo);
        }
    }

}