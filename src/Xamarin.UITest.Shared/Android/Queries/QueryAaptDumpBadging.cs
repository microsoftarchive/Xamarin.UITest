using System.Text.RegularExpressions;
using Xamarin.UITest.Shared.Dependencies;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Processes;
using System;
using System.Collections.Generic;

namespace Xamarin.UITest.Shared.Android.Queries
{
    internal class QueryAaptDumpBadging : IQuery<AaptDumpResult, IProcessRunner, IAndroidSdkTools>
    {
        static readonly Regex PackageNameRegex = new Regex(
            @"^package\s*:.*\s+name\='(?<pn>[^']*)'", 
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        static readonly Regex PermissionsRegex = new Regex(
            @"^\s*uses-permission\s*:\s*(name=)?'(?<permission>[^']*)'", 
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        readonly ApkFile _apkFile;

        public QueryAaptDumpBadging(ApkFile apkFile)
        {
            _apkFile = apkFile;
        }

        public AaptDumpResult Execute(IProcessRunner processRunner, IAndroidSdkTools androidSdkTools)
        {
            var result = GetAaptDumpResult(processRunner, androidSdkTools);

            if (result.IsValid)
            {
                return result;
            }

            if (result == null)
            {
                throw new Exception(
                    $"Unable to extract package name and permissions from apk file: {_apkFile.ApkPath}");
            }

            throw new Exception(
                $"Unable to extract package name and permissions from apk file: {_apkFile.ApkPath}, " +
                $"PackageName: {result.PackageName}, Permissions: {string.Join(",", result.Permissions)}");
        }

        AaptDumpResult GetAaptDumpResult(IProcessRunner processRunner, IAndroidSdkTools androidSdkTools)
        {
            var arguments = $"dump badging \"{_apkFile.ApkPath}\"";
            var result = processRunner.Run(androidSdkTools.GetAaptPath(), arguments);

            var packageName = ExtractPackageName(result);
            var permissions = ExtractPermissions(result);
            
            return new AaptDumpResult(packageName, permissions);
        }

        public static string ExtractPackageName(ProcessResult processResult)
        {
            var match = PackageNameRegex.Match(processResult.Output);

            return !match.Success ? null : match.Groups["pn"].Value;
        }

        public static List<string> ExtractPermissions(ProcessResult processResult)
        {
            var permissions = new List<string>();
            var matches = PermissionsRegex.Matches(processResult.Output);

            foreach(Match match in matches)
            {
                permissions.Add(match.Groups["permission"].Value);
            }

            return permissions;
        }
    }
}