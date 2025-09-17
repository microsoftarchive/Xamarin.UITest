using Xamarin.UITest.Shared.Execution;
using System;
using System.Linq;
using Xamarin.UITest.Shared.Android.Adb;

namespace Xamarin.UITest.Shared.Android.Queries
{

    public class QueryAdbKeyboardShown : IQuery<bool, AdbProcessRunner>
    {
        readonly AdbArguments _adbArguments;

		public QueryAdbKeyboardShown(string deviceSerial)
		{
            _adbArguments = new AdbArguments(deviceSerial);
		}

        public bool Execute(AdbProcessRunner processRunner)
        {
			var lines = DumpSys(processRunner);
			return lines.Any(x => x.Contains("mInputShown=true"));
        }

        string[] DumpSys(AdbProcessRunner processRunner)
        {
            return processRunner
                .Run(_adbArguments.InputServiceInformation())
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
