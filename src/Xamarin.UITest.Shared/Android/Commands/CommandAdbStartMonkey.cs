using System;
using System.Threading;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Xamarin.UITest.Shared.Dependencies;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Processes;
using Xamarin.UITest.Shared.Android.Adb;

namespace Xamarin.UITest.Shared.Android.Commands
{
    internal class CommandAdbStartMonkey : IQuery<int, IProcessRunner, IAndroidSdkTools>
	{
        readonly AdbArguments _adbArguments;
        readonly Random _random = new Random();

        public CommandAdbStartMonkey(string deviceSerial)
        {
            _adbArguments = new AdbArguments(deviceSerial);
        }

        public int Execute(IProcessRunner processRunner, IAndroidSdkTools androidSdkTool)
        {
            KillExistingMonkeyProcess(processRunner, androidSdkTool.GetAdbPath());

            int retries = 0;
            do
            {
                int port = _random.Next(1025, Int16.MaxValue);

                var runningProcess = processRunner.StartProcess(
                    androidSdkTool.GetAdbPath(),
                    _adbArguments.ShellMonkey(port)
                );

                var output = runningProcess.GetOutput();
                if (output.HasExited)
                {
                    if (!output.Output.Contains("Error binding to network socket."))
                    {
                        // The shim will always exit, but on success it will not output the line above
                        return port;
                    }
                    // retry with new port
                    continue;
                }
                Thread.Sleep(500);
                if (!output.HasExited)
                {
                    // Real adb will never terminate on success
                    return port;
                }
            } while(retries++ < 7);
            throw new Exception("Unable to start monkey on device");
        }

        void KillExistingMonkeyProcess(IProcessRunner processRunner, string adbPath)
        {
            // On device
            var command = processRunner.RunCommand(adbPath, _adbArguments.ShellProcessStatus());
            var matches = Regex.Matches(command.Output, @".+?\s(?<pid>[0-9]+).+?com.android.commands.monkey\r?\n?");

            foreach (Match match in matches)
            {
                processRunner.RunCommand(adbPath, _adbArguments.ShellKillProcess(match.Groups["pid"].Value));
            }
            KillOnHost(processRunner, adbPath);
        }

        void KillOnHost(IProcessRunner processRunner, string adbPath)
        {
            var processLister = new ProcessLister(processRunner);
            ProcessInfo[] pi = processLister.GetProcessInfos();
            var commandPrefix = string.Join(" ", adbPath,
             _adbArguments.ShellMonkey(null));

            foreach (var processInfo in pi)
            {
                if (processInfo.CommandLine.StartsWith(commandPrefix))
                {
                    try
                    {
                        Process.GetProcessById(processInfo.PID).Kill();
                    }
                    catch (Exception)
                    {
                        // TODO Handle this exception
                    }
                }
            }
        }
    }
}
