using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using Xamarin.UITest.iOS.ApplicationSigning.Entities;
using Xamarin.UITest.iOS.Entities;
using Xamarin.UITest.Shared.Resources;
using Xamarin.UITest.Shared.Zip;
using Xamarin.UITest.Tests.Helpers;

namespace Xamarin.UITest.Tests.iOS.Entities
{
    [TestFixture]
	public class ApplicationBundleTests
	{
        private const string TestAppIpaBundleName = "TestApp.ipa";
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
        public void ApplicationBundleCtor_NotValidPathToAppBundle_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new ApplicationBundle(appBundlePath: "NotExistingBundle.app"));
        }

        [Test]
        public void ApplicationBundleCtor_NullPathToAppBundle_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ApplicationBundle(appBundlePath: null));
        }

        [Test]
        public void ApplicationBundleCtor_IPAFormatAppBundle_ThrowsArgumentException()
        {
            string ipaAppBundleFilePath = Path.Combine("", TestAppIpaBundleName);

            Assert.Throws<ArgumentException>(() => new ApplicationBundle(appBundlePath: ipaAppBundleFilePath));
        }

        [Test]
        public void ApplicationBundleCtor_APPFormatAppBundle_ReturnsAPPApplicationBundle()
        {
            Assert.AreEqual(expected: ".app", new ApplicationBundle(appBundlePath: TestAppBundlePath).AppBundle.Extension);
        }

        [Test]
        public void ExtractEmbeddedProvisioningProfile_FromCorrectAppBundle_ReturnsFileInfo()
        {
            string tmpPath = Path.Combine(Path.GetTempPath(), "UnitTests", "ApplicationBundleTests");

            ApplicationBundle appBundle = new(appBundlePath: TestAppBundlePath);
            FileInfo provisioningProfileFile = appBundle.ExtractEmbeddedProvisioningProfile(tmpPath);

            Assert.True(provisioningProfileFile.Exists);
        }

        [Test]
        public void ReplaceEmbeddedProvisioningProfile_FromTestApp_DoesNotThrow()
        {
            string tmpPath = Path.Combine(Path.GetTempPath(), "UnitTests", "ApplicationBundleTests");
            ApplicationBundle appBundle = new(appBundlePath: TestAppBundlePath);
            FileInfo provisioningProfileFile = appBundle.ExtractEmbeddedProvisioningProfile(tmpPath);
            ProvisioningProfile extractedProfile = new(provisioningProfileFile: provisioningProfileFile);

            ApplicationBundle deviceAgentBundle = new(appBundlePath: DeviceAgentBundlePath);

            Assert.DoesNotThrow(() => deviceAgentBundle.ReplaceEmbeddedProvisioningProfile(newProvisioningProfile: extractedProfile));
        }

        [Test]
        public void GetEmbeddedFrameworks_BundleDoesNotContainFrameworks_ReturnsZeroFrameworks()
        {
            ApplicationBundle bundle = new(appBundlePath: TestAppBundlePath);

            List<DirectoryInfo> frameworks = bundle.GetEmbeddedFrameworks();

            Assert.AreEqual(expected: 0, actual: frameworks.Count);
        }

        [Test]
        public void GetEmbeddedFrameworks_BundleContainsSixFrameworks_ReturnsSixFrameworks()
        {
            ApplicationBundle bundle = new(appBundlePath: DeviceAgentBundlePath);

            List<DirectoryInfo> frameworks = bundle.GetEmbeddedFrameworks();

            Assert.AreEqual(expected: 6, actual: frameworks.Count);
        }

        [Test]
        public void GetXCTestBundles_BundleDoesNotContainXCTestBundles_ReturnZeroXCTestBundles()
        {
            ApplicationBundle bundle = new(appBundlePath: TestAppBundlePath);

            List<DirectoryInfo> xcTestBundles = bundle.GetXCTestBundles();

            Assert.AreEqual(expected: 0, actual: xcTestBundles.Count);
        }

        [Test]
        public void GetXCTestBundles_BundleContainsOneXCTestBundle_ReturnOneXCTestBundle()
        {
            ApplicationBundle bundle = new(appBundlePath: DeviceAgentBundlePath);

            List<DirectoryInfo> xcTestBundles = bundle.GetXCTestBundles();

            Assert.AreEqual(expected: 1, actual: xcTestBundles.Count);
        }

        [Test]
        public void GetDylibs_BundleDoesNotContainDylibs_ReturnZeroDylibs()
        {
            ApplicationBundle bundle = new(appBundlePath: TestAppBundlePath);

            List<FileInfo> dylibs = bundle.GetDylibsFromFrameworksDirectory();

            Assert.AreEqual(expected: 0, actual: dylibs.Count);
        }

        [Test]
        public void GetDylibs_BundleContainsOneDylibs_ReturnOneDylibs()
        {
            ApplicationBundle bundle = new(appBundlePath: DeviceAgentBundlePath);

            List<FileInfo> dylibs = bundle.GetDylibsFromFrameworksDirectory();

            Assert.AreEqual(expected: 1, actual: dylibs.Count);
        }
    }
}

