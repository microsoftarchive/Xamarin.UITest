using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Xamarin.UITest.Shared.Android.Adb;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Logging;
using Xamarin.UITest.Shared.Dependencies;

namespace Xamarin.UITest.Shared.Android.Queries
{
    public class QueryAdbDevices : IQuery<string[], AdbProcessRunner, IAndroidSdkTools>
    {
        static readonly Regex _incombatibleAdbRegex = new Regex(
            "adb server version \\(\\d+\\) doesn't match this client \\(\\d+\\); killing...");

        static readonly Regex _deviceRegex = new Regex(@"^(?<serial>[^\s]+)\t", RegexOptions.Compiled);

        public string[] Execute(AdbProcessRunner processRunner, IAndroidSdkTools sdkTools)
        {
            string[] serials = null;
            var adbArguments = new AdbArguments(null).Devices();
            int maxAttempts = 3;

            for (int r = 0; r < maxAttempts; r++)
            {
                var result = processRunner.Run(adbArguments, new[] { 1 });

                serials = ValidateAndFormatDevicesOutput(result);

                if (serials != null)
                {
                    break;
                }

                Thread.Sleep(TimeSpan.FromSeconds(3 + (r * 2)));
            }

            if (serials == null)
            {
                var result = processRunner.Run(adbArguments, new[] { 1 });

                CheckForAdbVersionError(result, sdkTools);

                serials = ValidateAndFormatDevicesOutput(result);
            }

            if (serials == null)
            {
                var result = processRunner.Run(adbArguments);

                serials = ValidateAndFormatDevicesOutput(result);
            }
            
            return serials;
        }

        string[] ValidateAndFormatDevicesOutput(string adbOutput)
        { 
            var deviceLines = adbOutput
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .Skip(1)
                    .ToArray();

            Log.Debug($"{nameof(QueryAdbDevices)} devices output: ", deviceLines);

            var cannotConnectError = "cannot connect to daemon";

            if (deviceLines.Any() && deviceLines.Last().Contains(cannotConnectError))
            {
                return null;
            }

            var serials = ExtractDeviceSerials(deviceLines);

            if (!serials.Any() && deviceLines.Any(e => e.Contains(cannotConnectError)))
            {
                return null;
            }

            Log.Debug($"{nameof(QueryAdbDevices)} found attached devices: ", serials);

            return serials;
        }

        string[] ExtractDeviceSerials(string[] deviceLines)
        {
            return deviceLines
                .Select(x => _deviceRegex.Match(x))
                .Where(x => x.Success)
                .Select(x => x.Groups["serial"].Value)
                .ToArray();
        }

        void CheckForAdbVersionError(string adbOutput, IAndroidSdkTools sdkTools)
        {
            if (_incombatibleAdbRegex.IsMatch(adbOutput))
            {
                var sdkPath = new System.IO.FileInfo(sdkTools.GetAdbPath()).Directory.Parent.FullName;

                var message =
                    "The running adb server is incompatible with the Android SDK version in use by UITest: " +
                    Environment.NewLine +
                    $"    {sdkPath}" + Environment.NewLine +
                    Environment.NewLine +
                    "You probably have multiple installations of the Android SDK and should update them or ensure " +
                    "that your IDE, simulator and shell all use the same instance.  The ANDROID_HOME environment " +
                    "variable can effect this.";

                Log.Info(message);

                throw new Exception(message);

            }
        }
    }
}