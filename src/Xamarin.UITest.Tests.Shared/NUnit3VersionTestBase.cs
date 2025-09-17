using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Xamarin.UITest.Tests.Shared
{
    public abstract class NUnit3VersionTestBase
    {
        // We don't support < 3.13.3
        // We need to keep the version pinned to 3.13.3 to ensure that we don't
        // introduce regressions that break 3.13.3 support.
        //
        // We rely on NUnit to not introduce breaking changes in their 3.x
        // releases.
        private readonly Version NUnit3MinVersion = new Version(3, 14, 0, 0);

        [Test]
        public void NUnitVersionIsCorrect()
        {
            var loadedNUnitVersion = Assembly.GetExecutingAssembly()
                .GetReferencedAssemblies()
                .Single(a => a.Name == "nunit.framework")
                .Version;

            Assert.AreEqual(NUnit3MinVersion, loadedNUnitVersion, $"NUnit version should be {NUnit3MinVersion}");

            var nunitFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "nunit.framework.dll");

            var fileNUnitVersion = Assembly.LoadFrom(nunitFilePath).GetName().Version;

            Assert.AreEqual(NUnit3MinVersion, fileNUnitVersion, $"NUnit version should be {NUnit3MinVersion}");
        }
    }
}
