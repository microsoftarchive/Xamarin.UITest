using System.IO;
using Xamarin.UITest.Shared.Dependencies;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Processes;

namespace Xamarin.UITest.Shared.Android.Commands
{
    internal class CommandAaptPackage : ICommand<IProcessRunner, IAndroidSdkTools>
    {
        readonly FileInfo _androidManifestFile;
        readonly string _targetPath;

        public CommandAaptPackage(FileInfo androidManifestFile, string targetPath)
        {
            _androidManifestFile = androidManifestFile;
            _targetPath = targetPath;
        }

        public void Execute(IProcessRunner processRunner, IAndroidSdkTools androidSdkTools)
        {
            var arguments = string.Format("package -M \"{0}\" -I \"{1}\" -F \"{2}\"", _androidManifestFile.FullName, androidSdkTools.GetAndroidJarPath(), _targetPath);
            processRunner.Run(androidSdkTools.GetAaptPath(), arguments);
        }
    }
}