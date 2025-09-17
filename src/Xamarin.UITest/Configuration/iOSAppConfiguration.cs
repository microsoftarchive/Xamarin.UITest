using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xamarin.UITest.iOS.ApplicationSigning.Entities;
using Xamarin.UITest.Shared.Extensions;
using Xamarin.UITest.Utils;
using Xamarin.UITest.XDB;
using Xamarin.UITest.XDB.Services;

namespace Xamarin.UITest.Configuration
{
    internal class iOSAppConfiguration : IiOSAppConfiguration
    {
        readonly IEnvironmentService EnvironmentService = XdbServices.GetRequiredService<IEnvironmentService>();
        readonly IGenericAppConfiguration _genericAppConfiguration;
        readonly DirectoryInfo _appBundleDirectory;
        readonly string _installedAppBundleId;
		readonly FileInfo _appBundleZip;
        readonly StartAction _startAction;
        readonly Uri _deviceUri;
        readonly string _deviceIdentifier;
        readonly AppDataMode _appDataMode;
        public string IDBPathOverride { get; }
        readonly IWaitTimes _waitTimes;
        readonly IEnumerable<string> _autArguments;
        readonly Dictionary<string, string> _autEnvironment;
        public string PathToProvisioningProfile { get; }
        public string DeviceAgentPathOverride { get; private set; }
        public bool ShouldInstallDeviceAgent { get; private set; }

        public iOSAppConfiguration(
            IGenericAppConfiguration genericAppConfiguration,
            DirectoryInfo appBundleDirectory,
            string installedAppBundleId,
            FileInfo appBundleZip,
            StartAction startAction,
            string deviceIdentifier,
            IWaitTimes waitTimes,
            string idbPathOverride,
            AppDataMode appDataMode,
            IEnumerable<string> autArguments,
            Dictionary<string, string> autEnvironment,
            string codesignIdentityName,
            string codesignIdentitySHA,
            string pathToProvisioningProfile,
            string deviceAgentBundleId,
            string deviceAgentPathOverride,
            bool shouldInstallDeviceAgent)
        {
			_appBundleZip = appBundleZip;
            _startAction = startAction;
            _appBundleDirectory = appBundleDirectory;
            _installedAppBundleId = installedAppBundleId;
            _genericAppConfiguration = genericAppConfiguration;
            _deviceIdentifier = deviceIdentifier;
            IDBPathOverride = idbPathOverride;
            _appDataMode = appDataMode;
            _autArguments = autArguments;
            _autEnvironment = autEnvironment;

            _waitTimes = waitTimes ?? new DefaultWaitTimes();

            _deviceUri = BuildDeviceUri();

            if (!string.IsNullOrWhiteSpace(codesignIdentityName) && !string.IsNullOrWhiteSpace(codesignIdentitySHA))
            {
                EnvironmentService.CodesignIdentity = new CodesignIdentity(name: codesignIdentityName, shaSum: codesignIdentitySHA);
            }
            if (!string.IsNullOrWhiteSpace(pathToProvisioningProfile))
            {
                try
                {
                    FileInfo provisioningProfileFile = new(fileName: pathToProvisioningProfile);
                    EnvironmentService.ProvisioningProfile = new ProvisioningProfile(provisioningProfileFile: provisioningProfileFile);
                }
                catch (Exception)
                {
                    throw new Exception(message: $"Could not access the provisioning profile at path: ${pathToProvisioningProfile}." +
                        $"Please check file at this path.");
                }
            }
            if (!string.IsNullOrWhiteSpace(deviceAgentBundleId))
            {
                EnvironmentService.DeviceAgentBundleId = deviceAgentBundleId;
            }
            DeviceAgentPathOverride = deviceAgentPathOverride;
            EnvironmentService.ShouldInstallDeviceAgent = shouldInstallDeviceAgent;
        }

        public StartAction StartAction
        {
            get { return _startAction; }
        }

        Uri BuildDeviceUri()
        {
            var ipAddress = !_genericAppConfiguration.DeviceIp.IsNullOrWhiteSpace()
                ? _genericAppConfiguration.DeviceIp
                : UITestConstants.DefaultDeviceIp;

            var port = _genericAppConfiguration.DevicePort.GetValueOrDefault(37265);

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

        public string ConfiguredDeviceIp
        {
            get { return _genericAppConfiguration.DeviceIp; }
        }

        public int? ConfiguredDevicePort
        {
            get { return _genericAppConfiguration.DevicePort; }
        }

        public Uri DeviceUri
        {
            get { return _deviceUri; }
        }

        public string InstalledAppBundleId
        {
            get { return _installedAppBundleId; }
        }

        public string DeviceIdentifier
        {
            get { return _deviceIdentifier; }
        }

        public DirectoryInfo AppBundleDirectory
        {
            get { return _appBundleDirectory; }
        }

		public FileInfo AppBundleZip
		{
			get { return _appBundleZip; }
		}

        public AppDataMode AppDataMode
        {
            get { return _appDataMode; }
        }

        public bool DisableSpecFlowIntegration
        {
            get { return _genericAppConfiguration.DisableSpecFlowIntegration; }
        }

        public void Verify()
        {
			var appOptions = new object[] { InstalledAppBundleId, AppBundleDirectory, AppBundleZip };

			if (appOptions.Count(x => x != null) > 1)
            {
                throw new Exception("Can't have both installed app and app bundle.");
            }

			if (InstalledAppBundleId == null && AppBundleDirectory == null && _appBundleZip == null)
            {
                throw new Exception("Must have either installed app or app bundle.");
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

        public IEnumerable<string> AutArguments
        {
            get { return _autArguments; }
        }

        public Dictionary<string, string> AutEnvironmentVars
        {
            get { return _autEnvironment; }
        }
    }
}