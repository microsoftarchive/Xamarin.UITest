using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Xamarin.UITest.Shared.Artifacts;
using Xamarin.UITest.Shared.Resources;
using Xamarin.UITest.XDB.Exceptions;
using Xamarin.UITest.XDB.Services.Processes;

namespace Xamarin.UITest.XDB.Services.OSX
{
    class XcodeService : IXcodeService
    {
        Lazy<Version> _currentVersion;
        IProcessService _processService;

        public XcodeService(IProcessService processService)
        {
            _processService = processService;
            _currentVersion = new Lazy<Version>(() => {
                var regexResults = Regex.Match(
                    _processService.Run("xcrun", "xcodebuild -version").StandardOutput,
                    "Xcode\\s(.*)"
                );
                if (regexResults.Groups.Count != 2)
                {
                    throw new ExternalProcessException("xcodebuild output not as expected");
                }
                try
                {
                    return new Version(regexResults.Groups[1].Value);
                }
                catch
                {
                    throw new ExternalProcessException("xcodebuild output not as expected");
                }
            });
        }

        public Version GetCurrentVersion()
        {
            return _currentVersion.Value;
        }

        public ProcessResult TestWithoutBuilding(string deviceId, string xctestrunPath, string derivedDataPath)
        {
            var cmd = "xcodebuild test-without-building";
            var args = $"-xctestrun {xctestrunPath} -destination id={deviceId} -derivedDataPath {derivedDataPath}";

            return _processService.Run("xcrun", $"{cmd} {args}");
        }
    }
}