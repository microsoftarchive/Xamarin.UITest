using System;
using System.Collections.Generic;

namespace Xamarin.UITest.Shared.Processes
{
    internal interface IProcessRunner
    {
        ProcessResult Run(string path, string arguments = null, IEnumerable<int> noExceptionOnExitCodes = null);
        ProcessResult RunCommand(string command, string arguments = null, CheckExitCode checkExitCode = CheckExitCode.FailIfNotSuccess);
        RunningProcess StartProcess(string path, string arguments = null, Predicate<string> dropFilter = null, int maxNumberOfLines = -1);
    }
}