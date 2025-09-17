using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xamarin.UITest.Shared.Android.Adb;

namespace Xamarin.UITest.Tests.Adb
{
    [TestFixture("1234")]
    [TestFixture("null")]
    [TestFixture("")]
    public class AdbArgumentsTests
    {
        const int ValidPort = 1234;
        const string ValidPackageName = "a.package.name";
        const string PathToApk = "A/Valid/Path/app.apk";

        AdbArguments _adbArguments;
        readonly string _prependSerial;
        readonly string _deviceSerial;

        public AdbArgumentsTests(string deviceSerial)
        {
            if (string.Equals(deviceSerial, "null"))
            {
                _deviceSerial = null;
            }
            else
            {
                _deviceSerial = deviceSerial;
            }

            if (!string.IsNullOrEmpty(_deviceSerial))
            {
                _prependSerial = $"-s {deviceSerial} ";
            }
        }

        [SetUp]
        public void BeforeEach()
        {
            _adbArguments = new AdbArguments(_deviceSerial);
        }

        [TestCase("/Some/Valid/Path/app.apk", 22)]
        [TestCase("/Some/Valid/Path/app.apk", 23)]
        public void InstallApp(string appPath, int sdkLevel)
        {
            var expected = sdkLevel >= 23 ? 
                $"install -g \"{appPath}\"" : 
                $"install \"{appPath}\"";
            
            var actual = _adbArguments.Install(appPath, sdkLevel);
            AssertAdbArguments(expected, actual);
        }

        [Test]
        public void ListPackagesWithoutPMArgs()
        {
            var expected = "shell pm list packages";
            var actual = _adbArguments.PackageManagerList();
            AssertAdbArguments(expected, actual);
        }

        [Test]
        public void ListPackagesWithPMArgs()
        {
            var expected = "shell pm list packages -e";
            var pmArgs = new PackageManagerCommandOptions().Packages(PackagesOption.ShowEnabledOnly);
            var actual = _adbArguments.PackageManagerList(pmArgs);
            AssertAdbArguments(expected, actual);
        }

        [Test]
        public void ShellEnableMockLocation()
        {
            var expected = $"shell appops set {ValidPackageName} 58 allow";
            var actual = _adbArguments.EnableMockLocation(ValidPackageName);
            AssertAdbArguments(expected, actual);
        }

        [Test]
        public void ShellEnableManageExternalStorage()
        {
            var expected = $"shell appops set {ValidPackageName} MANAGE_EXTERNAL_STORAGE allow";
            var actual = _adbArguments.EnableManageExternalStorage(ValidPackageName);
            AssertAdbArguments(expected, actual);
        }

        [Test]
        public void PortForward()
        {
            var expected = $"forward tcp:{ValidPort} tcp:{ValidPort}";
            var actual = _adbArguments.PortForward(ValidPort);
            AssertAdbArguments(expected, actual);
        }

        [Test]
        public void InstrumentWithoutAMArgs()
        {
            var expected = $"shell am instrument {ValidPackageName}";
            var actual = _adbArguments.ActivityManagerInstrument(ValidPackageName);
            AssertAdbArguments(expected, actual);
        }

        [Test]
        public void InstrumentWithAMArgs()
        {
            var expected = $"shell am instrument -e \"key\" \"value\" {ValidPackageName}";
            var amArgs = new ActivityManagerIntentArguments().AddData("key", "value");
            var actual = _adbArguments.ActivityManagerInstrument(ValidPackageName, amArgs);
            AssertAdbArguments(expected, actual);
        }

        [Test]
        public void Concatinate()
        {
            var expected = $"shell cat \"{PathToApk}\"";
            var actual = _adbArguments.ShellConcatinate(PathToApk);
            AssertAdbArguments(expected, actual);
        }

