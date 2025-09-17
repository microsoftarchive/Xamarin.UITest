using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using ICSharpCode.SharpZipLib.Zip;
using Xamarin.UITest.Shared.Resources;
using Xamarin.UITest.Shared.Hashes;
using Xamarin.UITest.Shared.Extensions;

namespace Xamarin.UITest.Repl
{
    public class ReplStarter
    {
        readonly EmbeddedResourceLoader _resourceLoader = new EmbeddedResourceLoader();
        readonly HashHelper _hashHelper = new HashHelper();

        public void RunAndroidRepl(Assembly uiTestAssembly, string deviceUrl, string deviceSerial, string sdkPath)
        {
            var args = $"{PrepareArguments("android", uiTestAssembly, deviceUrl, deviceSerial)} \"{sdkPath}\"";
            RunRepl(args);
        }

        public void RuniOSRepl(Assembly uiTestAssembly, string deviceUrl, string deviceSerial, bool useXDB = false)
        {
            var args = $"{PrepareArguments("ios", uiTestAssembly, deviceUrl, deviceSerial)} {useXDB}";
            RunRepl(args);
        }

        string PrepareArguments(string deviceType, Assembly uiTestAssembly, string deviceUrl, string deviceSerial)
        { 
            string uiTestAssemblyPath = GetUiTestAssemblyPathAndResolvePotentialShadowCopy(uiTestAssembly);
            return $"{deviceType} \"{uiTestAssemblyPath}\" \"{deviceUrl}\" \"{deviceSerial}\"";
        }

        static void UnzipArchive(Stream fileStream, string unzipPath)
        {
            using var archive = new ZipFile(fileStream);
            foreach (ZipEntry entry in archive)
            {
                var entryPath = Path.Combine(unzipPath, entry.Name);
                if (entry.IsDirectory)
                {
                    Directory.CreateDirectory(entryPath);
                    continue;
                }
                using var inputStream = archive.GetInputStream(entry);
                using var outputStream = File.OpenWrite(entryPath);
                inputStream.CopyTo(outputStream);
            }
        }

        void UnzipRepl(string unzipPath)
        {
            using var fileStream = _resourceLoader.GetEmbeddedResourceStream(Assembly.GetExecutingAssembly(), "xut-repl.zip");
            UnzipArchive(fileStream, unzipPath);
        }

