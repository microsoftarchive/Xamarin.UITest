using NUnit.Framework;

public class SliderTests : AndroidTestAppBase
{
    [Test]
    public void SetSliderValue()
    {
        SelectTestActivity(c => c.Button("Views Sample"));

        _app.WaitForElement("seekBar");
        _app.SetSliderValue("seekBar", 88);

        Assert.AreEqual(88, _app.Query(c => c.Marked("seekBar").Invoke("getProgress").Value<int>())[0]);
    }
}