        [Test]
        public void ConcatinateWithApp()
        {
            var expected = $"shell run-as {ValidPackageName} cat \"{PathToApk}\"";
            var actual = _adbArguments.ShellConcatinate(PathToApk, ValidPackageName);
            AssertAdbArguments(expected, actual);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void List(bool longFormat)
        {
            var expected = longFormat ? 
                $"shell ls -l \"{PathToApk}\"" : 
                $"shell ls \"{PathToApk}\"";

            var actual = _adbArguments.ShellList(PathToApk, longFormat);
            AssertAdbArguments(expected, actual);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ListWithApp(bool longFormat)
        {
            var expected = longFormat ?
                $"shell run-as {ValidPackageName} ls -l \"{PathToApk}\"" :
                $"shell run-as {ValidPackageName} ls \"{PathToApk}\"";

            var actual = _adbArguments.ShellList(PathToApk, longFormat, ValidPackageName);
            AssertAdbArguments(expected, actual);
        }

        [Test]
        public void KillProcessUsingPid()
        {
            var expected = $"shell kill -9 4321";
            var actual = _adbArguments.ShellKillProcess("4321");
            AssertAdbArguments(expected, actual);
        }

        [Test]
        public void ListAllProcessStatus()
        {
            var expected = $"shell ps";
            var actual = _adbArguments.ShellProcessStatus();
            AssertAdbArguments(expected, actual);
        }

        [Test]
        public void ListDevices()
        {
            var expected = "devices";
            var actual = _adbArguments.Devices();
            Assert.AreEqual(expected, actual);
        }

        [TestCase(24)]
        [TestCase(23)]
        public void Sha256(int sdkLevel)
        {
            string sha256SumArgs = $"shell sha256sum {ValidPackageName}";
            string sha256Args = $"shell sha256 {ValidPackageName}";

            var expected = sdkLevel < 24 ?
                new[] { sha256SumArgs , sha256Args } :
                new[] { sha256SumArgs };

            var actual = _adbArguments.Sha256(ValidPackageName, sdkLevel);
            AssertAdbArguments(expected, actual);
        }

        [TestCase(ValidPort)]
        [TestCase(null)]
        public void Monkey(int? port)
        {
             var expected = port == null ?
               "shell monkey --port " :
               $"shell monkey --port {ValidPort}";

            var actual = _adbArguments.ShellMonkey(port);
            AssertAdbArguments(expected, actual);
        }

        [Test]
        public void InputMethod()
        {
            var expected = "shell dumpsys input_method";
            AssertAdbArguments(expected, _adbArguments.InputServiceInformation());
        }

        [Test]
        public void GetWindowInformation()
        {
            var expected = "shell dumpsys window windows";
            AssertAdbArguments(expected, _adbArguments.CurrentWindowInformation());
        }

        [Test]
        public void StartActivityUsingActionAndComponent()
        {
            var action = "android.intent.action.MAIN";
            var component = "svrPackage/Classname";
            var expected = $"shell am start -a {action} -n {component}";

            var amArgs = new ActivityManagerIntentArguments()
                .AddAction(action)
                .AddComponent(component);

            var actual = _adbArguments.ActivityManagerStart(amArgs);
            AssertAdbArguments(expected, actual);
        }

        [Test]
        public void UninstallUsingPackageName()
        {
            var expected = $"uninstall \"{ValidPackageName}\"";
            var actual = _adbArguments.Uninstall(ValidPackageName);
            AssertAdbArguments(expected, actual);
        }

        [Test]
        public void GetSDKVersionFromProperty()
        {
            var expected = "shell getprop ro.build.version.sdk";
            var actual = _adbArguments.GetSDKVersionFromProperty();
            AssertAdbArguments(expected, actual);
        }

        void AssertAdbArguments(string expectedArgs, string actualArgs)
        {
            Assert.AreEqual(_prependSerial + expectedArgs, actualArgs);
        }

        void AssertAdbArguments(IEnumerable<string> expected, IEnumerable<string> actual)
        {
            CollectionAssert.AllItemsAreUnique(actual);
            CollectionAssert.AllItemsAreUnique(expected);
            Assert.True(actual.Count() == expected.Count());

            foreach (var actualValue in actual)
            {
                var result = expected.SingleOrDefault(actualValue.EndsWith);
                Assert.IsNotNull(result);
            }

            if (!string.IsNullOrEmpty(_deviceSerial))
            {
                Assert.True(actual.All(e => e.StartsWith("-s 1234 ")));
            }
        }
    }
}
