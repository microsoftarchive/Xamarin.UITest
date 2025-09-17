using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.UITest.Shared.Logging;

namespace Xamarin.UITest.Shared.Processes
{
    internal class ProcessRunner : IProcessRunner
    {
        const int ThreadStillRunningExitCode = 259;

        public ProcessResult Run(string path, string arguments = null, IEnumerable<int> noExceptionOnExitCodes = null)
        {
            var output = RunProcessWaitForExit(ProcessRunner.FindCommand(path), arguments);

            if (ProcessRunner.ExitCodeIsUnexpected(output.ExitCode, noExceptionOnExitCodes))
            {
                var message = $"Failed to execute: {path} {arguments} - exit code: {output.ExitCode}{Environment.NewLine}{output.Output}";
                throw new Exception(message);
            }

            Log.Debug(output.ToString());

            return output;
        }

        internal static bool ExitCodeIsUnexpected(int exitCode, IEnumerable<int> noExceptionOnExitCodes = null)
        {
            return exitCode != 0 && (!noExceptionOnExitCodes?.Any(e => e.Equals(exitCode)) ?? true);
        }

        public ProcessResult RunMonoConsoleApp(string path, string arguments = null)
        {
            if (Platform.Instance.IsWindows)
            {
                return Run(path, arguments);
            }

            if (Platform.Instance.IsUnix)
            {
                return RunCommand("mono", string.Format("\"{0}\" {1}", path, arguments));
            }

            if (Platform.Instance.IsOSX)
            {
                return Run("/Library/Frameworks/Mono.framework/Commands/mono", string.Format("\"{0}\" {1}", path, arguments));
            }

            throw new Exception("Unsupported operating system");
        }

        public ProcessResult RunCommand(string command, string arguments = null, CheckExitCode checkExitCode = CheckExitCode.FailIfNotSuccess)
        {
            Log.Debug("Running command.", new { Path = command, Arguments = arguments, Environment.CurrentDirectory });
            var processResult = RunProcessWaitForExit(command, arguments);

            Log.Debug(processResult.Output);

            if (checkExitCode == CheckExitCode.FailIfNotSuccess && processResult.ExitCode != 0)
            {
                throw new Exception(
                    $"Failed to execute: {{{command} {arguments}}}{Environment.NewLine}" +
                    $"Exit code: {processResult.ExitCode}{Environment.NewLine}" +
                    $"{processResult.Output}");
            }

            return processResult;
        }

        /// <summary>
        /// Warning: it's possible that not all output will be captured from the process.  If you can wait for the
        /// process to complete then use <c>ProcessRunner.Run()</c> instead of <c>RunningProcess</c>.
        /// 
        /// See http://alabaxblog.info/2013/06/redirectstandardoutput-beginoutputreadline-pattern-broken/ for more
        /// info, although we have experienced data loss even though we're not using process.WaitForExit(timeout)
        /// </summary>
        /// <param name="path"></param>
        /// <param name="arguments"></param>
        /// <param name="dropFilter"></param>
        /// <param name="maxNumberOfLines"></param>
        /// <returns></returns>
        public RunningProcess StartProcess(string path, string arguments = null, Predicate<string> dropFilter = null, int maxNumberOfLines = -1)
        {
            path = ProcessRunner.FindCommand(path);

            Log.Debug("Starting process.", new { Path = path, Arguments = arguments, CurrentDirectory = Environment.CurrentDirectory });
            return new RunningProcess(path, arguments, dropFilter, maxNumberOfLines);
        }

        internal static string FindCommand(string path)
        {
            if (!File.Exists(path))
            {
                var exePath = Path.ChangeExtension(path, "exe");
                var noExtensionPath = Path.ChangeExtension(path, null);

                if (File.Exists(exePath))
                {
                    path = exePath;
                }
                else if (File.Exists(noExtensionPath))
                {
                    path = noExtensionPath;
                }
                else
                {
                    throw new ArgumentException("File not found: " + path);
                }
            }

            return path;
        }

        ProcessResult RunProcessWaitForExit(string path, string arguments = null)
        {
            var psi = new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            if (arguments != null)
            {
                psi.Arguments = arguments;
            }

            var process = new Process
            {
                StartInfo = psi,
                EnableRaisingEvents = true
            };

            var processOutput = new List<ProcessOutput>();
            var processOutputLock = new object();

            var stopwatch = Stopwatch.StartNew();

            process.Start();

            Task.WaitAll(
                ProcessRunner.ReadOutput(process.StandardOutput, processOutput, processOutputLock, stopwatch),
                ProcessRunner.ReadOutput(process.StandardError, processOutput, processOutputLock, stopwatch));

            process.WaitForExit();

            if (process.ExitCode == ThreadStillRunningExitCode)
            {
                Log.Debug($"Process exited with code {ThreadStillRunningExitCode}, sleeping before checking it again");
                Thread.Sleep(200);
            }
            
            var exitCode = process.ExitCode;
            if (exitCode == ThreadStillRunningExitCode)
            {
                Log.Debug($"Process exit code is still {ThreadStillRunningExitCode}, forcing to 0");
                exitCode = 0;
            }

            ProcessOutput[] processOutputArray;

            lock (processOutputLock)
            {
                processOutputArray = processOutput.ToArray();
            }

            return new ProcessResult(processOutputArray, exitCode, stopwatch.ElapsedMilliseconds, true);
        }

        static async Task ReadOutput(
            StreamReader reader,
            List<ProcessOutput> processOutput,
            object processOutputLock,
            Stopwatch stopwatch)
        {
            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                lock (processOutputLock)
                {
                    processOutput.Add(new ProcessOutput(line));
                }
            }
        }
    }
}
