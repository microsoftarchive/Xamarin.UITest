using System.Collections.Generic;
using Xamarin.UITest.Shared.Dependencies;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Processes;

namespace Xamarin.UITest.Shared.Android.Commands
{
    internal class CommandApkSignerSign : ICommand<IProcessRunner, IAndroidSdkTools>
    {
        readonly string _sourceApkFilePath;
        readonly string _targetApkFilePath;
        readonly string _keyStoreLocation;
        readonly string _storePassword;
        readonly string _keyAlias;
        readonly string _keyPassword;

        public CommandApkSignerSign(string sourceApkFilePath, string targetApkFilePath, string keyStoreLocation, string storePassword, string keyAlias, string keyPassword)
        {
            _sourceApkFilePath = sourceApkFilePath;
            _targetApkFilePath = targetApkFilePath;
            _keyStoreLocation = keyStoreLocation;
            _storePassword = storePassword;
            _keyAlias = keyAlias;
            _keyPassword = keyPassword;
        }

        public void Execute(IProcessRunner processRunner, IAndroidSdkTools sdkTools)
        {
            List<string> opts = new List<string>();
            opts.Add("--v1-signing-enabled true");
            opts.Add("--v2-signing-enabled true");

            string helpResult = processRunner.Run(sdkTools.GetApkSignerPath(), "sign --help").Output;
            if (helpResult.Contains("--v3-signing-enabled"))
            {
                opts.Add("--v3-signing-enabled false");
            }
            if (helpResult.Contains("--v4-signing-enabled"))
            {
                opts.Add("--v4-signing-enabled false");
            }

            string arguments = string.Format("sign --ks \"{0}\" --ks-pass pass:{1} --ks-key-alias {2}  --key-pass pass:{3} {4} --out \"{5}\" \"{6}\"",
                _keyStoreLocation,
                _storePassword,
                _keyAlias,
                _keyPassword,
                string.Join(" ", opts),
                _targetApkFilePath,
                _sourceApkFilePath);

            processRunner.Run(sdkTools.GetApkSignerPath(), arguments);
        }
    }
}
