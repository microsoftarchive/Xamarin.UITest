using System;
using Xamarin.UITest.Shared.Dependencies;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Processes;

namespace Xamarin.UITest.Shared.Android.Queries
{
    internal class QueryAaptListFiles : IQuery<string[], IProcessRunner, IAndroidSdkTools>
    {
        readonly string _apkFilePath;

        public QueryAaptListFiles(string apkFilePath)
        {
            _apkFilePath = apkFilePath;
        }

        public string[] Execute(IProcessRunner processRunner, IAndroidSdkTools androidSdkTools)
        {
            var arguments = string.Format("list \"{0}\"", _apkFilePath);
            var result = processRunner.Run(androidSdkTools.GetAaptPath(), arguments);

            return result
                .Output
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}