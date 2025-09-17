using System;
using System.IO;
using NUnit.Framework;
using Should;
using Xamarin.UITest.Shared.Android.Commands;

namespace Xamarin.UITest.Tests
{
    [TestFixture]
    public class CommandApkSignerVerifyTest
    {
        [Test]
        public void CanDetectWronglySignedFiles()
        {
            var apkPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Files", "goto_demoapp.apk");
            try
            {
                var command = new CommandApkSignerVerify(apkPath);
                ExecutorHelper.GetDefault().Execute(command);
                Assert.Fail("Exception was expected");
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("is not correctly signed.");
            }
        }

        [Test]
        public void CanDetectCorrectlySignedFiles()
        {
            var apkPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Files", "goto_demoapp_apksigner.apk");
            var command = new CommandApkSignerVerify(apkPath);
            ExecutorHelper.GetDefault().Execute(command);
            Assert.Pass();
        }
    }
}
