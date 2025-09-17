using System.IO;
using NUnit.Framework;
using Should;
using Xamarin.UITest.Shared.Android;
using Xamarin.UITest.Shared.Artifacts;

namespace Xamarin.UITest.Tests
{
    [TestFixture]
    public class ApkFileTests
    {
        [Test]
        public void CanExtractFingerprint()
        {
            var apkPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Files", "goto_demoapp.apk");
            var apkFile = new ApkFile(apkPath, ExecutorHelper.GetDefault());

            var artifactFolder = new ArtifactFolder();

            apkFile.GetFingerprints(artifactFolder).ShouldContain("0B:06:A7:41:89:21:D3:FB:59:9B:40:A3:73:A8:28:E6:FC:2B:15:CB:EB:98:B9:E3:9B:3B:C2:57:F8:3E:C1:0A");
        }

        [Test]
        public void CanExtractPackageName()
        {
            var apkPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Files", "goto_demoapp.apk");

            var apkFile = new ApkFile(apkPath, ExecutorHelper.GetDefault());

            apkFile.PackageName.ShouldEqual("com.lesspainful.simpleui");
        }
    }
}
