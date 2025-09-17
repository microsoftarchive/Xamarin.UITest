using System.IO;
using NUnit.Framework;
using Should;
using Xamarin.UITest.Shared.Dependencies;

namespace Xamarin.UITest.Tests.Utils
{
    [TestFixture]
    public class AndroidSdkDependenciesTests
    {
        [Test]
        public void ResolvedMatch()
        {
            var directory = new DirectoryInfo("/");

            var dependencies = new AndroidSdkDependencies(directory, "zipalign", "aapt", "adb", "androidjar", "apksigner", "source");

            dependencies.ToString().ShouldEqual(directory.FullName + " - Valid SDK. [ Source: source ]");
        }

        [Test]
        public void Invalid_NoDirectory()
        {
            var directory = new DirectoryInfo("/asdadasdkasjlk");

            var dependencies = new AndroidSdkDependencies(directory, null, null, null, null, null, "sourcex");

            dependencies.ToString().ShouldEqual(directory.FullName + " - Does not exist. [ Source: sourcex ]");
        }        
        
        [Test]
        public void Invalid_NullDirectory()
        {
            var dependencies = new AndroidSdkDependencies(null, null, null, null, null, null, "sourcex");

            dependencies.ToString().ShouldEqual("(No path) - Not set. [ Source: sourcex ]");
        }

        [Test]
        public void Invalid_NoToolsFound()
        {
            var directory = new DirectoryInfo("/");

            var dependencies = new AndroidSdkDependencies(directory, null, null, null, null, null, "source");

            dependencies.ToString().ShouldEqual(directory.FullName + " - Tools not found: zipalign, aapt, adb, android.jar, apksigner [ Source: source ]");
        }

        [Test]
        public void Invalid_SomeToolsFound()
        {
            var directory = new DirectoryInfo("/");

            var dependencies = new AndroidSdkDependencies(directory, "zipalign", null, null, null, null, "source");

            dependencies.ToString().ShouldEqual(directory.FullName + " - Partial match. Found: zipalign Missing: aapt, adb, android.jar, apksigner [ Source: source ]");
        }

        [Test]
        public void Invalid_SomeToolsFound2()
        {
            var directory = new DirectoryInfo("/");

            var dependencies = new AndroidSdkDependencies(directory, "zipalign", null, "adb", null, null, "source");

            dependencies.ToString().ShouldEqual(directory.FullName + " - Partial match. Found: zipalign, adb Missing: aapt, android.jar, apksigner [ Source: source ]");
        }
    }
}