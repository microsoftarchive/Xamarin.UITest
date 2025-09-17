using System;
using NUnit.Framework;
using Should;
using Xamarin.UITest.iOS;
using Xamarin.UITest.Utils;

namespace Xamarin.UITest.Tests.Utils
{
    [TestFixture]
    class EnsureVersionTests
    {
        [Test]
        public void EnsurePerformsActionIfFulfilled()
        {
            var actionPerformed = false;
            var ensure = new EnsureVersion(new VersionNumber("1.2"), "Foo");
            ensure.AtLeast("1.0", () => { actionPerformed = true; });
            actionPerformed.ShouldBeTrue();    
        }

        [Test]
        public void EnsureDoesNoPerformsActionIfUnFulfilled()
        {
            var actionPerformed = false;
            var ensure = new EnsureVersion(new VersionNumber("1.2"), "Foo");
            try
            {
                ensure.AtLeast("1.3", () => { actionPerformed = true; });
            }
            catch 
            {
                actionPerformed.ShouldBeFalse();
                return;
            }
            Assert.Fail();
        }

        [TestCase(null)]
        [TestCase("{0},{1},{2}")]
        public void EnsureIncludesNameAndVersionOnError(string optionalString)
        {
            var ensure = new EnsureVersion(new VersionNumber("1.3"), "Foo");
            try
            {
                ensure.AtLeast("9.2.100", () => { }, optionalString);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("Foo");
                e.Message.ShouldContain("9.2.100");
                e.Message.ShouldContain("1.3");
                return;
            }
            Assert.Fail();
        }
    }
}
