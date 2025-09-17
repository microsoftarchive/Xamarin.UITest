using System;
using NUnit.Framework;
using Xamarin.UITest.Queries;

[TestFixture]
public class SwipeTests : AndroidTestAppBase
{
    protected override void OnAppStarted()
    {
        SelectTestActivity(c => c.Button("buttonScrollplicated"));
    }

    private bool testingButtonIsVisible()
    {
        return _app.Query("testing").Length > 0;
    }

    [Test]
    public void TestSwipeWithQuery()
    {
        Func<AppQuery, AppQuery> q = e => e.Class("HorizontalScrollView");

        _app.SwipeRightToLeft(q, withInertia: false);
        Assert.IsFalse(testingButtonIsVisible());
        _app.Screenshot("Swipe right to left with query");

        _app.SwipeLeftToRight(q, withInertia: false);
        Assert.IsTrue(testingButtonIsVisible());
        _app.Screenshot("Swipe left to right with query");

        //Shouldn't be able to swipe button too far off screen
        q = e => e.Marked("testing");
        _app.SwipeRightToLeft(q, 0.6, withInertia: false);
        Assert.IsTrue(testingButtonIsVisible());
        _app.Screenshot("Swipe right to left with non-swipable query");

        _app.SwipeLeftToRight(q, 0.6, withInertia: false);
        Assert.IsTrue(testingButtonIsVisible());
        _app.Screenshot("Swipe left to right with non-swipable  query");
    }

    /*
     * When swiping a non-swipable element, the screen contents shouldn't change
     */
    [Test]
    public void TestSwipeWithNonSwipableQuery()
    {
        Func<AppQuery, AppQuery> q = e => e.Marked("Its Scollplicated"); //[Sic]

        _app.SwipeRightToLeft(q, withInertia: false);
        Assert.IsTrue(testingButtonIsVisible());
        _app.Screenshot("Swipe right to left with non-swipable query");

        _app.SwipeLeftToRight(q, withInertia: false);
        Assert.IsTrue(testingButtonIsVisible());
        _app.Screenshot("Swipe left to right with non-swipable  query");
    }

    [Test]
    public void TestDefaultSwipe()
    {
        _app.SwipeRightToLeft(withInertia: false);
        Assert.IsFalse(testingButtonIsVisible());
        _app.Screenshot("Default swipe right to left");

        _app.SwipeLeftToRight(withInertia: false);
        Assert.IsTrue(testingButtonIsVisible());
        _app.Screenshot("Default swipe left to right");
    }

    [Test]
    public void TestWeakSwipe()
    {
        _app.SwipeRightToLeft(0.05f, withInertia: false);
        Assert.IsTrue(testingButtonIsVisible());
        _app.Screenshot("Swipe Right To Left 5%");
    }

    [Test]
    public void TestStrongSwipe()
    {
        _app.SwipeRightToLeft(0.95f, withInertia: false);
        Assert.IsFalse(testingButtonIsVisible());
        _app.Screenshot("Swipe right to left 95%");

        _app.SwipeLeftToRight(0.95f, withInertia: false);
        Assert.IsTrue(testingButtonIsVisible());
        _app.Screenshot("Swipe left to right 95%");
    }
}
