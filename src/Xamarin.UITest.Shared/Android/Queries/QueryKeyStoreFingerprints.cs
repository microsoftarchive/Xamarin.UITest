using Xamarin.UITest.Shared.Dependencies;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Processes;

namespace Xamarin.UITest.Shared.Android.Queries
{
    internal class QueryKeyStoreFingerprints : IQuery<string[], IExecutor, IProcessRunner, IJdkTools>
    {
        readonly string _keyStorePath;
        readonly string _alias;
        readonly string _password;

        public QueryKeyStoreFingerprints(string keyStorePath, string alias, string password)
        {
            _keyStorePath = keyStorePath;
            _alias = alias;
            _password = password;
        }

        public string[] Execute(IExecutor executor, IProcessRunner processRunner, IJdkTools jdkTools)
        {
            var arguments = string.Format("-J-Duser.language=en -list -v -alias {0} -keystore \"{1}\" -storepass {2}", _alias, _keyStorePath, _password);
            var result = processRunner.Run(jdkTools.GetKeyToolPath(), arguments);

            return executor.Execute(new QueryExtractFingerprints(result.Output));
        }
    }
}
