using System.Globalization;
using System.Threading;
using NUnit.Framework;

public class PinchTests : AndroidTestAppBase
{
    [Test]
    public void PinchToZoomInOnButtonDoesNotError()
    {
        _app.PinchToZoomIn(c => c.Button("Views Sample"));
    }

    [Test]
    public void PinchToZoomInFrenchCultureDoesNotError()
    {
        var originalCulture = Thread.CurrentThread.CurrentCulture.Name;
        Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("fr-FR");
        _app.PinchToZoomIn(c => c.Button("Views Sample"));
        Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(originalCulture);
    }
}
