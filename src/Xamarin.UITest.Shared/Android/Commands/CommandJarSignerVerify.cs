using System;
using Xamarin.UITest.Shared.Dependencies;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Processes;

namespace Xamarin.UITest.Shared.Android.Commands
{
    [Obsolete("No support for jarsigner anymore, use CommandApkSignerVerify instead")]
    internal class CommandJarSignerVerify : ICommand<IProcessRunner, IJdkTools>
    {
        readonly string _apkFilePath;

        public CommandJarSignerVerify(string apkFilePath)
        {
            _apkFilePath = apkFilePath;
        }

        public void Execute(IProcessRunner processRunner, IJdkTools jdkTools)
        {
            string arguments = string.Format("-verify \"{0}\"", _apkFilePath);
            string errorMessage = string.Format("File {0} is not correctly signed", _apkFilePath);
            try
            {
                var result = processRunner.Run(jdkTools.GetJarSignerPath(), arguments);
                if (!result.Output.Contains("jar verified."))
                {
                    throw new Exception(errorMessage);
                }
            }
            catch (Exception e)
            {
                string message = e.Message;
                if ((message.Contains("nvalid") && message.Contains("signature file digest")) || message.Contains("digest error for assets/actions") || message.Contains("SHA1 digest error for"))
                {
                    throw new Exception(errorMessage, e);
                }
                throw;
            }
        }
    }
}