namespace Xamarin.UITest.Configuration
{
    internal class GenericAppConfiguration : IGenericAppConfiguration
    {
        readonly string _deviceIp;
        readonly bool _debug;
        readonly int? _devicePort;
        readonly bool _enableScreenshots;
        readonly string _logDirectory;
        readonly IdeIntegrationMode _ideIntegrationMode;
        readonly bool _disableSpecFlowIntegration;

        public GenericAppConfiguration(string deviceIp, bool debug, int? devicePort, bool enableScreenshots, string logDirectory, IdeIntegrationMode ideIntegrationMode, bool disableSpecFlowIntegration)
        {
            _deviceIp = deviceIp;
            _debug = debug;
            _devicePort = devicePort;
            _enableScreenshots = enableScreenshots;
            _logDirectory = logDirectory;
            _ideIntegrationMode = ideIntegrationMode;
            _disableSpecFlowIntegration = disableSpecFlowIntegration;
        }

        public bool EnableScreenshots
        {
            get { return _enableScreenshots; }
        }

        public bool Debug
        {
            get { return _debug; }
        }

        public string DeviceIp
        {
            get { return _deviceIp; }
        }

        public int? DevicePort
        {
            get { return _devicePort; }
        }

        public string LogDirectory
        {
            get { return _logDirectory;  }
        }

        public IdeIntegrationMode IdeIntegrationMode
        {
            get { return _ideIntegrationMode; }
        }

        public bool DisableSpecFlowIntegration
        {
            get { return _disableSpecFlowIntegration; }
        }
    }
}