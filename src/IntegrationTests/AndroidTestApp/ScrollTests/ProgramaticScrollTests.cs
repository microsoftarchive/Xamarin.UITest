using NUnit.Framework;
using Xamarin.UITest;

public class ProgramaticScrollTests : ScrollTestsBase
{
    public ProgramaticScrollTests() : base(ScrollStrategy.Programmatically)
    { }

    [Test]
    public void ScrollToNonExistantWithinIsIgnored()
    {
        SelectTestActivity(_scrollplicatedQuery);

        // Make sure gui has initialized
        _app.WaitForElement(_testing);
        _app.ScrollTo(_one, strategy: _strategy, withinQuery: _nonExisting);
    }
}
