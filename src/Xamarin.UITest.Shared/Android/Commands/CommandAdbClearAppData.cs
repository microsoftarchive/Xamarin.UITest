using Xamarin.UITest.Shared.Execution;
using System;
using System.Linq;
using Xamarin.UITest.Shared.Android.Adb;
using System.Collections.Generic;

namespace Xamarin.UITest.Shared.Android.Commands
{
    public class CommandAdbClearAppData : ICommand<AdbProcessRunner>
    {
        readonly AdbArguments _adbArguments;
        readonly string _apkFilePackageName;
        readonly string _testServerApkPackageName;

        public CommandAdbClearAppData(
            string deviceSerial,
            string apkFilePackageName,
            string testServerApkPackageName
        )
        {
            _adbArguments = new AdbArguments(deviceSerial);
            _apkFilePackageName = apkFilePackageName;
            _testServerApkPackageName = testServerApkPackageName;
        }

        public void Execute(AdbProcessRunner processRunner)
        {
            if (processRunner == null)
                throw new ArgumentNullException(nameof(processRunner));

            var svr = _testServerApkPackageName;

            var installedApps = processRunner.Run(_adbArguments.PackageManagerList()).Split(new[] { '\n' });

            var isAppInstalled = installedApps.Any(s => s.Trim().Equals($"package:{_apkFilePackageName}"));

            var isServerInstalled = installedApps.Any(s => s.Trim().Equals($"package:{svr}"));

            if (!(isServerInstalled && isAppInstalled))
            {
                return;
            }

            ClearData(processRunner);
        }

        void ClearData(AdbProcessRunner processRunner)
        {
            string svr = _testServerApkPackageName;

            var clearAppData3 = $"{svr}/sh.calaba.instrumentationbackend.ClearAppData3";

            var report = processRunner.Run(_adbArguments.ActivityManagerInstrument(clearAppData3, null, "w"));

            if (CalabashSuccessful(report))
            {
                return;
            }

            throw new Exception($"ClearAppData3 failed. Report {report}");
        }

        bool CalabashSuccessful(string report)
        {
            Dictionary<string, string> statusParams = new Dictionary<string, string>();
            foreach (var row in report.Split('\n'))
            {
                string[] entry = row.Split(':');
                if (entry.Length == 2)
                {
                    var key = entry[0].Trim();
                    var value = entry[1].Trim();
                    switch (key)
                    {
                        case "INSTRUMENTATION_STATUS":
                            var statusEntry = value.Split('=');
                            if (statusEntry.Length == 2)
                            {
                                var statusKey = statusEntry[0].Trim();
                                var statusValue = statusEntry[1].Trim();
                                statusParams[statusKey] = statusValue;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            if (statusParams.Count == 0)
            {
                throw new Exception("Clear add data finished with no report.");
            }

            string keyStatus = "ClearAppData3-status";

            string status = statusParams.ContainsKey(keyStatus) ? statusParams[keyStatus] : "UNDEFINED";
            if (!"SUCCESSFUL".Equals(status))
            {
                string keyMessage = "ClearAppData3-message";
                string message = statusParams.ContainsKey(keyMessage) ? statusParams[keyMessage] : "no message";
                throw new Exception($"Clear app data status is {status}. Message: {message}");
            }

            return true;
        }
    }
}