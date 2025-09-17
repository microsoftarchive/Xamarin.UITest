using NUnit.Framework;
using Xamarin.UITest;

public class GestureWebScrollTests : WebScrollTestsBase
{
    public GestureWebScrollTests() : base(ScrollStrategy.Gesture)
    { }

    [Test]
    public void ScrollLeftRightWeb()
    {
        SelectTestActivity(_scrollWebViewQuery);
        _app.WaitForElement(_firstInListWeb);
        _app.ScrollRight(strategy: _strategy, withInertia: false);
        _app.WaitForNoElement(_firstInListWeb);
        _app.ScrollLeft(strategy: _strategy, withInertia: false);
        _app.ScrollLeft(strategy: _strategy, withInertia: false);
        _app.WaitForElement(_firstInListWeb);
    }
}
