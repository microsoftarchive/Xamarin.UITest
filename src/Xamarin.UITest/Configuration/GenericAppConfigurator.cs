using Xamarin.UITest.Utils;

namespace Xamarin.UITest.Configuration
{
    /// <summary>
    /// Represents the generic specific part of the <see cref="ConfigureApp"/> fluent API. Should not be used directly.
    /// </summary>
    public class GenericAppConfigurator : IFluentInterface
    {
        string _deviceIp;
        bool _debug;
        bool _enableLocalScreenshots;
        int? _devicePort;
        string _logDirectory;
        IdeIntegrationMode _ideIntegrationMode = IdeIntegrationMode.PreferExplicitConfiguration;
        bool _disableSpecFlowIntegration;

        /// <summary>
        /// Enables debug logging from the test runner.
        /// </summary>
        public GenericAppConfigurator Debug()
        {
            _debug = true;
            return this;
        }

        /// <summary>
        /// Enables local screenshot saving. Always enabled in the cloud.
        /// </summary>
        public GenericAppConfigurator EnableLocalScreenshots()
        {
            _enableLocalScreenshots = true;
            return this;
        }

        /// <summary>
        /// Always uses settings from IDE if they're present, overriding other configured values. 
        /// If not set, explicit configuration will disable IDE integration.
        /// </summary>
        public GenericAppConfigurator PreferIdeSettings()
        {
            _ideIntegrationMode = IdeIntegrationMode.PreferIdeSettingsIfPresent;
            return this;
        }

        /// <summary>
        /// Specifies that the app is an iOS app.
        /// </summary>
        public iOSAppConfigurator iOS
        {
            get { return new iOSAppConfigurator(this); }
        }

        /// <summary>
        /// Specifies that the app is an Android app.
        /// </summary>
        public AndroidAppConfigurator Android
        {
            get { return new AndroidAppConfigurator(this); }
        }

        /// <summary>
        /// Configures the ip address of the device. Generally best left unset unless you are 
        /// running an iOS application on a physical device.
        /// </summary>
        /// <param name="ipAddress">The ip address of the device.</param>
        public GenericAppConfigurator DeviceIp(string ipAddress)
        {
            _deviceIp = ipAddress;
            return this;
        }

        /// <summary>
        /// Configures the port of the device. Generally best left unset.
        /// </summary>
        /// <param name="port">The port of the Calabash HTTP server on the device.</param>
        public GenericAppConfigurator DevicePort(int port)
        {
            _devicePort = port;
            return this;
        }

        /// <summary>
        /// Sets the directory to store local log files in
        /// </summary>
        /// <param name="directory">The full path of the directory to store local log files in</param>
        public GenericAppConfigurator LogDirectory(string directory)
        {
            _logDirectory = directory;
            return this;
        }

        /// <summary>
        /// Disables automatic screenshot generation after each SpecFlow step.
        /// </summary>
        public GenericAppConfigurator DisableSpecFlowIntegration()
        {
            _disableSpecFlowIntegration = true;
            return this;
        }

        /// <summary>
        /// Builds the generic app configuration which contains the shared configuration
        /// across platforms.
        /// The <see cref="Android"/> or <see cref="iOS"/> properties should be used
        /// instead as part of the fluent API unless the configuration is needed.
        /// </summary>
        /// <returns>An <see cref="IGenericAppConfiguration"/> to be consumed by the 
        /// platform specific configurations.</returns>
        public IGenericAppConfiguration GetConfiguration()
        {
            return new GenericAppConfiguration(_deviceIp, _debug, _devicePort, _enableLocalScreenshots, _logDirectory, _ideIntegrationMode, _disableSpecFlowIntegration);
        }
    }
}
