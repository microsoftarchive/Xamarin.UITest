using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.UITest.Shared.Extensions;
using Xamarin.UITest.Utils;
using Xamarin.UITest.XDB.Services.Processes;
using Xamarin.UITest.XDB.Services.OSX;
using Xamarin.UITest.XDB.Services.OSX.IDB;
using Xamarin.UITest.XDB.Entities;
using Xamarin.UITest.XDB.Exceptions;
using Xamarin.UITest.XDB.Enums;
using Xamarin.UITest.MacOS.Utilities;

namespace Xamarin.UITest.XDB.Services
{
    class iOSDeviceAgentService : IiOSDeviceAgentService
    {
        readonly IDependenciesDeploymentService _dependenciesDeploymentService;
        readonly IEnvironmentService _environmentService;
        readonly IHttpService _httpService;
        readonly ILoggerService _loggerService;
        readonly IXcodeService xcodeService;

        static readonly TimeSpan _ShortWaitSeconds = TimeSpan.FromSeconds(0.5);
        static readonly TimeSpan _EndSessionWaitSeconds = TimeSpan.FromSeconds(60);

        const double _FlickDuration = 0.075;

        public iOSDeviceAgentService(
            IDependenciesDeploymentService dependenciesDeploymentService,
            IEnvironmentService environmentService,
            IHttpService httpService,
            ILoggerService loggerService,
            IXcodeService xcodeService)
        {
            _dependenciesDeploymentService = dependenciesDeploymentService;
            _environmentService = environmentService;
            _httpService = httpService;
            _loggerService = loggerService;
            this.xcodeService = xcodeService;
        }

        const int _deviceAgentPort = 27753;

        public async Task DeleteSessionAsync(string deviceAddress)
        {
            var timeout = _EndSessionWaitSeconds;

            var url = RouteUrl(deviceAddress, VersionedRoutes.Session);

            IHttpResult<StatusResult> deleteResult = null;

            try
            {
                deleteResult = await _httpService.DeleteAsync<StatusResult>(
                    url,
                    timeout: timeout).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new DeviceAgentException($"Unable to end session: {ex.Message}", ex);
            }

            deleteResult.Content.Validate("delete session");

            _loggerService.LogDebug(
                $"DeviceAgent delete session response: {deleteResult.StatusCode} - {deleteResult.Content?.Status}");
        }

        public async Task DoubleTouchAsync(string deviceAddress, PointF point)
        {
            await PointGestureAsync(deviceAddress, point, "double_tap").ConfigureAwait(false);
        }

        public async Task DragAsync(
            string deviceAddress,
            PointF from,
            PointF to,
            TimeSpan? duration,
            TimeSpan? holdTime,
            bool allowInertia = true)
        {


            object options;
            double holdTimeSeconds = 0.0;

            if (holdTime.HasValue)
            {
                holdTimeSeconds = holdTime.Value.TotalSeconds;
            }

            if (duration.HasValue)
            {
                options = new
                {
                    duration = duration.Value.TotalSeconds,
                    allow_inertia = allowInertia,
                    first_touch_hold_duration = holdTimeSeconds
                };

            } else {
                options = new
                {
                    allow_inertia = allowInertia,
                    first_touch_hold_duration = holdTimeSeconds
                };
            }

            var specifiers = new
            {
                coordinates = new[]
                {
                    new { x = from.X, y = from.Y },
                    new { x = to.X, y = to.Y }
                }
            };


            await GestureAsync(deviceAddress, "drag", options, specifiers).ConfigureAwait(false);
        }

        public async Task<UIElement> DumpElements(string deviceAddress)
        {
            return await TreeAsync(deviceAddress);
        }

        public async Task SetInputViewPickerWheelValueAsync(string deviceAddress, int pickerIndex, int wheelIndex, string value)
        {
            var options = new
            {
                picker_index = pickerIndex.ToString(),
                picker_wheel_index = wheelIndex.ToString(),
                picker_wheel_value = value
            };

            await GestureAsync(deviceAddress, "set_picker_wheel_value", options).ConfigureAwait(false);
        }

