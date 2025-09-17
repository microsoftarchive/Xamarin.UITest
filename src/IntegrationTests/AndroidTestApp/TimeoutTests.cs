using System;
using System.Diagnostics;
using NUnit.Framework;
using Xamarin.UITest.Utils;
using Xamarin.UITest;

[TestFixture]
public class TimeoutTests : AndroidTestAppBase
{
    public override void BeforeEach()
    {
        //App started in tests not in base
    }

    [Test]
    public void GestureUntilDefaultTimeOut()
    {
        _app = _appConfiguration.StartApp();
        _app.Tap("Directional Swipe Measurer");
        _app.WaitForElement("Swipe Away!");
        Stopwatch stopwatch = null;

        var actual = Assert.Throws<Exception>(() => StartTimerAndScroll(ref stopwatch));
        stopwatch.Stop();

        Assert.GreaterOrEqual(stopwatch.Elapsed, TimeSpan.FromSeconds(15));
        Assert.AreEqual("Error while performing ScrollDownTo(Marked(\"None Existant Field\"), Marked(\"home\"), Gesture, 0,67, 500, True, null)",
                        actual.Message);
    }

    [Test]
    public void GestureUntilUserDefinedTimeOut()
    {
        _app = _appConfiguration.WaitTimes(new UserCreatedTimeouts()).StartApp();
        _app.Tap("Directional Swipe Measurer");
        _app.WaitForElement("Swipe Away!");
        Stopwatch stopwatch = null;

        var actual = Assert.Throws<Exception>(() => StartTimerAndScroll(ref stopwatch));
        stopwatch.Stop();

        Assert.LessOrEqual(stopwatch.Elapsed, TimeSpan.FromSeconds(10));
        Assert.GreaterOrEqual(stopwatch.Elapsed, TimeSpan.FromSeconds(2));
        Assert.AreEqual("Error while performing ScrollDownTo(Marked(\"None Existant Field\"), Marked(\"home\"), Gesture, 0,67, 500, True, null)",
                        actual.Message);
    }

    void StartTimerAndScroll(ref Stopwatch stopwatch)
    {
        stopwatch = Stopwatch.StartNew();
        _app.ScrollDownTo("None Existant Field", "home", ScrollStrategy.Gesture);
    }

    class UserCreatedTimeouts : IWaitTimes
    {
        public TimeSpan GestureCompletionTimeout
        {
            get
            {
                return TimeoutValue;
            }
        }

        public TimeSpan GestureWaitTimeout
        {
            get
            {
                return TimeoutValue;
            }
        }

        public TimeSpan WaitForTimeout
        {
            get
            {
                return TimeoutValue;
            }
        }

        TimeSpan TimeoutValue { get; } = TimeSpan.FromSeconds(2);
    }
}
