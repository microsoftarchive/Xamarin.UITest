using System.IO;
using System.Reflection;
using NUnit.Framework;
using Xamarin.UITest.iOS.ApplicationSigning.Entities;
using Xamarin.UITest.iOS.ApplicationSigning.Managers;
using Xamarin.UITest.iOS.Entities;
using Xamarin.UITest.Shared.Resources;
using Xamarin.UITest.Shared.Zip;
using Xamarin.UITest.Tests.Helpers;
using Xamarin.UITest.XDB;
using Xamarin.UITest.XDB.Services;
using Xamarin.UITest.XDB.Services.Processes;

namespace Xamarin.UITest.Tests.iOS.ApplicationSigning.Managers
{
    [TestFixture]
    public class ApplicationSigningManagerTests
    {
        private const string TestAppBundleName = "TestApp.app";
        private const string DeviceAgentBundleName = "DeviceAgent-Runner.app";

        private static string FixturesDirectoryPath => Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures");

        private static string TestAppBundleFixturePath => Path.Combine(FixturesDirectoryPath, TestAppBundleName);
        private static string DeviceAgentBundleFixturePath => Path.Combine(FixturesDirectoryPath, DeviceAgentBundleName);

        private static string TestAppsDirectoryPath => Path.Combine(TestContext.CurrentContext.TestDirectory, TestContext.CurrentContext.Test.Name);

        private static string TestAppBundlePath => Path.Combine(TestAppsDirectoryPath, TestAppBundleName);
        private static string DeviceAgentBundlePath => Path.Combine(TestAppsDirectoryPath, DeviceAgentBundleName);

        private static void UnzipEmbeddedFixture(string zipName, string unzipPath)
        {
            EmbeddedResourceLoader resourceLoader = new EmbeddedResourceLoader();
            using var fileStream = resourceLoader.GetEmbeddedResourceStream(Assembly.GetExecutingAssembly(), zipName);
            Directory.CreateDirectory(unzipPath);
            ZipHelper.Unzip(fileStream, unzipPath);
        }

        private void UnzipTestAppsFixtures()
        {
            UnzipEmbeddedFixture(zipName: "TestApp.app.zip", unzipPath: FixturesDirectoryPath);
            UnzipEmbeddedFixture(zipName: "DeviceAgent-Runner.app.zip", unzipPath: FixturesDirectoryPath);
        }

        private void CopyTestAppsFixtures()
        {
            FileSystemHelper.CopyDirectory(sourceDirectory: new DirectoryInfo(path: DeviceAgentBundleFixturePath), destinationDirectoryPath: DeviceAgentBundlePath, recursive: true);
            FileSystemHelper.CopyDirectory(sourceDirectory: new DirectoryInfo(path: TestAppBundleFixturePath), destinationDirectoryPath: TestAppBundlePath, recursive: true);
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            UnzipTestAppsFixtures();
        }

        [SetUp]
        public void BeforeEach()
        {
            CopyTestAppsFixtures();
        }

        [TearDown]
        public void AfterEach()
        {
            new DirectoryInfo(path: TestAppBundlePath).Delete(recursive: true);
            new DirectoryInfo(path: DeviceAgentBundlePath).Delete(recursive: true);
        }

        [Test]
        public void SignBundle_WithExtractedSigningDataFromTestApp_SigningDeviceAgentBundle()
        {
            ApplicationBundle testAppBundle = new ApplicationBundle(appBundlePath: TestAppBundlePath);
            ApplicationBundle deviceAgentBundle = new ApplicationBundle(appBundlePath: DeviceAgentBundlePath);

            string tmpPath = Path.Combine(Path.GetTempPath(), "UnitTests", "ApplicationSigningManagerTests");
            ProvisioningProfile extractedProvisioningProfile = new(provisioningProfileFile: testAppBundle.ExtractEmbeddedProvisioningProfile(extractionPath: tmpPath));
            CodesignIdentity extractedCodesignIdentity = extractedProvisioningProfile.ExtractCodesignIdentity();

            IProcessService processService = XdbServices.GetRequiredService<IProcessService>();
            ILoggerService loggerService = XdbServices.GetRequiredService<ILoggerService>();
            ApplicationSigningManager.SignBundle(
                processService: processService,
                loggerService: loggerService,
                bundle: deviceAgentBundle,
                provisioningProfile: extractedProvisioningProfile,
                codesignIdentity: extractedCodesignIdentity);
        }

        [Test]
        public void SignBundle_WithExtractedSigningDataFromDeviceAgent_SigningTestAppBundle()
        {
            ApplicationBundle testAppBundle = new ApplicationBundle(appBundlePath: TestAppBundlePath);
            ApplicationBundle deviceAgentBundle = new ApplicationBundle(appBundlePath: DeviceAgentBundlePath);

            string tmpPath = Path.Combine(Path.GetTempPath(), "UnitTests", "ApplicationSigningManagerTests");
            ProvisioningProfile extractedProvisioningProfile = new(provisioningProfileFile: deviceAgentBundle.ExtractEmbeddedProvisioningProfile(extractionPath: tmpPath));
            CodesignIdentity extractedCodesignIdentity = extractedProvisioningProfile.ExtractCodesignIdentity();

            IProcessService processService = XdbServices.GetRequiredService<IProcessService>();
            ILoggerService loggerService = XdbServices.GetRequiredService<ILoggerService>();
            ApplicationSigningManager.SignBundle(
                processService: processService,
                loggerService: loggerService,
                bundle: testAppBundle,
                provisioningProfile: extractedProvisioningProfile,
                codesignIdentity: extractedCodesignIdentity);
        }
    }
}

