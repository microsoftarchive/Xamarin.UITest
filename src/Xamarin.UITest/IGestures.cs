using System;
using Xamarin.UITest.Queries;
using Xamarin.UITest.Utils;

namespace Xamarin.UITest
{
    internal interface IGestures
    {
        AppResult[] Query(AppQuery query);
        AppResult[] QueryGestureWait(AppQuery query);
        AppWebResult[] QueryGestureWait(AppWebQuery query);

        TreeElement[] Dump();
        TreeElement[] DumpWithDeviceAgent();
        void WaitForNoneAnimatingOrElapsed(TimeSpan? timeout = null);

        void Scroll(AppQuery withinQuery, ScrollDirection direction, ScrollStrategy strategy, double swipePercentage,
            int swipeSpeed, bool withInertia = true);

        void ScrollTo(AppQuery toQuery, AppQuery withinQuery, ScrollDirection direction, ScrollStrategy strategy,
            double swipePercentage, int swipeSpeed, bool withInertia = true, TimeSpan? timeout = null);

        void ScrollTo(AppWebQuery toQuery, AppQuery withinQuery, ScrollDirection direction, ScrollStrategy strategy,
            double swipePercentage, int swipeSpeed, bool withInertia = true, TimeSpan? timeout = null);

        void SwipeCoordinates(int fromX, int toX, int fromY, int toY, bool withInertia, TimeSpan duration);
    }
}