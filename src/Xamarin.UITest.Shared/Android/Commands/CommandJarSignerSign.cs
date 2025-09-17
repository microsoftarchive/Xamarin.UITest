using System;
using Xamarin.UITest.Shared.Dependencies;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Processes;

namespace Xamarin.UITest.Shared.Android.Commands
{
    [Obsolete("No support for jarsigner anymore, use CommandApkSignerSign instead")]
    internal class CommandJarSignerSign : ICommand<IProcessRunner, IJdkTools>
    {
        readonly string _sourceApkFilePath;
        readonly string _targetApkFilePath;
        readonly string _keyStoreLocation;
        readonly string _storePassword;
        readonly string _keyAlias;
        readonly string _keyPassword;
        readonly string _signingAlgorithm;

        public CommandJarSignerSign(string sourceApkFilePath, string targetApkFilePath, string keyStoreLocation, string storePassword, string keyAlias, string keyPassword, string signingAlgorithm)
        {
            _signingAlgorithm = signingAlgorithm;
            _sourceApkFilePath = sourceApkFilePath;
            _targetApkFilePath = targetApkFilePath;
            _keyStoreLocation = keyStoreLocation;
            _storePassword = storePassword;
            _keyAlias = keyAlias;
            _keyPassword = keyPassword;
        }

        public void Execute(IProcessRunner processRunner, IJdkTools jdkTools)
        {
            string arguments = string.Format("-sigalg {6} -digestalg SHA1 -signedjar \"{0}\" -storepass {1} -keypass {5} -keystore \"{2}\" \"{3}\" \"{4}\"", _targetApkFilePath, _storePassword, _keyStoreLocation, _sourceApkFilePath, _keyAlias, _keyPassword, _signingAlgorithm);
            processRunner.Run(jdkTools.GetJarSignerPath(), arguments);
        }
    }
}