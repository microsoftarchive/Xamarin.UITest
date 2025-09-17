using Xamarin.UITest.Shared.Dependencies;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Processes;

namespace Xamarin.UITest.Shared.Android.Queries
{
    internal class QueryRsaFileFingerprints : IQuery<string[], IExecutor, IProcessRunner, IJdkTools>
    {
        readonly string _rsaFilePath;

        public QueryRsaFileFingerprints(string rsaFilePath)
        {
            _rsaFilePath = rsaFilePath;
        }

        public string[] Execute(IExecutor executor, IProcessRunner processRunner, IJdkTools jdkTools)
        {
            var arguments = string.Format("-J-Duser.language=en -v -printcert -file \"{0}\"", _rsaFilePath);
            var result = processRunner.Run(jdkTools.GetKeyToolPath(), arguments);

            return executor.Execute(new QueryExtractFingerprints(result.Output));
        }
    }
}
