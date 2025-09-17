using System;
using NUnit.Framework;
using Xamarin.UITest.Shared.Queries;

namespace Xamarin.UITest.Tests.DataTypes
{
    [TestFixture]
    public class QuoteUnescapedStringTests
    {
        [Test]
        public void ReturnUnescapedString()
        {
            var expected = "hello";
            var unescaped = new QuoteUnescapedString(expected).UnescapedString;
            Assert.AreEqual(expected, unescaped);
        }

        [TestCase("", true)]
        [TestCase(null, true)]
        [TestCase("hello", false)]
        public void ReturnIsNullEmptyOrWhitespace(string input, bool expected)
        {
            var actual = new QuoteUnescapedString(input).IsNullOrWhiteSpace();
            Assert.AreEqual(expected, actual);
        }
    }
}

