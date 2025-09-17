using System;
using NUnit.Framework;
using Xamarin.UITest.Shared.Queries;

namespace Xamarin.UITest.Tests.DataTypes
{
    [TestFixture]
    public class XPathSingleQuoteWorkAroundStringTests
    {
        [TestCase("//*[@id='identifier']")]
        [TestCase("//*[@id=\"identifier\"]")]
        public void First(string xPathAttributeLocator)
        {
            const string expected = "//*[@id=\"identifier\"]";
            var xPathWorkAroundString = new XPathSingleQuoteWorkAroundString(xPathAttributeLocator);
            var result = xPathWorkAroundString.ToString();
            Assert.AreEqual(expected, result);
        }

        [TestCase("//*[text()='text']", "//*[text()=\"text\"]")]
        [TestCase("//*[text()=\"text\"]", "//*[text()=\"text\"]")]
        [TestCase("//*[.='text']", "//*[.=\"text\"]")]
        [TestCase("//*[.=\"text\"]", "//*[.=\"text\"]")]
        public void TextLoactorHasEscapedDoubleQuotes(string xPathTextLocator, string expected)
        {
            var xPathWorkAroundString = new XPathSingleQuoteWorkAroundString(xPathTextLocator);
            var result = xPathWorkAroundString.ToString();
            Assert.AreEqual(expected, result);
        }
    }
}

