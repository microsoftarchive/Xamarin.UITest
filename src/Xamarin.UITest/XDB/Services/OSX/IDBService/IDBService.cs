using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xamarin.UITest.iOS.ApplicationSigning.Entities;
using Xamarin.UITest.XDB.Entities;
using Xamarin.UITest.XDB.Exceptions;
using Xamarin.UITest.XDB.Services.Processes;

namespace Xamarin.UITest.XDB.Services.OSX.IDB
{
    /// <summary>
    /// Class for managing iOS simulators and physical devices via IDB.
    /// </summary>
    internal class IDBService : IIDBService
    {
        private readonly IDependenciesDeploymentService DependenciesDeploymentService;
        /// <summary>
        /// <see cref="ILoggerService"/> - service for writing logs.
        /// </summary>
        private readonly ILoggerService LoggerService;

        /// <summary>
        /// Path to the temporary folder which contains deployed embedded resources such as DeviceAgent.
        /// </summary>
        private readonly string TempFolderPath;

        internal readonly IDBCommandProvider IDBCommandProvider;

        /// <summary>
        /// Creates new instance of <see cref="IDBService"/>.
        /// This service is responsible for managing devices
        /// (creating simulators, installing/uninstalling applications on devices etc.).
        /// </summary>
        public IDBService(
            IEnvironmentService environmentService,
            IDependenciesDeploymentService dependenciesDeploymentService,
            ILoggerService loggerService,
            IProcessService processService)
        {
            DependenciesDeploymentService = dependenciesDeploymentService;
            LoggerService = loggerService;
            TempFolderPath = Path.Combine(
                    Path.GetTempPath(),
                    "xdb",
                    "DeviceAgent.iOS.Dependencies",
                    dependenciesDeploymentService.HashId);
            IDBCommandProvider = new IDBCommandProvider(
                environmentService: environmentService,
                dependenciesDeploymentService: dependenciesDeploymentService,
                loggerService: loggerService,
                processService: processService,
                tempFolderPath: TempFolderPath);
        }

        #region Helper methods.

        internal ProcessResult ExecuteCommandWithErrorHandling(int eventId, Func<ProcessResult> command, string errorMessage, int[] successCodes = null)
        {
            successCodes ??= new int[] { 0 };

            ProcessResult result;
            try
            {
                result = command();
            }
            catch (Exception ex)
            {
                LoggerService.LogError("Exception", eventId, ex);
                throw new IDBException($"{errorMessage}: {ex.Message}", ex);
            }

            if (!successCodes.Contains(result.ExitCode))
            {
                LoggerService.LogError("Unsuccessful", eventId);

                throw new IDBException(string.Concat(
                    errorMessage,
                    Environment.NewLine,
                    Environment.NewLine,
                    "ExitCode: ",
                    result.ExitCode,
                    Environment.NewLine,
                    result.CombinedOutput));
            }

            return result;
        }

        private void LogEvent(int eventId, string eventName, Dictionary<string, string> parameters)
        {
            StringBuilder logMessageBuilder = new StringBuilder();
            logMessageBuilder.Append($"{eventName}:");
            logMessageBuilder.Append(Environment.NewLine);
            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                logMessageBuilder.Append(Environment.NewLine);
                logMessageBuilder.Append($"    {parameter.Key}: {parameter.Value}");
            }

            LoggerService.LogInfo(logMessageBuilder.ToString(), eventId);
        }

        internal ProcessResult ExecuteCommand(Func<ProcessResult> command, string commandName, UDID UDID, Dictionary<string, string> parameters, string errorMessage)
        {
            int eventId = LoggerService.GetEventId();

            if (UDID.IsSimulator)
            {
                LaunchSimulator(UDID: UDID, eventId: eventId);
            }

            LogEvent(eventId: eventId, eventName: commandName, parameters: parameters);
            return ExecuteCommandWithErrorHandling(eventId: eventId, command: command, errorMessage: errorMessage);
        }

        internal void LaunchSimulator(UDID UDID, int eventId)
        {
            LoggerService.LogInfo("Launching simulator if not already running.", eventId);

            ExecuteCommandWithErrorHandling(
                eventId,
                () => IDBCommandProvider.LaunchSimulator(UDID: UDID),
                "Failed to launch simulator");
            ExecuteCommandWithErrorHandling(
                eventId,
                () => IDBCommandProvider.FocusSimulator(UDID: UDID),
                "Failed to bring simulator window to foreground");
        }

