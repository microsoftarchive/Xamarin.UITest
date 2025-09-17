using System;
using NUnit.Framework;
using Should;

public class GeneralScrollTests : AndroidTestAppBase
{
    [Test]
    public void ScrollToNoTarget()
    {
        SelectTestActivity(c => c.Button("buttonScrollplicated"));

        try
        {
            _app.ScrollTo(c => c.Text("XBottom Text"));
            Assert.Fail("No exception was thrown");
        }
        catch (Exception e)
        {
            e.InnerException.Message.ShouldContain("Unable to scrollTo, no view anywhere in the view hierarchy matches");
        }
    }
}
