using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using Xamarin.UITest.Configuration;
using Xamarin.UITest.Shared.Artifacts;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Extensions;
using Xamarin.UITest.Shared.Http;
using Xamarin.UITest.Shared.iOS;
using Xamarin.UITest.Shared.iOS.Queries;
using Xamarin.UITest.Shared.Logging;
using Xamarin.UITest.Shared.Processes;
using Xamarin.UITest.Shared.Resources;
using Xamarin.UITest.XDB;
using System.Threading.Tasks;
using Xamarin.UITest.Utils;
using Xamarin.UITest.XDB.Services.OSX;
using Xamarin.UITest.XDB.Services.OSX.IDB;
using Xamarin.UITest.XDB.Entities;
using Xamarin.UITest.XDB.Services;
using Xamarin.UITest.Helpers;
using Xamarin.UITest.Shared.Zip;
using System.Runtime.CompilerServices;
using Xamarin.UITest.XDB.Entities.iOSDevice;

[assembly: InternalsVisibleTo("Xamarin.UITest.Tests.Shared")]
//[assembly: InternalsVisibleTo("NSubstitute")]

namespace Xamarin.UITest.iOS
{
    class iOSAppLauncher
    {
        private IEnvironmentService EnvironmentService;
        readonly IProcessRunner _processRunner;
        readonly IExecutor _executor;
        readonly EmbeddedResourceLoader _resourceLoader;
        readonly iDeviceTools _iTools;

        const int HostProxyPortOffset = 8;
        const int DEVICE_AGENT_PORT = 27753;

        public iOSAppLauncher(IProcessRunner processRunner, IExecutor executor, EmbeddedResourceLoader resourceLoader)
        {
            EnvironmentService = XdbServices.GetRequiredService<IEnvironmentService>();
            _processRunner = processRunner;
            _executor = executor;
            _resourceLoader = resourceLoader;
            _iTools = new iDeviceTools(_processRunner);
        }

        public LaunchAppResult LaunchApp(
            IiOSAppConfiguration appConfiguration,
            HttpClient httpClient)
        {
            if (appConfiguration.AppBundleDirectory == null
                && appConfiguration.AppBundleZip == null
                && appConfiguration.InstalledAppBundleId == null)
            {
                throw new Exception("Unknown configuration.");
            }

            var clearAppData = appConfiguration.AppDataMode != AppDataMode.DoNotClear;

            LaunchAppResult result = LaunchAppLocal(appConfiguration, httpClient, clearAppData);

            result.CalabashDevice = EnsureCalabashRunning(result.DeviceConnectionInfo.Connection);

            return result;
        }

