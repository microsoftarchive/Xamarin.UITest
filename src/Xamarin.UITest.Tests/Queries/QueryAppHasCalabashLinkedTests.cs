using System;
using System.IO;
using NUnit.Framework;
using Should;
using Xamarin.UITest.iOS;
using Xamarin.UITest.Shared.iOS;
using Xamarin.UITest.Shared.Processes;

namespace Xamarin.UITest.Tests.iOS
{
    [TestFixture]
    [Platform(Exclude = "Win32NT")]
    public class QueryAppHasCalabashLinkedTests
    {

        void WriteInfoPlist(string directory)
        {
            const string text = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
 <dict>
  <key>CFBundleExecutable</key>
    <string>MyApp</string>
</dict>
</plist>";

            var xmlPath = Path.Combine(directory, "Info.xml");
            File.WriteAllText(xmlPath, text);

            var plistPath = Path.Combine(directory, "Info.plist");
            ProcessRunner runner = new ProcessRunner();

            if (UITest.Shared.Processes.Platform.Instance.IsOSX)
            {
                var cmd = "-convert binary1 -o " + plistPath + " " + xmlPath;
                runner.Run("/usr/bin/plutil", cmd);
            }
            else
            {
                runner.Run("/usr/bin/plistutil", $"-i {xmlPath} -o {plistPath}");
            }
        }

        string PathToAppBundle(string binaryName)
        {
            string tmpDir = Path.GetTempPath();

            if (!Directory.Exists(tmpDir)) {
                Directory.CreateDirectory(tmpDir);
            }

            var uniqueDir = Guid.NewGuid().ToString();
            tmpDir = Path.Combine(tmpDir, uniqueDir);
            Directory.CreateDirectory(tmpDir);

            // Regex will match i386 2x in this example:
            //
            // $ lipo -info /tmp/T/arm-v7-v7s-64-i386-x86-64/MyApp.app/MyApp
            // Architectures in the fat file: /tmp/T/arm-v7-v7s-64-i386-x86-64/MyApp.app/MyApp are: i386 x86_64 armv7 armv7s arm64
            //
            // Use ToUpper to avoid the extra match.
            //
            // I don't think the Regex needs to be changed; there is no case
            // where Xamarin Studio spits out binary with the arch name in the
            // path.
            var appDir = Path.Combine(tmpDir, binaryName.ToUpper(), "MyApp.app");

            if (Directory.Exists(appDir)) {
                Directory.Delete(appDir, true);
            }

            Directory.CreateDirectory(appDir);
            return appDir;
        }

        Lipo LipoFactory(string binaryName)
        {
            var appBundle = PathToAppBundle(binaryName);
            WriteInfoPlist(appBundle);

            if (binaryName.Equals("no-executable"))
            {
                // do nothing
            }
            else if (binaryName.Equals("incomplete-bundle"))
            {
                var monotouch32 = Path.Combine(appBundle, ".monotouch-32");
                Directory.CreateDirectory(monotouch32);

                var monotouch64 = Path.Combine(appBundle, ".monotouch-64");
                Directory.CreateDirectory(monotouch64);
            }
            else
            {
                var binaryPath = Path.Combine(
                    TestContext.CurrentContext.TestDirectory,
                    "..", "..", "..", "..", "binaries", "lipo", binaryName);
                
                if (!File.Exists(binaryPath)) {
                    throw new Exception("File does not exist at path " + binaryPath);
                }
                var destination = Path.Combine(appBundle, "MyApp");

                File.Copy(binaryPath, destination);
            }

            return new Lipo(appBundle);
        }

        [Test]
        public void AppExecutableExists_ExecutableExists()
        {
            Lipo lipo = LipoFactory("i386-x86_64");
            var appBundlePath = lipo.GetAppBundlePath();
            QueryAppHasCalabashLinked testObject;
            testObject = new QueryAppHasCalabashLinked(appBundlePath);

            LinkStatus status = testObject.AppExecutableExists(lipo);
            status.ShouldEqual(LinkStatus.ExecutableExists);
        }

        [Test]
        public void AppExecutableExists_ExecutableDoesNotExist()
        {
            Lipo lipo = LipoFactory("no-executable");
            var appBundlePath = lipo.GetAppBundlePath();
            QueryAppHasCalabashLinked testObject;
            testObject = new QueryAppHasCalabashLinked(appBundlePath);

            LinkStatus status = testObject.AppExecutableExists(lipo);
            status.ShouldEqual(LinkStatus.NoExecutable);
        }

        [Test]
        public void AppExecutableExists_IncompleteBundle()
        {
            Lipo lipo = LipoFactory("incomplete-bundle");
            var appBundlePath = lipo.GetAppBundlePath();
            QueryAppHasCalabashLinked testObject;
            testObject = new QueryAppHasCalabashLinked(appBundlePath);

            LinkStatus status = testObject.AppExecutableExists(lipo);
            status.ShouldEqual(LinkStatus.IncompleteBundleGeneratedByXamarinStudio);
        }
    }
}
