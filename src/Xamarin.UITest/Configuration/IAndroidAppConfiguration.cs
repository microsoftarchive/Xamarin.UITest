using System;
using System.IO;
using Xamarin.UITest.Utils;

namespace Xamarin.UITest.Configuration
{
    /// <summary>
    /// Configuration for Android apps. Not to be used directly, should be created by using the fluent <see cref="ConfigureApp"/> API.
    /// </summary>
	public interface IAndroidAppConfiguration
	{
        /// <summary>
        /// The location of the keystore that was used to sign the tested app. If left empty a new keystore will be built.
        /// </summary>
	    FileInfo KeyStore  { get; }

        /// <summary>
        /// The store password of the given keystore. Corresponds to the <c>-storepass</c> argument in jarsigner.
        /// </summary>
        string KeyStorePassword { get; }

        /// <summary>
        /// The private key password of the given keystore key. Corresponds to the <c>-keypass</c> argument in jarsigner.
        /// </summary>
        string KeyStoreKeyPassword { get; }

        /// <summary>
        /// The key alias to use for given keystore. Corresponds to the alias argument in jarsigner.
        /// </summary>
        string KeyStoreKeyAlias { get; }

        /// <summary>
        /// The location of the SI file used for generating the signed test server.
        /// </summary>
        FileInfo SIFile { get; }

        /// <summary>
        /// The location of the apk file to test.
        /// </summary>
	    FileInfo ApkFile { get; }

		/// <summary>
		/// The package name of the installed app.
		/// </summary>
		string InstalledAppPackageName { get; }
        
        /// <summary>
        /// Enable debug logging.
        /// </summary>
        bool Debug { get; }

        /// <summary>
        /// The desired state of the app after test initialization. Either the test framework can start the app or connect to an already running app in more advanced scenarios.
        /// </summary>
        StartAction StartAction { get; }
	    
        /// <summary>
        /// Whether to clear app data or not.
        /// </summary>
        AppDataMode AppDataMode { get; }

        /// <summary>
        /// The uri of the device test server.
        /// </summary>
        Uri DeviceUri { get; }

        /// <summary>
        /// The configured ip address. Use <c>DeviceUri</c> for the active configuration.
        /// </summary>
        /// <value>The configured ip address.</value>
        string ConfiguredDeviceIp { get; }

        /// <summary>
        /// The configured port. Use <c>DeviceUri</c> for the active configuration.
        /// </summary>
        /// <value>The configured ip address.</value>
        int? ConfiguredDevicePort { get; }

        /// <summary>
        /// The serial of the device. Can be found using adb.
        /// </summary>
        string DeviceSerial { get; }

        /// <summary>
        /// Enable screenshots. Local screenshots are disabled by default to speed up tests.
        /// </summary>
        bool EnableScreenshots { get; }

        /// <summary>
        /// Specify the specific launchable activity to use
        /// </summary>
        string LaunchableActivity { get; }

        /// <summary>
        /// Specify the location of the log directory for local test runs.
        /// </summary>
        string LogDirectory { get; }

        /// <summary>
        /// Verifies the configuration.
        /// </summary>
        void Verify();

        /// <summary>
        /// Default wait times.
        /// </summary>
        IWaitTimes WaitTimes { get; }

        /// <summary>
        /// The IDE integration mode. Decides what settings to use in case of both explicit configuration and active IDE integration.
        /// </summary>
        IdeIntegrationMode IdeIntegrationMode { get; }

        /// <summary>
        /// Whether to disables automatic screenshot generation after each SpecFlow step.
        /// </summary>
        bool DisableSpecFlowIntegration { get; }
	}
}