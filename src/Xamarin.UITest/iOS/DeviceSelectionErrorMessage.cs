using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Xamarin.UITest.iOS
{
    internal static class DeviceSelectionErrorMessage
    {

        internal static string Generate(string processOutput)
        {
            var selectedDeviceMatch = Regex.Match(processOutput, ".+Unknown (?:hardware )?device specified:\\s+(.+?)\\r*$", RegexOptions.Multiline);
            var selectedDevice = selectedDeviceMatch.Groups[1];
            var availableDeviceList = processOutput.Split(new String[] {Environment.NewLine}, StringSplitOptions.None).Where(l => !l.Contains("Known Devices:")).Where(l=>!l.Contains("device specified"));
            return string.Format("Unknown device: {0}.{2}Available devices: {1}", selectedDevice, string.Join(Environment.NewLine, availableDeviceList), Environment.NewLine);
        }

    }
}