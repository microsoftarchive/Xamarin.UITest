using System.Linq;
using Xamarin.UITest.Shared.Execution;
using System;
using Xamarin.UITest.Shared.Android.Adb;

namespace Xamarin.UITest.Shared.Android.Queries
{
    public class QueryAdbKeyguardEnabled : IQuery<bool, AdbProcessRunner>
    {
        readonly AdbArguments _adbArguments;

        public QueryAdbKeyguardEnabled(string deviceSerial)
        {
            _adbArguments = new AdbArguments(deviceSerial);
        }

        public bool Execute(AdbProcessRunner processRunner)
        {
            var windowsDumpResult = processRunner.Run(
                _adbArguments.CurrentWindowInformation()
            );
            var lines = windowsDumpResult.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            return lines.Any(x => x.Contains("mCurrentFocus") && x.Contains("Keyguard"));
        }
    }
}