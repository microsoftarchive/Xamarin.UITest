using System;
using Xamarin.UITest.Queries;

namespace Xamarin.UITest.Android.Scroll
{
    class GestureInteraction : IScrollInteraction 
    {
        readonly AppQuery _query;
        readonly AndroidGestures _gestures;
        readonly TimeSpan _timeout;
        readonly ScrollDirection _start;
        readonly ScrollDirection _end;
        readonly double _swipePercentage;
        readonly int _swipeSpeed;
        readonly bool _withInertia;

        public GestureInteraction(AppQuery query, AndroidGestures gestures, double swipePercentage, int swipeSpeed,
            bool withInertia, TimeSpan timeout, ScrollDirection direction)
        {
            _query = query; 
            _gestures = gestures;
            _timeout = timeout;
            _swipePercentage = swipePercentage;
            _swipeSpeed = swipeSpeed;
            _withInertia = withInertia;

            if (Axis.Vertical.OnAxis(direction))
            {
                _start = Axis.Vertical.Start();
                _end = Axis.Vertical.End();
            }
            else
            {
                _start = Axis.Horizontal.Start();
                _end = Axis.Horizontal.End();
            }
        }

        public AppQuery Query()
        {
            return _query;
        }

        public bool Supports(ScrollDirection direction)
        {
            return true;
        }

        public bool Scroll(ScrollDirection direction)
        {
            _gestures.Pan(_query, direction, _swipePercentage, _swipeSpeed, _withInertia);
            return true; // No way of knowing if we did scroll :-(
        }

        void TimelimitedScroll(ScrollDirection direction)
        {
            var maxWaitUtc = DateTime.UtcNow + _timeout;
            while (DateTime.UtcNow < maxWaitUtc)
            {
                Scroll(direction);
            }
        }

        public void ScrollToStart()
        {
            TimelimitedScroll(_start);
        }

        public void ScrollToEnd()
        {
            TimelimitedScroll(_end);
        }
    }
}