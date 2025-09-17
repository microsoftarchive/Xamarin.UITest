using System;
using System.Linq;
using System.Text;

namespace Xamarin.UITest.Shared.Processes
{
    internal class ProcessResult
    {
        public readonly ProcessOutput[] ProcessOutput;
        public readonly int ExitCode;
        public readonly long ElapsedMilliseconds;
        public readonly bool HasExited;

        internal ProcessResult(ProcessOutput[] processOutput, int exitCode, long elapsedMilliseconds, bool hasExited)
        {
            ProcessOutput = processOutput;
            ExitCode = exitCode;
            ElapsedMilliseconds = elapsedMilliseconds;
            HasExited = hasExited;
        }

        public string Output
        {
            get { return string.Join(Environment.NewLine, ProcessOutput.Select(x => x.Data)); }
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            if (HasExited)
            {
                stringBuilder.AppendFormat("Finished with exit code {2} in {0} ms.{1}", ElapsedMilliseconds, Environment.NewLine, ExitCode);
            }
            else
            {
                stringBuilder.AppendFormat("Process output after {0} ms (still running).{1}", ElapsedMilliseconds, Environment.NewLine);
            }

            foreach (var processOutput in ProcessOutput ?? new ProcessOutput[0])
            {
                if (processOutput != null)
                {
                    stringBuilder.AppendLine(processOutput.Data);
                }
            }

            return stringBuilder.ToString();
        }
    }
}