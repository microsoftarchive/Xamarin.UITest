using NUnit.Framework;
using Should;
using Xamarin.UITest.Utils;

namespace Xamarin.UITest.Tests.Dependencies
{
    [TestFixture]
    public class VersionTests
    {
        [Test]
        public void NormalVersion()
        {
            var version = new VersionNumber("1.2.3.4");

            version.Major.ShouldEqual(1);
            version.Minor.ShouldEqual(2);
            version.BuildOrPatch.ShouldEqual(3);
            version.Revision.ShouldEqual(4);
        }

        [Test]
        public void NormalVersion3Digits()
        {
            var version = new VersionNumber("1.2.3");

            version.Major.ShouldEqual(1);
            version.Minor.ShouldEqual(2);
            version.BuildOrPatch.ShouldEqual(3);
            version.Revision.ShouldEqual(0);
        }
        [Test]
        public void NormalVersion2Digits()
        {
            var version = new VersionNumber("1.2");

            version.Major.ShouldEqual(1);
            version.Minor.ShouldEqual(2);
            version.BuildOrPatch.ShouldEqual(0);
            version.Revision.ShouldEqual(0);
        }

        [Test]
        public void NormalVersion1Digits()
        {
            var version = new VersionNumber("1");

            version.Major.ShouldEqual(1);
            version.Minor.ShouldEqual(0);
            version.BuildOrPatch.ShouldEqual(0);
            version.Revision.ShouldEqual(0);
        }

        [Test]
        public void SupportsNuGetPreReleaseVersions()
        {
            var version = new VersionNumber("1.2.3-pre4");

            version.Major.ShouldEqual(1);
            version.Minor.ShouldEqual(2);
            version.BuildOrPatch.ShouldEqual(3);
            version.Label.ShouldEqual("pre4");
        }

        [Test]
        public void SupportsCalabashiOSPreReleaseVersions()
        {
            var version = new VersionNumber("1.2.3.pre4");
            
            version.Major.ShouldEqual(1);
            version.Minor.ShouldEqual(2);
            version.BuildOrPatch.ShouldEqual(3);
            version.Label.ShouldEqual("pre4");
        }

        [Test]
        public void MajorMinorBuildRevisionOrdering()
        {
            new VersionNumber("2.0.0.0").ShouldBeGreaterThan(new VersionNumber("1.0.0.0"));
            new VersionNumber("0.2.0.0").ShouldBeGreaterThan(new VersionNumber("0.1.0.0"));
            new VersionNumber("0.0.2.0").ShouldBeGreaterThan(new VersionNumber("0.0.1.0"));
            new VersionNumber("0.0.0.2").ShouldBeGreaterThan(new VersionNumber("0.0.0.1"));
        }

        [Test]
        public void PreReleaseOrdering()
        {
            new VersionNumber("0.0.0.0").ShouldBeGreaterThan(new VersionNumber("0.0.0.0-alpha"));
            new VersionNumber("0.0.0.0-beta").ShouldBeGreaterThan(new VersionNumber("0.0.0.0-alpha"));
        }

        [Test]
        public void Equals()
        {
            Assert.IsTrue(new VersionNumber("0") == new VersionNumber("0.0.0.0"));
            Assert.IsTrue(new VersionNumber("0.0") == new VersionNumber("0.0.0.0"));
            Assert.IsTrue(new VersionNumber("0.0.0") == new VersionNumber("0.0.0.0"));
            Assert.IsTrue(new VersionNumber("0.0.0.0") == new VersionNumber("0.0.0.0"));
            Assert.IsTrue(new VersionNumber("0.0.0.0-abc") == new VersionNumber("0.0.0.0-abc"));
        }
    }
}