        public async Task EnterTextAsync(string deviceAddress, string text)
        {
            var options = new Dictionary<string, string>
            {
                { "string", text }
            };

            await GestureAsync(deviceAddress, "enter_text", options).ConfigureAwait(false);
        }

        public async Task FlickAsync(string deviceAddress, PointF from, PointF to)
        {
            var options = new { duration = _FlickDuration };

            var specifiers = new
            {
                coordinates = new[]
                {
                    new { x = from.X, y = from.Y },
                    new { x = to.X, y = to.Y }
                }
            };

            await GestureAsync(deviceAddress, "drag", options, specifiers).ConfigureAwait(false);
        }

        public async Task GestureAsync(
            string deviceAddress,
            string gesture,
            object options = null,
            object specifiers = null)
        {
            object data;

            if (specifiers != null)
            {
                data = new
                {
                    specifiers,
                    gesture,
                    options = options ?? new { }
                };
            }
            else
            {
                data = new
                {
                    gesture,
                    options = options ?? new { }
                };
            }

            var result = await RequestAsync<StatusResult>(
                deviceAddress,
                VersionedRoutes.Gesture,
                data,
                gesture).ConfigureAwait(false);

            _loggerService.LogDebug(
                $"DeviceAgent {gesture} response: {result.StatusCode} - {result.Content.Status}");
        }

        public async Task LaunchTestAsync(
            string deviceId,
            string deviceAddress)
        {
            var eventId = _loggerService.GetEventId();

            _loggerService.LogInfo(
                string.Concat(
                    $"{nameof(LaunchTestAsync)}:",
                    Environment.NewLine,
                    $"    {nameof(deviceId)}: {deviceId}"),
                eventId);

            var deviceAgentBundleVersion = _dependenciesDeploymentService.DeviceAgentBundleVersion;

            var versionResult = await VersionAsync(deviceAddress).ConfigureAwait(false);

            if (versionResult != null &&
                versionResult.Bundle_Version == deviceAgentBundleVersion)
            {
                await DeleteSessionAsync(deviceAddress).ConfigureAwait(false);
                _loggerService.LogInfo($"Using existing DeviceAgent server: '{deviceAgentBundleVersion}'");
                return;
            }

            _loggerService.LogDebug($"DeviceAgent '{deviceAgentBundleVersion}' not found, launching");

            // TODO: Why do we need that extra shutdown. We should test it and remove it if possible.
            await ShutdownAsync(deviceAddress, false).ConfigureAwait(false);

            if (!XdbServices.GetRequiredService<IEnvironmentService>().ShouldInstallDeviceAgent)
            {
                _loggerService.LogWarn("Automatical installation of DeviceAgent is disabled");
            }
            else
            {
                XdbServices.GetRequiredService<IIDBService>().InstallDeviceAgent(new UDID(UDID: deviceId));
            }

            var launchTask = Task.Run(() =>
                WithErrorHandling(
                    eventId,
                    () => StartTest(new UDID(UDID: deviceId)),
                    "Failed to launch DeviceAgent"));

            var pingTask = PingAsync(deviceAddress, 60, TimeSpan.FromSeconds(1), false);

            Task.WaitAny(new[] { launchTask, pingTask });

            if (launchTask.IsFaulted)
            {
                var aex = launchTask.Exception as AggregateException;
                if (aex != null)
                {
                    throw aex.InnerExceptions.First();
                }
                throw launchTask.Exception;
            }

            if (launchTask.IsCompleted)
            {
                throw new DeviceAgentException("DeviceAgent is not running");
            }

            if (pingTask.IsFaulted)
            {
                throw new DeviceAgentException("Unable to contact DeviceAgent", pingTask.Exception);
            }
        }

        public ProcessResult StartTest(UDID UDID)
        {
            var path = Path.Combine(Path.GetTempPath(),
                "xdb",
                "logs",
                DateTime.Now.ToString("yyyy.MM.dd.HHmmss"));

            Directory.CreateDirectory(path);


            var logsDir = path;
            var logsDerivedData = Path.Combine(logsDir, "DerivedData");
            return xcodeService.TestWithoutBuilding(
                UDID.ToString(),
                XCTestRunFilePath(UDID, logsDir),
                logsDerivedData);
        }

