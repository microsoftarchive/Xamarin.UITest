using Xamarin.UITest.Shared.Artifacts;

namespace Xamarin.UITest.Shared.Android
{
    public interface ISigner
    {
        void SignApk(ApkFile apkFile, string targetApkPath, KeyStore.Credentials credentials);
        ApkFile ResignApk(ArtifactFolder artifactFolder, string apkFilePath, KeyStore.Credentials credentials);
    }
}
