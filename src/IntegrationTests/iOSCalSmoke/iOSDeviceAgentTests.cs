using NUnit.Framework;
using System.Linq;
using System;
using System.Threading;
using IntegrationTests.Shared;

public class iOSDeviceAgentTests : IOSTestBase
{
    protected override AppInformation _appInformation => TestApps.iOSDeviceAgent;

    const string _VolumeRowId = "volume row";

    [Test]
    public void DeviceAgentGestureTest()
    {
        var miscButton = _app.Query(c => c.Marked("Misc")).First();

        _app.WaitForNoElement(c => c.Id(_VolumeRowId));

        var specifiers = new
        {
            coordinate = new
            {
                x = miscButton.Rect.CenterX,
                y = miscButton.Rect.CenterY
            }
        };

        _app.InvokeDeviceAgentGesture("touch", specifiers: specifiers);

        _app.WaitForElement(c => c.Id(_VolumeRowId));
    }

    [Test]
    public void DeviceAgentQueryTest()
    {
        // TODO: implement
    }

    [Test]
    public void DismissSpringboardAlertTest()
    {
        _app.Tap("Misc");
        _app.Tap(c => c.Id("alerts and sheets row"));
        _app.Tap(c => c.Id("calendar row"));
        _app.Tap(c => c.Id("contacts row"));
        _app.Tap(c => c.Id("reminders row"));
    }

    [Test]
    public void TwoFingerTap()
    {
        _app.Tap("Touch");
        _app.TwoFingerTapCoordinates(100, 100);
        _app.WaitForElement(c => c.Marked("gesture performed").Text("Two-finger Tap"));
    }

    [Test]
    public void SetOrientationTest()
    {
        _app.Screenshot("Before SetOrientationLandscape");
        _app.SetOrientationLandscape();
        _app.Screenshot("Before SetOrientationPortrait");
        _app.SetOrientationPortrait();
        _app.Screenshot("After SetOrientationPortrait");
    }

    [Test]
    public void VolumeTest()
    {
        if (_app.Device.IsSimulator)
        {
            _app.PressVolumeUp();
            _app.PressVolumeDown();
            return;
        }

        _app.Tap("Misc");

        _app.Tap(c => c.Id(_VolumeRowId));

        var previousVolume = Convert.ToDouble(_app.WaitForElement(c => c.Marked("current volume")).First().Text);

        _app.PressVolumeUp();

        Thread.Sleep(300);

        var currentVolume = Convert.ToDouble(_app.Query(c => c.Marked("current volume")).First().Text);

        Assert.IsTrue(currentVolume >= previousVolume);

        previousVolume = currentVolume;

        _app.PressVolumeDown();

        Thread.Sleep(300);

        currentVolume = Convert.ToDouble(_app.Query(c => c.Marked("current volume")).First().Text);

        Assert.IsTrue(currentVolume < previousVolume);

        previousVolume = currentVolume;

        _app.PressVolumeUp();

        Thread.Sleep(300);

        currentVolume = Convert.ToDouble(_app.Query(c => c.Marked("current volume")).First().Text);

        Assert.IsTrue(currentVolume > previousVolume);
    }

    [Test]
    public void ClearTextWithHandlers()
    {
        _app.Tap(c => c.Marked("Misc"));
        _app.Tap(c => c.Id("text input row"));
        _app.EnterText(c => c.Id("text field"), "My text field");
        _app.ClearText(c => c.Id("text field"));
        _app.ClearText(c => c.Id("text view"));
        _app.EnterText(c => c.Id("key input"), "My key input");
        _app.ClearText(c => c.Id("key input"));
    }

    [Test]
    public void AutLaunchedWithEnvVars()
    {
        _app.Tap(c => c.Marked("Misc"));
        _app.Tap(c => c.Marked("environment row"));
        _app.WaitForElement(c => c.Marked("environment page"));
        _app.WaitForElement(c => c.Text("Of course not!"));
    }

    [Test]
    public void AutLaunchedWithArguments()
    {
        _app.Tap(c => c.Marked("Misc"));
        _app.Tap(c => c.Marked("arguments row"));
        _app.WaitForElement(c => c.Marked("arguments page"));
        _app.WaitForElement(c => c.Text("The Calabus Driver is on the job!")); // requires test parameter options in class comment
    }
}