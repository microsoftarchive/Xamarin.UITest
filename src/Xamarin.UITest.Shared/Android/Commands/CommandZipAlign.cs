using Xamarin.UITest.Shared.Dependencies;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Processes;

namespace Xamarin.UITest.Shared.Android.Commands
{
    internal class CommandZipAlign : ICommand<IProcessRunner, IAndroidSdkTools>
    {
        readonly string _sourceApkPath;
        readonly string _targetApkPath;

        public CommandZipAlign(string sourceApkPath, string targetApkPath)
        {
            _sourceApkPath = sourceApkPath;
            _targetApkPath = targetApkPath;
        }

        public void Execute(IProcessRunner processRunner, IAndroidSdkTools androidSdkTools)
        {
            string arguments = string.Format("-p -v -f 4 \"{0}\" \"{1}\"", _sourceApkPath, _targetApkPath);
            processRunner.Run(androidSdkTools.GetZipAlignPath(), arguments);
        }
    }
}