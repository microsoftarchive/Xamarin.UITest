using System;
using System.Collections.Generic;
using Xamarin.UITest.Shared.Logging;

namespace Xamarin.UITest.Shared.Android.Adb
{
    internal class AdbArguments
    {
        readonly PackageManagerCommandOptions _pmCommandOptions;
        readonly ShellCommandOptions _shellCommandOptions;

        readonly string _deviceSerial;

        public AdbArguments(string deviceSerial)
        {
            _deviceSerial = deviceSerial;
            _pmCommandOptions = new PackageManagerCommandOptions();
            _shellCommandOptions = new ShellCommandOptions();
        }

        public string Install(string appPath, int sdkLevel)
        {
            //install with the flag -g sets all permissions from the android application manifest
            //except not manually changable permissions
            return sdkLevel >= 23 ?
                PrependSerialNumber($"install -g \"{appPath}\"") :
                PrependSerialNumber($"install \"{appPath}\"");
        }

        public string Shell(ShellCommandOptions args)
        {
            if (args == null)
            {
                throw new ArgumentNullException($"{nameof(ShellCommandOptions)} cannot be null.");
            }

            return Shell($"{args}");
        }

        public string PackageManagerList(PackageManagerCommandOptions args = null)
        {
            args = args ?? _pmCommandOptions.Packages();
            return Shell($"pm list {args}");
        }

        public string EnableMockLocation(string packageName)
        {
            return Shell($"appops set {packageName} 58 allow");
        }

        //for Android API >= 30
        public string EnableManageExternalStorage(string packageName)
        {
            return Shell($"appops set {packageName} MANAGE_EXTERNAL_STORAGE allow");
        }

        public string GetSDKVersionFromProperty()
        {
            return Shell(_shellCommandOptions.PropertByName("ro.build.version.sdk"));
        }

        public string PortForward(int port)
        {
            return PrependSerialNumber($"forward tcp:{port} tcp:{port}");
        }

        public string ActivityManagerInstrument(string packageName, ActivityManagerIntentArguments args = null, string option = null)
        {
            var amOption = string.IsNullOrEmpty(option) ? "" : $" -{option}";
            var amArgs = string.IsNullOrEmpty(args?.ToString()) ? "" : $" {args}";
            return Shell($"am instrument{amOption}{amArgs} {packageName}");
        }

        public string ShellConcatinate(string path, string runAsUser = null)
        {
            return Shell(UseRunAsIfNeeded($"cat \"{path}\"", runAsUser));
        }

        public string ShellList(string path, bool longFormat = false, string runAsUser = null)
        {
            var args = longFormat == false ? "ls" : "ls -l";
            var cmd = UseRunAsIfNeeded($"{args} \"{path}\"", runAsUser);

            return Shell(cmd);
        }

        string UseRunAsIfNeeded(string cmd, string runas)
        {
            if (runas == null)
            {
                return cmd;
            }

            return $"run-as {runas} {cmd}";
        }

        public string ShellKillProcess(string pid)
        {
            return Shell($"kill -9 {pid}");
        }

        public string ShellProcessStatus()
        {
            return Shell("ps");
        }

        public string Devices()
        {
            return "devices";
        }

        public IEnumerable<string> Sha256(string packageName, int sdkLevel)
        {
            string sha256SumArgs = Shell($"sha256sum {packageName}");
            string sha256Args = Shell($"sha256 {packageName}");

            return sdkLevel < 24 ?
                new string[] { sha256Args, sha256SumArgs } :
                new string[] { sha256SumArgs };
        }

        public string ShellMonkey(int? port)
        {
            var args = port == null ?
                "monkey --port " :
                $"monkey --port {port}";

            return Shell(args);
        }

        public string InputServiceInformation()
        {
            return Shell(_shellCommandOptions.DumpSystem("input_method"));
        }

        public string CurrentWindowInformation()
        {
            return Shell(_shellCommandOptions.DumpSystem("window windows"));
        }

        public string ActivityManagerStart(ActivityManagerIntentArguments args)
        {
            return Shell($"am start {args}");
        }

        public string Uninstall(string packageName)
        {
            return PrependSerialNumber($"uninstall \"{packageName}\"");
        }

        string Shell(string command)
        {
            return PrependSerialNumber($"shell {command}");
        }

        string PrependSerialNumber(string command)
        {
            var fullAdbArgs = string.IsNullOrWhiteSpace(_deviceSerial) ?
                                    command :
                                    $"-s {_deviceSerial} {command}";
            Log.Debug($"{nameof(AdbArguments)}: '{fullAdbArgs}'.");
            return fullAdbArgs;
        }
    }
}
