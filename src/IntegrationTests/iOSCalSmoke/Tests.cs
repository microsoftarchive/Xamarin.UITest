using NUnit.Framework;
using IntegrationTests.Shared;
using System.Linq;
using System;

[TestFixture]
public class Tests : IOSTestBase
{
    protected override AppInformation _appInformation => TestApps.iOSCalWebView;

    protected string UIWebViewLabel()
    {
        var webViewLabel = "UIWebView";
        return webViewLabel;
    }

    protected string WKWebViewLabel()
    {
        var webViewLabel = "WKWebView";

        if (_app.Query(c => c.Class("UITabBarButton").Marked("UIWebView")).Length == 2)
        {
            // iOS version doesn't support WKWebViews so a second UIWebView is being shown instead;
            webViewLabel = "UIWebView";
        }

        return webViewLabel;
    }

    protected void WaitForWebPageLoaded(string className)
    {
        var timeout = 30;
        string message = string.Format(
            "Timed out waiting for web page to load {0} seconds", timeout
        );

        _app.WaitFor(
            () => _app.Query(e => e.Class(className).Invoke("isLoading")).Single().ToString().Equals("0"),
            message,
            TimeSpan.FromSeconds(timeout));
    }

    protected void WaitForWebPageJSInvoke(string className, string expected, string javaScript = "document.getElementsByTagName('h1')[0].innerHTML")
    {
        var timeout = 30;
        string message = string.Format(
            "Timed out waiting for JSInvoke for {0} seconds", timeout
        );

        _app.WaitFor(
            () => _app.Query(c => c.Class(className).InvokeJS(javaScript)).Single().Equals(expected),
            message,
            TimeSpan.FromSeconds(timeout));
    }

    [Test]
    public void WKWebViewIsQueryable ()
    {
        var webViewLabel = WKWebViewLabel();
        _app.Tap(c => c.Marked(webViewLabel));

        WaitForWebPageLoaded(webViewLabel);

        _app.WaitForElement(e => e.Class(webViewLabel).Css("a"));
    }

    [Test]
    public void WKWebViewInvokeJS()
    {
        var webViewLabel = WKWebViewLabel();
        _app.Tap(c => c.Marked(webViewLabel));

        WaitForWebPageLoaded(webViewLabel);

        WaitForWebPageJSInvoke(webViewLabel, "H1 Header!");
    }

    [Test]
    public void UIWebViewIsQueryable()
    {
        var webViewLabel = UIWebViewLabel();
        _app.Tap(c => c.Marked(webViewLabel));

        WaitForWebPageLoaded(webViewLabel);

        _app.WaitForElement(e => e.Class(webViewLabel).Css("a"));
    }

    [Test]
    public void UIWebViewInvokeJS()
    {
        var webViewLabel = UIWebViewLabel();
        _app.Tap(c => c.Marked(webViewLabel));

        WaitForWebPageLoaded(webViewLabel);

        WaitForWebPageJSInvoke(webViewLabel, "H1 Header!");

    }
}
