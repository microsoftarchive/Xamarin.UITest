using NSubstitute;
using Xamarin.UITest.XDB.Services;
using Xamarin.UITest.XDB.Services.OSX;
using Xamarin.UITest.XDB.Services.OSX.IDB;
using Xamarin.UITest.XDB.Services.Processes;

namespace Xamarin.UITest.Tests.XDB.iOSDeviceAgentTests
{
    public abstract class iOSDeviceAgentServiceTestBase
    {
        protected const string DefaultBundleId = "bundle";
        protected const string DefaultDeviceId = "1234";
        protected const string DefaultSimulatorGuidId = "159be932-a1f5-4ae9-98bd-bbc356482160";
        protected const string AppBundlePath = "usr/apps/test.app";
        protected const string DefaultDeviceAddress = "127.0.0.1";

        // http://127.0.0.1:27753/1.0/session
        protected readonly string _defaultSessionUrl = $"http://{DefaultDeviceAddress}:27753/1.0/session";
        // http://127.0.0.1:27753/1.0/ping
        protected readonly string _defaultPingUrl = $"http://{DefaultDeviceAddress}:27753/1.0/ping";
        // http://127.0.0.1:27753/1.0/shutdown
        protected readonly string _defaultShutdownUrl = $"http://{DefaultDeviceAddress}:27753/1.0/shutdown";

        internal IEnvironmentService EnvironmentService;
        internal ILoggerService LoggerService;
        internal IDependenciesDeploymentService DependencyDeploymentService;
        internal IProcessService ProcessService;
        internal IHttpService HttpService;
        internal IXcodeService XcodeService;

        public iOSDeviceAgentServiceTestBase()
        {
            EnvironmentService = Substitute.For<IEnvironmentService>();
            DependencyDeploymentService = Substitute.For<IDependenciesDeploymentService>();
            ProcessService = Substitute.For<IProcessService>();
            HttpService = Substitute.For<IHttpService>();
            LoggerService = Substitute.For<ILoggerService>();
            XcodeService = Substitute.For<IXcodeService>();
        }

        internal iOSDeviceAgentService InitialiseIOSDeviceAgentService(
            IIDBService idbService = null,
            IHttpService httpService = null
        )
        {
            return new iOSDeviceAgentService(
                DependencyDeploymentService,
                new EnvironmentService(),
                httpService ?? HttpService,
                LoggerService,
                XcodeService
            );
        }

        internal IDBService CreateIDBService(
            string deviceId,
            int installAppExitCode = 0,
            int launchSimulatorExitCode = 0,
            int isInstalledExitCode = 0,
            int installDeviceAgentExitCode = 0
        )
        {
            var idbService = new IDBService(
                environmentService: EnvironmentService,
                dependenciesDeploymentService: DependencyDeploymentService,
                loggerService: LoggerService,
                processService: ProcessService);
            idbService.IDBCommandProvider.InstallApp(UDID: new UDID(deviceId), pathToBundle: AppBundlePath)
                .Returns(new ProcessResult(installAppExitCode, "", "", ""));
            idbService.IDBCommandProvider.LaunchSimulator(UDID: new UDID(deviceId))
                .Returns(new ProcessResult(launchSimulatorExitCode, "", "", ""));

            idbService.InstallDeviceAgent(UDID: new UDID(deviceId));

            return idbService;
        }
    }
}
