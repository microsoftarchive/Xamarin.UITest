using System;
using Xamarin.UITest.Shared.Processes;

namespace Xamarin.UITest.iOS
{
    class OTool
    {
        IProcessRunner _processRunner;
        Version _xcodeVersion;

        public OTool(Version xcodeVersion, IProcessRunner processRunner)
        {
            _processRunner = processRunner;
            _xcodeVersion = xcodeVersion;
        }

        public LinkStatus CheckForExecutable(string path)
        {
            var otoolCommand = _xcodeVersion.Major >= 8 ?
                                            "otool-classic" :
                                            "otool";

            var otoolResult = _processRunner.RunCommand(
                "xcrun",
                $"{otoolCommand} -hv -arch all \"{path}\"",
                CheckExitCode.AllowAnything
            );

            if (otoolResult.ExitCode != 0)
            {
                return LinkStatus.CheckFailed;
            }

            return IsExecutable(otoolResult.Output);
        }

        LinkStatus IsExecutable(string otoolResultOutput)
        {
            var result = !otoolResultOutput.EndsWith(
                "is not an object file",
                StringComparison.InvariantCultureIgnoreCase
            );
            return result ? LinkStatus.ExecutableExists : LinkStatus.NoExecutable;
        }
    }
}

