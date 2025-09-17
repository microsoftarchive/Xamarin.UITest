using System;
using System.IO;
using System.Reflection;
using System.Text;
using Xamarin.UITest.XDB.Services.Processes;

namespace Xamarin.UITest.XDB.Services
{
    class DependenciesDeploymentService : IDependenciesDeploymentService
    {
        readonly IProcessService _processService;

        const string HashResource = "Xamarin.UITest.XDB.dependencies_hash.txt";
        const string DependenciesResource = "Xamarin.UITest.XDB.dependencies.zip";
        const string DeviceAgentBundleVersionResource = "Xamarin.UITest.XDB.deviceagent_cfbundleversion.txt";
        const string DeviceAgentRunnerApp = "DeviceAgent-Runner.app";
        static Lazy<string> _deviceAgentBundleVersion = new Lazy<string>(() => {
            using (var versionStream = XDBCoreAssembly.GetManifestResourceStream(DeviceAgentBundleVersionResource))
            {
                using (var reader = new StreamReader(versionStream, Encoding.UTF8))
                {
                    return reader.ReadToEnd().Trim();
                }
            }
        });

        static Lazy<string> _hash = new Lazy<string>(() => {
            using (var versionStream = XDBCoreAssembly.GetManifestResourceStream(HashResource))
            {
                using (var reader = new StreamReader(versionStream, Encoding.UTF8))
                {
                    return reader.ReadToEnd().Trim();
                }
            }
        });

        static Assembly XDBCoreAssembly => typeof(DependenciesDeploymentService).GetTypeInfo().Assembly;

        public string DeviceAgentBundleVersion => _deviceAgentBundleVersion.Value;

        public string HashId =>  _hash.Value;

        public string PathToDeviceTestRunner { get; } = Path.Combine("ipa", DeviceAgentRunnerApp);

        public string PathToSimTestRunner { get; } = Path.Combine("app", DeviceAgentRunnerApp);

        private ILoggerService LoggerService = XdbServices.GetRequiredService<ILoggerService>();

        public DependenciesDeploymentService(IProcessService processService)
        {
            _processService = processService;
        }

        public void Install(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (Directory.GetFiles(directory).Length > 0)
            {
                LoggerService.LogDebug("Dependencies deployment directory exists. Skipping installation...");
                return;
            }

            var tempZipPath = Path.Combine(directory, "dependencies.zip");

            using (var dependenciesStream = XDBCoreAssembly.GetManifestResourceStream(DependenciesResource))
            {
                using (var tempZip = File.Create(tempZipPath))
                {
                    dependenciesStream.CopyTo(tempZip);
                }
            }

            var unzipCommand = "/usr/bin/unzip";
            var unzipArguments = $"-q {tempZipPath} -d {directory}";

            var unzipResult = _processService.Run(unzipCommand, unzipArguments);

            if (unzipResult.ExitCode != 0)
            {
                throw new IOException(
                    $"Unpacking dependencies failed: {unzipCommand} {unzipArguments}");
            }

            File.Delete(tempZipPath);
        }
    }
}