        LaunchAppResult LaunchAppLocal(
            IiOSAppConfiguration appConfiguration, HttpClient httpClient, bool clearAppData)
        {
            SetXamarinStudioAppleSDKOverridePath();

            var result = new LaunchAppResult();

            IiOSAppBundle appBundle = null;

            var connectedDevice = ResolveConnectediOSDevice(appConfiguration.DeviceIdentifier);

            bool useSim = false;

            if (appConfiguration.AppBundleDirectory != null || appConfiguration.AppBundleZip != null)
            {
                var appBundleArtifactFolder = GetArtifactFolderForBundle(appConfiguration);

                appBundle = XdbServices.GetRequiredService<IiOSBundleService>()
                    .LoadBundle(appBundleArtifactFolder.AppBundlePath);

                result.ArtifactFolder = appBundleArtifactFolder.ArtifactFolder;

                useSim = ValidBundleSupportsSim(appBundle, result.ArtifactFolder, connectedDevice);

#if DEBUG
                if (!useSim)
                {
                    Log.Info("Debug run configuration active. Installing onto physical device enabled.");
                }
#else
                if (!useSim)
                {
                    throw new Exception("This app bundle is not valid for running on physical device. " +
                                        "To fix this issue please ensure that your target device is a simulator." +
                                        "To run against an installed app on a physical device you can use" +
                                        $"InstalledApp(\"{appBundle.BundleId}\")");
                }
#endif

                Log.Debug("Starting app bundle. ", new { AppBundle = appBundle.Path });
            }
            else
            {
                useSim = connectedDevice == null;

                result.ArtifactFolder = new ArtifactFolder(appConfiguration.InstalledAppBundleId);

                Log.Debug("Starting app. ", new { BundleId = appConfiguration.InstalledAppBundleId });
            }

            var instruments = new Instruments(result.ArtifactFolder, _resourceLoader, _iTools);

            PreRunCleanup(appConfiguration.DeviceUri.Port);

            string simId = null;

            if (useSim)
            {
                simId = GetOrValidateSimId(instruments, appConfiguration.DeviceIdentifier);

                QuitSimulatorIfNecessary(useSim, instruments, simId);
            }
            else
            {
                PrepareForDevice(appConfiguration, connectedDevice, true, instruments);
            }

            if (!string.IsNullOrWhiteSpace(appBundle?.Path))
            {
                EnvironmentService.AppBundlePath = appBundle.Path;
            }

            if (!string.IsNullOrWhiteSpace(appBundle?.BundleId))
            {
                EnvironmentService.AppBundleId = appBundle.BundleId;
            }
            else
            {
                EnvironmentService.AppBundleId = appConfiguration.InstalledAppBundleId;
            }

            result.DeviceConnectionInfo = StartUsingDeviceAgentAsync(
                useSim ? simId : connectedDevice.GetUUID(),
                appConfiguration.DeviceUri,
                clearAppData,
                appConfiguration.AutArguments,
                appConfiguration.AutEnvironmentVars,
                result.ArtifactFolder,
                appConfiguration
            ).Result;

            return result;
        }

        void QuitSimulatorIfNecessary(bool useSim, Instruments instruments, string simId)
        {
            if (!useSim)
            {
                return;
            }

            if (Environment.GetEnvironmentVariable("UITEST_FORCE_IOS_SIM_RESTART") == "1")
            {
                instruments.QuitSimulator();
                return;
            }

            var unknownApps = new ProcessLister(_processRunner)
                .GetProcessInfos()
                .Where(p => p.CommandLine.Contains(
                    $"/Library/Developer/CoreSimulator/Devices/{simId}/data/Containers/Bundle/Application/"))
                .Where(p => !p.CommandLine.Contains("DeviceAgent-Runner.app"))
                .Where(p => !p.CommandLine.Contains($" {UITestConstants.AUTArgIdentifier}"));

            if (unknownApps.Any())
            {
                instruments.QuitSimulator();
            }
        }

        void SetXamarinStudioAppleSDKOverridePath()
        {
            try
            {
                var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                var xamarinSettingsFile = Path.Combine(homeDir, "Library", "Preferences", "Xamarin", "Settings.plist");

                if (!File.Exists(xamarinSettingsFile))
                {
                    return;
                }

                var sdkRoot = PListHelper.ReadPListValueFromFile(xamarinSettingsFile, "AppleSdkRoot");
                Log.Info("Using Xamarin Studio override for Apple SDK: " + sdkRoot);
                Environment.SetEnvironmentVariable("DEVELOPER_DIR", sdkRoot);
            }
            catch (Exception ex)
            {
                // Only log this exception if is not a "could not find key exception"
                // This exception just means that Xamarin Studio is using the default location
                if (!ex.Message.StartsWith("Could not find key", StringComparison.InvariantCultureIgnoreCase))
                {
                    Log.Debug("Failed trying to read Xamarin Studio Apple SDK path.", ex);
                }
            }
        }

        void PrepareForDevice(
            IiOSAppConfiguration appConfiguration,
            DeviceInfo connectedDevice,
            bool useDeviceAgentPhysicalDevice,
            Instruments instruments)
        {
            if (!useDeviceAgentPhysicalDevice)
            {
                instruments.QuitSimulator();
            }

            VerifyConfigForDevice(appConfiguration, useDeviceAgentPhysicalDevice);

            SetUpPortForwarding(appConfiguration, connectedDevice, useDeviceAgentPhysicalDevice);
        }

        void VerifyConfigForDevice(IiOSAppConfiguration appConfiguration, bool useDeviceAgentPhysicalDevice)
        {
            if (!useDeviceAgentPhysicalDevice)
            {
                VerifyWithoutDeviceAgent(appConfiguration);
            }
        }

