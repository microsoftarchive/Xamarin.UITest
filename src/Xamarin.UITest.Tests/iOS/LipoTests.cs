using System;
using System.IO;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.UITest.Shared.iOS;
using Xamarin.UITest.Shared.Processes;
using Should;

namespace Xamarin.UITest.Tests.iOS
{
    [TestFixture]
    [Platform(Exclude = "Win32NT")]
    public class LipoTests
    {
        const string Archi386 = "i386";
        const string Archx86_64 = "x86_64";
        const string ArchBothSim= "i386-x86_64";
        const string ArchArm = "arm-v7-v7s-64";
        const string ArchAll = "arm-v7-v7s-64-i386-x86-64";

        bool _isOSX;

        public LipoTests()
        {
            _isOSX = UITest.Shared.Processes.Platform.Instance.IsOSX;
        }

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
            if (_isOSX)
            {
                var cmd = "-convert binary1 -o " + plistPath + " " + xmlPath;
                runner.Run("/usr/bin/plutil", cmd);
            }
            else
            {
                var args = $"-o {plistPath} -i {xmlPath}";
                runner.Run("/usr/bin/plistutil", args);
            }
        }

        string PathToAppBundle(string binaryName)
        {
            string tmpDir = Path.GetTempPath();

            if (!Directory.Exists(tmpDir)) {
                Directory.CreateDirectory(tmpDir);
            }

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

        Lipo LipoFactory(string tempDirectory, string binaryName)
        {
            var appBundle = PathToAppBundle(Path.Combine(tempDirectory, binaryName));

            WriteInfoPlist(appBundle);

            var binaryPath = Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                "..", "..", "..", "..", "..", "binaries", "lipo", binaryName);
            
            if (!File.Exists(binaryPath)) {
                throw new Exception("File does not exist at path " + binaryPath);
            }

            var destination = Path.Combine(appBundle, "MyApp");

            File.Copy(binaryPath, destination);

            return new Lipo(appBundle);
        }

        void DeleteLipoDirectory(string tempDirectory, string binaryName)
        {
            var path = Path.Combine(tempDirectory, binaryName.ToUpper());

            new DirectoryInfo(PathToAppBundle(path)).Delete(true);
        }

        [Test]
        public void LipoOutput()
        {
            if (!_isOSX)
            {
                Assert.Ignore();
            }

            var tempGuid = Guid.NewGuid().ToString("N");
            var lipo = LipoFactory(tempGuid, ArchArm);

            var matches = lipo.GetLipoOutput("-info /usr/bin/xcrun");
            matches.Length.ShouldEqual(1);

            matches[0].ShouldContain("x86_64 arm64e");

            DeleteLipoDirectory(tempGuid, ArchArm);
        }

        [Test]
        public void CFExecutableName()
        {
            var tempGuid = Guid.NewGuid().ToString("N");
            var lipo = LipoFactory(tempGuid, ArchAll);

            var name = lipo.GetExecutableName();
            name.ShouldEqual("MyApp");

            DeleteLipoDirectory(tempGuid, ArchAll);
        }

        [Test]
        public void PathToExecutableBinaryExists()
        {
            var tempGuid = Guid.NewGuid().ToString("N");
            var lipo = LipoFactory(tempGuid, ArchArm);

            var path = lipo.GetExecutablePath();
            var expected = Path.Combine(lipo.GetAppBundlePath(), "MyApp");
            path.ShouldEqual(expected);

            DeleteLipoDirectory(tempGuid, ArchArm);
        }

        [Test]
        public void GetExecutableArches()
        {
            if (!_isOSX)
            {
                Assert.Ignore();
            }

            var tempGuid = Guid.NewGuid().ToString("N");
            var lipo = LipoFactory(tempGuid, ArchAll);

            var expected = new List<string> {
                "i386",
                "armv7",
                "armv7s",
                "x86_64",
                "arm64"
            };
            var actual = lipo.GetAppExecutableArches();

            CollectionAssert.AreEquivalent(expected, actual);

            DeleteLipoDirectory(tempGuid, ArchAll);
        }

        [Test]
        public void HasArchTrue()
        {
            if (!_isOSX)
            {
                Assert.Ignore();
            }

            var testGuid = Guid.NewGuid().ToString("N");

            var lipo_i386 = LipoFactory(testGuid, Archi386);
            var lipo_x86_64 = LipoFactory(testGuid, Archx86_64);
            var lipo_BothSim = LipoFactory(testGuid, ArchBothSim);

            Assert.IsTrue(lipo_i386.HasArch("i386"));
            Assert.IsTrue(lipo_x86_64.HasArch("x86_64"));
            Assert.IsTrue(lipo_BothSim.HasArch("i386"));

            DeleteLipoDirectory(testGuid, Archi386);
            DeleteLipoDirectory(testGuid, Archx86_64);
            DeleteLipoDirectory(testGuid, ArchBothSim);
        }

        [Test]
        public void HasArchFalse()
        {
            if (!_isOSX)
            {
                Assert.Ignore();
            }

            var testGuid = Guid.NewGuid().ToString("N");

            var lipo_i386 = LipoFactory(testGuid, Archi386);
            var lipo_x86_64 = LipoFactory(testGuid, Archx86_64);
            var lipo_BothSim = LipoFactory(testGuid, ArchBothSim);

            Assert.IsFalse(lipo_i386.HasArch("x86_64"));
            Assert.IsFalse(lipo_x86_64.HasArch("i386"));
            Assert.IsFalse(lipo_BothSim.HasArch("arm64"));

            DeleteLipoDirectory(testGuid, Archi386);
            DeleteLipoDirectory(testGuid, Archx86_64);
            DeleteLipoDirectory(testGuid, ArchBothSim);
        }

