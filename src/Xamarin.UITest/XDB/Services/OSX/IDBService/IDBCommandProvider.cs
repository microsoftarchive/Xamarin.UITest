using System;
using System.IO;
using System.Linq;
using Xamarin.UITest.iOS.ApplicationSigning.Entities;
using Xamarin.UITest.iOS.ApplicationSigning.Managers;
using Xamarin.UITest.iOS.Entities;
using Xamarin.UITest.XDB.Entities;
using Xamarin.UITest.XDB.Services.Processes;

namespace Xamarin.UITest.XDB.Services.OSX.IDB
{
	internal class IDBCommandProvider
	{
        private readonly IDependenciesDeploymentService DependenciesDeploymentService;
        private readonly IEnvironmentService EnvironmentService;
        private readonly ILoggerService LoggerService;
        private readonly IProcessService ProcessService;
        private readonly string TempFolderPath;
        private readonly string IDBBinaryPath = null;

        internal IDBCommandProvider(
            IEnvironmentService environmentService,
            IDependenciesDeploymentService dependenciesDeploymentService,
            ILoggerService loggerService,
            IProcessService processService,
            string tempFolderPath)
        {
            EnvironmentService = environmentService;
            DependenciesDeploymentService = dependenciesDeploymentService;
            LoggerService = loggerService;
            ProcessService = processService;
            TempFolderPath = tempFolderPath;

            IDBBinaryPath = IDBLocator.GetIDBPath(processService: processService, environmentService: environmentService, loggerService: loggerService);
        }

        /// <summary>
        /// Runs IDB command.
        /// </summary>
        /// <param name="arguments">Arguments for IDB tool.</param>
        /// <returns><see cref="ProcessResult"/> of executed command.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        private ProcessResult RunCommand(string arguments)
        {
            LoggerService.LogDebug($"Running IDB command: {arguments}");
            return ProcessService.Run(IDBBinaryPath, arguments);
        }

        internal ProcessResult LaunchSimulator(UDID UDID)
        {
            if (!UDID.IsSimulator)
            {
                throw new ArgumentException($"Provided UDID is not a valid simulator's UDID: {UDID}.");
            }
            return RunCommand(IDBCommands.Boot(UDID: UDID));
        }

        internal ProcessResult FocusSimulator(UDID UDID)
        {
            if (!UDID.IsSimulator)
            {
                throw new ArgumentException($"Provided UDID is not a valid simulator's UDID: {UDID}.");
            }
            return RunCommand(IDBCommands.Focus(UDID: UDID));
        }

        internal ProcessResult CreateDirectory(UDID UDID, string bundleId, string relativePathToDirectory)
        {
            return RunCommand(IDBCommands.File(UDID: UDID, bundleId: bundleId, operation: "mkdir", path: relativePathToDirectory));
        }

        internal ProcessResult RemoveDirectory(UDID UDID, string bundleId, string relativePathToDirectory)
        {
            return RunCommand(IDBCommands.File(UDID: UDID, bundleId: bundleId, operation: "rm", path: relativePathToDirectory));
        }

        internal ProcessResult InstallApp(UDID UDID, string pathToBundle)
        {
            return RunCommand(IDBCommands.Install(bundlePath: pathToBundle, UDID: UDID));
        }

        internal ProcessResult GetAppsList(UDID UDID)
        {
            return RunCommand(IDBCommands.ListApps(UDID: UDID));
        }

        internal ProcessResult InstallDeviceAgent(UDID UDID)
        {
            string deviceAgentBundlePath;
            if (string.IsNullOrWhiteSpace(EnvironmentService.DeviceAgentPathOverride))
            {
                DependenciesDeploymentService.Install(TempFolderPath);
                string deviceAgentBundlePathForSimulator = Path.Combine(TempFolderPath, DependenciesDeploymentService.PathToSimTestRunner);
                string deviceAgentBundlePathForDevice = Path.Combine(TempFolderPath, DependenciesDeploymentService.PathToDeviceTestRunner);
                deviceAgentBundlePath = UDID.IsSimulator ? deviceAgentBundlePathForSimulator : deviceAgentBundlePathForDevice;
                if (UDID.IsPhysicalDevice)
                {
                    LoggerService.LogInfo(message: "Installing DeviceAgent on physical device");
                    LoggerService.LogInfo(message: $"Need to resign DeviceAgent{Environment.NewLine}");

                    CodesignIdentity codesignIdentity = EnvironmentService.CodesignIdentity;
                    if (codesignIdentity == null)
                    {
                        throw new Exception(message: "Codesign identity is not set");
                    }
                    LoggerService.LogInfo(message: "Codesign identity that will be used to resign DeviceAgent:");
                    LoggerService.LogInfo(message: $"{codesignIdentity.Name}{Environment.NewLine}");

                    ProvisioningProfile provisioningProfile = EnvironmentService.ProvisioningProfile;
                    if (provisioningProfile == null)
                    {
                        throw new Exception(message: "Provisioning profile is not set");
                    }
                    LoggerService.LogInfo(message: "Provisioning profile that will be used to resign DeviceAgent:");
                    LoggerService.LogInfo(message: $"{provisioningProfile.ProvisioningProfileFile.FullName}{Environment.NewLine}");

                    ApplicationBundle deviceAgentBundle = new(deviceAgentBundlePath);
                    ApplicationSigningManager.SignBundle(
                        processService: ProcessService,
                        loggerService: LoggerService,
                        bundle: deviceAgentBundle,
                        provisioningProfile: provisioningProfile,
                        codesignIdentity: codesignIdentity);
                }
            }
            else
            {
                deviceAgentBundlePath = EnvironmentService.DeviceAgentPathOverride;
            }
            return RunCommand(IDBCommands.Install(UDID: UDID, bundlePath: deviceAgentBundlePath));
        }

