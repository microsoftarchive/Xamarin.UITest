using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Xamarin.UITest.Shared.Processes;

namespace Xamarin.UITest.Shared.iOS
{

    public class Lipo
    {
        private List<string> _appExecutableArches;

        private List<string> AppExecutableArches {
            get {
                if (_appExecutableArches == null) {
                    var path = GetExecutablePath();
                    var output = GetLipoOutput("-info " + path);
                    var firstLine = output[0];

                    // i386, x86_64, arm64, armv7, armv7s
                    var groups = Regex.Matches(firstLine, @"(i386|x86_64|arm(64|v\ds?))");

                    List<String> arches = new List<string>();

                    foreach (Match arch in groups) {
                        arches.Add(arch.ToString());
                    }

                    _appExecutableArches = arches;
                }

                return _appExecutableArches;
            }
        }

        readonly ProcessRunner _processRunner;
        readonly string _appBundlePath;

        public Lipo(string appBundlePath)
        {
            _appBundlePath = appBundlePath;
            _processRunner = new ProcessRunner();
        }

        public string GetAppBundlePath()
        {
            return _appBundlePath;
        }

        public string[] GetLipoOutput(string command)
        {
            var result = _processRunner.Run("/usr/bin/xcrun", "lipo " + command);
            return result.Output.Split(new [] { Environment.NewLine },
                StringSplitOptions.None);
        }

        public string GetExecutableName()
        {
            var infoPlist = Path.Combine(_appBundlePath, "Info.plist");
            return PListHelper.ReadPListValueFromFile(infoPlist, "CFBundleExecutable");
        }

        public string GetExecutablePath()
        {
            var executableName = GetExecutableName();
            return Path.Combine(_appBundlePath, executableName);
        }

        public List<string> GetAppExecutableArches()
        {
            return AppExecutableArches;
        }

        public bool HasArch(string arch)
        {
            return GetAppExecutableArches().Contains(arch);
        }

        public bool HasArchX86_64()
        {
            return HasArch("x86_64");
        }

        public bool HasArchI386()
        {
            return HasArch("i386");
        }

        public bool IsSimulatorBinary()
        {
            return HasArchI386() || HasArchX86_64();
        }

        public bool HasSingleSimulatorArch()
        {
            return HasArchI386() ^ HasArchX86_64();
        }
    }
}
