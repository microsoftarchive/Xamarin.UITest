using System;
using NUnit.Framework;
using Xamarin.UITest.Shared.Queries;

namespace Xamarin.UITest.Tests.DataTypes
{
    [TestFixture]
    public class SingleQuoteEscapedStringTests
    {
        [Test]
        public void EscapeBackslashesAdded()
        {
            var original = "'hello'";
            var expected = "\\'hello\\'"; //required for sending to Android Server
            var actual = new SingleQuoteEscapedString(original);
            Assert.AreEqual(expected, actual.ToString());
        }

        [Test]
        public void EscapedBackslashesAddedToCssLocator()
        {
            var original = "[id='the-id']";
            var expected = "[id=\\'the-id\\']"; //required for sending to Android Server
            var actual = new SingleQuoteEscapedString(original);
            Assert.AreEqual(expected, actual.ToString());
        }
    }
}