        void SetUpPortForwarding(
            IiOSAppConfiguration appConfiguration,
            DeviceInfo connectedDevice,
            bool useDeviceAgentPhysicalDevice)
        {
            if (appConfiguration.DeviceUri.Host == UITestConstants.DefaultDeviceIp)
            {
                _iTools.iProxy.StartForward(
                    connectedDevice.GetUUID(),
                    appConfiguration.DeviceUri.Port,
                    appConfiguration.DeviceUri.Port);

                if (useDeviceAgentPhysicalDevice)
                {
                    _iTools.iProxy.StartForward(
                        connectedDevice.GetUUID(),
                        DEVICE_AGENT_PORT,
                        DEVICE_AGENT_PORT);
                }
            }
        }

        AppBundleArtifactFolder GetArtifactFolderForBundle(IiOSAppConfiguration appConfiguration)
        {
            var result = new AppBundleArtifactFolder();

            if (appConfiguration.AppBundleDirectory != null)
            {
                result.AppBundlePath = appConfiguration.AppBundleDirectory.FullName;

                result.ArtifactFolder = new ArtifactFolder(appConfiguration.AppBundleDirectory);

                if (!IsValidAppBundle(result.AppBundlePath))
                {
                    throw new Exception("App bundle directory doesn't contain a valid app bundle.");
                }
            }
            else
            {
                result.ArtifactFolder = new ArtifactFolder(appConfiguration.AppBundleZip);

                result.AppBundlePath = result.ArtifactFolder.CreateArtifact(
                    "AppBundle",
                    path => UnzipAppBundle(appConfiguration, path));

                if (!IsValidAppBundle(result.AppBundlePath))
                {
                    var subAppBundlePath = Directory.EnumerateDirectories(result.AppBundlePath)
                        .FirstOrDefault(IsValidAppBundle);

                    if (subAppBundlePath.IsNullOrWhiteSpace())
                    {
                        throw new Exception("Zip file didn't contain a valid app bundle.");
                    }

                    result.AppBundlePath = subAppBundlePath;
                }
            }

            return result;
        }

        Uri GetiOS8ProxyURI(Uri deviceUri)
        {
            var builder = new UriBuilder(deviceUri)
            {
                Port = (deviceUri.Port + HostProxyPortOffset),
                Host = UITestConstants.DefaultDeviceIp
            };

            return builder.Uri;
        }

        /// <summary>
        /// Cleans up run environment.
        /// Removes any portforwards from old runs that may prevent a new run.
        /// </summary>
        /// <param name="port">Port to check forwards for.</param>
        void PreRunCleanup(int port)
        {
            iProxy.StopAllForwards(port, _processRunner);
            iProxy.StopAllForwards(DEVICE_AGENT_PORT, _processRunner);
        }

        bool IsValidAppBundle(string path)
        {
            return Directory.EnumerateFiles(path)
                .Any(x => String.Equals("Info.plist", Path.GetFileName(x), StringComparison.InvariantCultureIgnoreCase));
        }

        bool ValidBundleSupportsSim(IiOSAppBundle appBundle, ArtifactFolder artifactFolder, DeviceInfo connectedDevice)
        {
            switch (_executor.Execute(new QueryAppHasCalabashLinked(appBundle.Path)))
            {
                case LinkStatus.NoExecutable:
                    throw new Exception(
                        $"The app bundle at path {appBundle.Path} does not contain a executable binary");

                case LinkStatus.IncompleteBundleGeneratedByXamarinStudio:
                    throw new Exception(
                        "This app is not compatible with UITest because it was built for multiple simulator " +
                        "architectures. Please select a single architecture in the project's iOS Build options " +
                        "and rebuild the application.");

                case LinkStatus.NotLinked:
                    throw new Exception(
                        $"The app bundle in {appBundle.Path} does not seem to be properly linked with Calabash. " +
                        "Please verify that it includes the Calabash component.");

                case LinkStatus.CheckFailed:
                    Log.Info(
                        $"Unable to determine if app bundle in {appBundle.Path} was linked with Calabash. " +
                        "Will continue as if it is.");
                    break;
            }

            if (appBundle.DTPlatform == "iphonesimulator")
            {
                return true;
            }

            if (connectedDevice == null)
            {
                throw new Exception(
                    "This app bundle is not valid for running on a simulator. " +
                    "To fix this issue please ensure that your target device is a " +
                    "simulator. DTPlatformName is '" +
                    appBundle.DTPlatform +
                    "', not 'iphonesimulator' in the app's Info.plist.");
            }

            return false;
        }

