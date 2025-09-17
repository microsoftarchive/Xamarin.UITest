using System.Linq;
using Microsoft.Win32;
using System.Runtime.Versioning;
using Xamarin.UITest.Shared.Processes;

namespace Xamarin.UITest.Shared.Dependencies
{
    public class RegistryReader
    {
        public PotentialLocation[] ReadPotentialJdkPaths()
        {
            if (!Platform.Instance.IsWindows)
            {
                return new PotentialLocation[0];
            }
            else
            {
                return new[] { "SOFTWARE", @"SOFTWARE\Wow6432Node" }
                    .Select(ReadJdkKey)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => new PotentialLocation(x, "Registry"))
                    .ToArray();
            }
        }

        public PotentialLocation[] ReadPotentialAndroidSdkPaths()
        {
            if (!Platform.Instance.IsWindows)
            {
                return new PotentialLocation[0];
            }
            else
            {
                return new[] { "SOFTWARE", @"SOFTWARE\Wow6432Node" }
                    .Select(ReadAndroidSdkKey)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => new PotentialLocation(x, "Registry"))
                    .ToArray();
            }
        }

#if NET6_0_OR_GREATER
        [SupportedOSPlatform("windows")]
#endif
        string ReadAndroidSdkKey(string prefix)
        {
            var androidSdkSubKey = Registry.LocalMachine.OpenSubKey(prefix + @"\Android SDK Tools");

            if (androidSdkSubKey == null)
            {
                return null;
            }

            return androidSdkSubKey.GetValue("Path") as string;
        }

#if NET6_0_OR_GREATER
        [SupportedOSPlatform("windows")]
#endif
        string ReadJdkKey(string prefix)
        {
            var jdkSubKey = Registry.LocalMachine.OpenSubKey(prefix + @"\JavaSoft\Java Development Kit");

            if (jdkSubKey == null)
            {
                return null;
            }

            var currentVersion = jdkSubKey.GetValue("CurrentVersion") as string;

            if (string.IsNullOrEmpty(currentVersion))
            {
                return null;
            }

            var jdkVersionSubKey = jdkSubKey.OpenSubKey(currentVersion);

            if (jdkVersionSubKey == null)
            {
                return null;
            }

            return jdkVersionSubKey.GetValue("JavaHome") as string;
        }


    }
}