        private static List<iOSAppInfo> DeserializeiOSAppInfosListFromString(string listAppsOutput)
        {
            List<string> listOfAppInfosAsJSONObjects = listAppsOutput.Split('\n').ToList();

            List<iOSAppInfo> listOfAppInfos = new List<iOSAppInfo>();
            foreach (string appInfoAsJSONObject in listOfAppInfosAsJSONObjects)
            {
                listOfAppInfos.Add(JsonConvert.DeserializeObject<iOSAppInfo>(appInfoAsJSONObject, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
            }

            return listOfAppInfos;
        }

        #endregion

        #region Public methods for performing IDB interactions.

        /// <summary>
        /// Gets the temporary folder with unpacked DeviceAgent.
        /// </summary>
        public string GetTempFolderPath()
        {
            return TempFolderPath;
        }

        public string GetDeviceAgentBundlePathForPhysicalDevice()
        {
            return Path.Combine(path1: TempFolderPath, path2: DependenciesDeploymentService.PathToDeviceTestRunner);
        }

        /// <summary>
        /// Clears XCAppData inside application bundle's container.
        /// </summary>
        /// <param name="UDID"><see cref="string"/> with UDID of device where application bundle's container is located.</param>
        /// <param name="bundleId">ID of application bundle.</param>
        public void ClearXCAppData(UDID UDID, string bundleId)
        {
            if (!IsAppInstalled(UDID: UDID, bundleId: bundleId))
            {
                return;
            }

            // To clear XCAppData inside application bundle's container
            // we need to empty Documents, Library and tmp directories.
            // To do so first we need to delete them.
            List<string> directories = new() { "Documents", "Library", "tmp" };
            foreach (string directoryToRemove in directories)
            {
                ExecuteCommand(
                command: () => IDBCommandProvider.RemoveDirectory(UDID: UDID, bundleId: bundleId, relativePathToDirectory: directoryToRemove),
                commandName: nameof(ClearXCAppData),
                UDID: UDID,
                parameters: new Dictionary<string, string> { [nameof(bundleId)] = bundleId, [nameof(directoryToRemove)] = directoryToRemove },
                errorMessage: $"Failed to clear data for app {bundleId}: Failed to remove directory: {directoryToRemove}.");
            }

            // Then we creating new ones that should be empty.
            foreach (string directoryToCreate in directories)
            {
                ExecuteCommand(
                command: () => IDBCommandProvider.CreateDirectory(UDID: UDID, bundleId: bundleId, relativePathToDirectory: directoryToCreate),
                commandName: nameof(ClearXCAppData),
                UDID: UDID,
                parameters: new Dictionary<string, string> { [nameof(bundleId)] = bundleId, [nameof(directoryToCreate)] = directoryToCreate },
                errorMessage: $"Failed to clear data for app {bundleId}: Failed to create empty directory: {directoryToCreate}.");
            }
        }

        /// <summary>
        /// Installs application bundle to the specified device.
        /// </summary>
        /// <param name="UDID"><see cref="UDID"/> of device to perform installation on.</param>
        /// <param name="pathToBundle">Source path of application bundle that should be installed.</param>
        public void InstallApp(UDID UDID, string pathToBundle)
        {
            ExecuteCommand(
                command: () => IDBCommandProvider.InstallApp(UDID: UDID, pathToBundle: pathToBundle),
                commandName: nameof(InstallApp),
                UDID: UDID,
                parameters: new Dictionary<string, string> { [nameof(UDID)] = UDID.ToString(), [nameof(pathToBundle)] = pathToBundle },
                errorMessage: $"Failed to install app {pathToBundle}");
        }

        public bool IsAppInstalled(UDID UDID, string bundleId)
        {
            ProcessResult commandExecutionResult = ExecuteCommand(
                command: () => IDBCommandProvider.GetAppsList(UDID: UDID),
                commandName: nameof(IsAppInstalled),
                UDID: UDID,
                parameters: new Dictionary<string, string> { [nameof(UDID)] = UDID.ToString() },
                errorMessage: $"Failed to get list of applications installed on device {UDID}");

            List<iOSAppInfo> listOfAppInfos = DeserializeiOSAppInfosListFromString(commandExecutionResult.StandardOutput);
            return listOfAppInfos.Where(x => x.BundleId == bundleId).Any();
        }

        /// <summary>
        /// Installs DeviceAgent on the specified device.
        /// </summary>
        /// <param name="UDID"><see cref="UDID"/> with target device's UDID.</param>
        public void InstallDeviceAgent(UDID UDID)
        {
            ExecuteCommand(
                command: () => IDBCommandProvider.InstallDeviceAgent(UDID: UDID),
                commandName: nameof(InstallDeviceAgent),
                UDID: UDID,
                parameters: new Dictionary<string, string>{ [nameof(UDID)] = UDID.ToString() },
                errorMessage: "Failed to install DeviceAgent");
        }

        /// <summary>
        /// Set simulated location for device.
        /// </summary>
        /// <param name="UDID"><see cref="string"/> with UDID of device to simulate location for.</param>
        /// <param name="latLong"><see cref="LatLong"/> of location that should be set.</param>
        public void SetLocation(UDID UDID, LatLong latLong)
        {
            ExecuteCommand(
                command: () => IDBCommandProvider.SetLocation(UDID: UDID, latLong: latLong),
                commandName: nameof(SetLocation),
                UDID: UDID,
                parameters: new Dictionary<string, string> { [nameof(UDID)] = UDID.ToString(), [nameof(latLong)] = latLong.ToString() },
                errorMessage: "Unable to set location");
        }

        /// <summary>
        /// Stops simulating of location for device.
        /// </summary>
        /// <param name="UDID"><see cref="string"/> with UDID of device to stop location simulation for.</param>
        public void StopSimulatingLocation(UDID UDID)
        {
            ExecuteCommand(
                command: () => IDBCommandProvider.StopSimulatingLocation(UDID: UDID),
                commandName: nameof(StopSimulatingLocation),
                UDID: UDID,
                parameters: new Dictionary<string, string> { [nameof(UDID)] = UDID.ToString() },
                errorMessage: "Unable to stop location simulation");
        }

        /// <summary>
        /// Uninstalls application bundle from device.
        /// </summary>
        /// <param name="UDID"><see cref="string"/> with UDID of device to uninstall bundle from.</param>
        /// <param name="bundleId">Application bundle's identifier.</param>
        public void UninstallApp(UDID UDID, string bundleId)
        {
            ExecuteCommand(
                command: () => IDBCommandProvider.UninstallApp(UDID: UDID, bundleId: bundleId),
                commandName: nameof(UninstallApp),
                UDID: UDID,
                parameters: new Dictionary<string, string> { [nameof(UDID)] = UDID.ToString(), [nameof(bundleId)] = bundleId },
                errorMessage: $"Failed to uninstall app {bundleId}");
        }

        #endregion
    }
}

