using NUnit.Framework;
using Should;

public class DragAndDropTests : AndroidTestAppBase
{
    protected override void OnAppStarted()
    {
        SelectTestActivity(c => c.Button("Drag and Drop"));

        _app.SetOrientationPortrait(); // TODO: Remove this once Monkey has been replaced with test-cloud-android-shell-java
    }

    [Test]
    public void DragAndDropTest()
    {
        _app.WaitForElement("redImageView");

        _app.Query(c => c.Id("targetTextView").Property("text").Value<string>())[0].ShouldEqual("green");

        _app.DragAndDrop("redImageView", "targetTextView");

        _app.Query(c => c.Id("targetTextView").Property("text").Value<string>())[0].ShouldEqual("red");
    }
}