        void VerifyWithoutDeviceAgent(IiOSAppConfiguration appConfiguration)
        {
            var message = "() supports physical devices only when using DeviceAgent. ";

            if (appConfiguration.AppBundleDirectory != null)
            {
                throw new Exception(nameof(appConfiguration.AppBundleDirectory) + message);
            }

            if (appConfiguration.AppBundleZip != null)
            {
                throw new Exception(nameof(appConfiguration.AppBundleZip) + message);
            }
        }

        string GetOrValidateSimId(Instruments instruments, string deviceIdentifier)
        {
            if (deviceIdentifier.IsNullOrWhiteSpace())
            {
                deviceIdentifier = instruments.GetDefaultSimDeviceIdentifier(
                    XdbServices.GetRequiredService<IXcodeService>().GetCurrentVersion().Major
                );
            }
            else
            {
                var hardwareDeviceIdentifiers = _executor.Execute(new QueryConnectediOSDevices(null));
                if (hardwareDeviceIdentifiers.Any(d => d.GetUUID().Equals(deviceIdentifier)))
                {
                    throw new Exception($"Device {deviceIdentifier} exists, but is not a simulator.");
                }
            }

            instruments.EnsureNoOthersRunning(deviceIdentifier);

            return deviceIdentifier;
        }

        DeviceInfo ResolveConnectediOSDevice(string deviceIdentifierConfigured)
        {
            var devicesConnected = _executor.Execute(new QueryConnectediOSDevices(deviceIdentifierConfigured));

            if (deviceIdentifierConfigured.IsNullOrWhiteSpace())
            {
                if (devicesConnected.Length > 1)
                {
                    throw new Exception(
                        String.Format(
                            "Found {0} connected iOS devices. Either only have 1 connected or select one using DeviceIdentifier during configuration. Devices found: {1}",
                            devicesConnected.Count(), String.Join(", ", devicesConnected.Select(d => d.GetUUID()))));
                }

                if (devicesConnected.Length == 1)
                {
                    return devicesConnected.First();
                }

                return null;
            }

            var deviceInfo = devicesConnected.SingleOrDefault(d => d.GetUUID().Equals(deviceIdentifierConfigured));

            if (deviceInfo != null)
            {
                return deviceInfo;
            }

            return null;
        }

        internal async Task<DeviceConnectionInfo> StartUsingDeviceAgentAsync(
            string deviceIdentifier,
            Uri deviceUri,
            bool clearAppData,
            IEnumerable<string> arguments,
            Dictionary<string, string> environment,
            ArtifactFolder artifactFolder,
            IiOSAppConfiguration appConfiguration)
        {
            var connection = new HttpCalabashConnection(new HttpClient(deviceUri));
            EnvironmentService.SetIOSDevice(new UDID(deviceIdentifier));
            IiOSDevice iOSDevice = EnvironmentService.IOSDevice;

            iOSDevice.PrepareDevice(httpCalabashConnection: connection, shouldClearAppData: clearAppData);

            IiOSDeviceAgentService deviceAgentService = XdbServices.GetRequiredService<IiOSDeviceAgentService>();
            IIDBService idbService = XdbServices.GetRequiredService<IIDBService>();

            if (!string.IsNullOrWhiteSpace(EnvironmentService.AppBundlePath))
            {
                idbService.InstallApp(new UDID(UDID: deviceIdentifier), EnvironmentService.AppBundlePath);
            }
            if (string.IsNullOrWhiteSpace(EnvironmentService.DeviceAgentPathOverride))
            {
                iOSDevice.PrepareSigningInfo();
            }

            await deviceAgentService.LaunchTestAsync(deviceIdentifier, deviceUri.Host);

            IEnumerable<string> combinedArguments = new[] { UITestConstants.AUTArgIdentifier };

            if (arguments != null)
            {
                combinedArguments = arguments.Concat(combinedArguments);
            }

            await deviceAgentService.StartAppAsync(
                deviceUri.Host,
                EnvironmentService.AppBundleId,
                combinedArguments,
                environment);

            return new DeviceConnectionInfo(
                deviceIdentifier,
                connection,
                true,
                deviceUri.Host);
        }

