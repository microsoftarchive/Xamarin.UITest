using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xamarin.UITest.Shared.Extensions;

namespace Xamarin.UITest.Shared.Dependencies
{
    public class AndroidSdkDependencies
    {
        readonly string _source;
        public string ZipAlignPath { get; private set; }
        public string AaptPath { get; private set; }
        public string AdbPath { get; private set; }
        public string AndroidJarPath { get; private set; }
        public string ApkSignerPath { get; private set; }
        public DirectoryInfo AndroidSdkDirectory { get; private set; }

        public AndroidSdkDependencies(DirectoryInfo androidSdkDirectory, string zipAlignPath, string aaptPath, string adbPath, string androidJarPath, string apkSignerPath, string source)
        {
            _source = source;
            ZipAlignPath = zipAlignPath;
            AaptPath = aaptPath;
            AdbPath = adbPath;
            AndroidJarPath = androidJarPath;
            ApkSignerPath = apkSignerPath;
            AndroidSdkDirectory = androidSdkDirectory;
        }

        public bool IsSatisfied
        {
            get { return !new[] { ZipAlignPath, AaptPath, AdbPath, AndroidJarPath, ApkSignerPath }.Any(string.IsNullOrWhiteSpace); }
        }

        public override string ToString()
        {
            if (AndroidSdkDirectory == null)
            {
                return string.Format("(No path) - Not set. [ Source: {0} ]", _source);
            }

            if (!AndroidSdkDirectory.Exists)
            {
                return string.Format("{0} - Does not exist. [ Source: {1} ]", AndroidSdkDirectory.FullName, _source);
            }

            if (IsSatisfied)
            {
                return string.Format("{0} - Valid SDK. [ Source: {1} ]", AndroidSdkDirectory.FullName, _source);
            }

            var resolved = new List<string>();
            var unresolved = new List<string>();

            if (!ZipAlignPath.IsNullOrWhiteSpace())
            {
                resolved.Add("zipalign");
            }
            else
            {
                unresolved.Add("zipalign");
            }

            if (!AaptPath.IsNullOrWhiteSpace())
            {
                resolved.Add("aapt");
            }
            else
            {
                unresolved.Add("aapt");
            }

            if (!AdbPath.IsNullOrWhiteSpace())
            {
                resolved.Add("adb");
            }
            else
            {
                unresolved.Add("adb");
            }

            if (!AndroidJarPath.IsNullOrWhiteSpace())
            {
                resolved.Add("android.jar");
            }
            else
            {
                unresolved.Add("android.jar");
            }

            if (!ApkSignerPath.IsNullOrWhiteSpace())
            {
                resolved.Add("apksigner");
            }
            else
            {
                unresolved.Add("apksigner");
            }

            if (resolved.Any())
            {
                return string.Format("{0} - Partial match. Found: {1} Missing: {2} [ Source: {3} ]", AndroidSdkDirectory.FullName, string.Join(", ", resolved), string.Join(", ", unresolved), _source);
            }

            return string.Format("{0} - Tools not found: {1} [ Source: {2} ]", AndroidSdkDirectory.FullName, string.Join(", ", unresolved), _source);
        }
    }
}