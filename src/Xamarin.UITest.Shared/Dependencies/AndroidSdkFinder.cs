using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml;
using Xamarin.UITest.Shared.Extensions;
using Xamarin.UITest.Shared.Logging;
using Xamarin.UITest.Shared.Processes;
using Xamarin.UITest.Shared.Android;

namespace Xamarin.UITest.Shared.Dependencies
{
    public class AndroidSdkFinder
    {
        readonly RegistryReader _registryReader = new RegistryReader();

        public AndroidSdkTools GetTools()
        {
            if (UITestReplSharedSdkLocation.SharedSdkPathIsSet())
            {
                var sharedSdkPath = UITestReplSharedSdkLocation.GetSharedSdkPathAndReset();
                var potentialLocation = new PotentialLocation(sharedSdkPath, "UITest and REPL shared");
                var dependencies = ResolveDependencies(potentialLocation);

                if (!dependencies.IsSatisfied)
                {
                    throw new Exception($"Android sdk at {dependencies.AndroidSdkDirectory.FullName} is not valid.");
                }

                Log.Debug("Using Android SDK: " + dependencies.AndroidSdkDirectory);
                return new AndroidSdkTools(dependencies);
            }

            var potentialLocations = new List<PotentialLocation>();
            var androidHome = Environment.GetEnvironmentVariable("ANDROID_HOME");
            potentialLocations.Add(new PotentialLocation(androidHome, "ANDROID_HOME"));

            if (Platform.Instance.IsWindows)
            {
                var monoPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Xamarin", "MonoAndroid", "android-sdk-windows");
                if (File.Exists(monoPath))
                {
                    potentialLocations.Add(new PotentialLocation(monoPath, "XS Config file"));
                }
                potentialLocations.AddRange(_registryReader.ReadPotentialAndroidSdkPaths());
            }

            if (Platform.Instance.IsOSXOrUnix)
            {
                var configFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "xbuild", "monodroid-config.xml");
                if (File.Exists(configFile))
                {
                    try
                    {
                        var xsConfigFile = XDocument.Load(configFile);
                        potentialLocations.Add(new PotentialLocation((string)xsConfigFile.Root.Element("android-sdk").Attribute("path"), "XS Config file"));
                    }
                    catch (XmlException)
                    {
                        // malformed config, ignore
                    }
                }

                var userHome = Environment.GetEnvironmentVariable("HOME");

                if (!userHome.IsNullOrWhiteSpace())
                {
                    potentialLocations.Add(new PotentialLocation(Path.Combine(userHome, "Library/Developer/Xamarin/android-sdk-mac_x86"), "Xamarin < 3 install path"));
                    potentialLocations.Add(new PotentialLocation(Path.Combine(userHome, "Library/Developer/Xamarin/android-sdk-macosx"), "Xamarin 3 install path"));
                }

                potentialLocations.Add(new PotentialLocation("/usr/local/Cellar/android-sdk/", "Homebrew install path"));
            }
            var androidSdkDependencies = potentialLocations
                .Select(ResolveDependencies)
                .ToArray();

            foreach (var dependencies in androidSdkDependencies)
            {
                Log.Debug("Potential Android SDK location: " + dependencies);
            }

            var validDependencies = androidSdkDependencies.FirstOrDefault(x => x.IsSatisfied);

            if (validDependencies != null)
            {
                Log.Debug("Using Android SDK: " + validDependencies.AndroidSdkDirectory);
                return new AndroidSdkTools(validDependencies);
            }

            var locations = string.Join(Environment.NewLine, androidSdkDependencies.Select(x => x.ToString()));

            if (Platform.Instance.IsWindows)
            {
                locations = "Windows Registry" + Environment.NewLine + locations;
            }

            throw new Exception(string.Format("Android SDK not found. Please install it and if it is still not located, please set the ANDROID_HOME environment variable to point to the directory.{0}{0}Searched locations: {0}{1}", Environment.NewLine, locations));
        }

        AndroidSdkDependencies ResolveDependencies(PotentialLocation location)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            if (location.Path.IsNullOrWhiteSpace())
            {
                return new AndroidSdkDependencies(null, null, null, null, null, null, location.Source);
            }

            return AndroidSdkTools.BuildAndroidSdkDependencies(location);
        }

    }
}