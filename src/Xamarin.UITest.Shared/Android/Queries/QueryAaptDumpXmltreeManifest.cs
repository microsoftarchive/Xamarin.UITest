using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;
using Xamarin.UITest.Shared.Dependencies;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Processes;
using System;

namespace Xamarin.UITest.Shared.Android.Queries
{
    internal class QueryAaptDumpXmltreeManifest : IQuery<AaptDumpResult, IProcessRunner, IAndroidSdkTools>
    {
        readonly ApkFile _apkFile;

        public QueryAaptDumpXmltreeManifest(ApkFile apkFile)
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
                $"PackageName: {result.PackageName}, Permissions: {result.Permissions}");
        }

        AaptDumpResult GetAaptDumpResult(IProcessRunner processRunner, IAndroidSdkTools androidSdkTools)
        {
            var arguments = $"dump xmltree \"{_apkFile.ApkPath}\" AndroidManifest.xml";
            var result = processRunner.Run(androidSdkTools.GetAaptPath(), arguments);

            var parser = new AndroidXmltreeParser();
            var xmlDocument = parser.GetXml(result.Output);

            var packageName = ExtractPackageName(xmlDocument);
            var permissions = ExtractPermissions(xmlDocument);

            return new AaptDumpResult(packageName, permissions);
        }

        public static string ExtractPackageName(XDocument document)
        {
            var element = document.XPathSelectElement("//manifest");

            if (element == null)
            {
                throw new Exception("Element 'manifest' not found.");
            }

            var attribute = element.Attribute("package");

            if (attribute == null)
            {
                throw new Exception("Attribute 'package' not found on manifesst element.");
            }

            return attribute.Value;
        }

        public static List<string> ExtractPermissions(XDocument document)
        {
            const string androidNamespace = "http://schemas.android.com/apk/res/android";
            return document.Descendants("uses-permission")
                .Select(x => x.Attribute(XName.Get("name", androidNamespace)).Value)
                .Select(x => x.ToString()).ToList();
        }
    }
}