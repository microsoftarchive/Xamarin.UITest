using System.Collections.Generic;
using System.IO;
using Xamarin.UITest.Events;
using Xamarin.UITest.iOS;
using Xamarin.UITest.Utils;
using System;

namespace Xamarin.UITest.Configuration
{
    /// <summary>
    /// Represents the iOS specific part of the <see cref="ConfigureApp"/> fluent API. Should not be used directly.
    /// </summary>
    public class iOSAppConfigurator : IFluentInterface
    {
        readonly GenericAppConfigurator _genericAppConfigurator;
        DirectoryInfo _appBundleDirectory;
        string _installedAppBundleId;
        FileInfo _appBundleZip;
        string _deviceIdentifier;
        IWaitTimes _waitTimes;
        string IDBPathOverride;
        IEnumerable<string> _autArguments;
        Dictionary<string, string> _autEnvironment;
        string CodesignIdentityNameOverride;
        string CodesignIdentitySHAOverride;
        string ProvisioningProfilePathOverride;
        string _DeviceAgentBundleId = null;
        string DeviceAgentPathOverride = null;
        bool ShouldInstallDeviceAgent = true;

        /// <summary>
        /// Part of the <see cref="ConfigureApp"/> fluent API. Should not be used directly.
        /// </summary>
        public iOSAppConfigurator(GenericAppConfigurator genericAppConfigurator = null)
        {
            _genericAppConfigurator = genericAppConfigurator ?? new GenericAppConfigurator();
        }

        /// <summary>
        /// Enables debug logging from the test runner.
        /// </summary>
        public iOSAppConfigurator Debug()
        {
            _genericAppConfigurator.Debug();
            return this;
        }

        /// <summary>
        /// Enables local screenshot saving. Always enabled in the cloud.
        /// </summary>
        public iOSAppConfigurator EnableLocalScreenshots()
        {
            _genericAppConfigurator.EnableLocalScreenshots();
            return this;
        }

        /// <summary>
        /// Always uses settings from IDE if they're present, overriding other configured values.
        /// If not set, explicit configuration will disable IDE integration.
        /// </summary>
        public iOSAppConfigurator PreferIdeSettings()
        {
            _genericAppConfigurator.PreferIdeSettings();
            return this;
        }

        /// <summary>
        /// Configures the ip address of the device. Generally best left unset unless you are
        /// running an iOS application on a physical device.
        /// </summary>
        /// <param name="ipAddress">The ip address of the device.</param>
        public iOSAppConfigurator DeviceIp(string ipAddress)
        {
            _genericAppConfigurator.DeviceIp(ipAddress);
            return this;
        }

        /// <summary>
        /// Configures the port of the device. Generally best left unset.
        /// </summary>
        /// <param name="port">The port of the Calabash HTTP server on the device.</param>
        public iOSAppConfigurator DevicePort(int port)
        {
            _genericAppConfigurator.DevicePort(port);
            return this;
        }

        /// <summary>
        /// Sets the directory to store local log files in
        /// </summary>
        /// <param name="directory">The full path of the directory to store local log files in</param>
        public iOSAppConfigurator LogDirectory(string directory)
        {
            _genericAppConfigurator.LogDirectory(directory);
            return this;
        }

        /// <summary>
        /// Disables automatic screenshot generation after each SpecFlow step.
        /// </summary>
        public iOSAppConfigurator DisableSpecFlowIntegration()
        {
            _genericAppConfigurator.DisableSpecFlowIntegration();
            return this;
        }

        /// <summary>
        /// Configures the app bundle to use.
        /// </summary>
        /// <param name="path">Path to the directory containing the app bundle.</param>
        public iOSAppConfigurator AppBundle(string path)
        {
            _appBundleDirectory = new DirectoryInfo(path);
            return this;
        }

        /// <summary>
        /// Configures the zipped app bundle to use.
        /// </summary>
        /// <param name="path">Path to the directory containing the zipped app bundle.</param>
        public iOSAppConfigurator AppBundleZip(string path)
        {
            _appBundleZip = new FileInfo(path);
            return this;
        }

        /// <summary>
        /// Configures the installed app to use. Will force a run on physical device.
        /// </summary>
        /// <param name="bundleId">The bundle id of the installed application.</param>
        public iOSAppConfigurator InstalledApp(string bundleId)
        {
            _installedAppBundleId = bundleId;
            return this;
        }

        /// <summary>
        /// Configures the device to use with the device identifier (UUID).
        /// </summary>
        /// <param name="deviceIdentifier">The device identifier (UUID) found in the XCode Organizer.</param>
        public iOSAppConfigurator DeviceIdentifier(string deviceIdentifier)
        {
            _deviceIdentifier = deviceIdentifier;
            return this;
        }

        /// <summary>
        /// Configures the default wait times for the framework.
        /// </summary>
        /// <param name="waitTimes">An implementation providing defaults.</param>
        public iOSAppConfigurator WaitTimes(IWaitTimes waitTimes)
        {
            _waitTimes = waitTimes;
            return this;
        }

        /// <summary>
        /// Configures custom path to IDB.
        /// </summary>
        /// <param name="pathToIDB">Path to IDB.</param>
        /// <returns></returns>
        public iOSAppConfigurator OverrideIDBPath(string pathToIDB)
        {
            IDBPathOverride = pathToIDB;
            return this;
        }