        private string XCTestRunFilePath(UDID UDID, string logsDir)
        {
            IIDBService idbService = XdbServices.GetRequiredService<IIDBService>();
            if (UDID.IsSimulator)
            {
                var runnerPath = Path.Combine(idbService.GetTempFolderPath(), _dependenciesDeploymentService.PathToSimTestRunner);

                var templatePath = Path.Combine(
                    runnerPath,
                    "PlugIns",
                    "DeviceAgent.xctest",
                    "DeviceAgent-simulator-template.xctestrun");

                var path = Path.Combine(logsDir, "DeviceAgent-simulator.xctestrun");

                File.WriteAllText(path, File.ReadAllText(templatePath).Replace("TEST_HOST_PATH", runnerPath));

                return path;
            }
            else
            {
                string xctestrunDevicePath = Path.Combine(
                    idbService.GetTempFolderPath(),
                    _dependenciesDeploymentService.PathToDeviceTestRunner,
                    "PlugIns",
                    "DeviceAgent.xctest",
                    "DeviceAgent-device.xctestrun");
                if (!_environmentService.ShouldInstallDeviceAgent)
                {
                    if (string.IsNullOrWhiteSpace(_environmentService.DeviceAgentBundleId))
                    {
                        throw new Exception(message: "DeviceAgentBundleId must be specified when installation is disabled");
                    }

                    PlistBuddy.SetValueForKey(xctestrunDevicePath, "TestTargetName:TestHostBundleIdentifier", _environmentService.DeviceAgentBundleId);
                }

                return xctestrunDevicePath;
            }
        }

        public async Task PinchAsync(
            string deviceAddress,
            PointF point,
            PinchDirection direction,
            float? amount = null,
            TimeSpan? duration = null)
        {
            var pinch_direction = direction.ToString("F").ToLower();
            object options;

            if (amount.HasValue && duration.HasValue)
            {
                options = new
                {
                    pinch_direction,
                    amount = amount.Value,
                    duration = duration.Value.TotalSeconds
                };
            }
            else if (duration.HasValue)
            {
                options = new
                {
                    pinch_direction,
                    duration = duration.Value.TotalSeconds
                };
            }
            else if (amount.HasValue)
            {
                options = new
                {
                    pinch_direction,
                    amount = amount.Value
                };
            }
            else
            {
                options = new
                {
                    pinch_direction
                };
            }

            await PointGestureAsync(deviceAddress, point, "pinch", options).ConfigureAwait(false);
        }

        public async Task PingAsync(
            string deviceAddress,
            int attempts = 1,
            TimeSpan? retryInterval = null,
            bool logErrors = true)
        {
            var url = RouteUrl(deviceAddress, VersionedRoutes.Ping);

            IHttpResult<StatusResult> result = null;

            try
            {
                result = await _httpService.GetAsync<StatusResult>(
                    url,
                    attempts: attempts,
                    retryInterval: retryInterval,
                    logErrors: logErrors,
                    timeout: _ShortWaitSeconds).ConfigureAwait(false);
            }
            catch (System.Exception ex)
            {
                throw new DeviceAgentException($"Unable to contact DeviceAgent on {deviceAddress}", ex);
            }

            if (result == null || result.Content == null) {
                throw new DeviceAgentException($"Unable to contact DeviceAgent on {deviceAddress}");
            }
            result.Content.Validate("ping");
        }

        public async Task<object> QueryAsync(string deviceAddress, object query)
        {
            var url = RouteUrl(deviceAddress, VersionedRoutes.Query);

            IHttpResult<object> result;

            try
            {
                result = await _httpService.PostAsJsonAsync<object>(url, query).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new DeviceAgentException($"DeviceAgent query failed: {ex.Message}", ex);
            }

            return result.Content;
        }

        public async Task SetOrientationAsync(string deviceAddress, DeviceOrientation orientation)
        {
            var data = new
            {
                orientation = (int)orientation
            };

            var result = await RequestAsync<SetOrientationResult>(
                deviceAddress,
                VersionedRoutes.RotateHomeButtonTo,
                data,
                "set orientation").ConfigureAwait(false);

            _loggerService.LogDebug(
                $"DeviceAgent set orientation response: {result.StatusCode} - {result.Content.Status}");
        }

