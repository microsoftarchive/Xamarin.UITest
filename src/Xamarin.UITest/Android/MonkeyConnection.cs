using System;
using System.Threading;
using Xamarin.UITest.Shared;
using Xamarin.UITest.Shared.Android.Commands;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Logging;

namespace Xamarin.UITest.Android
{
    class MonkeyConnection : IDisposable
    {
        readonly int _port;
        readonly AndroidGestures _gestures;

        public MonkeyConnection(CommandAdbStartMonkey monkeyStarter, IExecutor executor,
            WaitForHelper waitForHelper, AndroidGestures gestures)
        {
            _gestures = gestures;

            _port = executor.Execute(monkeyStarter);

            // Ensure monkey is started and ready
            var shouldStartWithin = TimeSpan.FromSeconds(30);
            waitForHelper.WaitFor(() =>
                {
                    SendCommand("sleep 0");
                    return true;
                },
                timeout: shouldStartWithin,
                timeoutMessage:
                    $"Monkey did no start on port {_port} in due time ({shouldStartWithin.TotalSeconds} seconds)");

        }

        public void SendCommand(string command)
        {
            _gestures.PerformAction("send_tcp", _port, command, true);
        }

        public void Dispose()
        {
            SendCommand("quit");
        }
    }
}
