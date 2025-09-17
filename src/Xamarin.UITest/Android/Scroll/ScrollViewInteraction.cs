using System;
using Xamarin.UITest.Queries;

namespace Xamarin.UITest.Android.Scroll
{
    internal class ScrollViewInteraction : IProgramaticScrollInteraction
    {
        readonly AppQuery _query;
        readonly AndroidGestures _gestures;
        readonly Axis _supports;
        readonly ViewMoveHelper _moveHelper;

        public ScrollViewInteraction(AppQuery query, AndroidGestures gestures, Axis supports)
        {
            _query = query;
            _gestures = gestures;
            _supports = supports;
            _moveHelper = new ViewMoveHelper(query, gestures);
        }

        public AppQuery Query()
        {
            return _query;
        }

        public bool Supports(ScrollDirection direction)
        {
            return _supports.OnAxis(direction);
        }

        int DirectionToInt(ScrollDirection direction)
        {
            switch (direction)
            {
                case ScrollDirection.Left:
                    return 0x00000011;
                case ScrollDirection.Right:
                    return 0x00000042;
                case ScrollDirection.Up:
                    return 0x00000021;
                case ScrollDirection.Down:
                    return 0x00000082;
                default:
                    throw new Exception("Not supported");
            }
        }

        public bool Scroll(ScrollDirection direction)
        {
            if (Supports(direction))
            {
                _moveHelper.PreScroll();
                _gestures.Query(_query.Invoke("pageScroll", DirectionToInt(direction)));
                return _moveHelper.PostScroll();
            }
            throw new Exception(String.Format("view cannot be scrolled programatically in direction {0}", direction.ToString()));
        }

        public void ScrollToStart()
        {
            _gestures.Query(_query.Invoke("fullScroll", DirectionToInt(_supports.Start())));
        }

        public void ScrollToEnd()
        {
            _gestures.Query(_query.Invoke("fullScroll", DirectionToInt(_supports.End())));
        }
    }
}