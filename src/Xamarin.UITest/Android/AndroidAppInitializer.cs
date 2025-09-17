using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xamarin.UITest.Configuration;
using Xamarin.UITest.Shared;
using Xamarin.UITest.Shared.Android;
using Xamarin.UITest.Shared.Android.Commands;
using Xamarin.UITest.Shared.Android.Queries;
using Xamarin.UITest.Shared.Artifacts;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Extensions;
using Xamarin.UITest.Shared.Http;
using Xamarin.UITest.Shared.Logging;
using Xamarin.UITest.Shared.Resources;
using Xamarin.UITest.Shared.Screenshots;
using Xamarin.UITest.Utils;

namespace Xamarin.UITest.Android
{
    internal class AndroidAppInitializer
    {
        readonly IAndroidAppConfiguration _appConfiguration;
        readonly IExecutor _executor;
        readonly IWaitTimes _waitTimes;

        public AndroidAppInitializer(IAndroidAppConfiguration appConfiguration, IExecutor executor, IWaitTimes waitTimes)
        {
            _appConfiguration = appConfiguration;
            _executor = executor;
            _waitTimes = waitTimes;
        }

        public void VerifyConfiguration()
        {
            if (ShouldOverrideApks())
            {
                return;
            }
            _appConfiguration.Verify();
        }

        public bool ShouldCreateTestServer()
        {
            return false;
        }

        static bool ShouldOverrideApks()
        {
            return false;
        }

        public AndroidDeps PrepareEnvironment()
        {
            Log.Info("Android test running Xamarin.UITest version: " + Assembly.GetExecutingAssembly().GetName().Version);

            IScreenshotTaker screenshotTaker;

            var waitForHelper = new WaitForHelper(_waitTimes.WaitForTimeout);
            var androidConfig = new AndroidConfig(_appConfiguration.DeviceUri, _appConfiguration.ApkFile);

            ArtifactFolder artifactFolder;
            string deviceSerial = GetDeviceSerial(_appConfiguration);
            var httpClient = new HttpClient(_appConfiguration.DeviceUri);
            var gestures = new AndroidGestures(httpClient, waitForHelper, _waitTimes);
            var monkeyStarter = new CommandAdbStartMonkey(deviceSerial);

            // In TestCloudAndroidAppConfiguration, StartAction will always be
            // StartAction.LaunchApp - so this is a branching on whether the
            // runtime environment is TestCloud.
            if (_appConfiguration.StartAction == StartAction.ConnectToApp)
            {
                artifactFolder = new ArtifactFolder();

                Log.Info(string.Format("Initializing Android app on device {0}.", deviceSerial));

                // For some reason the start action always has the
                // JavaScreenshotTaker?
                //
                // In other contexts (see below in this method) the screenshot
                // taker is controlled by _appConfiguration.EnableScreenshot
                screenshotTaker = new JavaScreenshotTaker(artifactFolder, deviceSerial, _executor);

                var connectToAppLifeCycle = new AndroidAppLifeCycle(httpClient,
                        _executor, deviceSerial, artifactFolder, _appConfiguration.LaunchableActivity, false);

                return new AndroidDeps(
                    gestures,
                    screenshotTaker,
                    waitForHelper,
                    androidConfig,
                    artifactFolder,
                    connectToAppLifeCycle,
                    _appConfiguration,
                    new SharedTestServer(httpClient),
                    monkeyStarter,
                    deviceSerial);
            }

            var resourceLoader = new EmbeddedResourceLoader();
            var testServerHash = resourceLoader.GetEmbeddedResourceSha1Hash(typeof(AndroidTestServerFactory).Assembly, "TestServer.apk");

            var artifactDependencies = new List<object> { testServerHash };

            if (_appConfiguration.KeyStore != null)
            {
                artifactDependencies.Add(_appConfiguration.KeyStore);
                artifactDependencies.Add(_appConfiguration.KeyStoreKeyAlias);
                artifactDependencies.Add(_appConfiguration.KeyStoreKeyPassword);
                artifactDependencies.Add(_appConfiguration.KeyStorePassword);
            }

            if (_appConfiguration.SIFile != null)
            {
                artifactDependencies.Add(_appConfiguration.SIFile);
            }

            if (_appConfiguration.ApkFile != null)
            {
                artifactDependencies.Add(_appConfiguration.ApkFile);
                Log.Info(string.Format("Initializing Android app on device {0} with apk: {1}", deviceSerial, _appConfiguration.ApkFile.FullName));
            }
            else
            {
                artifactDependencies.Add(_appConfiguration.InstalledAppPackageName);
                Log.Info(string.Format("Initializing Android app on device {0} with installed app: {1}", deviceSerial, _appConfiguration.InstalledAppPackageName));
            }

            if (ShouldOverrideApks())
            {
                artifactFolder = new ArtifactFolder();
            }
            else
            {
                artifactFolder = new ArtifactFolder(artifactDependencies.ToArray());
            }

            if (_appConfiguration.EnableScreenshots)
            {
                screenshotTaker = new JavaScreenshotTaker(artifactFolder, deviceSerial, _executor);
            }
            else
            {
                Log.Info("Skipping local screenshots. Can be enabled with EnableScreenshots() when configuring app.");
                screenshotTaker = new NullScreenshotTaker();
            }

            bool clearAppData = _appConfiguration.AppDataMode != AppDataMode.DoNotClear;

            var localAppLifeCycle = new AndroidAppLifeCycle(
                httpClient,
                _executor,
                deviceSerial,
                artifactFolder,
                _appConfiguration.LaunchableActivity,
                clearAppData);

            return new AndroidDeps(
                gestures,
                screenshotTaker,
                waitForHelper,
                androidConfig,
                artifactFolder,
                localAppLifeCycle,
                _appConfiguration,
                new SharedTestServer(httpClient),
                monkeyStarter,
                deviceSerial);
        }

