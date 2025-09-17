using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using Xamarin.UITest.Shared.Android;
using Xamarin.UITest.Shared.Android.Adb;
using Xamarin.UITest.Shared.Android.Commands;
using Xamarin.UITest.Shared.Dependencies;
using Xamarin.UITest.Shared.Processes;

namespace Xamarin.UITest.Tests.AdbCommand
{
    [TestFixture]
    public class CommandAdbInstallPackageTests
    {
        IApkFileInformation mockApk;
        AdbProcessRunner _adbProcessRunner;
        IProcessRunner _mockProcessRunner;

        const string AdbListPackagesCommand = "shell pm list packages";
        const string ApkFilePath = "/a/path/file.apk";
        const string DeviceSerial = "1234";

        const string SinglePackage = "package:PackageOne";
        const string MultiplePackages = "package:PackageOne\npackage:PackageTwo\npackage:PackageThree";
        const string PackageWithPrependedWarning = "Object#timeout is deprecated, use Timeout.timeout instead. package:PackageOne";
        const string MultiplePackagesWithPrependedWarning = "Object#timeout is deprecated, use Timeout.timeout instead. package:PackageOne\npackage:PackageTwo";
        const string PackagesWhereStartOfNameIsDifferent = "package:com.PackageOne\npackage:dif_com.PackageOne";

        const string DefaultExpectedPackageName = "PackageOne";
        const string PrependedExpectedPackageName = "com.PackageOne";

        [TestCase(SinglePackage,DefaultExpectedPackageName)]
        [TestCase(MultiplePackages,DefaultExpectedPackageName)]
        [TestCase(PackageWithPrependedWarning,DefaultExpectedPackageName)]
        [TestCase(MultiplePackagesWithPrependedWarning,DefaultExpectedPackageName)]
        [TestCase(PackagesWhereStartOfNameIsDifferent,PrependedExpectedPackageName)]
        public void ApkInstalledAndValidatedWithSerialNumber(string adbPackagesReponse, string expectedPackageName)
        {
            SetupMockProcessResult(adbPackagesReponse, expectedPackageName);

            Assert.DoesNotThrow(() =>
            {
                GetInstallCommandInstance("1234").ExecuteInner((args) => _adbProcessRunner.Run(args), 22);
            });

            EnsureReceived(new string[] { $"-s {DeviceSerial} install \"{ApkFilePath}\"", $"-s {DeviceSerial} {AdbListPackagesCommand}" });
            EnsureDidNotReceive(new string[] { $"-s {DeviceSerial} shell appops set {expectedPackageName} 58 allow" });
        }

        [TestCase(SinglePackage, DefaultExpectedPackageName)]
        [TestCase(MultiplePackages, DefaultExpectedPackageName)]
        [TestCase(PackageWithPrependedWarning, DefaultExpectedPackageName)]
        [TestCase(MultiplePackagesWithPrependedWarning, DefaultExpectedPackageName)]
        [TestCase(PackagesWhereStartOfNameIsDifferent, PrependedExpectedPackageName)]
        public void ApkInstalledAndValidatedWithoutSerialNumber(string adbPackagesReponse, string expectedPackageName)
        {
            SetupMockProcessResult(adbPackagesReponse, expectedPackageName);

            Assert.DoesNotThrow(() =>
            {
                GetInstallCommandInstance().ExecuteInner((args) => _adbProcessRunner.Run(args), 22);
            });

            EnsureReceived(new string[] { $"install \"{ApkFilePath}\"", AdbListPackagesCommand });
            EnsureDidNotReceive(new string[] { $"shell appops set {expectedPackageName} 58 allow" });
        }

