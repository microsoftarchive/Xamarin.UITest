using System;
using System.Linq;
using NUnit.Framework;
using Should;
using Xamarin.UITest.Queries;
using Xamarin.UITest.Utils;
using Xamarin.UITest.Configuration;
using Xamarin.UITest;

[TestFixture]
public abstract class ScrollTestsBase : AndroidTestAppBase
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

    protected readonly Func<AppQuery, AppQuery> _nonExisting = c => c.Text("XBottom Text");
    protected readonly Func<AppQuery, AppQuery> _one = c => c.Button("oneButton");
    protected readonly Func<AppQuery, AppQuery> _scrollplicatedQuery = c => c.Button("buttonScrollplicated");
    protected readonly Func<AppQuery, AppQuery> _testing = c => c.Button("testing");

    readonly Func<AppQuery, AppQuery> _lastInList = c => c.Text("Bottom Text");
    readonly Func<AppQuery, AppQuery> _lastInListViews = c => c.Text("TheEnd");
    readonly Func<AppQuery, AppQuery> _lastInListBottom = c => c.Id("t2extViewBottom");
    readonly Func<AppQuery, AppQuery> _firstInList = c => c.Id("textView");
    readonly Func<AppQuery, AppQuery> _firstInListViews = c => c.Text("Item 0");
    readonly Func<AppQuery, AppQuery> _shortScrollQuery = c => c.Button("buttonShortScroll");
    readonly Func<AppQuery, AppQuery> _multiScrollQuery = c => c.Button("buttonMultiScoll");
    readonly Func<AppQuery, AppQuery> _scrollViewTop = c => c.Id("scrollViewTop");
    readonly Func<AppQuery, AppQuery> _scrollViewBottom = c => c.Id("scrollViewBottom");
    readonly Func<AppQuery, AppQuery> _two = c => c.Button("twoButton");
    readonly Func<AppQuery, AppQuery> _middle = c=> c.Button("middle");
    readonly Func<AppQuery, AppQuery> _horizontalScroll = c => c.Button("Horizontalscroll");
    readonly Func<AppQuery, AppQuery> _start = c => c.Button("Start");
    readonly Func<AppQuery, AppQuery> _end = c => c.Button("End");

    protected readonly ScrollStrategy _strategy;

    public ScrollTestsBase(ScrollStrategy strategy)
    {
        _strategy = strategy;
    }

    protected override AndroidAppConfigurator ReConfigureApp(AndroidAppConfigurator app)
    {
        return app.WaitTimes(new ShortWaitTimes());
    }

    [Test]
    public void ScrollDownUp()
    {
        SelectTestActivity(_shortScrollQuery);
        _app.WaitForElement(_firstInList);
        _app.ScrollDown(strategy: _strategy, withInertia: false);
        _app.WaitForNoElement(_firstInList);
        _app.ScrollUp(strategy: _strategy, withInertia: false);
        _app.ScrollUp(strategy: _strategy, withInertia: false);
        _app.WaitForElement(_firstInList);
    }

    [Test]
    public void ScrollDownUpToExistingComponent()
    {
        SelectTestActivity(_shortScrollQuery);
        _app.ScrollDownTo(_lastInList, strategy: _strategy);
        _app.WaitForElement(_lastInList);
        _app.ScrollUpTo(_firstInList, strategy: _strategy);
        _app.WaitForElement(_firstInList);
    }

    [Test]
    public void ScrollDownToNonExistingComponent()
    {
        SelectTestActivity(_shortScrollQuery);
        try
        {
            _app.ScrollDownTo(_nonExisting, strategy: _strategy, timeout: TimeSpan.FromSeconds(10));
            Assert.Fail();
        }
        catch (Exception e)
        {
            e.InnerException.Message.ShouldContain(
                _strategy == ScrollStrategy.Programmatically
                    ? "Unable to scroll view found by"
                    : "Timeout before element was found");
        }
    }

    [Test]
    public void ScrollDownToExistingComponentWithInNotVisible()
    {
        SelectTestActivity(_shortScrollQuery);
        try
        {
            _app.ScrollDownTo(_lastInList, _lastInList, strategy: _strategy);
            Assert.Fail();
        }
        catch (Exception e)
        {
            e.InnerException.Message.ShouldContain("Unable to scroll, no element were found by query");
        }

    }

    [Test]
    public void ScrollToBottomTop()
    {
        SelectTestActivity(_shortScrollQuery);
        _app.ScrollToVerticalEnd(strategy: _strategy, timeout: TimeSpan.FromSeconds(7));
        _app.WaitForElement(_lastInList);
        _app.WaitForNoElement(_firstInList);
        _app.ScrollToVerticalStart(strategy: _strategy, timeout: TimeSpan.FromSeconds(7));
        _app.WaitForElement(_firstInList);
    }

    [Test]
    [Ignore("This test is not working")]
    public void ScrollToBottomTopListViews(
        [Values("ListViewLong", "RecycleViewLong")] string markedForActivity)
    {
        SelectTestActivity(c=>c.Button(markedForActivity));
        _app.WaitForNoElement(_lastInListViews);
        _app.ScrollToVerticalEnd(strategy: _strategy, timeout: TimeSpan.FromSeconds(17));
        _app.WaitForElement(_lastInListViews);
        _app.WaitForNoElement(_firstInListViews);
        _app.ScrollToVerticalStart(strategy: _strategy, timeout: TimeSpan.FromSeconds(17));
        _app.WaitForElement(_firstInListViews);
    }

    [Test]
    public void ScrollToRightmostLeftmost()
    {
        SelectTestActivity(_horizontalScroll);
        _app.WaitForNoElement(_end);
        _app.ScrollToHorizontalEnd(strategy: _strategy, timeout: TimeSpan.FromSeconds(10));
        _app.WaitForElement(_end);
        _app.WaitForNoElement(_start);
        _app.ScrollToHorizontalStart(strategy: _strategy, timeout: TimeSpan.FromSeconds(10));
        _app.WaitForElement(_start);
    }

    [Test]
    [Ignore("This test is not working")]
    public void ScrollToExistingComponentListViews(
        [Values("ListViewLong", "RecycleViewLong")] string markedForActivity)
    {
        SelectTestActivity(c=>c.Button(markedForActivity));
        _app.ScrollDownTo(_lastInListViews, strategy: _strategy, timeout: TimeSpan.FromMinutes(1));
        _app.WaitForElement(_lastInListViews);
        _app.ScrollUpTo(_firstInListViews, strategy: _strategy, timeout: TimeSpan.FromMinutes(1));
        _app.WaitForElement(_firstInListViews);
    }

    [Test]
    public void WithTargetsRightViewTop()
    {
        SelectTestActivity(_multiScrollQuery);
        _app.WaitForNoElement(_lastInList);
        _app.ScrollDownTo(_lastInList, _scrollViewTop, strategy: _strategy, timeout: TimeSpan.FromMinutes(1));
        _app.WaitForElement(_lastInList);
    }

    [Test]
    public void WithTargetsRightViewButtom()
    {
        SelectTestActivity(_multiScrollQuery);
        _app.WaitForNoElement(_lastInListBottom);
        _app.ScrollDownTo(_lastInListBottom, _scrollViewBottom, strategy: _strategy, timeout: TimeSpan.FromMinutes(1));
        _app.WaitForElement(_lastInListBottom);
    }

    [Test]
    public void ScrollLeftRight()
    {
        SelectTestActivity(_scrollplicatedQuery);
        _app.WaitForElement(_testing);
        _app.ScrollRight(strategy: _strategy, withInertia: false);
        _app.WaitForNoElement(_testing);
        _app.ScrollLeft(strategy: _strategy, withInertia: false);
        _app.ScrollLeft(strategy: _strategy, withInertia: false);
        _app.Screenshot("Scrolled left");
        _app.WaitForElement(_testing);
    }

    [Test]
    public void ScrollToLeftRight()
    {
        SelectTestActivity(_scrollplicatedQuery);
        _app.ScrollTo(_one);
        _app.ScrollLeftTo(_two, strategy: _strategy, timeout: TimeSpan.FromMinutes(1));
        _app.ScrollRightTo(_one, strategy: _strategy, timeout: TimeSpan.FromMinutes(1));
    }

    void AssertNotThereScrollNowThere(Func<AppQuery,AppQuery> query, ScrollStrategy strategy)
    {
        var isThere = _app.Query(query).Any();
        isThere.ShouldBeFalse();
        _app.Screenshot("scrollingTo");
        _app.ScrollTo(query, strategy: strategy, timeout: TimeSpan.FromMinutes(1));
        var isThereNow = _app.WaitForElement(query).Any();
        isThereNow.ShouldBeTrue();
    }

    [Test]
    public void ScrollTo()
    {
        SelectTestActivity(_scrollplicatedQuery);

        // Make sure gui has initialized
        _app.WaitForElement(_testing);
        AssertNotThereScrollNowThere(_one, _strategy);
        AssertNotThereScrollNowThere(_two, _strategy);
        AssertNotThereScrollNowThere(_testing, _strategy);
        AssertNotThereScrollNowThere(_two, _strategy);
        AssertNotThereScrollNowThere(_middle, _strategy);
    }

    [Test]
    public void ScrollToNonExisting()
    {
        SelectTestActivity(_scrollplicatedQuery);

        // Make sure gui has initialized
        _app.WaitForElement(_testing);

        try
        {
            _app.ScrollTo(_nonExisting, strategy: _strategy);
            Assert.Fail();
        }
        catch (Exception e)
        {
            e.InnerException.Message.ShouldContain("no view anywhere in the view hierarchy");
        }
    }
}
