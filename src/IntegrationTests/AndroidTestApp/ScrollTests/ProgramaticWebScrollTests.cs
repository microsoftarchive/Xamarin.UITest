using System;
using NUnit.Framework;
using Should;
using Xamarin.UITest;

public class ProgramaticWebScrollTests : WebScrollTestsBase
{
    public ProgramaticWebScrollTests() : base(ScrollStrategy.Programmatically)
    { }

    [Test]
    public void ScrollRightLeftWebNoSupport()
    {
        SelectTestActivity(_scrollWebViewQuery);
        try
        {
            _app.ScrollRight(strategy: _strategy);
            Assert.Fail();
        }
        catch (Exception e)
        {
            e.InnerException.Message.ShouldContain("Unable to determine what view to programmatically scroll");
        }

        try
        {
            _app.ScrollLeft(strategy: _strategy);
            Assert.Fail();
        }
        catch (Exception e)
        {
            e.InnerException.Message.ShouldContain("Unable to determine what view to programmatically scroll");
        }
    }
}
