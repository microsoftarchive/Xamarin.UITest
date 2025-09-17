using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Xamarin.UITest.Shared.Processes
{
    internal class ProcessLister
    {
        private readonly IProcessRunner ProcessRunner;
        private readonly IPlatform Platform;

        internal ProcessLister(IProcessRunner processRunner) : this(processRunner, Processes.Platform.Instance)
        {
        }

        internal ProcessLister(IProcessRunner processRunner, IPlatform platform)
        {
            ProcessRunner = processRunner;
            Platform = platform;
        }

        public ProcessInfo[] GetProcessInfos()
        {
            if (Platform.IsOSXOrUnix)
            {
                var ps = ProcessRunner.RunCommand("ps", string.Join(" ", "-xww", "-o pid,user,args"));
                var matches = Regex.Matches(ps.Output, "^\\s*(?<pid>\\d+)\\s(?<user>.+?)\\s(?<args>.+)$", RegexOptions.Multiline);
                return [.. matches.Cast<Match>().Select(m => new ProcessInfo(int.Parse(m.Groups["pid"].Value), m.Groups["args"].Value, m.Groups["user"].Value))];
            }
            if (Platform.IsWindows)
            {
                var ps = ProcessRunner.RunCommand("WMIC", "PATH win32_process GET Commandline, processid /FORMAT:CSV");
                var lines = ps.Output.Split([Environment.NewLine], StringSplitOptions.None);
                var processInfos = from line in lines
                                   let values = line.Split(',')
                                   where values.Count() > 2
                                   where int.TryParse(values[2], out int pid)
                                   select new ProcessInfo(int.Parse(values[2]), values[1]);
                return [.. processInfos];
            }
            throw new Exception("Unsupported platform");
        }
    }
}