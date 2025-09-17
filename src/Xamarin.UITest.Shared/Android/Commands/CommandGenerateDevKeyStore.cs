using System;
using Xamarin.UITest.Shared.Dependencies;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Processes;

namespace Xamarin.UITest.Shared.Android.Commands
{
    internal class CommandGenerateDevKeyStore : ICommand<IProcessRunner, IJdkTools>
    {
        readonly string _keyStorePath;
        readonly string _keyAlias;
        readonly string _storePassword;
        readonly string _keyPassword;

        // Helper method to properly escape arguments for macOS/Linux
        private string EscapeMacOSArg(string arg)
        {
            if (string.IsNullOrEmpty(arg))
                return arg;

            // If the arg contains spaces or special characters, wrap it in quotes
            if (arg.Contains(" ") || arg.Contains("\"") || arg.Contains("'") || 
                arg.Contains(";") || arg.Contains("&") || arg.Contains("|"))
            {
                return $"\"{arg.Replace("\"", "\\\"")}\"";
            }

            return arg;
        }

        public CommandGenerateDevKeyStore(string keyStorePath, string keyAlias, string storePassword, string keyPassword)
        {
            _keyStorePath = keyStorePath;
            _keyAlias = keyAlias;
            _storePassword = storePassword;
            _keyPassword = keyPassword;
        }

        public void Execute(IProcessRunner processRunner, IJdkTools jdkTools)
        {
            var toolPath = jdkTools.GetKeyToolPath();
            var isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;

            // On macOS/Linux, we need to properly quote and escape the arguments
            string arguments;

            if (isWindows) {
                // Windows version
                arguments = $"-genkey -noprompt -keyalg RSA -keysize 2048 -validity 1000 -alias {_keyAlias} " +
                           $"-keypass {_keyPassword} -storepass {_storePassword} " +
                           $"-dname \"CN=dev.xamarin.com, OU=DEV, O=XAM, L=SF, S=CA, C=US\" " +
                           $"-keystore \"{_keyStorePath}\"";
            } else {
                // macOS/Linux version
                arguments = $"-genkey -noprompt -keyalg RSA -keysize 2048 -validity 1000 " +
                           $"-alias {EscapeMacOSArg(_keyAlias)} " +
                           $"-keypass {EscapeMacOSArg(_keyPassword)} " + 
                           $"-storepass {EscapeMacOSArg(_storePassword)} " +
                           $"-dname CN=dev.xamarin.com,OU=DEV,O=XAM,L=SF,S=CA,C=US " +
                           $"-keystore {EscapeMacOSArg(_keyStorePath)}";
            }
            processRunner.Run(toolPath, arguments);
        }
    }
}