        void RunRepl(string replArguments)
        {
            var unzipPath = Path.Combine(Path.GetTempPath(), "uitest", "repl");
            if (Directory.Exists(unzipPath))
            {
                Directory.Delete(unzipPath, true);
            }
            Directory.CreateDirectory(unzipPath);
            UnzipRepl(unzipPath);

            var replFileInfo = new FileInfo(Path.Combine(unzipPath,
#if NET6_0_OR_GREATER
                "Xamarin.UITest.Repl.Console.dll"
#else
                "Xamarin.UITest.Repl.Console.exe"
#endif
            ));


#if DEBUG
            var potentialReplLocations = new List<string>
            {
                // Adding .exe.
                $"../../../Xamarin.UITest.Repl.Console/bin/Debug/net462/Xamarin.UITest.Repl.Console.exe",
                $"../../../../Xamarin.UITest.Repl.Console/bin/Debug/net462/Xamarin.UITest.Repl.Console.exe",
                $"../../../../../Xamarin.UITest/src/Xamarin.UITest.Repl.Console/bin/Debug/net462/Xamarin.UITest.Repl.Console.exe",

                // Adding .dll for .NET 6.
                $"../../../Xamarin.UITest.Repl.Console/bin/Debug/net6.0/Xamarin.UITest.Repl.Console.dll",
                $"../../../../Xamarin.UITest.Repl.Console/bin/Debug/net6.0/Xamarin.UITest.Repl.Console.dll",
                $"../../../../../Xamarin.UITest/src/Xamarin.UITest.Repl.Console/bin/Debug/net6.0/Xamarin.UITest.Repl.Console.dll",

                // Adding .dll for .NET 8.
                $"../../../Xamarin.UITest.Repl.Console/bin/Debug/net8.0/Xamarin.UITest.Repl.Console.dll",
                $"../../../../Xamarin.UITest.Repl.Console/bin/Debug/net8.0/Xamarin.UITest.Repl.Console.dll",
                $"../../../../../Xamarin.UITest/src/Xamarin.UITest.Repl.Console/bin/Debug/net8.0/Xamarin.UITest.Repl.Console.dll"
            };

            if (!Environment.GetEnvironmentVariable("HOME").IsNullOrWhiteSpace())
            {
                // Adding .exe.
                potentialReplLocations.Add(Path.Combine(Environment.GetEnvironmentVariable("HOME"), $"dev/Xamarin.UITest/src/Xamarin.UITest.Repl.Console/bin/Debug/net462/Xamarin.UITest.Repl.Console.exe"));
                potentialReplLocations.Add(Path.Combine(Environment.GetEnvironmentVariable("HOME"), $"src/Xamarin.UITest/src/Xamarin.UITest.Repl.Console/bin/Debug/net462/Xamarin.UITest.Repl.Console.exe"));

                // Adding .dll for .NET 6.
                potentialReplLocations.Add(Path.Combine(Environment.GetEnvironmentVariable("HOME"), $"dev/Xamarin.UITest/src/Xamarin.UITest.Repl.Console/bin/Debug/net6.0/Xamarin.UITest.Repl.Console.dll"));
                potentialReplLocations.Add(Path.Combine(Environment.GetEnvironmentVariable("HOME"), $"src/Xamarin.UITest/src/Xamarin.UITest.Repl.Console/bin/Debug/net6.0/Xamarin.UITest.Repl.Console.dll"));

                // Adding .dll for .NET 8.
                potentialReplLocations.Add(Path.Combine(Environment.GetEnvironmentVariable("HOME"), $"dev/Xamarin.UITest/src/Xamarin.UITest.Repl.Console/bin/Debug/net8.0/Xamarin.UITest.Repl.Console.dll"));
                potentialReplLocations.Add(Path.Combine(Environment.GetEnvironmentVariable("HOME"), $"src/Xamarin.UITest/src/Xamarin.UITest.Repl.Console/bin/Debug/net8.0/Xamarin.UITest.Repl.Console.dll"));
            }

            var existingReplFileInfo = potentialReplLocations
                .Select(x => new FileInfo(x))
                .FirstOrDefault(x => x.Exists);

            if (existingReplFileInfo != null)
            {
                replFileInfo = existingReplFileInfo;
            }
#endif

            var info = new ProcessStartInfo();

            if (Environment.OSVersion.Platform == PlatformID.MacOSX
                || Environment.OSVersion.Platform == PlatformID.Unix)
            {
                if (!File.Exists("/usr/bin/osascript"))
                {
                    throw new Exception("The REPL requires /usr/bin/osascript to run.");
                }

                var appleScript = BuildAppleScript(replFileInfo, replArguments);

                var hash = _hashHelper.GetSha256Hash(appleScript);
                var scriptFileName = Path.Combine(Path.GetTempPath(), string.Format("repl-script-{0}.scpt", hash));

                if (!File.Exists(scriptFileName))
                {
                    File.WriteAllText(scriptFileName, appleScript);
                }

                info.FileName = "/usr/bin/osascript";
                info.Arguments = scriptFileName;
            }
            else
            {
                info.FileName = replFileInfo.FullName;
                info.Arguments = replArguments;
            }

            var process = Process.Start(info);

            process.WaitForExit();
        }

        static string GetUiTestAssemblyPathAndResolvePotentialShadowCopy(Assembly uiTestAssembly)
        {
            var uri = new Uri(uiTestAssembly.Location);

            if (uri.IsFile)
            {
                if (!uri.LocalPath.IsNullOrWhiteSpace() && File.Exists(uri.LocalPath))
                {
                    return uri.LocalPath;
                }
            }

            return uiTestAssembly.Location;
        }

        string BuildAppleScript(FileInfo extractedReplFileInfo, string replArguments)
        {
            var appleScript = _resourceLoader.GetEmbeddedResourceString(Assembly.GetExecutingAssembly(), "StartRepl.AppleScript");
#if NET6_0_OR_GREATER
            var startReplScript = "/usr/local/share/dotnet/dotnet " + String.Format("\"{0}\" {1}", extractedReplFileInfo.FullName, replArguments);
#else
            var startReplScript = "/Library/Frameworks/Mono.framework/Commands/mono " + String.Format("\"{0}\" {1}", extractedReplFileInfo.FullName, replArguments);
#endif
            startReplScript = startReplScript.Replace("\"", "\\\"");

            appleScript = appleScript.Replace("###START-REPL-SCRIPT###", startReplScript);
            return appleScript;
        }
    }
}