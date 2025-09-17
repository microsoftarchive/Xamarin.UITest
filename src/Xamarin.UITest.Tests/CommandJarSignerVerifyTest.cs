using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Should;
using Xamarin.UITest.Shared.Android.Commands;

namespace Xamarin.UITest.Tests
{

    [TestFixture, Ignore("CommandJarSignerVerify is obsolete and no more in usage")]
    class CommandJarSignerVerifyTest
    {
        [Test]
        public void CanDetectWronglySignedFiles()
        {
            var apkPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Files", "goto_demoapp_errorSign.apk");
            try
            {
                var command = new CommandApkSignerVerify(apkPath);
                ExecutorHelper.GetDefault().Execute(command);
                Assert.Fail("Exception was expected");
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("is not correctly signed");
            }
        }

        [Test]
        public void CanDetectCorrectlySignedFiles()
        {
            var apkPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Files", "goto_demoapp.apk");
            var command = new CommandApkSignerVerify(apkPath);
            ExecutorHelper.GetDefault().Execute(command);
            Assert.Pass();
        }
    }
}
