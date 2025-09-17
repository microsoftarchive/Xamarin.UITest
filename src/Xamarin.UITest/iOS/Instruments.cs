using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Linq;
using Xamarin.UITest.Shared.Artifacts;
using Xamarin.UITest.Shared.Logging;
using Xamarin.UITest.Shared.Processes;
using Xamarin.UITest.Shared.Resources;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Xamarin.UITest.iOS
{
    internal class Instruments
    {
        class Simulator
        {

            [JsonProperty("state")]
            public string State { get; set; }

            [JsonProperty("availability")]
            public string Availability { get; set; }

            [JsonProperty("isAvailable")]
            public string IsAvailable { get; set; }

            [JsonProperty("availabilityError")]
            public string AvailabilityError { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("udid")]
            public string Udid { get; set; }
        }

        class SimCtlSimulators
        {

            [JsonProperty("devices")]
            public Dictionary<string, List<Simulator>> Simulators { get; set; }
        }

        Dictionary<Version, List<Simulator>> _SimCtlSimulators;

        readonly ArtifactFolder _artifactFolder;
        readonly EmbeddedResourceLoader _resourceLoader;
        readonly ProcessRunner _processRunner;
        readonly iDeviceTools _iTools;
        readonly Regex _instrumentsCommand = new Regex(".+instruments\\s");
        readonly Regex _simVersionRegex = new Regex("(\\d+(-\\d){1,2}$)");

        static bool _xcodeSimQueriesAreSafe;

        public Instruments(ArtifactFolder artifactFolder, EmbeddedResourceLoader resourceLoader, iDeviceTools iTools)
        {
            _artifactFolder = artifactFolder;
            _resourceLoader = resourceLoader;
            _processRunner = new ProcessRunner();
            _iTools = iTools;
        }

        Dictionary<Version, List<Simulator>> GetSimctlSimulators()
        {
            if (_SimCtlSimulators != null)
            {
                return _SimCtlSimulators;
            }

            EnsureXcodeSimQueriesAreSafe();

            var cmd = "/usr/bin/xcrun";
            var args = "simctl list devices --json";

            var simctlResult = _processRunner.RunCommand(cmd, args, CheckExitCode.AllowAnything);

            if (simctlResult.ExitCode != 0)
            {
                throw new Exception(string.Concat(
                    cmd, " ", args, Environment.NewLine,
                    "Exit Code: ", simctlResult.ExitCode, Environment.NewLine,
                    simctlResult.Output));
            }

            var simCtlSims = JsonConvert.DeserializeObject<SimCtlSimulators>(simctlResult.Output);

            var iosSims = simCtlSims.Simulators
                .Where(s => (s.Key.Contains(" ") && s.Key.Contains("iOS")) || s.Key.Contains(".iOS-"))
                .ToList();

            Version SimVersion(string simName)
            {
                return new Version(
                    simName.Contains(" ")
                        ? simName.Split(' ').Last()
                        : _simVersionRegex.Match(simName).Captures[0].Value.Replace("-", ".")
                    );
            }

            _SimCtlSimulators = iosSims.ToDictionary(s => SimVersion(s.Key), s => s.Value);

            return _SimCtlSimulators;
        }

        internal Version GetDefaultIOSVersion()
        {
            var cmd = "/usr/bin/xcrun";
            var args = "--sdk iphoneos --show-sdk-platform-version";

            var xcrunResult = _processRunner.RunCommand(cmd, args, CheckExitCode.AllowAnything);

            if (xcrunResult.ExitCode != 0)
            {
                throw new Exception(string.Concat(
                    cmd, " ", args, Environment.NewLine,
                    "Exit Code: ", xcrunResult.ExitCode, Environment.NewLine,
                    xcrunResult.Output));
            }

            if (Version.TryParse(xcrunResult.Output, out Version version))
            {
                return version;
            }

            Log.Debug($"Failed to parse iOS Version from string '{xcrunResult.Output}'");
            return null;
        }

        internal string GetDefaultSimDeviceIdentifier(int xcodeVersion)
        {
            var cachedDeviceIdentifyer = _artifactFolder.CreateArtifact(
                "defaultSimDeviceIdentierForInstumentsBuild-Xcode" + xcodeVersion, path =>
            {
                var iosSims = GetSimctlSimulators();
                var iosVersion = GetDefaultIOSVersion();
                if (iosVersion == null || !iosSims.ContainsKey(iosVersion))
                {
                    iosVersion = iosSims.Keys.Max();
                    Log.Debug($"Failed to determine default iOS Version for current Xcode. Version '{iosSims.Keys.Max().ToString()}' will be used");
                }

                // This logic is  still not right - the `Max()` might be listed but not  available.
                var availableSims = iosSims[iosVersion]
                    .Where(s => (s.Availability == "(available)" || s.IsAvailable == "YES" || s.IsAvailable == "true"))
                    .ToList();

                if (availableSims.Any())
                {
                    var iPhoneSims = availableSims.Where(
                        s => s.Name.StartsWith("iPhone", StringComparison.Ordinal)
                    ).ToList();

                    var sims = iPhoneSims.Any() ? iPhoneSims : availableSims;

                    File.WriteAllText(path, sims.OrderByDescending(s => s.Name).First().Udid);
                }
                else
                {
                    throw new Exception("Unable to identify default simulator. `ConfigureApp.iOS.DeviceIdentifier()` will avoid this issue.");
                }
            });

            return File.ReadAllText(cachedDeviceIdentifyer);
        }

        internal void QuitSimulator()
        {
            var quitSimScript = _artifactFolder.CreateArtifact("QuitSimulator.AppleScript", path =>
            {
                File.WriteAllText(
                    path,
                    _resourceLoader.GetEmbeddedResourceString(
                        Assembly.GetExecutingAssembly(),
                        "QuitSimulator.AppleScript"));
            });

            _processRunner.Run("/usr/bin/osascript", quitSimScript);
        }

        /// <summary>
        /// Having switched Xcode versions, the first few calls to `simctl` or `instruments` will fail.
        /// To work around this, we call `simctl help` until no message containing `CoreSimulator` is 
        /// written to stderr.  The issue may be a problem with the Xcode 8 betas and this code may
        /// become redundant with a later Xcode release.
        /// </summary>
        void EnsureXcodeSimQueriesAreSafe()
        {
            if (_xcodeSimQueriesAreSafe)
            {
                return;
            }

            var max_tries = 30;
            for (var i = 1; i <= max_tries; i++)
            {
                var simctlCommand = _processRunner.RunCommand(
                    "/usr/bin/xcrun", "simctl help 2>&1", CheckExitCode.AllowAnything);

                if (!simctlCommand.Output.Contains("CoreSimulator"))
                {
                    break;
                }

                Log.Debug(
                    "Invalid CoreSimulator service for active Xcode:" +
                    $"try {i} of {max_tries}{Environment.NewLine}{simctlCommand.Output}");

                Thread.Sleep(500);
            }

            _xcodeSimQueriesAreSafe = true;
        }

        public void EnsureNoOthersRunning(string deviceIdentifier)
        {
            var sims = GetSimctlSimulators();
            foreach (var version in sims.Keys)
            {
                foreach (var sim in sims[version])
                {
                    if (sim.State == "Booted" && sim.Udid != deviceIdentifier)
                    {
                        QuitSimulator();
                        return;
                    }
                }
            }
        }
    }
}