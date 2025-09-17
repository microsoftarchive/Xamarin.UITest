using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace IntegrationTests.Shared
{
    public class NUnit3VersionTestBase
    {
        private readonly Version NUnit3MinVersion = new Version(3, 7, 0, 0); // The oldest NUnit v3 version we "support"

        [Test]
        public void NUnitVersionIsCorrect()
        {
            var nunitVersion = Assembly.GetExecutingAssembly()
                .GetReferencedAssemblies()
                .Single(a => a.Name == "nunit.framework")
                .Version;

            Assert.AreEqual(NUnit3MinVersion, nunitVersion, $"NUnit version should be {NUnit3MinVersion}");
        }
    }
}
