using System;
using System.Threading;
using Xamarin.UITest.Queries;
using Xamarin.UITest.Shared;

namespace Xamarin.UITest.Android.Scroll
{
    class WebViewScrollInteraction : IProgramaticScrollInteraction
    {
        readonly AppQuery _query;
        readonly AndroidGestures _gestures;
        readonly ViewMoveHelper _moveHelper;
        readonly WaitForHelper _waitForHelper;

        public WebViewScrollInteraction(AppQuery query, AndroidGestures gestures) 
        {
            _gestures = gestures;
            _query = query;
            _moveHelper = new ViewMoveHelper(query, gestures);
            _waitForHelper = new WaitForHelper(TimeSpan.FromMilliseconds(500));
        }

        public AppQuery Query()
        {
            return _query;
        }

        public bool Supports(ScrollDirection direction)
        {
            return Axis.Vertical.OnAxis(direction);
        }

        private string DirectionToMethodName(ScrollDirection direction)
        {
            return direction == ScrollDirection.Down ? "pageDown" : "pageUp";
        }

        public bool Scroll(ScrollDirection direction)
        {
            if (Supports(direction))
            {
                _moveHelper.PreScroll();
                _gestures.Query(_query.Invoke(DirectionToMethodName(direction), false));
                _waitForHelper.WaitForOrElapsed(() => _moveHelper.PostScroll());
                return _moveHelper.PostScroll();
            } 
            throw new Exception(String.Format("Webviews cannot be scrolled programatically in direction {0}", direction.ToString()));
        }

        public void ScrollToStart()
        {
            _gestures.Query(_query.Invoke(DirectionToMethodName(ScrollDirection.Up), true));
        }

        public void ScrollToEnd()
        {
            _gestures.Query(_query.Invoke(DirectionToMethodName(ScrollDirection.Down), true));
        }
    }
}