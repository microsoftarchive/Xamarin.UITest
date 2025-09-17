using System;
using Xamarin.UITest.Shared.Dependencies;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Processes;

namespace Xamarin.UITest.Shared.Android.Commands
{
    internal class CommandApkSignerVerify: ICommand<IProcessRunner, IAndroidSdkTools>
    {
        readonly string _apkFilePath;

        public CommandApkSignerVerify(string apkFilePath)
        {
            _apkFilePath = apkFilePath;
        }

        public void Execute(IProcessRunner processRunner, IAndroidSdkTools sdkTools)
        { 
            string arguments = string.Format("verify --verbose \"{0}\"", _apkFilePath);

            try
            {
                var result = processRunner.Run(sdkTools.GetApkSignerPath(), arguments);
                if (!result.Output.Contains("Verifies"))
                {
                    throw new Exception(string.Format("File {0} is not correctly signed. Output: {1}", _apkFilePath, result));
                }
            }
            catch(Exception e)
            {
                throw new Exception(string.Format("File {0} is not correctly signed. Error: {1}", _apkFilePath, e.Message));
            }
        }
    }
}
