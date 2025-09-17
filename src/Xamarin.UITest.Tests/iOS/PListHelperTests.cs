using System;
using NUnit.Framework;
using System.IO;
using Xamarin.UITest.Shared.Processes;
using Xamarin.UITest.Shared.iOS;
using Should;

namespace Xamarin.UITest.Tests.iOS
{
    [TestFixture]
    [Platform(Exclude = "Win32NT")]
    public class PListHelperTests
    {
        const string PListXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
 <dict>
  <key>CFBundleExecutable</key>
  <string>MyApp</string>
</dict>
</plist>";
        const string PListDuplicateKeyXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
 <dict>
  <key>CFBundleExecutable</key>
  <string>MyApp</string>
  <key>CFBundleExecutable</key>
  <string>MyOtherApp</string>
</dict>
</plist>";

        const string BundleExecutableKey = "CFBundleExecutable";
        const string BundleExecutableValue = "MyApp";

        [Test]
        public void ReadXmlTest()
        {
            var actualValue = PListHelper.ReadPListValueFromString(PListXml, BundleExecutableKey);
            actualValue.ShouldEqual(BundleExecutableValue);
        }
        [Test]
        public void ReadXmlNonExistantKeyTest()
        {
            var ex = Assert.Throws<Exception>(delegate
            {
                PListHelper.ReadPListValueFromString(PListXml, "NonExistantKey");
            });
            StringAssert.StartsWith("Could not find key: ", ex.Message);
        }


        [Test]
        public void ReadXmlDuplicateKeyTest()
        {
            var ex = Assert.Throws<Exception>(delegate
            {
                PListHelper.ReadPListValueFromString(PListDuplicateKeyXml, BundleExecutableKey);
            });
            StringAssert.StartsWith("Found multiple instances of key: ", ex.Message);
        }

        [Test]
        public void ReadFileTest()
        {
            var tempPlist = WriteTempPlist(PListXml);
            var actualValue = PListHelper.ReadPListValueFromFile(tempPlist, BundleExecutableKey);
            actualValue.ShouldEqual(BundleExecutableValue);
            File.Delete(tempPlist);
        }

        [Test]
        public void ReadFileNonExistantKeyTest()
        {
            var ex = Assert.Throws<Exception>(delegate
            {
                var tempPlist = WriteTempPlist(PListXml);
                try
                {
                    PListHelper.ReadPListValueFromFile(tempPlist, "NonExistantKey");
                }
                catch
                {
                    File.Delete(tempPlist);
                    throw;
                }
            });
            StringAssert.StartsWith("Could not find key: ", ex.Message);
        }

        [Test]
        public void ReadFileNonExistantFileTest()
        {
            var tempDir = Path.GetTempPath();
            var tempGuid = Guid.NewGuid().ToString("N");
            var tempPlist = Path.Combine(tempDir, $"{tempGuid}.plist");

            var exLocal = Assert.Throws<Exception>(delegate
            {
                try
                {
                    PListHelper.ReadPListValueFromFile(tempPlist, BundleExecutableKey);
                }
                catch (Exception ex)
                {
                    File.Delete(tempPlist);
                    if (ex.Message.StartsWith("Failed to execute: /usr/bin/plistutil", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new Exception("file does not exist");
                    }
                    throw;
                }
            });

            StringAssert.Contains("file does not exist", exLocal.Message);
        }

        [Test]
        public void UpdateNonExistantFileTest()
        {
            if (!UITest.Shared.Processes.Platform.Instance.IsOSX)
            {
                Assert.Ignore("Can only set plist values on OSX");
            }

            var tempDir = Path.GetTempPath();

            var tempGuid = Guid.NewGuid().ToString("N");

            var tempPlist = Path.Combine(tempDir, $"{tempGuid}.plist");

            const string setToValue = "MyNewApp";

            PListHelper.SetOrAddPListValueInFile(tempPlist, BundleExecutableKey, "string", setToValue);

            var actualValue = PListHelper.ReadPListValueFromFile(tempPlist, BundleExecutableKey);

            actualValue.ShouldEqual(setToValue);

            File.Delete(tempPlist);
        }

        [Test]
        public void UpdateNonExistingKeyTest()
        {
            if (!UITest.Shared.Processes.Platform.Instance.IsOSX)
            {
                Assert.Ignore("Can only set plist values on OSX");
            }

            var tempPlist = WriteTempPlist(PListXml);

            const string setToKey = "NonExistantKey";
            const string setToValue = "MyNewApp";

            PListHelper.SetOrAddPListValueInFile(tempPlist, setToKey, "string", setToValue);

            var actualValue = PListHelper.ReadPListValueFromFile(tempPlist, setToKey);

            actualValue.ShouldEqual(setToValue);

            File.Delete(tempPlist);
        }

        [Test]
        public void UpdateExistingKeyTest()
        {
            if (!UITest.Shared.Processes.Platform.Instance.IsOSX)
            {
                Assert.Ignore("Can only set plist values on OSX");
            }

            var tempPlist = WriteTempPlist(PListXml);

            const string setToValue = "MyNewApp";

            PListHelper.SetOrAddPListValueInFile(tempPlist, BundleExecutableKey, "string", setToValue);

            var actualValue = PListHelper.ReadPListValueFromFile(tempPlist, BundleExecutableKey);

            actualValue.ShouldEqual(setToValue);

            File.Delete(tempPlist);
        }

        string WriteTempPlist(string plistXml)
        {
            var tempDir = Path.GetTempPath();

            var tempGuid = Guid.NewGuid().ToString("N");

            var xmlPath = Path.Combine(tempDir, $"{tempGuid}.xml");

            File.WriteAllText(xmlPath, plistXml);

            var plistPath = Path.Combine(tempDir, $"{tempGuid}.plist");

            var runner = new ProcessRunner();
            
            if (UITest.Shared.Processes.Platform.Instance.IsOSX)
            {
                var cmd = "-convert binary1 -o " + plistPath + " " + xmlPath;
                runner.Run("/usr/bin/plutil", cmd);
            }
            else
            {
                runner.Run("/usr/bin/plistutil", $"-i {xmlPath} -o {plistPath}");
            }

            File.Delete(xmlPath);

            return plistPath;
        }
    }
}