        void UnzipAppBundle(IiOSAppConfiguration appConfiguration, string path)
        {
            Log.Info("Unzipping AppBundle.");
            Directory.CreateDirectory(path);
            ZipHelper.Unzip(zipFile: appConfiguration.AppBundleZip.FullName, target: path);
        }

        static iOSCalabashDevice GetCalabashDevice(ICalabashConnection connection)
        {
            Exception lastEx = null;
            for (int retryCount = 0; retryCount < 10; retryCount++)
            {
                try
                {
                    var versionResult = connection.Version();

                    if (versionResult.StatusCode == 200)
                    {
                        var versionInfoDictionary =
                            JsonConvert.DeserializeObject<Dictionary<string, object>>(versionResult.Contents);
                        var versionInfo = new iOSVersionInfo(versionInfoDictionary);
                        var calabashServerVersion = versionInfoDictionary.TryGetString("version", "0.0");
                        return new iOSCalabashDevice(versionInfo, new VersionNumber(calabashServerVersion));
                    }
                }
                catch (Exception ex)
                {
                    // mono 6.x introduced `System.Net.Http.HttpRequestException` coming
                    // from a failed connection to the Calabash Server.
                    if (ex is System.Net.Http.HttpRequestException ||
                        ex.InnerException is System.Net.Http.HttpRequestException
                        || ex is WebException || ex.InnerException is WebException)
                    {
                        lastEx = ex;
                    }
                    else
                    {
                        throw;
                    }
                }

                Thread.Sleep(TimeSpan.FromMilliseconds(1000));
            }

            if (lastEx != null)
            {
                var innerExceptionMsg = lastEx.InnerException == null ? string.Empty : lastEx.InnerException.ToString();

                Log.Debug(
                    $"Error contacting Calabash server. {lastEx.Message}\n{lastEx.StackTrace}\n{innerExceptionMsg}");
            }

            return null;
        }

        public iOSCalabashDevice EnsureCalabashRunning(ICalabashConnection connection)
        {
            var device = GetCalabashDevice(connection);

            if (device == null)
            {
                throw new Exception(string.Concat(
                    "Unable to contact test backend running in app. ",
                    "Check that your app is running and that it is properly configured."));
            }
            return device;
        }

        public LaunchAppResult ConnectToApp(IiOSAppConfiguration appConfiguration, HttpClient httpClient)
        {
            Log.Info(string.Format("Initializing iOS app on {0}.", appConfiguration.DeviceUri));

            // Temporary connection to calabash to read version. Works because /version is not redirected by the proxy
            var connection = new HttpCalabashConnection(httpClient);

            var deviceInfo = EnsureCalabashRunning(connection);

            DeviceConnectionInfo deviceConnectionInfo = new DeviceConnectionInfo(
                appConfiguration.DeviceIdentifier,
                connection,
                true,
                appConfiguration.DeviceUri.Host);

            return new LaunchAppResult(new ArtifactFolder(), deviceConnectionInfo, deviceInfo);
        }

        class AppRunningFailedException : Exception
        {
            public AppRunningFailedException(string message, Exception innerException) : base(message, innerException)
            {
            }
        }

        class AppLaunchFailedException : AppRunningFailedException
        {
            public AppLaunchFailedException(string message, Exception innerException) : base(message, innerException)
            {
            }
        }

        class AppInstallationFailedException : AppRunningFailedException
        {
            public AppInstallationFailedException(string message, Exception innerException) : base(message, innerException)
            {
            }
        }

        class AppBundleArtifactFolder
        {
            public string AppBundlePath { get; set; }
            public ArtifactFolder ArtifactFolder { get; set; }
        }
    }
}
