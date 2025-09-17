using Xamarin.UITest.Shared.Execution;
using System;
using Xamarin.UITest.Shared.Logging;
using Xamarin.UITest.Shared.Android.Adb;

namespace Xamarin.UITest.Shared.Android.Queries
{
    public class QueryAdbSdkVersion : IQuery<int, AdbProcessRunner, IExecutor>
    {
        readonly AdbArguments _adbArguements;

        public QueryAdbSdkVersion(string deviceSerial)
        {
            _adbArguements = new AdbArguments(deviceSerial);
        }

        public int Execute(AdbProcessRunner processRunner, IExecutor executor)
        {
            int sdkVersion = 0;

            var output = GetSdkVersion(processRunner);
            Int32.TryParse(output, out sdkVersion);
            Log.Debug(string.Format("Device sdk version {0}.", sdkVersion));

            return sdkVersion;
        }

        string GetSdkVersion(AdbProcessRunner processRunner)
        {
            return processRunner
                .Run
                (
                    _adbArguements.GetSDKVersionFromProperty()
                );
        }
    }
}
