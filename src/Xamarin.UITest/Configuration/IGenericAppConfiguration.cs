namespace Xamarin.UITest.Configuration
{
    /// <summary>
    /// Shared configuration for apps. Not to be used directly, should be created by using the fluent <see cref="ConfigureApp"/> API.
    /// </summary>
    public interface IGenericAppConfiguration
    {
        /// <summary>
        /// Enable debug logging.
        /// </summary>
        bool Debug { get; }

        /// <summary>
        /// The ip adress of the device.
        /// </summary>
        string DeviceIp { get; }

        /// <summary>
        /// The port to use for communication with the test server.
        /// </summary>
        int? DevicePort { get; }

        /// <summary>
        /// Enable screenshots. Local screenshots are disabled by default to speed up tests.
        /// </summary>
        bool EnableScreenshots { get; }

        /// <summary>
        /// Specify the location of the log directory for local test runs.
        /// </summary>
        string LogDirectory { get; }

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