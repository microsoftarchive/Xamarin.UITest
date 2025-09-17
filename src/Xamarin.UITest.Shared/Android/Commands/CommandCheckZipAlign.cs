using System;
using Xamarin.UITest.Shared.Dependencies;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Processes;
using Xamarin.UITest.Shared.Logging;

namespace Xamarin.UITest.Shared.Android.Commands
{
    internal class CommandCheckZipAlign : ICommand<IProcessRunner, IAndroidSdkTools>
    {
        readonly ApkFile _apkFile;

        public CommandCheckZipAlign(ApkFile apkFile)
        {
            _apkFile = apkFile;
        }

        public void Execute(IProcessRunner processRunner, IAndroidSdkTools androidSdkTools)
        {
            string arguments = string.Format("-c -p -v 4 \"{0}\"", _apkFile.ApkPath);
            string errorMessage = string.Format("Apk {0} is not correctly aligned", _apkFile.PackageName);

            try
            {
                ProcessResult result = processRunner.Run(androidSdkTools.GetZipAlignPath(), arguments);
                string output = result.Output;

                if (!output.Contains("Verification succesful"))
                {
                    throw new Exception(errorMessage);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}