        [TestCase(SinglePackage, DefaultExpectedPackageName)]
        [TestCase(MultiplePackages, DefaultExpectedPackageName)]
        [TestCase(PackageWithPrependedWarning, DefaultExpectedPackageName)]
        [TestCase(MultiplePackagesWithPrependedWarning, DefaultExpectedPackageName)]
        [TestCase(PackagesWhereStartOfNameIsDifferent, PrependedExpectedPackageName)]
        public void ApkInstalledAndValidatedWithSerialNumberWhenSDKGreaterThan22(string adbPackagesReponse, string expectedPackageName)
        {
            SetupMockProcessResult(adbPackagesReponse, expectedPackageName);

            Assert.DoesNotThrow(() =>
            {
                GetInstallCommandInstance("1234").ExecuteInner((args) => _adbProcessRunner.Run(args), 23);
            });

            EnsureReceived(new string[] 
            { 
                $"-s {DeviceSerial} install -g \"{ApkFilePath}\"", 
                $"-s {DeviceSerial} {AdbListPackagesCommand}", 
                $"-s {DeviceSerial} shell appops set {expectedPackageName} 58 allow" 
            });
        }

        [TestCase(SinglePackage, DefaultExpectedPackageName)]
        [TestCase(MultiplePackages, DefaultExpectedPackageName)]
        [TestCase(PackageWithPrependedWarning, DefaultExpectedPackageName)]
        [TestCase(MultiplePackagesWithPrependedWarning, DefaultExpectedPackageName)]
        [TestCase(PackagesWhereStartOfNameIsDifferent, PrependedExpectedPackageName)]
        public void ApkInstalledAndValidatedWithoutSerialNumberWhenSDKGreaterThan22(string adbPackagesReponse, string expectedPackageName)
        {
            SetupMockProcessResult(adbPackagesReponse, expectedPackageName);

            Assert.DoesNotThrow(() =>
            {
                GetInstallCommandInstance().ExecuteInner((args) => _adbProcessRunner.Run(args), 23);
            });

            EnsureReceived(new string[]
            {
                $"install -g \"{ApkFilePath}\"",
                AdbListPackagesCommand,
                $"shell appops set {expectedPackageName} 58 allow"
            });
        }

        [Test]
        public void ThrowsExceptionsWithMessageWhenPackageIsNotInstalled()
        {
            const string invalidPackageName = "com.Invalid";
            var expectedErrorMessage = 
                $"App installation failed with output: {MultiplePackages}. Expected Package Name: {invalidPackageName}. Adb Packages Output: {MultiplePackages}";

            SetupMockProcessResult(MultiplePackages, invalidPackageName);

            var actualException = Assert.Throws<Exception>(() =>
            {
                GetInstallCommandInstance().ExecuteInner((args) => _adbProcessRunner.Run(args), 23);
            });

            Assert.AreEqual(expectedErrorMessage, actualException.Message);
        }

        void SetupMockProcessResult(string adbPackagesResponse, string expectedPackageName)
        {
            mockApk = Substitute.For<IApkFileInformation>();
            mockApk.PackageName.Returns(expectedPackageName);
            mockApk.ApkPath.Returns(ApkFilePath);

            var mockSdkTools = Substitute.For<IAndroidSdkTools>();
            mockSdkTools.GetAdbPath().Returns("adb");

            _mockProcessRunner = Substitute.For<IProcessRunner>();
            var processOutput = new ProcessResult(new[] { new ProcessOutput(adbPackagesResponse) }, 1, 1, true);
            _mockProcessRunner.Run(Arg.Any<string>(), Arg.Any<string>()).Returns(processOutput);

            _adbProcessRunner = new AdbProcessRunner(_mockProcessRunner, mockSdkTools);
        }

        CommandAdbInstallPackage GetInstallCommandInstance(string serialNumber = "")
        {
            return string.IsNullOrEmpty(serialNumber) ? new CommandAdbInstallPackage(mockApk) : new CommandAdbInstallPackage(serialNumber, mockApk);
        }

        void EnsureReceived(IEnumerable<string> adbCommands)
        { 
            foreach (var adbCommand in adbCommands)
            {
                _mockProcessRunner.Received().Run(Arg.Is("adb"), Arg.Is(adbCommand));
            }
        }

        void EnsureDidNotReceive(IEnumerable<string> adbCommands)
        { 
            foreach (var adbCommand in adbCommands)
            {
                _mockProcessRunner.DidNotReceive().Run(Arg.Is("adb"), Arg.Is(adbCommand));
            }
        }
    }
}

