using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xamarin.UITest.Android;
using Xamarin.UITest.Events;
using Xamarin.UITest.Utils;
using System;

namespace Xamarin.UITest.Configuration
{
    /// <summary>
    /// Represents the Android specific part of the <see cref="ConfigureApp"/> fluent API. Should not be used directly.
    /// </summary>
    public class AndroidAppConfigurator : IFluentInterface
    {
        readonly GenericAppConfigurator _genericAppConfigurator;
        FileInfo _apkFile;
        string _deviceSerial;
        FileInfo _sIFile;
        KeyStoreInfo _keyStore = new KeyStoreInfo(null, DefaultKeyStoreSecrets.StorePassword, DefaultKeyStoreSecrets.KeyPassword, DefaultKeyStoreSecrets.KeyAlias);
        IWaitTimes _waitTimes;
        string _launchableActivity;
		string _installedAppPackageName;

        /// <summary>
        /// Part of the <see cref="ConfigureApp"/> fluent API. Should not be used directly.
        /// </summary>
        public AndroidAppConfigurator(GenericAppConfigurator genericAppConfigurator = null)
        {
            _genericAppConfigurator = genericAppConfigurator ?? new GenericAppConfigurator();
        }

        /// <summary>
        /// Enables debug logging from the test runner.
        /// </summary>
        public AndroidAppConfigurator Debug()
        {
            _genericAppConfigurator.Debug();
            return this;
        }

        /// <summary>
        /// Enables local screenshot saving. Always enabled in the cloud.
        /// </summary>
        public AndroidAppConfigurator EnableLocalScreenshots()
        {
            _genericAppConfigurator.EnableLocalScreenshots();
            return this;
        }

        /// <summary>
        /// Always uses settings from IDE if they're present, overriding other configured values. 
        /// If not set, explicit configuration will disable IDE integration.
        /// </summary>
        public AndroidAppConfigurator PreferIdeSettings()
        {
            _genericAppConfigurator.PreferIdeSettings();
            return this;
        }

        /// <summary>
        /// Disables automatic screenshot generation after each SpecFlow step.
        /// </summary>
        public AndroidAppConfigurator DisableSpecFlowIntegration()
        {
            _genericAppConfigurator.DisableSpecFlowIntegration();
            return this;
        }

        /// <summary>
        /// Configures the ip address of the device. Generally best left unset unless you are 
        /// running an iOS application on a physical device.
        /// </summary>
        /// <param name="ipAddress">The ip address of the device.</param>
        public AndroidAppConfigurator DeviceIp(string ipAddress)
        {
            _genericAppConfigurator.DeviceIp(ipAddress);
            return this;
        }

        /// <summary>
        /// Configures the port of the device. Generally best left unset.
        /// </summary>
        /// <param name="port">The port of the Calabash HTTP server on the device.</param>
        public AndroidAppConfigurator DevicePort(int port)
        {
            _genericAppConfigurator.DevicePort(port);
            return this;
        }

        /// <summary>
        /// Sets the directory to store local log files in
        /// </summary>
        /// <param name="directory">The full path of the directory to store local log files in</param>
        public AndroidAppConfigurator LogDirectory(string directory)
        {
            _genericAppConfigurator.LogDirectory(directory);
            return this;
        }

        /// <summary>
        /// Configures the adb serial  of the device. Generally best left unset unless you want to run a specific device.
        /// </summary>
        /// <param name="deviceSerial">The device serial from adb devices.</param>
        public AndroidAppConfigurator DeviceSerial(string deviceSerial)
        {
            _deviceSerial = deviceSerial;
            return this;
        }

        /// <summary>
        /// Configures the keystore that the provided apk file is signed with. 
        /// A keystore is not required, but will ensure that the apk file is pristine and unchanged. 
        /// If a keystore is provided, it will be used to sign the auxiliary apks installed along with the app on the device. 
        /// If a keystore is not provided, Xamarin.UITest will generate a keystore and resign the apk.
        /// </summary>
        /// <param name="path">Path to the keystore file.</param>
        /// <param name="storePassword">Password to the keystore. Corresponds to the <c>-storepass</c> argument in jarsigner.</param>
        /// <param name="keyPassword">Password to the matching private key in the keystore. Corresponds to the <c>-keypass</c> argument in jarsigner.</param>
        /// <param name="keyAlias">Alias to the key in the keystore. Corresponds to the <c>alias</c> argument in jarsigner.</param>
        public AndroidAppConfigurator KeyStore(string path, string storePassword = DefaultKeyStoreSecrets.StorePassword, string keyPassword = DefaultKeyStoreSecrets.KeyPassword, string keyAlias = DefaultKeyStoreSecrets.KeyAlias)
        {
            _keyStore = new KeyStoreInfo(new FileInfo(path), storePassword, keyPassword, keyAlias);
            return this;
        }

