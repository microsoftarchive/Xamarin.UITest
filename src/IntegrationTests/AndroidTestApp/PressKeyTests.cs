using System;
using NUnit.Framework;

public class PressKeyTests : AndroidTestAppBase
{
    [Test]
    [Category("pressmenu")]
    public void EnterMenu()
    {
        _app.WaitForElement("Web View");

        try
        {
            _app.WaitForElement("More options", string.Empty, TimeSpan.FromSeconds(2));
        }
        catch (Exception e)
        {
            if (e.InnerException.GetType() == typeof(TimeoutException))
            {
                Assert.Ignore();
            }
            throw;
        }

        _app.PressMenu();
        _app.WaitForElement("Settings");
        _app.PressMenu();
        _app.WaitForNoElement("Settings");
    }
}
