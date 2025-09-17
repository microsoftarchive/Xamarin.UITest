using System;
using NUnit.Framework;
using Xamarin.UITest;

public class GestureScrollTests : ScrollTestsBase
{
    public GestureScrollTests() : base(ScrollStrategy.Gesture)
    { }

    [Test]
    public void ScrollToNonExistantWithin()
    {
        SelectTestActivity(_scrollplicatedQuery);

        // Make sure gui has initialized
        _app.WaitForElement(_testing);

        try
        {
            _app.ScrollTo(_one, strategy: _strategy, withinQuery: _nonExisting);
            Assert.Fail();
        }
        catch (Exception e)
        {
            Assert.That(e.InnerException.Message.Contains("Unable to find view to scroll within"));
        }
    }
}
