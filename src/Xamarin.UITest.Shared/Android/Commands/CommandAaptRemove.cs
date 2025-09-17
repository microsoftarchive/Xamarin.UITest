using System.IO;
using Xamarin.UITest.Shared.Dependencies;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Processes;

namespace Xamarin.UITest.Shared.Android.Commands
{
    internal class CommandAaptRemove : ICommand<IProcessRunner, IAndroidSdkTools>
    {
        readonly FileInfo _apkFile;
        readonly string[] _paths;

        public CommandAaptRemove(FileInfo apkFile, string[] paths)
        {
            _apkFile = apkFile;
            _paths = paths;
        }

        public void Execute(IProcessRunner processRunner, IAndroidSdkTools androidSdkTools)
        {
            var arguments = string.Format("remove \"{0}\" {1}", _apkFile.FullName, string.Join(" ", _paths));
            processRunner.Run(androidSdkTools.GetAaptPath(), arguments);
        }
    }
}