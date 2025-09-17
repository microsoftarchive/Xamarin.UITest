using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xamarin.UITest;

[TestFixture]
public class XPathLocatorTests : AndroidTestAppBase
{
    readonly static IDictionary<string, string> DevicesWithoutFullXpathSupport = new Dictionary<string, string>()
    {
        { "Samsung Galaxy S III" , "4.3" },
        { "Samsung Galaxy Note 3 (Quad-Core)", "4.3" },
        { "Samsung Galaxy S III (US Carrier)" , "4.3" },
        { "Samsung Galaxy S4 (Octo-core)", "4.3" },
        { "Samsung Galaxy Note II", "4.3" }
    };

    public override void BeforeEach()
    {
        base.BeforeEach();

        _app.Tap("Web View");
        _app.WaitForElement(e => e.Css("#inputButton"));
    }

    [Test]
    public void XPathQueryAll()
    {
        var result = _app.Query(e => e.XPath("*"));
        Assert.True(result.Count() > 0);
    }

    [Test]
    public void XPathQueryElementByAttribute()
    {
        var result = _app.Query(e => e.XPath("//*[@id='result']"));
        Assert.True(result.Count() == 1);
    }

    [Test]
    public void XPathQueryChildElementByAttribute()
    {
        var result = _app.Query(e => e.XPath("//*[@id='inputForm']//*[@id='inputText']"));
        Assert.True(result.Count() == 1);
    }

    [Test]
    public void XpathQueryDirectChildOfParentElement()
    {
        var result = _app.Query(e => e.XPath("//div/form/label"));
        Assert.True(result.Count() == 1);
    }

    [Test]
    public void XpathQueryElementByTextUsingDotNotation()
    {
        var result = _app.Query(e => e.XPath("//*[.='Input some text:']"));
        Assert.True(result.Count() == 1);
    }

    [Test]
    public void XpathQueryElementByTextUsingJSMethod()
    {
        var result = _app.Query(e => e.XPath("//*[text()='Input some text:']"));
        Assert.True(result.Count() == 1);
    }
}