        /// <summary>
        /// Configures the signing info file that the test server will be "signed" with.
        /// A signing info file is not required, but can be used instead of a keystore for signing the test server. The signing info file can be freely shared 
        /// without the risk of leaking keying material.
        /// The signing info file can be generated using the console tool.  
        /// </summary>
        /// <param name="path">Path to the signing info file.</param>
        public AndroidAppConfigurator SigningInfoFile(string path)
        {
            _sIFile = new FileInfo(path);
            return this;
        }

        /// <summary>
        /// Configures the apk file to use.
        /// </summary>
        /// <param name="path">Path to the apk file.</param>
        public AndroidAppConfigurator ApkFile(string path)
        {
            _apkFile = new FileInfo(path);
            return this;
        }

		/// <summary>
		/// Configures the already installed app to use.
		/// </summary>
		/// <param name="packageName">Package name of the installed app.</param>
		public AndroidAppConfigurator InstalledApp(string packageName)
		{
			_installedAppPackageName = packageName;
			return this;
		}


		/// <summary>
        /// Configures the default wait times for the framework.
        /// </summary>
        /// <param name="waitTimes">An implementation providing defaults.</param>
        public AndroidAppConfigurator WaitTimes(IWaitTimes waitTimes)
        {
            _waitTimes = waitTimes;
            return this;
        }

        /// <summary>
        /// Configures the activity to launch.
        /// </summary>
        /// <param name="activity">The activity to launch.</param>
        public AndroidAppConfigurator LaunchableActivity(string activity)
        {
            _launchableActivity = activity;
            return this;
        }

        /// <summary>
        /// Builds the configuration and launches the app on the selected device.
        /// </summary>
        /// <param name="appDataMode">The app data mode. Whether to clear app data or not before app launch.</param>
        /// <returns>The <see cref="AndroidApp"/> to use in the tests.</returns>
        public AndroidApp StartApp(AppDataMode appDataMode = AppDataMode.Auto)
        {
            UnhandledExceptionWorkaround.ClearUncaughtExceptionsFromOtherThreads();
            var app = new AndroidApp(GetConfiguration(StartAction.LaunchApp, appDataMode));
            EventManager.AfterAppStarted(app);
            return app;
        }

        /// <summary>
        /// Builds the configuration and connects to an already running app. 
        /// Used for advanced scenarios. Regular users should use <see cref="StartApp"/>
        /// instead.
        /// </summary>
        /// <returns>The <see cref="AndroidApp"/> to use in the tests.</returns>
        public AndroidApp ConnectToApp()
        {
            return new AndroidApp(GetConfiguration(StartAction.ConnectToApp));
        }

        /// <summary>
        /// Builds the Android app configuration which can be used for input for <see cref="AndroidApp"/>.
        /// The <see cref="StartApp"/> and <see cref="ConnectToApp"/> methods should be used instead unless
        /// you have specific needs for the configuration.
        /// </summary>
        /// <param name="startAction">The start action for the configuration. See <see cref="StartApp"/> and <see cref="ConnectToApp"/> methods.</param>
        /// <param name="appDataMode">The app data mode. Whether to clear app data or not before app launch.</param>
        /// <returns>An <see cref="IAndroidAppConfiguration"/> which can used for input for <see cref="AndroidApp"/>.</returns>
        public IAndroidAppConfiguration GetConfiguration(StartAction startAction, AppDataMode appDataMode = AppDataMode.Auto)
        {
            return new AndroidAppConfiguration(_genericAppConfigurator.GetConfiguration(), _keyStore, _apkFile, _deviceSerial, _sIFile, startAction, _waitTimes, _launchableActivity, _installedAppPackageName, appDataMode);
        }
    }
}
