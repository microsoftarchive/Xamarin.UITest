using Xamarin.UITest.Shared.Android.Adb;
using Xamarin.UITest.Shared.Execution;

namespace Xamarin.UITest.Shared.Android.Commands
{
    public class CommandAdbPortForward : ICommand<AdbProcessRunner>
    {
        readonly AdbArguments _adbArguments;
        readonly int _testServerPort;

        public CommandAdbPortForward(int testServerPort) : this(null, testServerPort)
        { }

        public CommandAdbPortForward(string deviceSerial, int testServerPort)
        {
            _adbArguments = new AdbArguments(deviceSerial);
            _testServerPort = testServerPort;
        }

        public void Execute(AdbProcessRunner processRunner)
        {
            processRunner.Run(_adbArguments.PortForward(_testServerPort));
        }
    }
}