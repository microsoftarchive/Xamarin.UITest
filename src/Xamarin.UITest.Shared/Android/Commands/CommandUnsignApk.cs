using System;
using System.IO;
using System.Linq;
using Xamarin.UITest.Shared.Android.Queries;
using Xamarin.UITest.Shared.Dependencies;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Extensions;
using Xamarin.UITest.Shared.Processes;

namespace Xamarin.UITest.Shared.Android.Commands
{
    [Obsolete("Apksigner doesn't need to unsign apk before resigning it with a different keystore")]
    internal class CommandUnsignApk : ICommand<IProcessRunner, IAndroidSdkTools, IExecutor>
    {
        readonly string _sourceApkPath;
        readonly string _targetApkPath;

        public CommandUnsignApk(string sourceApkPath, string targetApkPath)
        {
            _sourceApkPath = sourceApkPath;
            _targetApkPath = targetApkPath;
        }

        public void Execute(IProcessRunner processRunner, IAndroidSdkTools androidSdkTools, IExecutor executor)
        {
            File.Copy(_sourceApkPath, _targetApkPath);

            var fileList = executor.Execute(new QueryAaptListFiles(_targetApkPath));

            var filesToRemove = fileList
                .Where(IsSigningFile)
                .ToArray();

            if (!filesToRemove.Any())
            {
                return;
            }

            executor.Execute(new CommandAaptRemove(new FileInfo(_targetApkPath), filesToRemove));
        }

        bool IsSigningFile(string fileName)
        {
            return fileName.StartsWithIgnoreCase("META-INF") 
                && (fileName.EndsWithIgnoreCase(".MF")
                    || fileName.EndsWithIgnoreCase(".RSA") 
                    || fileName.EndsWithIgnoreCase(".DSA") 
                    || fileName.EndsWithIgnoreCase(".EC") 
                    || fileName.EndsWithIgnoreCase(".SF"));
        }
    }
}