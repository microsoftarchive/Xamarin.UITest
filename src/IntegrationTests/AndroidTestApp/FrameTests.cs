using System;
using System.Linq;
using NUnit.Framework;
using Should;
using Xamarin.UITest;

[TestFixture]
public class FrameTests : AndroidTestAppBase
{
    [Test]
    [Ignore("This test is not working")]
    public void QueryFrame()
    {
        SelectTestActivity(c => c.Button("Frames Web View"));

        try
        {
            _app.WaitForElement(
                c => c.WebView().All().Frame("#same-domain").Css("#textarea"), 
                timeout:TimeSpan.FromMinutes(1)).Length.ShouldEqual(1);
        }
        catch
        {
            var froyoError = "Old Android doesn't support the SSL cert used by the frames test page. :(";

            if (_app.Query(c => c.WebView().All().Css("*")).Any(e => e.TextContent == froyoError))
            {
                return;
            }

            throw;
        }
        // Unfortunately, cross-domain iframe queries won't work on Android.
        // It's probably possible to use window.postMessage() if the user would add some javascript to the
        // iframe content page.
        //_app.WaitForElement(
        //    c => c.WebView().All().Frame("#x-origin").Css("#textarea")
        //    timeout:TimeSpan.FromMinutes(1)).Length.ShouldEqual(1);
    }
}