        public async Task ShutdownAsync(string deviceAddress, bool errorIfUnavailable)
        {
            var timeout = _ShortWaitSeconds;
            var endSessionTimeout = _EndSessionWaitSeconds;

            var url = RouteUrl(deviceAddress, VersionedRoutes.Session);

            IHttpResult<StatusResult> deleteResult = null;

            try
            {
                deleteResult = await _httpService.DeleteAsync<StatusResult>(
                    url,
                    timeout: endSessionTimeout,
                    errorIfUnavailable: errorIfUnavailable).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new DeviceAgentException($"Unable to end session: {ex.Message}", ex);
            }

            if (errorIfUnavailable)
            {
                deleteResult.Content.Validate("delete session");
            }

            _loggerService.LogDebug(
                $"DeviceAgent delete session response: {deleteResult.StatusCode} - {deleteResult.Content?.Status}");

            url = RouteUrl(deviceAddress, VersionedRoutes.Shutdown);

            IHttpResult<ShutdownResult> shutdownResult = null;

            try
            {
                shutdownResult = await _httpService.PostAsync<ShutdownResult>(
                    url,
                    timeout: timeout,
                    errorIfUnavailable: errorIfUnavailable).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new DeviceAgentException($"Unable to shutdown DeviceAgent: {ex.Message}", ex);
            }

            if (errorIfUnavailable)
            {
                shutdownResult.Content.Validate("shutdown");
            }

            _loggerService.LogDebug(
                $"DeviceAgent shutdown response: {shutdownResult.StatusCode} - {shutdownResult.Content?.Message}");

            var started = DateTime.UtcNow;

            while (DateTime.UtcNow < started + TimeSpan.FromSeconds(15))
            {
                try
                {
                    await PingAsync(deviceAddress, logErrors: false).ConfigureAwait(false);
                }
                catch (DeviceAgentException)
                {
                    return;
                }
            }

            throw new DeviceAgentException("Unable to shut down existing DeviceAgent service");
        }

        public async Task DismissSpringboardAlertsAsync(string deviceAddress)
        {
            var result = await RequestAsync<StatusResult>(
                deviceAddress,
                VersionedRoutes.DismissSpringboardAlerts,
                null,
                "springboard-dismiss-alerts").ConfigureAwait(false);

            _loggerService.LogDebug(
                $"DeviceAgent springboard dismiss alerts response: {result.StatusCode} - {result.Content.Status}");
        }

        public async Task StartAppAsync(
            string deviceAddress,
            string bundleId,
            string launchArgs,
            string environmentVars)
        {
            IEnumerable<string> launchArgsList;
            IDictionary<string, string> environmentVarsDict;

            try
            {
                launchArgsList = JsonConvert.DeserializeObject<IEnumerable<string>>(launchArgs);
            }
            catch (System.Exception ex)
            {
                throw new ArgumentException("launchArgs", ex);
            }

            try
            {
                environmentVarsDict = JsonConvert.DeserializeObject<IDictionary<string, string>>(environmentVars);
            }
            catch (System.Exception ex)
            {
                throw new ArgumentException("environmentVars", ex);
            }

            await StartAppAsync(deviceAddress, bundleId, launchArgsList, environmentVarsDict).ConfigureAwait(false);
        }

        public async Task StartAppAsync(
            string deviceAddress,
            string bundleId,
            IEnumerable<string> launchArgs = null,
            IDictionary<string, string> environmentVars = null)
        {
            var data = new
            {
                bundleID = bundleId,
                launchArgs = launchArgs ?? new string[] { },
                environment = environmentVars ?? new Dictionary<string, string> { }
            };

            var result = await RequestAsync<StatusResult>(
                deviceAddress,
                VersionedRoutes.Session,
                data,
                "start session").ConfigureAwait(false);

            _loggerService.LogDebug(
                $"DeviceAgent Start Session response: {result.StatusCode} - {result.Content.Status}");
        }

