using NUnit.Framework;
using Should;

public class BackdoorTests : AndroidTestAppBase
{
    [Test]
    public void BackdoorTest()
    {
        const string stringArg = "my string arg";
        _app.Invoke("backdoorWithString", stringArg).ToString().ShouldEqual(stringArg);
        _app.Invoke("backdoorString").ToString().ShouldEqual("string");

        var result = _app.Invoke("backdoorWithString", new object[] { stringArg, new [] { "a", "b", "c" } });
        result.ShouldEqual(stringArg + ":3");
    }
}
