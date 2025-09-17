using System;
using System.Linq;
using NUnit.Framework;
using Should;
using Xamarin.UITest.Queries;
using Xamarin.UITest.Utils;
using Xamarin.UITest.Configuration;
using Xamarin.UITest;

[TestFixture]
public abstract class WebScrollTestsBase : AndroidTestAppBase
{
    class ShortWaitTimes : IWaitTimes
    {
        public TimeSpan GestureWaitTimeout
        {
            get { return TimeSpan.FromSeconds(4); }
        }

        public TimeSpan WaitForTimeout
        {
            get { return TimeSpan.FromSeconds(4); }
        }

        public TimeSpan GestureCompletionTimeout
        {
            get { return TimeSpan.FromSeconds(4); }
        }
    }

    protected readonly Func<AppQuery, AppQuery> _scrollWebViewQuery = c => c.Button("buttonScrollWebView");
    protected readonly Func<AppQuery, AppWebQuery> _firstInListWeb = c => c.Css("#linefirst");

    protected readonly ScrollStrategy _strategy;

    readonly Func<AppQuery, AppWebQuery> _nonExistingWeb = c => c.Css("#donotexist");
    readonly Func<AppQuery, AppWebQuery> _lastInListWeb = c => c.Css("#linelast");
    readonly Func<AppQuery, AppWebQuery> _line2_2 = c => c.Css("#line2-2");
    readonly Func<AppQuery, AppWebQuery> _line12 = c => c.Css("#line12");

    public WebScrollTestsBase(ScrollStrategy strategy)
    {
        _strategy = strategy;
    }

    protected override AndroidAppConfigurator ReConfigureApp(AndroidAppConfigurator app)
    {
        return app.WaitTimes(new ShortWaitTimes());
    }

    [Test]
    public void WebScrollDownToNonExistingComponent()
    {
        SelectTestActivity(_scrollWebViewQuery);

        try
        {
            _app.ScrollDownTo(_nonExistingWeb, strategy: _strategy, timeout: TimeSpan.FromSeconds(5));
            Assert.Fail();
        }
        catch (Exception e)
        {
            e.InnerException.Message.ShouldContain(
                _strategy == ScrollStrategy.Programmatically
                    ? "Unable to determine what view to programmatically scroll"
                    : "Timeout before element was found");
        }
    }

    [Test]
    public void WebScrollToExistingComponent()
    {

        SelectTestActivity(_scrollWebViewQuery);
        _app.ScrollDownTo(_lastInListWeb, strategy: _strategy, timeout: TimeSpan.FromMinutes(1));
        _app.WaitForElement(_lastInListWeb);
        _app.ScrollUpTo(_firstInListWeb, strategy: _strategy, timeout: TimeSpan.FromMinutes(2));
        _app.WaitForElement(_firstInListWeb);
    }

    [Test]
    public void WebScrollToBottomTop()
    {
        SelectTestActivity(_scrollWebViewQuery);
        _app.WaitForElement(_firstInListWeb);
        _app.ScrollToVerticalEnd(strategy: _strategy, timeout: TimeSpan.FromSeconds(15));
        _app.WaitForNoElement(_firstInListWeb);
        _app.ScrollToVerticalStart(strategy: _strategy, timeout: TimeSpan.FromSeconds(15));
        _app.WaitForElement(_firstInListWeb);
    }

    void AssertNotThereScrollNowThere(Func<AppQuery, AppWebQuery> query, ScrollStrategy strategy)
    {
        var isThere = _app.Query(query).Any();
        isThere.ShouldBeFalse();
        _app.ScrollTo(query, strategy: strategy, timeout: TimeSpan.FromMinutes(1));
        _app.Screenshot("scrolledTo");
        var isThereNow = _app.WaitForElement(query).Any();
        isThereNow.ShouldBeTrue();
    }

    [Test]
    public void ScrollToWebView()
    {
        SelectTestActivity(_scrollWebViewQuery);
        _app.WaitForElement(_firstInListWeb);
        AssertNotThereScrollNowThere(_line2_2, _strategy);
        AssertNotThereScrollNowThere(_line12, _strategy);
        AssertNotThereScrollNowThere(_firstInListWeb, _strategy);
    }

    [Test]
    public void ScrollToNonExistingWeb()
    {
        SelectTestActivity(_scrollWebViewQuery);

        try
        {
            _app.ScrollTo(_nonExistingWeb, strategy: _strategy);
            Assert.Fail();
        }
        catch (Exception e)
        {
            e.InnerException.Message.ShouldContain("no view anywhere in the view hierarchy");
        }
    }

    [Test]
    public void ScrollDownUpWeb()
    {
        SelectTestActivity(_scrollWebViewQuery);
        _app.WaitForElement(_firstInListWeb);
        _app.ScrollDown(strategy: _strategy, withInertia: false);
        _app.WaitForNoElement(_firstInListWeb);
        _app.ScrollUp(strategy: _strategy, withInertia: false);
        _app.ScrollUp(strategy: _strategy, withInertia: false);
        _app.WaitForElement(_firstInListWeb);
    }
}
