using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.UITest.Shared;
using Xamarin.UITest.Utils;

namespace Xamarin.UITest.Configuration
{
    /// <summary>
    /// Configuration for iOS apps. Not to be used directly, should be created by using the fluent <see cref="ConfigureApp"/> API.
    /// </summary>
    public interface IiOSAppConfiguration
    {
        /// <summary>
        /// Enable debug logging.
        /// </summary>
        bool Debug { get; }

        /// <summary>
        /// Enable screenshots. Local screenshots are disabled by default to speed up tests.
        /// </summary>
        bool EnableScreenshots { get; }

        /// <summary>
        /// Specify the location of the log directory for local test runs.
        /// </summary>
        string LogDirectory { get; }

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
        /// The device identifier. List of know devices can be found by running "instruments -s devices"
        /// </summary>
        string DeviceIdentifier { get; }

        /// <summary>
        /// The bundle id of an already installed to test.
        /// </summary>
        string InstalledAppBundleId { get; }

        /// <summary>
        /// The location of an app bundle for a simulator test.
        /// </summary>
        DirectoryInfo AppBundleDirectory { get; }

        /// <summary>
        /// The location of a zip archive for a simulator test.
        /// </summary>
        FileInfo AppBundleZip { get; }

        /// <summary>
        /// The desired state of the app after test initialization. Either the test framework can start the app or connect to an already running app in more advanced scenarios.
        /// </summary>
        StartAction StartAction { get; }

        /// <summary>
        /// Verify the state of a configuration. Throw exception if invalid.
        /// </summary>
        void Verify();

        /// <summary>
        /// Default wait times.
        /// </summary>
        IWaitTimes WaitTimes { get; }

        /// <summary>
        /// Path to IDB overriden by user.
        /// </summary>
        string IDBPathOverride { get; }

        /// <summary>
        /// The IDE integration mode. Decides what settings to use in case of both explicit configuration and active IDE integration.
        /// </summary>
        IdeIntegrationMode IdeIntegrationMode { get; }

        /// <summary>
        /// Whether to clear app data or not.
        /// </summary>
        AppDataMode AppDataMode { get; }

        /// <summary>
        /// Whether to disables automatic screenshot generation after each SpecFlow step.
        /// </summary>
        bool DisableSpecFlowIntegration { get; }

        /// <summary>
        /// Arguments to send to the AUT.
        /// </summary>
        IEnumerable<string> AutArguments { get; }

        /// <summary>
        /// Environment to send to the AUT.
        /// </summary>
        Dictionary<string, string> AutEnvironmentVars { get; }

        /// <summary>
        /// Path override to DeviceAgent bundle that should be used for testing.
        /// </summary>
        string DeviceAgentPathOverride { get; }

        /// <summary>
        /// Flag that determines should DeviceAgent be installed/reinstalled.
        /// </summary>
        bool ShouldInstallDeviceAgent { get; }
    }
}