using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Xamarin.UITest.Shared.Processes;

namespace Xamarin.UITest.Tests.Processes
{
    [TestFixture]
    class ProcessListerTests
    {
        void CanParseOutput(string[] output, bool isOSX)
        {
            ProcessOutput[] processOutput = output.Select(l=>new ProcessOutput(l)).ToArray();
            var processResult = new ProcessResult(processOutput, 0, 1, true);
            
            var runner = Substitute.For<IProcessRunner>();
            runner.RunCommand(Arg.Any<string>(), Arg.Any<string>()).Returns(processResult);

			var platform = Substitute.For<IPlatform>();
            platform.IsOSXOrUnix.Returns(isOSX);
            platform.IsWindows.Returns(!isOSX);
 
            var processLister = new ProcessLister(runner, platform);
            Assert.That(processLister.GetProcessInfos().Length, Is.EqualTo(2));
        }

        [Test]
        public void CanParseWindowsOutput()
        {
            CanParseOutput(new [] {"", "Node,CommandLine,ProcessId", "WIN-5PSRQBG5J76,,0", "WIN-5PSRQBG5J76,TPAutoConnect.exe -q -i vmware -a COM1 -F 30,1788"}, false);
        }

        [Test]
        public void CanParseOSXOutput()
        {
            CanParseOutput(new[] {"PID ARGS", "265 foo /sbin/launchd", "269 bar /usr/libexec/UserEventAgent (Aqua)" }, true);
        }
    }
}