        public TestApkFiles PrepareApkFiles(IAndroidAppConfiguration appConfiguration, ArtifactFolder artifactFolder)
        {
            //if (ShouldOverrideApks() && !ShouldCreateTestServer())
            //{
            //    return GetOverriddenApks();
            //}

            ApkFile testServerApkFile;

            if (!appConfiguration.InstalledAppPackageName.IsNullOrWhiteSpace())
            {
                if (appConfiguration.SIFile != null)
                {
                    Log.Info("Using signing info file to create signed test server.");

                    var testServerFactory = new AndroidTestServerFactory(_executor);
                    testServerApkFile = testServerFactory.BuildTestServerWithSi(appConfiguration.InstalledAppPackageName, appConfiguration.SIFile, artifactFolder);
                }
                else if (appConfiguration.KeyStore == null || !appConfiguration.KeyStore.Exists)
                {
                    if (appConfiguration.KeyStore != null)
                    {
                        Log.Info("KeyStore was set but file not found, using default keystore.");
                    }

                    var keyStore = GetDefaultKeyStore(artifactFolder);

                    var testServerFactory = new AndroidTestServerFactory(_executor);
                    testServerApkFile = testServerFactory.BuildTestServer(appConfiguration.InstalledAppPackageName, keyStore, artifactFolder);
                }
                else
                {
                    var keyStore = new KeyStore(_executor, appConfiguration.KeyStore, appConfiguration.KeyStoreKeyAlias, appConfiguration.KeyStorePassword, appConfiguration.KeyStoreKeyPassword);

                    var testServerFactory = new AndroidTestServerFactory(_executor);
                    testServerApkFile = testServerFactory.BuildTestServer(appConfiguration.InstalledAppPackageName, keyStore, artifactFolder);
                }

                return new TestApkFiles(null, testServerApkFile);
            }

            ApkFile appApkFile;

            if (appConfiguration.SIFile != null)
            {
                Log.Info("Using signing info file to create signed application under test.");
                appApkFile = new ApkFile(appConfiguration.ApkFile.FullName, _executor);
                appApkFile.EnsureDotNetAssembliesAreBundled();
                appApkFile.EnsureInternetPermission();

                var testServerFactory = new AndroidTestServerFactory(_executor);
                testServerApkFile = testServerFactory.BuildTestServerWithSi(appApkFile, appConfiguration.SIFile, artifactFolder);
            }
            else if (appConfiguration.KeyStore == null)
            {
                var keyStore = GetDefaultKeyStore(artifactFolder);

                appApkFile = keyStore.ResignApk(artifactFolder, appConfiguration.ApkFile.FullName);
                appApkFile.EnsureDotNetAssembliesAreBundled();
                appApkFile.EnsureInternetPermission();

                var testServerFactory = new AndroidTestServerFactory(_executor);
                testServerApkFile = testServerFactory.BuildTestServer(appApkFile, keyStore, artifactFolder);
            }
            else
            {
                var keyStore = new KeyStore(_executor, appConfiguration.KeyStore, appConfiguration.KeyStoreKeyAlias, appConfiguration.KeyStorePassword, appConfiguration.KeyStoreKeyPassword);
                appApkFile = new ApkFile(appConfiguration.ApkFile.FullName, _executor);
                appApkFile.EnsureDotNetAssembliesAreBundled();
                appApkFile.EnsureInternetPermission();

                _executor.Execute(new CommandCheckZipAlign(appApkFile));

                var testServerFactory = new AndroidTestServerFactory(_executor);
                testServerApkFile = testServerFactory.BuildTestServer(appApkFile, keyStore, artifactFolder);
            }

            return new TestApkFiles(appApkFile, testServerApkFile);
        }

