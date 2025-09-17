using NUnit.Framework;
using Should;
using Xamarin.UITest.Shared.Dependencies;

namespace Xamarin.UITest.Tests.Utils
{
    [TestFixture]
    public class VersionSelectorTests
    {
        readonly VersionSelector _selector = new VersionSelector();

        [Test]
        public void WontFindNonMatches()
        {
            var result = _selector.PickLatest(@"jdk(\d+)", new[] { "test" });

            result.ShouldBeNull();
        }

        [Test]
        public void WillFindSingleMatch()
        {
            var result = _selector.PickLatest(@"jdk(\d+)", new[] { "jdk1" });

            result.ShouldEqual("jdk1");
        }        
        
        [Test]
        public void WillFindSingleMatchPrefix()
        {
            var result = _selector.PickLatest(@"jdk(\d+)", new[] { "jdk1test" });

            result.ShouldEqual("jdk1test");
        }

        [Test]
        public void WillFindHighestSimpleMatch()
        {
            var result = _selector.PickLatest(@"jdk(\d+)", new[] { "jdk1", "jdk2" });

            result.ShouldEqual("jdk2");
        }        
        
        [Test]
        public void WillFindHighestComplexMatch()
        {
            var result = _selector.PickLatest(@"jdk(\d+)\.(\d+)", new[] { "jdk1.2", "jdk1.4", "jdk1.3" });

            result.ShouldEqual("jdk1.4");
        }        
        
        [Test]
        public void WillFindHighestComplexWithOptionalMatch1()
        {
            var result = _selector.PickLatest(@"jdk(\d+)\.?(\d+)?", new[] { "jdk1.2", "jdk1.4", "jdk2" });

            result.ShouldEqual("jdk2");
        }

        [Test]
        public void WillFindHighestComplexWithOptionalMatch2()
        {
            var result = _selector.PickLatest(@"jdk(\d+)\.?(\d+)?", new[] { "jdk2", "jdk2.17", "jdk2.1" });

            result.ShouldEqual("jdk2.17");
        }

        [Test]
        public void WillFindNoNumberMatch_NoPattern()
        {
            var result = _selector.PickLatest(new[] { "jdk" });

            result.ShouldEqual("jdk");
        }

        [Test]
        public void WillFindSingleMatch_NoPattern()
        {
            var result = _selector.PickLatest(new[] { "jdk1" });

            result.ShouldEqual("jdk1");
        }

        [Test]
        public void WillFindSingleMatchPrefix_NoPattern()
        {
            var result = _selector.PickLatest(new[] { "jdk1test" });

            result.ShouldEqual("jdk1test");
        }

        [Test]
        public void WillFindHighestSimpleMatch_NoPattern()
        {
            var result = _selector.PickLatest(new[] { "jdk1", "jdk2" });

            result.ShouldEqual("jdk2");
        }

        [Test]
        public void WillFindHighestComplexMatch_NoPattern()
        {
            var result = _selector.PickLatest(new[] { "jdk1.2", "jdk1.4", "jdk1.3" });

            result.ShouldEqual("jdk1.4");
        }

        [Test]
        public void WillFindHighestComplexWithOptionalMatch1_NoPattern()
        {
            var result = _selector.PickLatest(new[] { "jdk1.2", "jdk1.4", "jdk2" });

            result.ShouldEqual("jdk2");
        }

        [Test]
        public void WillFindHighestComplexWithOptionalMatch2_NoPattern()
        {
            var result = _selector.PickLatest(new[] { "jdk2", "jdk2.17", "jdk2.1" });

            result.ShouldEqual("jdk2.17");
        }

        [Test]
        public void WillFindHighestComplexMatch_Jdk9Format()
        {
            var result = _selector.PickLatest(@"jdk-?(\d+)\.?(\d+)?\.?(\d+)?_?(\d+)?\.jdk", 
                                              new[] { "jdk-9.0.4.jdk", "jdk1.8.0_112.jdk", "jdk-9.0.3.jdk", "jdk1.10.0_112.jdk" });

            result.ShouldEqual("jdk-9.0.4.jdk");
        }

        [Test]
        public void WillFindHeightComplexMatch_Jdk9Format2()
        {
            var result = _selector.PickLatest(@"jdk-?(\d+)\.?(\d+)?\.?(\d+)?_?(\d+)?\.jdk",
                                              new[] { "jdk-9.0.4.jdk", "jdk-10.jdk" });

            result.ShouldEqual("jdk-10.jdk");
        }
    }
}