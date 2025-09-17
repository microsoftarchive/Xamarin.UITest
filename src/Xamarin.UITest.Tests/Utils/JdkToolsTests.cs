using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Xamarin.UITest.Shared.Dependencies;
using Xamarin.UITest.Shared.Extensions;

namespace Xamarin.UITest.Tests.Utils
{
    [TestFixture]
    public class JdkToolsTests
    {

        [Test]
        public void ValidTools()
        {
            var paths = GetStandardInstallationPathPotentials();
            foreach (string path in paths)
            {
                var directory = new DirectoryInfo(path);
                var jdkTools = new JdkTools(directory);

                if (jdkTools.AreValid())
                {
                    Assert.Pass("Valid jdk tools have been found");
                }
            }
            Assert.Fail("Valid jdk tools have been not found", paths);
        }

        [Test]
        public void NoValidTools()
        {
            var directory = new DirectoryInfo("/");
            var jdkTools = new JdkTools(directory);

            Assert.IsFalse(jdkTools.AreValid());
        }

        IEnumerable<string> GetStandardInstallationPathPotentials()
        {
            var locations = new List<PotentialLocation>();
            var platform = UITest.Shared.Processes.Platform.Instance;
            if (platform.IsWindows)
            {
                locations.AddRange(new RegistryReader().ReadPotentialJdkPaths());
            }
            else if (platform.IsOSX)
            {
                const string defaultInstallPath = "/Library/Java/JavaVirtualMachines/";

                if (Directory.Exists(defaultInstallPath))
                {
                    var jdkVersions = Directory.EnumerateDirectories(defaultInstallPath);
                    var versionSelector = new VersionSelector();
                    var jdkVersion = versionSelector.PickLatest(@"jdk-?(\d+)\.?(\d+)?\.?(\d+)?_?(\d+)?\.jdk", jdkVersions.ToArray());

                    if (jdkVersion.IsNullOrWhiteSpace())
                    {
                        jdkVersion = versionSelector.PickLatest(@"zulu-?(\d+)?\.jdk", jdkVersions.ToArray());
                    }

                    if (jdkVersion.IsNullOrWhiteSpace())
                    {
                        jdkVersion = versionSelector.PickLatest(@"Adopt-?(\d+)?\.jdk", jdkVersions.ToArray());
                    }

                    if (!jdkVersion.IsNullOrWhiteSpace())
                    {
                        locations.Add(new PotentialLocation(Path.Combine(defaultInstallPath, jdkVersion, "Contents", "Home"), "Standard install path"));
                    }
                }
                else
                {
                    throw new Exception("No directory /Library/Java/JavaVirtualMachines/ found");
                }
            }
            else if (platform.IsUnix)
            {
                const string defaultInstallPath = "/usr/lib/jvm/";

                if (Directory.Exists(defaultInstallPath))
                {
                    // Unix paths (examples):
                    // /usr/lib/jvm/default-java
                    // /usr/lib/jvm/java-1.8.0-openjdk-amd64
                    // /usr/lib/jvm/java-8-openjdk-amd64
                    // /usr/lib/jvm/java-8-oracle-amd64
                    // /usr/lib/jvm/jdk1.8.0_11
                    var jdkVersions = Directory.EnumerateDirectories(defaultInstallPath);
                    var versionSelector = new VersionSelector();
                    var jdkVersion = versionSelector.PickLatest(@"jdk-?(\d+)\.?(\d+)?\.?(\d+)?_?(\d+)?", jdkVersions.ToArray());

                    var paths = string.Empty;
                    foreach(var path in jdkVersions)
                    {
                        paths = $"{paths} {path}";
                    }
                    if (!jdkVersion.IsNullOrWhiteSpace())
                    {
                        locations.Add(new PotentialLocation(Path.Combine(defaultInstallPath, jdkVersion), "Standard install path"));
                    }
                }
            }

            if(locations.Count() == 0)
            {
                throw new Exception( "Default installation path not found" );
            }

            return locations.Select(l => l.Path);
        }
    }
}
