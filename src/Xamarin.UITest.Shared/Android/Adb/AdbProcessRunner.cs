using Xamarin.UITest.Shared.Dependencies;
using Xamarin.UITest.Shared.Processes;

namespace Xamarin.UITest.Shared.Android.Adb
{
    public class AdbProcessRunner
    {
        readonly IProcessRunner _processRunner;
        readonly IAndroidSdkTools _sdkTools;

        internal AdbProcessRunner(IProcessRunner processRunner, IAndroidSdkTools sdkTools)
        {
            _processRunner = processRunner;
            _sdkTools = sdkTools;
        }

        public string Run(string adbArguments, int[] noExceptionsOnExitCodes = null)
        {
            var processResult = _processRunner.Run(_sdkTools.GetAdbPath(), adbArguments, noExceptionsOnExitCodes);

            return processResult.Output;
        }
    }
}