        internal ProcessResult SetLocation(UDID UDID, LatLong latLong)
        {
            return RunCommand(IDBCommands.SetLocation(
                UDID: UDID,
                latitude: latLong.Latitude.ToString(),
                longitude: latLong.Longitude.ToString()));
        }

        internal ProcessResult StopSimulatingLocation(UDID UDID)
        {
            LatLong defaultLocation = new(lattitude: -122.147911, longitude: 37.485023);
            return RunCommand(IDBCommands.SetLocation(
                UDID: UDID,
                latitude: defaultLocation.Latitude.ToString(),
                longitude: defaultLocation.Longitude.ToString()));
        }

        internal ProcessResult UninstallApp(UDID UDID, string bundleId)
        {
            return RunCommand(IDBCommands.Uninstall(UDID: UDID, bundleId: bundleId));
        }

        /// <summary>
        /// Class with IDB commands formatters.
        /// </summary>
        private static class IDBCommands
        {
            /// <summary>
            /// Command for installing application bundle on device.
            /// </summary>
            /// <param name="UDID">UDID of device to install bundle on.</param>
            /// <param name="bundlePath">Path to the bundle that should be installed.</param>
            /// <returns><see cref="string"/> with arguments for IDB.</returns>
            internal static string Install(UDID UDID, string bundlePath)
            {
                return $"install --udid {UDID} \"{bundlePath}\"";
            }

            /// <summary>
            /// Command for booting simulator.
            /// </summary>
            /// <param name="UDID">UDID of simulator to boot.</param>
            /// <returns><see cref="string"/> with arguments for IDB.</returns>
            internal static string Boot(UDID UDID)
            {
                return $"boot {UDID}";
            }

            /// <summary>
            /// Command for bringing simulator window to the foreground.
            /// </summary>
            /// <param name="UDID"><see cref="UDID"/> of simulator which window should be brought to the foreground.</param>
            /// <returns><see cref="string"/> with arguments for IDB.</returns>
            internal static string Focus(UDID UDID)
            {
                return $"focus --udid {UDID}";
            }

            /// <summary>
            /// Command for setting location of device.
            /// </summary>
            /// <param name="UDID">UDID of device to set location for.</param>
            /// <param name="latitude">Latitude to set.</param>
            /// <param name="longitude">Longitude to set.</param>
            /// <returns><see cref="string"/> with arguments for IDB.</returns>
            internal static string SetLocation(UDID UDID, string latitude, string longitude)
            {
                return $"set-location --udid {UDID} {latitude} {longitude}";
            }

            /// <summary>
            /// Command for uninstalling application from device.
            /// </summary>
            /// <param name="UDID">UDID of device to uninstall application from.</param>
            /// <param name="bundleId">ID of application bundle that should be uninstalled.</param>
            /// <returns><see cref="string"/> with arguments for IDB.</returns>
            internal static string Uninstall(UDID UDID, string bundleId)
            {
                return $"uninstall --udid {UDID} {bundleId}";
            }

            /// <summary>
            /// Command for performing file/directory operations on device.
            /// </summary>
            /// <param name="UDID">UDID of device to perform operation for.</param>
            /// <param name="bundleId">ID of application bundle that should be operated.</param>
            /// <param name="operation">Operation to perform (pull, mv, mkdir, rm, ls, and other supported operations).</param>
            /// <param name="path">Path of the file/directory inside application bundle's container on device.</param>
            /// <returns><see cref="string"/> with arguments for IDB.</returns>
            internal static string File(UDID UDID, string bundleId, string operation, string path)
            {
                return $"file {operation} --udid {UDID} --application {bundleId}/{path}";
            }

            internal static string ListApps(UDID UDID)
            {
                return $"list-apps --udid {UDID} --json";
            }
        }
    }
}