        public async Task TouchAndHoldAsync(string deviceAddress, PointF point, TimeSpan? duration)
        {
            duration = duration ?? TimeSpan.FromSeconds(1);

            await PointGestureAsync(
                deviceAddress,
                point,
                "touch",
                new { duration = duration.Value.TotalSeconds }).ConfigureAwait(false);
        }

        public async Task TouchAsync(string deviceAddress, PointF point)
        {
            await PointGestureAsync(deviceAddress, point, "touch").ConfigureAwait(false);
        }

        public async Task TwoFingerTouchAsync(string deviceAddress, PointF point)
        {
            await PointGestureAsync(deviceAddress, point, "two_finger_tap").ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a <see cref="Task"/> which will request DeviceAgent's /tree command.
        /// This task will return top <see cref="UIElement"/> with all children elements as a tree.
        /// </summary>
        /// <param name="deviceAddress">Device's address.</param>
        /// <returns><see cref="Task"/> that will execute /tree request.</returns>
        async Task<UIElement> TreeAsync(string deviceAddress)
        {
            var url = RouteUrl(deviceAddress, VersionedRoutes.Tree);
            TimeSpan deviceAgentTreeCommandTimeout = TimeSpan.FromSeconds(2);

            IHttpResult<UIElement> result = await _httpService.GetAsync<UIElement>(
                url,
                timeout: deviceAgentTreeCommandTimeout,
                logErrors: false,
                errorIfUnavailable: false).ConfigureAwait(false);

            return result.Content;
        }

        async Task<VersionResult> VersionAsync(string deviceAddress)
        {
            var url = RouteUrl(deviceAddress, VersionedRoutes.Version);

            IHttpResult<VersionResult> result = null;

            result = await _httpService.GetAsync<VersionResult>(
                url,
                timeout: _ShortWaitSeconds,
                logErrors: false,
                errorIfUnavailable: false).ConfigureAwait(false);

            if (result.Content == null)
            {
                return null;
            }

            result.Content.Validate("version");

            return result.Content;
        }

        public async Task VolumeAsync(string deviceAddress, VolumeDirection direction)
        {
            var data = new {
                volume = direction.ToString("F").ToLower()
            };

            var result = await RequestAsync<VolumeResult>(
                deviceAddress,
                VersionedRoutes.Volume,
                data,
                "set volume").ConfigureAwait(false);

            if (result.Content.VolumeDirection != data.volume)
            {
                throw new DeviceAgentException(
                    $"DeviceAgent volume failed - volumeDirection: {result.Content.VolumeDirection}");
            }

            _loggerService.LogDebug(
                $"DeviceAgent volume response: {result.StatusCode} - {result.Content.Status}");
        }

        async Task<IHttpResult<T>> RequestAsync<T>(string deviceAddress, string route, object data, string action)
            where T : DeviceAgentResult
        {

            var url = RouteUrl(deviceAddress, route);

            IHttpResult<T> result;

            try
            {
                result = await _httpService.PostAsJsonAsync<T>(url, data).ConfigureAwait(false);
            }
            catch (OperationCanceledException cex)
            {
                throw new DeviceAgentException($"DeviceAgent {action} timed out", cex);
            }
            catch (Exception ex)
            {
                throw new DeviceAgentException($"DeviceAgent {action} failed: {ex.Message}", ex);
            }

            result.Content.Validate(action);

            return result;
        }

        async Task PointGestureAsync(string deviceAddress, PointF point, string gesture, object options = null)
        {
            var specifiers = new
            {
                coordinate = new
                {
                    x = point.X,
                    y = point.Y
                }
            };

            await GestureAsync(deviceAddress, gesture, options, specifiers).ConfigureAwait(false);
        }

        int WithErrorHandling(int eventId, Func<ProcessResult> action, string errorMessage, int[] successCodes = null)
        {
            successCodes = successCodes ?? new int[] { 0 };

            ProcessResult result;
            try
            {
                result = action();
            }
            catch (Exception ex)
            {
                _loggerService.LogError("Exception", eventId, ex);
                throw new DeviceAgentException($"{errorMessage}: {ex.Message}", ex);
            }

            if (!successCodes.Contains(result.ExitCode))
            {
                _loggerService.LogError("Unsuccessful", eventId);

                throw new DeviceAgentException(string.Concat(
                    errorMessage,
                    Environment.NewLine,
                    Environment.NewLine,
                    "ExitCode: ",
                    result.ExitCode,
                    Environment.NewLine,
                    result.CombinedOutput));
            }

            return result.ExitCode;
        }

        static string RouteUrl(string deviceAddress, string route)
        {
            var endpoint = $"http://{deviceAddress}:{_deviceAgentPort}";

            var deviceAgentUrlEnvVar = Environment.GetEnvironmentVariable("DEVICE_AGENT_URL");

            if (deviceAgentUrlEnvVar != null)
            {
                endpoint = deviceAgentUrlEnvVar.TrimEnd('/');
            }

            return $"{endpoint}/{route}";
        }

        internal class DeviceAgentResult
        {
            public string Error { get; set; }

            public virtual void Validate(string action)
            {
                if (!Error.IsNullOrWhiteSpace())
                {
                    throw new DeviceAgentException($"DeviceAgent {action} failed: {Error}");
                }
            }
        }

        internal class SetOrientationResult : StatusResult
        {
            public int Orientation { get; set; }
        }

        internal class StatusResult : DeviceAgentResult
        {
            public string Status { get; set; }
        }

        internal class ShutdownResult : DeviceAgentResult
        {
            public string Message { get; set; }
            public double Delay { get; set; }
        }

        internal class VersionResult : DeviceAgentResult
        {
            public string Bundle_Identifier { get; set; }
            public string Bundle_Name { get; set; }
            public string Bundle_Short_Version { get; set; }
            public string Bundle_Version { get; set; }
        }

        internal class VolumeResult : StatusResult
        {
            public string VolumeDirection { get; set; }
        }

        static class VersionedRoutes
        {
            public static string Gesture => VersionedRoute(Routes.Gesture);
            public static string Ping => VersionedRoute(Routes.Ping);
            public static string Query => VersionedRoute(Routes.Query);
            public static string RotateHomeButtonTo => VersionedRoute(Routes.RotateHomeButtonTo);
            public static string Session => VersionedRoute(Routes.Session);
            public static string Shutdown => VersionedRoute(Routes.Shutdown);
            public static string DismissSpringboardAlerts => VersionedRoute(Routes.DismissSpringboardAlerts);
            public static string Version => VersionedRoute(Routes.Version);
            public static string Volume => VersionedRoute(Routes.Volume);
            public static string Tree => VersionedRoute(Routes.Tree);

            static readonly ReadOnlyDictionary<string, string[]> _versionRoutes =
                new ReadOnlyDictionary<string, string[]>(new Dictionary<string, string[]>
                {
                    {
                        "1.0",
                        new string[]
                        {
                            Routes.Gesture,
                            Routes.Ping,
                            Routes.Query,
                            Routes.RotateHomeButtonTo,
                            Routes.Session,
                            Routes.Shutdown,
                            Routes.DismissSpringboardAlerts,
                            Routes.Version,
                            Routes.Volume,
                            Routes.Tree,
                        }
                    },
                });

            static readonly ReadOnlyDictionary<string, string> _routeVersions;

            static VersionedRoutes()
            {
                var routeVersions = new Dictionary<string, string>();
                foreach (var version in _versionRoutes.Keys)
                {
                    foreach (var route in _versionRoutes[version])
                    {
                        routeVersions.Add(route, version);
                    }
                }
                _routeVersions = new ReadOnlyDictionary<string, string>(routeVersions);
            }

            static string VersionedRoute(string route)
            {
                return $"{_routeVersions[route]}/{route}";
            }

            static class Routes
            {
                public static string Gesture => "gesture";
                public static string Ping => "ping";
                public static string Query => "query";
                public static string RotateHomeButtonTo => "rotate_home_button_to";
                public static string Session => "session";
                public static string Shutdown => "shutdown";
                public static string DismissSpringboardAlerts => "dismiss-springboard-alerts"; 
                public static string Version => "version";
                public static string Volume => "volume";
                public static string Tree => "tree";
            }
        }
    }
}