        KeyStore GetDefaultKeyStore(ArtifactFolder artifactFolder)
        {
            const string storeName = "xuitest";
            const string keyAlias = "xuitest";
            const string storePassword = "xuitest";
            const string keyPassword = "xuitest";

            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var xamarinKeyStorePath = Path.Combine(appDataFolder, @"Xamarin", "Mono for Android", storeName);

            if (File.Exists(xamarinKeyStorePath))
            {
                Log.Info("Signing apk with Xamarin keystore.");
                return new KeyStore(_executor, new FileInfo(xamarinKeyStorePath), keyAlias, storePassword, keyPassword);
            }

            Log.Info("Signing apk with internal keystore.");

            var devKeyStorePath = artifactFolder.CreateArtifact("dev.keystore", path => _executor.Execute(new CommandGenerateDevKeyStore(path, keyAlias, storePassword, keyPassword)));
            return new KeyStore(_executor, new FileInfo(devKeyStorePath), keyAlias, storePassword, keyPassword);
        }

        string GetDeviceSerial(IAndroidAppConfiguration appConfiguration)
        {
            var deviceSerials = _executor.Execute(new QueryAdbDevices());

            if (!appConfiguration.DeviceSerial.IsNullOrWhiteSpace())
            {
                if (deviceSerials.Contains(appConfiguration.DeviceSerial))
                {
                    return appConfiguration.DeviceSerial;
                }

                throw new Exception(string.Format("Configured device serial '{0}' not found. DeviceSerial does not need to be specified if only 1 device is connected. Devices found: {1}", appConfiguration.DeviceSerial, string.Join(", ", deviceSerials)));
            }

            if (!deviceSerials.Any())
            {
                throw new Exception("No devices connected.");
            }

            if (deviceSerials.Count() > 1)
            {
                throw new Exception(string.Format("Found {0} connected Android devices. Either only have 1 connected or select one using DeviceSerial during configuration. Devices found: {1}", deviceSerials.Count(), string.Join(", ", deviceSerials)));
            }

            return deviceSerials.First();
        }
    }
}