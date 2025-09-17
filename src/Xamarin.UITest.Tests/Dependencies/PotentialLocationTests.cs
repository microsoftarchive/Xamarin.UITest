using NUnit.Framework;
using Xamarin.UITest.Shared.Dependencies;

namespace Xamarin.UITest.Tests.Dependencies
{
    [TestFixture]
    public class PotentialLocationTests
    {
        [Test]
        public void InvalidPotentialLocationTest()
        {
            if (UITest.Shared.Processes.Platform.Instance.IsWindows)
            {
                // > . < | will throw ArgumentException on Windows
                var location = new PotentialLocation("C:\\contains:|<some>\"invalid-sdk-path", "xamarin-install-path");
                Assert.IsTrue(location.Path.Equals(""));
            }
        }

        [Test]
        public void NullPotentialLocationTest()
        {
            var location = new PotentialLocation(null, "xamarin-install-path");
            Assert.IsTrue(location.Path.Equals(""));
        }
    }
}
