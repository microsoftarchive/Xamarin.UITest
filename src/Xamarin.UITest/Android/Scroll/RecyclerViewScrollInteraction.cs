using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Xamarin.UITest.Queries;
using Xamarin.UITest.Queries.Tokens;
using Xamarin.UITest.Shared;
using Xamarin.UITest.Shared.Extensions;

namespace Xamarin.UITest.Android.Scroll
{
    class RecyclerViewScrollInteraction : IProgramaticScrollInteraction
	{
        readonly AppQuery _query;
        readonly AppTypedSelector<object> _layoutmanager;
        readonly AndroidGestures _gestures;

        public RecyclerViewScrollInteraction(AppQuery query, AndroidGestures gestures) 
        {
            _gestures = gestures;
            _query = query;
            _layoutmanager = _query.Invoke("getLayoutManager");
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
                var firstPositions = _gestures.Query(_layoutmanager.Invoke("findFirstCompletelyVisibleItemPosition").Value<int?>());

                if (firstPositions.Any() && firstPositions[0].HasValue)
                {
                    var firstPosition = firstPositions[0].Value;
                    var lastPositions = _gestures.Query(_layoutmanager.Invoke("findLastCompletelyVisibleItemPosition").Value<int?>());
                    var lastPosition = lastPositions.First().Value;

                    if (direction == ScrollDirection.Down)
                    {
                        _gestures.Query(_layoutmanager.Invoke("scrollToPositionWithOffset", lastPosition, 0));
                    } 
                    else if(direction == ScrollDirection.Up) 
                    {
                        var yposFirst = _gestures.Query(_layoutmanager.Invoke("findViewByPosition", firstPosition).Invoke("getTop").Value<int>()).First();
                        var yposLast = _gestures.Query(_layoutmanager.Invoke("findViewByPosition", lastPosition).Invoke("getTop").Value<int>()).First();

                        _gestures.Query(_layoutmanager.Invoke("scrollToPositionWithOffset", firstPosition, yposLast - yposFirst)) ;
                    }
                    var newfirstPosition = _gestures.Query(_layoutmanager.Invoke("findFirstCompletelyVisibleItemPosition").Value<int?>())[0].Value;
                    return newfirstPosition != firstPosition;
                }
            } 
            throw new Exception(String.Format("RecyclerView does not support be scrolled programatically in direction {0}", direction.ToString()));
        }

        public void ScrollToStart()
        {
            _gestures.Query(_layoutmanager.Invoke("scrollToPositionWithOffset", 0, 0));
        }

        public void ScrollToEnd()
        {
            var itemsCount = _gestures.Query(_layoutmanager.Invoke("getItemCount").Value<int?>());

            if (itemsCount.Any() && itemsCount[0].HasValue)
            {
                _gestures.Query(_layoutmanager.Invoke("scrollToPositionWithOffset", itemsCount[0].Value - 1, 0));
                return;
            } 
            throw new Exception("RecyclerView does not support be scrolled to end");
        }
	}

}