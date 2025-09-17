using System;
using System.Linq;
using Xamarin.UITest.Queries;

namespace Xamarin.UITest.Android.Scroll
{
    class IndexViewInteraction : IProgramaticScrollInteraction
    {
        readonly AndroidGestures _gestures;
        readonly AppQuery _query;

        public IndexViewInteraction(AppQuery query, AndroidGestures gestures) 
        {
            _gestures = gestures;
            _query = query;
        }

        public bool Supports(ScrollDirection direction)
        {
            return Axis.Vertical.OnAxis(direction);
        }

        public AppQuery Query()
        {
            return _query;
        }

        public bool Scroll(ScrollDirection direction)
        {
            if (Supports(direction))
            {

                int?[] firstPositions = _gestures.Query(_query.Invoke("getFirstVisiblePosition").Value<int?>());

                if (firstPositions.Any() && firstPositions[0].HasValue)
                {
                    var firstPosition = firstPositions[0].Value;
                    var lastPositions = _gestures.Query(_query.Invoke("getLastVisiblePosition").Value<int>());
                    var lastPosition = lastPositions.First();

                    if (direction == ScrollDirection.Down)
                    {
                        var selectionIndex = firstPosition + Math.Max(lastPosition - firstPosition, 1);
                        _gestures.Query(_query.Invoke("setSelection", selectionIndex));
                    }
                    else if (direction == ScrollDirection.Up)
                    {
                        var selectionIndex = Math.Max(0, firstPosition + Math.Min(firstPosition - lastPosition + 1, -1));
                        _gestures.Query(_query.Invoke("setSelection", selectionIndex));
                    }
                    var newfirstPosition = _gestures.Query(_query.Invoke("getFirstVisiblePosition").Value<int?>())[0].Value;
                    return newfirstPosition != firstPosition;
                }
            }
            throw new Exception(String.Format("IndexView cannot be scrolled programatically in direction {0}", direction.ToString()));

        }

        public void ScrollToStart()
        {
            _gestures.Query(_query.Invoke("setSelection", 0));
        }

        public void ScrollToEnd()
        {
            int[] count = _gestures.Query(_query.Invoke("getCount").Value<int>());
            _gestures.Query(_query.Invoke("setSelection", count.First()));
        }
    }
}