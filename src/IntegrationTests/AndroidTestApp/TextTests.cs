using System.Linq;
using NUnit.Framework;

[TestFixture]
public class TextTests : AndroidTestAppBase
{
    [Test]
    public void ClearTextUsingAppWebQuery()
    {
        _app.Tap("buttonGotoWebView");
        _app.EnterText(e => e.Css("#inputText"), "hello");
        _app.Tap(e => e.Css("#inputButton"));
        _app.WaitFor(() => _app.Query(e => e.Css("#result")).Single().TextContent.Equals("hello"));
        _app.ClearText(e => e.Css("#inputText"));
        _app.Tap(e => e.Css("#inputButton"));
        _app.WaitFor(() => _app.Query(e => e.Css("#result")).Single().TextContent.Equals(""));
    }
}