        [Test]
        public void HasX86_64True()
        {
            if (!_isOSX)
            {
                Assert.Ignore();
            }

            var testGuid = Guid.NewGuid().ToString("N");

            var lipo_x86_64 = LipoFactory(testGuid, Archx86_64);
            var lipo_BothSim = LipoFactory(testGuid, ArchBothSim);

            Assert.IsTrue(lipo_x86_64.HasArchX86_64());
            Assert.IsTrue(lipo_BothSim.HasArchX86_64());
        }

        [Test]
        public void HashX86_64False()
        {
            if (!_isOSX)
            {
                Assert.Ignore();
            }

            var testGuid = Guid.NewGuid().ToString("N");

            var lipo_i386 = LipoFactory(testGuid, Archi386);
            var lipo_Arm = LipoFactory(testGuid, ArchArm);

            Assert.IsFalse(lipo_i386.HasArchX86_64());
            Assert.IsFalse(lipo_Arm.HasArchX86_64());

            DeleteLipoDirectory(testGuid, Archi386);
            DeleteLipoDirectory(testGuid, ArchArm);
        }

        [Test]
        public void HashI386True()
        {
            if (!_isOSX)
            {
                Assert.Ignore();
            }

            var testGuid = Guid.NewGuid().ToString("N");

            var lipo_i386 = LipoFactory(testGuid, Archi386);
            var lipo_BothSim = LipoFactory(testGuid, ArchBothSim);

            Assert.IsTrue(lipo_i386.HasArchI386());
            Assert.IsTrue(lipo_BothSim.HasArchI386());

            DeleteLipoDirectory(testGuid, Archi386);
            DeleteLipoDirectory(testGuid, ArchBothSim);
        }

        [Test]
        public void HashI386False()
        {
            if (!_isOSX)
            {
                Assert.Ignore();
            }

            var testGuid = Guid.NewGuid().ToString("N");

            var lipo_x86_64 = LipoFactory(testGuid, Archx86_64);
            var lipo_Arm = LipoFactory(testGuid, ArchArm);
                
            Assert.IsFalse(lipo_x86_64.HasArchI386());
            Assert.IsFalse(lipo_Arm.HasArchI386());

            DeleteLipoDirectory(testGuid, Archx86_64);
            DeleteLipoDirectory(testGuid, ArchArm);
        }

        [Test]
        public void IsSimulatorBinaryTrue()
        {
            if (!_isOSX)
            {
                Assert.Ignore();
            }

            var testGuid = Guid.NewGuid().ToString("N");

            var lipo_i386 = LipoFactory(testGuid, Archi386);
            var lipo_x86_64 = LipoFactory(testGuid, Archx86_64);
            var lipo_BothSim = LipoFactory(testGuid, ArchBothSim);

            Assert.IsTrue(lipo_i386.IsSimulatorBinary());
            Assert.IsTrue(lipo_x86_64.IsSimulatorBinary());
            Assert.IsTrue(lipo_BothSim.IsSimulatorBinary());

            DeleteLipoDirectory(testGuid, Archi386);
            DeleteLipoDirectory(testGuid, Archx86_64);
            DeleteLipoDirectory(testGuid, ArchBothSim);
        }

        [Test]
        public void IsSimulatorBinaryFalse()
        {
            if (!_isOSX)
            {
                Assert.Ignore();
            }

            var testGuid = Guid.NewGuid().ToString("N");

            var lipo_Arm = LipoFactory(testGuid, ArchArm);

            Assert.IsFalse(lipo_Arm.IsSimulatorBinary());

            DeleteLipoDirectory(testGuid, ArchArm);
        }

        [Test]
        public void HasSingleSimulatorArchTrue()
        {
            if (!_isOSX)
            {
                Assert.Ignore();
            }

            var testGuid = Guid.NewGuid().ToString("N");

            var lipo_i386 = LipoFactory(testGuid, Archi386);
            var lipo_x86_64 = LipoFactory(testGuid, Archx86_64);

            Assert.IsTrue(lipo_i386.HasSingleSimulatorArch());
            Assert.IsTrue(lipo_x86_64.HasSingleSimulatorArch());

            DeleteLipoDirectory(testGuid, Archi386);
            DeleteLipoDirectory(testGuid, Archx86_64);
        }

        [Test]
        public void HasSingleSimulatorArchHasBoth()
        {
            if (!_isOSX)
            {
                Assert.Ignore();
            }

            var testGuid = Guid.NewGuid().ToString("N");

            var lipo_BothSim = LipoFactory(testGuid, ArchBothSim);

            Assert.IsFalse(lipo_BothSim.HasSingleSimulatorArch());

            DeleteLipoDirectory(testGuid, ArchBothSim);
        }

        [Test]
        public void HasSingleSimulatorArchHasNeither()
        {
            if (!_isOSX)
            {
                Assert.Ignore();
            }

            var testGuid = Guid.NewGuid().ToString("N");

            var lipo_Arm = LipoFactory(testGuid, ArchArm);

            Assert.IsFalse(lipo_Arm.HasSingleSimulatorArch());

            DeleteLipoDirectory(testGuid, ArchArm);
        }
    }
}
