using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Xamarin.UITest.Shared.Logging;
using Xamarin.UITest.Shared.Processes;

namespace Xamarin.UITest.iOS
{
    internal class iProxy
    {
        public static readonly string CommandName = "iproxy";
        readonly IProcessRunner _processRunner;
        readonly string _command;

        internal iProxy(IProcessRunner processRunner, string toolPath)
        {
            _processRunner = processRunner;
            _command = Path.Combine(toolPath, CommandName);
        }

        public void StartForward(string deviceIdentifier, int hostPort, int devicePort)
        {
            StopAllForwards(hostPort, _processRunner);
            var process = _processRunner.StartProcess(_command, string.Join(" ", hostPort, devicePort, deviceIdentifier));


            var waitUntil = DateTime.Now.AddSeconds(15);

            while (DateTime.Now < waitUntil)
            {
                var output = process.GetOutput().Output;
                if (output.Contains("forwarded to device port"))
                {
                    break;
                }
                if (output.Contains("CRITICAL"))
                {
                    var message = "Unable to start port forwarding (closing any running simulators may help):\n\n{0}";

                    throw new Exception(string.Format(message, output));
                }
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
            }
        }

        /// <summary>
        /// Searches and stops all port forwards processes.
        /// </summary>
        /// <param name="hostPort">Host's port.</param>
        /// <param name="processRunner"><see cref="IProcessRunner"/> instance.</param>
        static public void StopAllForwards(int hostPort, IProcessRunner processRunner)
        {
            var matches = FindExistingForward(hostPort, processRunner);

            foreach (var processInfo in matches)
            {
                Log.Debug(string.Format("Killing portforwarding process with PID {0}", processInfo.PID));
                Process.GetProcessById(processInfo.PID).Kill();
            }
        }

        /// <summary>
        /// Searches for specified <see cref="iProxy"/> process with port forwarding.
        /// </summary>
        /// <param name="hostPort">Host's port.</param>
        /// <param name="processRunner"><see cref="IProcessRunner"/> instance.</param>
        /// <returns></returns>
        static ProcessInfo[] FindExistingForward(int hostPort, IProcessRunner processRunner)
        {
            var processLister = new ProcessLister(processRunner);
            var processInfos = processLister.GetProcessInfos();
            return processInfos.Where(p => p.CommandLine.Contains(string.Format("iproxy {0}", hostPort))).ToArray();
        }
    }
}