using System.Linq;
using Xamarin.UITest.Queries;

namespace Xamarin.UITest.Android.Scroll
{
    class ViewMoveHelper {
        readonly AppQuery _query;
        readonly AndroidGestures _gestures;
        int oldY = -1;
        int oldX = -1;

        public ViewMoveHelper(AppQuery query, AndroidGestures gestures)
        {
            _query = query;
            _gestures = gestures;
        }

        public void PreScroll()
        {
            oldY = _gestures.Query(_query.Invoke("getScrollY").Value<int>()).Single();
            oldX = _gestures.Query(_query.Invoke("getScrollX").Value<int>()).Single();
        }

        public bool PostScroll()
        {
            var y = _gestures.Query(_query.Invoke("getScrollY").Value<int>()).Single();
            var x = _gestures.Query(_query.Invoke("getScrollX").Value<int>()).Single();
            return  y != oldY || x != oldX;
        }
    }
}