using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Xamarin.UITest.XDB.Services.Processes
{
    /// <summary>
    /// Default implementation of IProcessService. 
    /// </summary>
    class ProcessService : IProcessService
    {
        /// <summary>
        /// Runs the external command.
        /// </summary>
        /// <param name="command">Command to execute.</param>
        /// <param name="arguments">(Optional) Command arguments.</param>
        /// <param name="standardOutputCallback">
        /// (Optional) An action that will be called with each standard output line written by the executed command.
        /// </param>
        /// <param name="standardErrorCallback">
        /// (Optional) An action that will be called with each standard error line wrote by the executed command.
        /// </param>
        public Task<ProcessResult> RunAsync(
            string command, 
            string arguments = null, 
            Action<string> standardOutputCallback = null, 
            Action<string> standardErrorCallback = null)
        {
            if (string.IsNullOrWhiteSpace(command))
                throw new ArgumentNullException(nameof(command));

            var process = StartProcess(command, arguments);

            var standardOutput = new List<Tuple<DateTime, string>>();
            var standardError = new List<Tuple<DateTime, string>>();

            return Task.Run(async () => 
            {
                await Task.WhenAll(
                    ReadOutput(process.StandardOutput, standardOutput, standardOutputCallback), 
                    ReadOutput(process.StandardError, standardError, standardErrorCallback));
                    
                process.WaitForExit();

                var combinedOutput = new List<Tuple<DateTime, string>>();
                combinedOutput.AddRange(standardOutput);
                combinedOutput.AddRange(standardError);
                combinedOutput = combinedOutput.OrderBy(t => t.Item2).ToList();

                return new ProcessResult(
                    process.ExitCode, 
                    string.Join(Environment.NewLine, standardOutput.Select(t => t.Item2)),
                    string.Join(Environment.NewLine, standardError.Select(t => t.Item2)), 
                    string.Join(Environment.NewLine, combinedOutput.Select(t => t.Item2)));
                });
        }

        /// <summary>
        /// Runs the external command.
        /// </summary>
        /// <param name="command">Command to execute.</param>
        /// <param name="arguments">(Optional) Command arguments.</param>
        public ProcessResult Run(string command, string arguments = null) 
        {
            if (string.IsNullOrWhiteSpace(command))
                throw new ArgumentNullException(nameof(command));

            var process = StartProcess(command, arguments);

            var standardOutput = new List<Tuple<DateTime, string>>();
            var standardError = new List<Tuple<DateTime, string>>();

            Task.WaitAll(
                ReadOutput(process.StandardOutput, standardOutput), 
                ReadOutput(process.StandardError, standardError));
                    
            process.WaitForExit();

            var combinedOutput = new List<Tuple<DateTime, string>>();
            combinedOutput.AddRange(standardOutput);
            combinedOutput.AddRange(standardError);
            combinedOutput = combinedOutput.OrderBy(t => t.Item2).ToList();

            return new ProcessResult(
                process.ExitCode, 
                string.Join(Environment.NewLine, standardOutput.Select(t => t.Item2)),
                string.Join(Environment.NewLine, standardError.Select(t => t.Item2)), 
                string.Join(Environment.NewLine, combinedOutput.Select(t => t.Item2)));
        }

        static Process StartProcess(string command, string arguments = null)
        {
            if (string.IsNullOrWhiteSpace(command))
                throw new ArgumentNullException(nameof(command));

            var startInfo = new ProcessStartInfo
            {
                FileName = command,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                Arguments = arguments ?? string.Empty
            };

            var process = new Process() { StartInfo = startInfo };
            process.Start();

            return process;
        }

        static async Task ReadOutput(
            StreamReader streamReader, 
            List<Tuple<DateTime, string>> output,
            Action<string> callback = null)
        {
            string line;
            while ((line = await streamReader.ReadLineAsync()) != null)
            {
                if (callback != null)
                    callback(line);
                
                lock (output)
                {
                    output.Add(new Tuple<DateTime, string>(DateTime.Now, line));
                }
            }
        }
    }
}
