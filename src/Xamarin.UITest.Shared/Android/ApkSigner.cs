using System;
using System.IO;
using Xamarin.UITest.Shared.Android.Commands;
using Xamarin.UITest.Shared.Artifacts;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Hashes;
using Xamarin.UITest.Shared.Logging;

namespace Xamarin.UITest.Shared.Android
{
    public class ApkSigner: ISigner
    {
        readonly HashHelper _hashHelper = new HashHelper();
        readonly IExecutor _executor;

        public ApkSigner(IExecutor executor)
        {
            _executor = executor;
        }

        public ApkFile ResignApk(ArtifactFolder artifactsFolder, string apkFilePath, KeyStore.Credentials credentials)
        {

            Log.Debug($"Trying to resign apk: {apkFilePath} with apksigner.");
            var apkHash = _hashHelper.GetSha256Hash(new FileInfo(apkFilePath));

            var signedApkPath = artifactsFolder.CreateArtifact("final-" + apkHash + ".apk", path =>
                {
                    ApkFile apkFile = new ApkFile(apkFilePath, _executor);
                    SignApk(apkFile, path, credentials);
                }
            );
            return new ApkFile(signedApkPath, _executor);
        }


        public void SignApk(ApkFile apkFile, string targetApkPath, KeyStore.Credentials credentials)
        {
            Log.Debug($"Start signing apk: {apkFile.PackageName} with apksigner");

            string alignedApkPath;
            try
            {
                _executor.Execute(new CommandCheckZipAlign(apkFile));
                alignedApkPath = apkFile.ApkPath;
                Log.Debug("zipalign verification successful");
            }
            catch
            {
                Log.Debug($"Zipalign verification failed. Start zipalign {apkFile.PackageName}");
                FileInfo apkFileInfo = new FileInfo(apkFile.ApkPath);
                var apkFileName = apkFileInfo.Name;
                var alignedFileName = apkFileName.Contains("unsigned-") ? apkFileName.Replace("unsigned-", "aligned-") : apkFileName.Insert(0, "aligned-");

                alignedApkPath = $"{apkFileInfo.DirectoryName}{Path.DirectorySeparatorChar}{alignedFileName}";

                _executor.Execute(new CommandZipAlign(apkFile.ApkPath, alignedApkPath));

            }

            _executor.Execute(new CommandApkSignerSign(alignedApkPath,
                targetApkPath,
                credentials.KeyStoreFile,
                credentials.StorePassword,
                credentials.KeyAlias,
                credentials.KeyPassword));

            Log.Debug($"Signed apk: {apkFile.PackageName} with apksigner.");
        }
    }
}
