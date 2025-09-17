using System.IO;
using System.Linq;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.iOS;
using Xamarin.UITest.Shared.Processes;
using Xamarin.UITest.XDB;
using System;
using Xamarin.UITest.XDB.Services.OSX;

namespace Xamarin.UITest.iOS
{
    class QueryAppHasCalabashLinked : IQuery<LinkStatus, IProcessRunner>
    {
        readonly string _path;

        public QueryAppHasCalabashLinked(string appBundlePath)
        {
            _path = appBundlePath;
        }

        public LinkStatus Execute(IProcessRunner processRunner)
        {
            Lipo lipo = new Lipo(_path);
            LinkStatus status = AppExecutableExists(lipo);

            if (status == LinkStatus.NoExecutable || status == LinkStatus.IncompleteBundleGeneratedByXamarinStudio)
            {
                return status;
            }

            var xcodeVersion = XdbServices.GetRequiredService<IXcodeService>().GetCurrentVersion();

            var result = LinkStatus.NotLinked;

            var skipFiles = new string[] {
                "ABOUT",
                "embedded",
                "LICENSE",
                "NOTICE",
                "PkgInfo",
                "README",
            };

            var skipExtensions = new string[] {
                ".car",
                ".db",
                ".dll",
                ".gif",
                ".html",
                ".jpeg",
                ".jpg",
                ".json",
                ".lproj",
                ".md", 
                ".mdb",
                ".mobileprovision",
                ".mom",
                ".momd",
                ".nib",
                ".omo",
                ".otf",
                ".pdf",
                ".plist", 
                ".png",
                ".rtf",
                ".storyboard",
                ".storyboardc",
                ".strings",
                ".svg",
                ".tiff",
                ".ttf",
                ".txt",
                ".xcent",
                ".xib",
                ".xml", 
                ".yaml", 
                ".yml",

            };

            foreach (var file in Directory.EnumerateFiles(_path))
            {
                var fileInfo = new FileInfo(file);

                if (skipFiles.Contains(fileInfo.Name) || skipExtensions.Contains(fileInfo.Extension))
                {
                    continue;
                }
                
                var linkStatus = FileLinkedWithCalabash(xcodeVersion, processRunner, file);
                if (linkStatus == LinkStatus.Linked)
                {
                    return LinkStatus.Linked;
                }
                if (linkStatus == LinkStatus.CheckFailed)
                {
                    result = LinkStatus.CheckFailed;
                }
            }

            return result;
        }

        LinkStatus FileLinkedWithCalabash(Version xcodeVersion, IProcessRunner processRunner, string path)
        {
            var otool = new OTool(xcodeVersion, processRunner);
            var otoolResult = otool.CheckForExecutable(path);

            if (otoolResult == LinkStatus.ExecutableExists)
            {
                var stringsResult = processRunner.RunCommand(
                    "xcrun",
                    $"strings \"{path}\"",
                    CheckExitCode.AllowAnything
                );

                if (stringsResult.ExitCode != 0)
                {
                    return LinkStatus.CheckFailed;
                }

                return stringsResult.Output.Contains("CALABASH VERSION") ?
                                    LinkStatus.Linked :
                                    LinkStatus.NotLinked;
            }

            return LinkStatus.CheckFailed;
        }

        public LinkStatus AppExecutableExists(Lipo lipo)
        {
            var executablePath = lipo.GetExecutablePath();
            var appBundlePath = lipo.GetAppBundlePath();

            if (!File.Exists(executablePath))
            {
                // If .monotouch-32 and .monotouch-64 exist, then the app was
                // was built for both the i386 _and_ x86_64 arches.  At runtime
                // Xamarin Studio detects this, inspects the target simulator
                // and creates a suitable binary _before_ launching.  We just
                // need to fail with a good warning message.
                var dir32 = Path.Combine(appBundlePath, ".monotouch-32");
                var dir64 = Path.Combine(appBundlePath, ".monotouch-64");

                if (Directory.Exists(dir32) && Directory.Exists(dir64))
                {
                    return LinkStatus.IncompleteBundleGeneratedByXamarinStudio;
                }
                else
                {
                    return LinkStatus.NoExecutable;
                }
            }
            else
            {
                return LinkStatus.ExecutableExists;
            }
        }
    }

    internal enum LinkStatus
    {
        CheckFailed,
        NotLinked,
        Linked,
        NoExecutable,
        IncompleteBundleGeneratedByXamarinStudio,
        ExecutableExists
    }

}