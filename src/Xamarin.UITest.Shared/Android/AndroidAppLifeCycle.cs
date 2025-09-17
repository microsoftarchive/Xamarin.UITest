using System;
using System.Linq;
using Xamarin.UITest.Shared.Android.Commands;
using Xamarin.UITest.Shared.Artifacts;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Logging;
using Xamarin.UITest.Shared.Android.Queries;
using Xamarin.UITest.Shared.Http;

namespace Xamarin.UITest.Shared.Android
{
    public class AndroidAppLifeCycle
    {
        readonly IExecutor _executor;
        readonly string _deviceSerial;
        readonly ArtifactFolder _artifactFolder;
        readonly string _launchableActivity;
        readonly bool _clearAppData;
        readonly HttpApplicationStarter _applicationStarter;

        public AndroidAppLifeCycle(IExecutor executor, string deviceSerial, ArtifactFolder artifactFolder, string launchableActivity, bool clearAppData)
        {
            _executor = executor;
            _deviceSerial = deviceSerial;
            _artifactFolder = artifactFolder;
            _launchableActivity = launchableActivity;
            _clearAppData = clearAppData;
        }

        public AndroidAppLifeCycle(HttpClient httpClient, IExecutor executor, string deviceSerial, ArtifactFolder artifactFolder, string launchableActivity, bool clearAppData) :
                this(executor, deviceSerial, artifactFolder, launchableActivity, clearAppData)
        {
            _applicationStarter = new HttpApplicationStarter(httpClient);
        }

        public void EnsureInstalled(ApkFile appApkFile, ApkFile testServerApkFile)
        {
            var tokenStorage = new TokenStorage(_executor, _artifactFolder);

            if (tokenStorage.HasMatchingTokens(_deviceSerial, appApkFile, testServerApkFile))
            {
                Log.Info("Skipping installation: Already installed.");

                if (_clearAppData)
                {
                    _executor.Execute(new CommandAdbClearAppData(_deviceSerial,
                     appApkFile.PackageName, testServerApkFile.PackageName
                     ));
                }
            }
            else
            {
                var installedPackages = _executor.Execute(new QueryAdbInstalledPackageNames(_deviceSerial));

                if (installedPackages.Contains(appApkFile.PackageName))
                {
                    UninstallApps(appApkFile);
                }
                else
                {
                    Log.Debug($"Skipping uninstall: {appApkFile.PackageName} not installed.");
                }

                if (installedPackages.Contains(testServerApkFile.PackageName))
                {
                    UninstallApps(testServerApkFile);
                }
                else
                {
                    Log.Debug($"Skipping uninstall: {testServerApkFile.PackageName} not installed.");
                }

                InstallApps(appApkFile, testServerApkFile);
                tokenStorage.SaveTokens(_deviceSerial, appApkFile, testServerApkFile);
            }
        }

        public void LaunchApp(ApkFile appApkFile, ApkFile testServerApkFile, int testServerPort)
        {
            LaunchApp(appApkFile.PackageName, testServerApkFile, testServerPort);
        }

        public void LaunchApp(string appPackageName, ApkFile testServerApkFile, int testServerPort)
        {
            if (_executor.Execute(new QueryAdbKeyguardEnabled(_deviceSerial)))
            {
                _executor.Execute(new CommandAdbWakeUp(_deviceSerial, testServerApkFile));
                Log.Debug("Woke device up.");
            }

            _executor.Execute(new CommandAdbInstrument(_deviceSerial, appPackageName, testServerApkFile.PackageName, testServerPort, _launchableActivity));
            Log.Debug($"launchable activity set as {_launchableActivity}");

            _executor.Execute(new CommandAdbPortForward(_deviceSerial, testServerPort));
            Log.Debug(string.Format("Forwarded port {0}.", testServerPort));

            _applicationStarter.Execute();
        }

        public void EnsureInstalled(string appPackageName, ApkFile testServerApkFile)
        {
            var tokenStorage = new TokenStorage(_executor, _artifactFolder);

            var installedPackages = _executor.Execute(new QueryAdbInstalledPackageNames(_deviceSerial));

            if (!installedPackages.Contains(appPackageName))
            {
                throw new Exception(string.Format("Unable to launch. The app with package name: {0} is not installed.", appPackageName));
            }

            if (tokenStorage.HasMatchingTokens(_deviceSerial, testServerApkFile))
            {
                Log.Info("Skipping installation: Already installed.");
            }
            else
            {
                if (installedPackages.Contains(testServerApkFile.PackageName))
                {
                    UninstallApps(testServerApkFile);
                }
                else
                {
                    Log.Debug("Skipping test server uninstall: App not installed.");
                }

                InstallApps(testServerApkFile);
                tokenStorage.SaveTokens(_deviceSerial, testServerApkFile);
            }

            if (_clearAppData)
            {
                _executor.Execute(new CommandAdbClearAppData(_deviceSerial,
                 appPackageName, testServerApkFile.PackageName
                 ));
            }
        }

        void UninstallApps(params ApkFile[] apkFiles)
        {
            foreach (var apkFile in apkFiles)
            {
                _executor.Execute(new CommandAdbUninstallPackage(_deviceSerial, apkFile));
                Log.Debug(string.Format("Uninstalled {0}.", apkFile.PackageName));
            }
        }
        void InstallApps(params ApkFile[] apkFiles)
        {
            try
            {
                foreach (var apkFile in apkFiles)
                {
                    _executor.Execute(new CommandAdbInstallPackage(_deviceSerial, apkFile));
                    Log.Debug(string.Format("Installed {0}.", apkFile.PackageName));
                }
            }
            catch
            {
                const int UNABLE_TO_INSTALL_APP_EXIT_CODE = 110;
                Environment.Exit(UNABLE_TO_INSTALL_APP_EXIT_CODE);
            }

        }
    }
}