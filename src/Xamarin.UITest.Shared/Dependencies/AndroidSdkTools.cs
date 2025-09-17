using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xamarin.UITest.Shared.Extensions;


namespace Xamarin.UITest.Shared.Dependencies
{
    public interface IAndroidSdkTools
    {
        string GetAdbPath();
        string GetAaptPath();
        string GetAndroidJarPath();
        string GetZipAlignPath();
        string GetApkSignerPath();
    }

    public class AndroidSdkTools : IAndroidSdkTools
    {
        static readonly Regex ZigAlignPattern = new Regex(@"^zipalign(\.exe)?$");
        static readonly Regex AaptPattern = new Regex(@"^aapt(\.exe)?$");
        static readonly Regex AdbPattern = new Regex(@"^adb(\.exe)?$");
        static readonly Regex AndroidJarPattern = new Regex(@"^android\.jar$");
        static readonly Regex ApkSignerPattern = new Regex(@"^apksigner(\.exe|\.bat)?$");

        readonly AndroidSdkDependencies _dependencies;

        public AndroidSdkTools(AndroidSdkDependencies dependencies)
        {
            if (dependencies == null)
            {
                throw new ArgumentNullException("dependencies");
            }

            if (!dependencies.IsSatisfied)
            {
                throw new ArgumentException("Android SDK tools require satisfied dependencies.");
            }

            _dependencies = dependencies;
        }

        public static AndroidSdkDependencies BuildAndroidSdkDependencies(PotentialLocation potentialLocation)
        {
            if (potentialLocation == null)
            {
                throw new ArgumentNullException("potentialLocation");
            }

            if (potentialLocation.Path.IsNullOrWhiteSpace())
            {
                return new AndroidSdkDependencies(null, null, null, null, null, null, potentialLocation.Source);
            }

            var directoryInfo = new DirectoryInfo(potentialLocation.Path);

            if (!directoryInfo.Exists)
            {
                return new AndroidSdkDependencies(directoryInfo, null, null, null, null, null, potentialLocation.Source);
            }

            var walker = new DirectoryWalker(2);

            var matches = walker.GetMatches(directoryInfo.FullName, ZigAlignPattern, AaptPattern, AdbPattern, AndroidJarPattern, ApkSignerPattern);
            var matchLookup = matches.ToLookup(x => x.Pattern, x => x);

            var zipAlignPath = GetPath(matchLookup, ZigAlignPattern);
            var aaptPath = GetPath(matchLookup, AaptPattern);
            var adbPath = GetPath(matchLookup, AdbPattern);
            var androidJarPath = GetPath(matchLookup, AndroidJarPattern);
            var apkSignerPath = GetPath(matchLookup, ApkSignerPattern);

            return new AndroidSdkDependencies(directoryInfo, zipAlignPath, aaptPath, adbPath, androidJarPath, apkSignerPath, potentialLocation.Source);
        }

        static string GetPath(ILookup<Regex, WalkerFileMatch> matchLookup, Regex pattern)
        {
            var matches = matchLookup[pattern].ToArray();

            if (!matches.Any())
            {
                return null;
            }

            var versionSelector = new VersionSelector();

            var latest = versionSelector.PickLatest(matches.Select(x => x.RelativePath).ToArray());
            var match = matches.Single(x => x.RelativePath == latest);

            return match.AbsolutePath;
        }

        public string GetAaptPath()
        {
            return _dependencies.AaptPath;
        }

        public string GetAndroidJarPath()
        {
            return _dependencies.AndroidJarPath;
        }

        public string GetZipAlignPath()
        {
            return _dependencies.ZipAlignPath;
        }

        public string GetAdbPath()
        {
            var overrideAdbLocation = Environment.GetEnvironmentVariable("ADB_LOCATION");
            if (!string.IsNullOrWhiteSpace(overrideAdbLocation))
            {
                return overrideAdbLocation;
            }

            return _dependencies.AdbPath;
        }

        public string GetSdkPath()
        {
            return _dependencies.AndroidSdkDirectory.FullName;
        }

        public string GetApkSignerPath()
        {
            return _dependencies.ApkSignerPath;
        }
    }
}
