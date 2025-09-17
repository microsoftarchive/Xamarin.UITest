using System;
using System.IO;
using System.Linq;
using Xamarin.UITest.Configuration;
using Xamarin.UITest.Shared.Dependencies;
using Xamarin.UITest.Shared.Logging;
using Xamarin.UITest.Utils;

namespace Xamarin.UITest
{
    /// <summary>
    /// Main entry point for <c>Xamarin.UITest</c>. This is a fluent API that allows you to 
    /// configure your app for either Android or iOS and start it.
    /// </summary>
    public static class ConfigureApp
    {
        static ConfigureApp() 
        {
            CheckForLocalNetHttp();
        }

        /// <summary>
        /// Configures the ip address of the device. Generally best left unset unless you are 
        /// running an iOS application on a physical device.
        /// </summary>
        /// <param name="ipAddress">The ip address of the device.</param>
        public static GenericAppConfigurator DeviceIp(string ipAddress)
        {
            return new GenericAppConfigurator().DeviceIp(ipAddress);
        }

        /// <summary>
        /// Configures the port of the device. Generally best left unset.
        /// </summary>
        /// <param name="port">The port of the Calabash HTTP server on the device.</param>
        public static GenericAppConfigurator DevicePort(int port)
        {
            return new GenericAppConfigurator().DevicePort(port);
        }

        /// <summary>
        /// Specifies that the app is an iOS app.
        /// </summary>
        public static iOSAppConfigurator iOS
        {
            get 
            {
                CheckForSupportedNunitVersion();
                return new iOSAppConfigurator(); 
            }
        }

        /// <summary>
        /// Specifies that the app is an Android app.
        /// </summary>
        public static AndroidAppConfigurator Android
        {
            get 
            {
                CheckForSupportedNunitVersion();
                return new AndroidAppConfigurator(); 
            }
        }

        /// <summary>
        /// Enables debug logging from the test runner.
        /// </summary>
        public static GenericAppConfigurator Debug()
        {
            return new GenericAppConfigurator().Debug();
        }

        private static void CheckForSupportedNunitVersion()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            if (assemblies.Any(a => a.GetName().Name == "Xamarin.UITest.Repl.Evaluation"))
            {
                // Nunit not required for Repl
                return;
            }

            var nunitFrameworkAssembly = assemblies.FirstOrDefault(a => a.GetName().Name == "nunit.framework");

            if (nunitFrameworkAssembly == null)
            {
                // TODO: Update message with link to docs
                Log.Info(string.Format(
                    "WARNING: No compatible unit test framework has been found.  NUnit {0} is recommended.",
                    NUnitVersions.RecommendedNUnitString));
                return;
            }

            var nunitVersion = nunitFrameworkAssembly.GetName().Version;

            if (!NUnitVersions.IsSupported(nunitVersion))
            {
                // TODO: Update message with link to docs
                Log.Info(string.Format(
                    "WARNING: NUnit {0} is not supported.  The recommended version is {1}.",
                    nunitVersion,
                    NUnitVersions.RecommendedNUnitString));
                return;
            }
        }

        static void CheckForLocalNetHttp()
        {
            var execPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            if (File.Exists(Path.Combine(execPath, "System.Net.Http.dll")))
            {
                Log.Info("WARNING: Detected System.Net.Http.dll in bin folder. " +
                         "This may be due to this known mono issue: https://bugzilla.xamarin.com/show_bug.cgi?id=60315#c2 - " +
                         "we recommend you delete this dll before running tests to avoid test failures.");
            }
        }
    }
}
