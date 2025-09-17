using System;
using System.IO;
using Xamarin.UITest.Shared.Processes;
using System.Text.RegularExpressions;
using System.Linq;

namespace Xamarin.UITest.iOS
{
    internal class iAppData
    {
        public static readonly string CommandName = "iappdata";
        readonly IProcessRunner _processRunner;
        readonly string _command;

        internal iAppData(IProcessRunner processRunner, string toolPath)
        {
            _processRunner = processRunner;
            _command = Path.Combine(toolPath, CommandName);
        }

        public void ClearData(string deviceIdentifier, string bundleId)
        {
            var lsArgs = string.Format("--udid={0} --app={1} ls Library", deviceIdentifier, bundleId);
            var result = _processRunner.RunCommand(_command, lsArgs, CheckExitCode.AllowAnything);

            var matches = Pattern.Matches(result.Output);

            var toDelete = matches.OfType<Match>().Select(x => x.Groups["name"].Value)
                .Except(new [] {"Preferences"})
                .Select(x => Path.Combine("Library", x))
                .ToArray();

            foreach (var delete in toDelete)
            {
                var rmArgs = string.Format("--udid={0} --app={1} rm {2}", deviceIdentifier, bundleId, delete);
                _processRunner.RunCommand(_command, rmArgs, CheckExitCode.AllowAnything);
            }
        }

        public static Regex Pattern = new Regex(@"^(?<name>.+?)\s+(?:DIR\s+)?\d{4}-\d{2}-\d{2}.*$", RegexOptions.Multiline);
    }
}