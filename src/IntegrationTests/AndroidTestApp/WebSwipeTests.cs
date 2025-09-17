using System;
using System.Linq;
using NUnit.Framework;

public class WebSwipeTests : AndroidTestAppBase
{
    [Test]
    public void LeftToRightOnDiv()
    {
        var expected = "right";
        _app.ScrollDownTo("Web View Swipe", timeout: TimeSpan.FromMinutes(2));
        _app.Tap("Web View Swipe");
        _app.WaitForElement(e => e.Css("#swipeBox"));
        _app.Screenshot("Before Swipe");
        _app.SwipeLeftToRight(e => e.Css("#swipeBox"));
        var directionElement = _app.WaitForElement(e => e.Css("#direction"));
        _app.Screenshot("Swiped LeftToRight");
        var actual = directionElement.Single().TextContent;
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void RightToLeftOnDiv()
    {
        var expected = "left";
        _app.ScrollDownTo("Web View Swipe", timeout: TimeSpan.FromMinutes(2));
        _app.Tap("Web View Swipe");
        _app.WaitForElement(e => e.Css("#swipeBox"));
        _app.Screenshot("Before Swipe");
        _app.SwipeRightToLeft(e => e.Css("#swipeBox"));
        var directionElement = _app.WaitForElement(e => e.Css("#direction"));
        _app.Screenshot("Swiped RightToLeft");
        var actual = directionElement.Single().TextContent;
        Assert.AreEqual(expected, actual);
    }
}