        /// <summary>
        /// Sets the arguments to send to the AUT.
        /// </summary>
        /// <param name="arguments">A collection of string values to send to the AUT.</param>
        public iOSAppConfigurator AutArguments(IEnumerable<string> arguments)
        {
            _autArguments = arguments;
            return this;
        }

        /// <summary>
        /// Sets the environment to send to the AUT.
        /// </summary>
        /// <param name="environmentVars">A dictionary of environment variables to send to the AUT.</param>
        public iOSAppConfigurator AutEnvironmentVars(Dictionary<string, string> environmentVars)
        {
            _autEnvironment = environmentVars;
            return this;
        }

        /// <summary>
        /// Specifies signing identity name that will be used to resign DeviceAgent for testing on physical iOS device.
        /// </summary>
        /// <param name="codesignIdentityName">Identity's name.</param>
        public iOSAppConfigurator CodesignIdentityName(string codesignIdentityName)
        {
            CodesignIdentityNameOverride = codesignIdentityName;
            return this;
        }

        /// <summary>
        /// Specifies signing identity SHA sum.
        /// </summary>
        /// <param name="codesignIdentitySHA">Identity's SHA sum.</param>
        public iOSAppConfigurator CodesignIdentitySHA(string codesignIdentitySHA)
        {
            CodesignIdentitySHAOverride = codesignIdentitySHA;
            return this;
        }

        /// <summary>
        /// Specifies path to provisioning profile that will be used to resign DeviceAgent for testing on physical iOS device.
        /// </summary>
        /// <param name="provisioningProfilePath">Path to provisioning profile.</param>
        public iOSAppConfigurator ProvisioningProfile(string provisioningProfilePath)
        {
            ProvisioningProfilePathOverride = provisioningProfilePath;
            return this;
        }

        /// <summary>
        /// Sets DeviceAgent Bundle Identifier. Needed when custom built DeviceAgent is being used since its identifier will be different from embedded one.
        /// </summary>
        /// <param name="deviceAgentBundleId">DeviceAgent Bundle Identifier</param>
        public iOSAppConfigurator DeviceAgentBundleId(string deviceAgentBundleId)
        {
            _DeviceAgentBundleId = deviceAgentBundleId;
            return this;
        }

        /// <summary>
        /// Specifies path to bundle of DeviceAgent that should be used instead of embedded one.
        /// </summary>
        /// <param name="customDeviceAgentPath">DeviceAgent-Runner.app bundle path.</param>
        public iOSAppConfigurator CustomDeviceAgentPath(string customDeviceAgentPath)
        {
            DeviceAgentPathOverride = customDeviceAgentPath;
            return this;
        }

        /// <summary>
        /// Disables DeviceAgent install (and reinstall as well) on physical device.
        /// </summary>
        public iOSAppConfigurator DisableDeviceAgentInstall()
        {
            ShouldInstallDeviceAgent = false;
            return this;
        }

        /// <summary>
        /// Builds the configuration and launches the app on the selected device or simulator.
        /// </summary>
        /// <param name="appDataMode">The app data mode. Whether to clear app data or not before app launch.</param>
        /// <returns>The <see cref="iOSApp"/> to use in the tests.</returns>
        public iOSApp StartApp(AppDataMode appDataMode = AppDataMode.Auto)
        {
            UnhandledExceptionWorkaround.ClearUncaughtExceptionsFromOtherThreads();
            var app = new iOSApp(GetConfiguration(StartAction.LaunchApp, appDataMode));
            EventManager.AfterAppStarted(app);
            return app;
        }

        /// <summary>
        /// Builds the configuration and connects to an already running app.
        /// Used for advanced scenarios. Regular users should use <see cref="StartApp"/>
        /// instead.
        /// </summary>
        /// <returns>The <see cref="iOSApp"/> to use in the tests.</returns>
        public iOSApp ConnectToApp()
        {
            return new iOSApp(GetConfiguration(StartAction.ConnectToApp));
        }

        /// <summary>
        /// Builds the iOS app configuration which can be used for input for <see cref="iOSApp"/>.
        /// The <see cref="StartApp"/> and <see cref="ConnectToApp"/> methods should be used instead unless
        /// you have specific needs for the configuration.
        /// </summary>
        /// <param name="startAction">The start action for the configuration. See <see cref="StartApp"/> and <see cref="ConnectToApp"/> methods.</param>
        /// <param name="appDataMode">The app data mode. Whether to clear app data or not before app launch.</param>
        /// <returns>An <see cref="IiOSAppConfiguration"/> which can used for input for <see cref="iOSApp"/>.</returns>
        public IiOSAppConfiguration GetConfiguration(StartAction startAction, AppDataMode appDataMode = AppDataMode.Auto)
        {
            return new iOSAppConfiguration(
                _genericAppConfigurator.GetConfiguration(),
                _appBundleDirectory,
                _installedAppBundleId,
                _appBundleZip,
                startAction,
                _deviceIdentifier,
                _waitTimes,
                IDBPathOverride,
                appDataMode,
                _autArguments,
                _autEnvironment,
                CodesignIdentityNameOverride,
                CodesignIdentitySHAOverride,
                ProvisioningProfilePathOverride,
                _DeviceAgentBundleId,
                DeviceAgentPathOverride,
                ShouldInstallDeviceAgent);
        }
    }
}
