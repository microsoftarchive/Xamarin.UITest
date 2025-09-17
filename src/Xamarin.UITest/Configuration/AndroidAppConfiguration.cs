using System;
using System.IO;
using Xamarin.UITest.Shared.Extensions;
using Xamarin.UITest.Utils;

namespace Xamarin.UITest.Configuration
{
    internal class AndroidAppConfiguration : IAndroidAppConfiguration
    {
        readonly IGenericAppConfiguration _genericAppConfiguration;
        readonly KeyStoreInfo _keyStoreInfo;
        readonly FileInfo _apkFile;
        readonly StartAction _startAction;
        readonly Uri _deviceUri;
        readonly string _deviceSerial;
        readonly FileInfo _sIFile;
        readonly IWaitTimes _waitTimes;
        readonly string _launchableActivity;
		readonly string _installedAppPackageName;
        readonly AppDataMode _appDataMode;

        public AndroidAppConfiguration(IGenericAppConfiguration genericAppConfiguration, KeyStoreInfo keyStoreInfo, FileInfo apkFile, string deviceSerial, FileInfo sIFile, StartAction startAction, IWaitTimes waitTimes, string launchableActivity, string installedAppPackageName, AppDataMode appDataMode)
        {
            _genericAppConfiguration = genericAppConfiguration;
            _keyStoreInfo = keyStoreInfo;
            _apkFile = apkFile;
			_installedAppPackageName = installedAppPackageName;
            _deviceSerial = deviceSerial;
            _sIFile = sIFile;
            _startAction = startAction;
            _launchableActivity = launchableActivity;
            _appDataMode = appDataMode;

            _waitTimes = waitTimes ?? new DefaultWaitTimes();

            _deviceUri = BuildDeviceUri(ConfiguredDeviceIp, ConfiguredDevicePort, ApkFile, DeviceSerial, InstalledAppPackageName);
        }

        public static Uri BuildDeviceUri(string configuredDeviceIp, int? configuredDevicePort, FileInfo apkFile, string deviceSerial, string installedAppPackageName)
        {
            var seed = apkFile != null ? apkFile.FullName.GetHashCode() : 1337;
            seed += deviceSerial != null ? deviceSerial.GetHashCode() : 0;
            seed += installedAppPackageName != null ? installedAppPackageName.GetHashCode() : 0;

            var portSeededRandom = new Random(seed);

            var ipAddress = !configuredDeviceIp.IsNullOrWhiteSpace() ? configuredDeviceIp : "127.0.0.1";
            var port = configuredDevicePort.GetValueOrDefault(50000 + portSeededRandom.Next(15000));

            return new UriBuilder("http", ipAddress, port).Uri;
        }

        public bool Debug
        {
            get { return _genericAppConfiguration.Debug; }
        }

        public bool EnableScreenshots
        {
            get { return _genericAppConfiguration.EnableScreenshots; }
        }

        public string LogDirectory
        {
            get { return _genericAppConfiguration.LogDirectory; }
        }

        public Uri DeviceUri
        {
            get { return _deviceUri; }
        }

        public string ConfiguredDeviceIp
        {
            get { return _genericAppConfiguration.DeviceIp; }
        }

        public int? ConfiguredDevicePort
        {
            get { return _genericAppConfiguration.DevicePort; }
        }

        public string DeviceSerial
        {
            get { return _deviceSerial; }
        }

        public FileInfo KeyStore
        {
            get { return _keyStoreInfo.Path; }
        }

        public string KeyStorePassword
        {
            get { return _keyStoreInfo.StorePassword; }
        }

        public string KeyStoreKeyPassword
        {
            get { return _keyStoreInfo.KeyPassword; }
        }

        public string KeyStoreKeyAlias
        {
            get { return _keyStoreInfo.KeyAlias; }
        }

        public FileInfo ApkFile
        {
            get { return _apkFile; }
        }

		public string InstalledAppPackageName
		{
			get { return _installedAppPackageName; }
		}

        public FileInfo SIFile
        {
            get { return _sIFile; }
        }

        public StartAction StartAction
        {
            get { return _startAction; }
        }

        public AppDataMode AppDataMode
        {
            get { return _appDataMode; }
        }

        public bool DisableSpecFlowIntegration
        {
            get { return _genericAppConfiguration.DisableSpecFlowIntegration; }
        }

        public string LaunchableActivity
        {
            get { return _launchableActivity; }
        }

        public void Verify()
        {
            if(_startAction == StartAction.LaunchApp)
            {
				if (_apkFile != null && !_installedAppPackageName.IsNullOrWhiteSpace())
				{
					throw new Exception("Can not specify both ApkFile and InstalledApp.");
				}

				if(_apkFile == null && _installedAppPackageName.IsNullOrWhiteSpace())
                {
                    throw new Exception("ApkFile or InstalledApp has not been configured.");
                }

				if(_apkFile != null && !_apkFile.Exists)
                {
                    throw new Exception("ApkFile does not exist: " + _apkFile.FullName);
                }
            }

            if (_sIFile != null)
            {
                if (_keyStoreInfo.Path != null)
                {
                    throw new Exception("Signing info and Keystore cannot be combined.");
                }

                if (!_sIFile.Exists)
                {
                    throw new Exception(string.Format("Signing info file does not exist: {0}", _sIFile.FullName));
                }
            }
        }

        public IWaitTimes WaitTimes
        {
            get { return _waitTimes; }
        }

        public IdeIntegrationMode IdeIntegrationMode
        {
            get { return _genericAppConfiguration.IdeIntegrationMode; }
        }
    }
}