using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xamarin.UITest.Shared.Extensions;
using Xamarin.UITest.Shared.Logging;
using Xamarin.UITest.Shared.Processes;

namespace Xamarin.UITest.Shared.Dependencies
{
    public class JdkFinder
    {
        readonly DirectoryInfo _directory;
        readonly RegistryReader _registryReader = new RegistryReader();

        public JdkFinder()
        {
            var potentialLocations = new List<PotentialLocation>();

            var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
            potentialLocations.Add(new PotentialLocation(javaHome, "JAVA_HOME"));

            if (Platform.Instance.IsWindows)
            {
                potentialLocations.AddRange(_registryReader.ReadPotentialJdkPaths());
            }

            if (Platform.Instance.IsOSXOrUnix)
            {
                potentialLocations.AddRange(GetStandardInstallationPathPotentials());
                potentialLocations.Add(new PotentialLocation("/usr", "General system-wide binaries"));
            }

            if (Platform.Instance.IsUnix)
            {
                potentialLocations.AddRange(GetUnixStandardInstallationPathPotentials());
            }

            var jdkLocation = potentialLocations.FirstOrDefault(IsJDKDirectory);

            if (jdkLocation == null)
            {
                var locations = string.Join(Environment.NewLine, potentialLocations.Select(x => x.ToString()));

                if (Platform.Instance.IsWindows)
                {
                    locations = "Windows Registry" + Environment.NewLine + locations;
                }

                throw new Exception(string.Format("Java Development Kit (JDK) not found. Please make sure that it is installed and if it's still not located, please set the JAVA_HOME environment variable to point to the directory.{0}{0}Searched locations:{1}", Environment.NewLine, locations));
            }

            Log.Debug("Using JDK: " + jdkLocation);

            _directory = new DirectoryInfo(jdkLocation.Path);
        }

        PotentialLocation[] GetStandardInstallationPathPotentials()
        {
            const string defaultInstallPath = "/Library/Java/JavaVirtualMachines/";
            var locations = new List<PotentialLocation>();

            if (Directory.Exists(defaultInstallPath))
            {
                var jdkVersions = Directory.EnumerateDirectories(defaultInstallPath);
                var versionSelector = new VersionSelector();
                var jdkVersion = versionSelector.PickLatest(@"jdk-?(\d+)\.?(\d+)?\.?(\d+)?_?(\d+)?\.jdk", jdkVersions.ToArray());

                if (jdkVersion.IsNullOrWhiteSpace())
                {
                    jdkVersion = versionSelector.PickLatest(@"zulu-?(\d+)?\.jdk", jdkVersions.ToArray());
                }

                if (!jdkVersion.IsNullOrWhiteSpace())
                {
                    locations.Add(new PotentialLocation(Path.Combine(defaultInstallPath, jdkVersion, "Contents", "Home"), "Standard install path"));
                }
            }

            return locations.ToArray();
        }

        PotentialLocation[] GetUnixStandardInstallationPathPotentials()
        {
            const string defaultInstallPath = "/usr/lib/jvm/";
            var locations = new List<PotentialLocation>();

            if (Directory.Exists(defaultInstallPath))
            {
                var jdkVersions = Directory.EnumerateDirectories(defaultInstallPath);
                var versionSelector = new VersionSelector();
                var jdkVersion = versionSelector.PickLatest(@"jdk-?(\d+)\.?(\d+)?\.?(\d+)?_?(\d+)?", jdkVersions.ToArray());

                if (!jdkVersion.IsNullOrWhiteSpace())
                {
                    locations.Add(new PotentialLocation(Path.Combine(defaultInstallPath, jdkVersion), "Standard install path"));
                }
            }

            return locations.ToArray();
        }

        bool IsJDKDirectory(PotentialLocation location)
        {
            if (location == null || location.Path.IsNullOrWhiteSpace())
            {
                return false;
            }

            return new JdkTools(new DirectoryInfo(location.Path)).AreValid();
        }

        public JdkTools GetTools()
        {
            return new JdkTools(_directory);
        }
    }
}

