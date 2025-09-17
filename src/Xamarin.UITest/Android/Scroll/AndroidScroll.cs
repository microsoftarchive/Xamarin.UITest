using System;
using System.Linq;
using Xamarin.UITest.Queries;
using Xamarin.UITest.Queries.Tokens;
using Xamarin.UITest.Shared;
using Xamarin.UITest.Shared.Extensions;

namespace Xamarin.UITest.Android.Scroll
{
    internal class AndroidScroll
    {
        readonly AndroidGestures _androidGestures;
        readonly WaitForHelper _waitForHelper;
        readonly TimeSpan _gestureWaitTimeout;

        public AndroidScroll(AndroidGestures androidGestures, WaitForHelper waitForHelper, TimeSpan gestureWaitTimeout)
        {
            _androidGestures = androidGestures;
            _waitForHelper = waitForHelper;
            _gestureWaitTimeout = gestureWaitTimeout;
        }

        public IScrollInteraction GetScrollableInteraction(AppQuery withinQuery, ScrollStrategy strategy, 
            ScrollDirection direction, double swipePercentage, int swipeSpeed, bool withInertia, TimeSpan timeout, 
            AppQuery toQuery = null)
        {
            if (withinQuery == null)
            {
                if (strategy == ScrollStrategy.Gesture)
                {
                    return GestureScrollInteractionFromDestination(toQuery, direction, swipePercentage, swipeSpeed,
                        withInertia, timeout);
                }

                // See if we are able to find a suitable scroll target by ourselves
                var interaction = ProgrammaticScrollInteractionFromDestination(toQuery, direction);
                if (interaction != null)
                {
                    return interaction;
                }
                if (strategy == ScrollStrategy.Auto)
                {
                    return GestureScrollInteractionFromDestination(toQuery, direction, swipePercentage, swipeSpeed, 
                        withInertia, timeout);
                }
                throw new Exception("Unable to determine what view to programmatically scroll, try specifying view or changing strategy to Gesture.");
            }

            // Find interaction from withinQuery
            var firstQuery = withinQuery.Index(0);
            var results = _androidGestures.QueryGestureWait(firstQuery);

            if (results.Length != 1)
            {
                throw new Exception(String.Format("Unable to scroll, no element were found by query: {0}", _androidGestures.ToCodeString(withinQuery)));
            }
            
            if (strategy == ScrollStrategy.Gesture)
            {
                return new GestureInteraction(firstQuery, _androidGestures, swipePercentage, swipeSpeed, withInertia, 
                    timeout, direction);
            }

            var programaticScrollInteraction = ProgramaticScrollInteractionFromContext(firstQuery, direction);

            if (programaticScrollInteraction is WebViewScrollInteraction && toQuery != null)
            {
                throw new Exception(String.Format("Unable to scroll to view found by {0}, the WebView specified by {1} cannot be parent of a normal view", _androidGestures.ToCodeString(toQuery), _androidGestures.ToCodeString(withinQuery)));
            }

            if (programaticScrollInteraction != null)
            {
                return programaticScrollInteraction;
            }
 
            if (strategy == ScrollStrategy.Auto)
            {
                return new GestureInteraction(firstQuery, _androidGestures, swipePercentage, swipeSpeed, withInertia, 
                    timeout, direction);
            }
            throw new Exception(String.Format("Unable to find view that can programmatically be scrolled using query: {0}. Try updating query or change strategy to Gesture", _androidGestures.ToCodeString(withinQuery)));
        }

        public IScrollInteraction GetScrollableInteraction(AppQuery withinQuery, ScrollStrategy strategy,
            ScrollDirection direction, double swipePercentage, int swipeSpeed, bool withInertia, TimeSpan timeout,
            AppWebQuery toQuery)
        {
            if (withinQuery == null)
            {
                withinQuery = GetParentFromWebQuery(toQuery); 
                if (withinQuery != null)
                {
                    return GetScrollableInteraction(withinQuery, strategy, direction, swipePercentage, swipeSpeed, 
                        withInertia, timeout, toQuery);
                } 
                if(strategy == ScrollStrategy.Programmatically) 
                {
                    throw new Exception("Unable to determine what view to programmatically scroll. Specify a within query or change strategy to Gesture");
                }

                return GestureScrollInteractionFromDestination(null, direction, swipePercentage, swipeSpeed, 
                    withInertia, timeout);
            }

            var firstQuery = withinQuery.Index(0);
            var results = _androidGestures.QueryGestureWait(firstQuery);

            if (results.Length != 1)
            {
                throw new Exception(String.Format("Unable to scroll, no element were found by query: {0}", _androidGestures.ToCodeString(withinQuery)));
            }

            if (strategy == ScrollStrategy.Gesture)
            {
                return new GestureInteraction(firstQuery, _androidGestures, swipePercentage, swipeSpeed, withInertia,
                    timeout, direction);
            }
 
            var isWebView = _androidGestures.Query(firstQuery.Class("android.webkit.WebView")).Any();
            if (isWebView)
            {
                var webViewScrollInteraction = new WebViewScrollInteraction(firstQuery, _androidGestures);
                if (webViewScrollInteraction.Supports(direction))
                {
                    return webViewScrollInteraction;
                }
            }

            if (strategy == ScrollStrategy.Auto)
            {
                return new GestureInteraction(withinQuery, _androidGestures, swipePercentage, swipeSpeed, withInertia,
                    timeout, direction);
            }
            throw new Exception(String.Format("Unable to find view that can programmatically be scrolled using query: {0}. Try updating query to ensure it targets a WebView or change strategy to Gesture", _androidGestures.ToCodeString(withinQuery)));
        }

        AppQuery GetParentFromWebQuery(AppWebQuery targetView)
        {
            var targetViewExistsInViewQuery = AsAllQuery(targetView);
            var targetViewExistsInView = _androidGestures.QueryGestureWait(targetViewExistsInViewQuery);

            if (targetViewExistsInView.Any())
            {
                var webViewId = targetViewExistsInView.First().WebView;
                if (!webViewId.IsNullOrWhiteSpace())
                {
                    return new AppQuery(QueryPlatform.Android).Id(webViewId);
                }
            }
            return null;
        }

        IScrollInteraction ProgramaticScrollInteractionFromContext(AppQuery context, ScrollDirection direction)
        {
            var posibilites = new IProgramaticScrollInteraction[]
            {
                new ScrollViewInteraction(context.Class("android.widget.ScrollView").Index(0), _androidGestures, Axis.Vertical),
                new IndexViewInteraction(context.Class("android.widget.AbsListView").Index(0), _androidGestures),
                new WebViewScrollInteraction(context.Class("android.webkit.WebView").Index(0), _androidGestures), 
                new RecyclerViewScrollInteraction(context.Class("android.support.v7.widget.RecyclerView").Index(0), _androidGestures), 
                new ScrollViewInteraction(context.Class("android.widget.HorizontalScrollView").Index(0), _androidGestures, Axis.Horizontal), 
            };

            IScrollInteraction matching = null;

            try
            {
                _waitForHelper.WaitFor(() =>
                {
                    foreach (var possibility in posibilites)
                    {
                        if (!possibility.Supports(direction))
                        {
                            continue;
                        }
                        var results = _androidGestures.Query(possibility.Query());
                        if (!results.Any())
                        {
                            continue;
                        }
                        matching = possibility;
                        return true;
                    }
                    //we should return true to exit WaitFor
                    return true;
                }, timeout: _gestureWaitTimeout);
            }
            catch (TimeoutException)
            {
                // Ignore
            }
            return matching;
        }


        IScrollInteraction ProgrammaticScrollInteractionFromDestination(AppQuery targetView, ScrollDirection direction)
        {
            var context = QueryContext(targetView);
            return ProgramaticScrollInteractionFromContext(context, direction);
        }

        void ScrollToView(AppQuery query) 
        {
            _androidGestures.PerformAction("scroll_to_view", query.ToString());
        }

        void ScrollToView(AppWebQuery query) 
        {
            _androidGestures.PerformAction("scroll_to_view", query.ToString());
        }

        AppQuery GestureQueryFromDestination(AppQuery targetView, ScrollDirection? direction)
        {
            var context = QueryContext(targetView);

            var possibilites = new[]
            {
                new {Query = context.Class("android.widget.ScrollView"), Supports = Axis.Vertical},
                new {Query = context.Class("android.widget.HorizontalScrollView"), Supports = Axis.Horizontal}, 
                new {Query = context.Class("android.widget.AbsListView"), Supports = Axis.Vertical},
                new {Query = context.Class("android.webkit.WebView"), Supports = Axis.Both}
            };

            AppQuery query = new AppQuery(QueryPlatform.Android).Id("content"); // Fallback
            try
            {
                _waitForHelper.WaitFor(() =>
                {
                    float area = 0;
                    foreach (var possibility in possibilites)
                    {
                        if (!direction.HasValue || possibility.Supports.OnAxis(direction.Value))
                        {
                            var results = _androidGestures.Query(possibility.Query);
                            if (!results.Any())
                            {
                                continue;
                            }
                            var bestForClass = results.Select((r, i) => (new {area = r.Rect.Width*r.Rect.Height, index = i})).OrderBy(e => e.area).First();
                            if (area >= bestForClass.area)
                            {
                                continue;
                            }
                            area = bestForClass.area;
                            query = possibility.Query.Index(bestForClass.index);
                        }
                    }
                    //we should return true to exit WaitFor
                    return true;
                }, timeout: _gestureWaitTimeout);
            }
            catch (TimeoutException)
            {
                // Ignore
            }
            return query;
        }

        IScrollInteraction GestureScrollInteractionFromDestination(AppQuery targetView, ScrollDirection direction,
            double swipePercentage, int swipeSpeed, bool withInertia, TimeSpan timeout)
        {
            return new GestureInteraction(GestureQueryFromDestination(targetView, direction), _androidGestures, 
                swipePercentage, swipeSpeed, withInertia, timeout, direction);
        }

        AppQuery GestureScrollToWithinQueryFromDestination(AppQuery targetView)
        {
            return GestureQueryFromDestination(targetView, null);
        }

        AppQuery QueryContext(AppQuery targetView)
        {
            AppQuery context = new AppQuery(QueryPlatform.Android);
            if (targetView != null)
            {
                var targetViewExistsInView = AsAllQuery(targetView);
                if (_androidGestures.Query(targetViewExistsInView).Any())
                {
                    context = targetViewExistsInView.Parent();
                }
            }
            return context;
        }

        AppQuery AsAllQuery(AppQuery query) 
        {
            return new AppQuery((new AppQuery(QueryPlatform.Android)).All(), ((ITokenContainer)query).Tokens);
        }

        AppWebQuery AsAllQuery(AppWebQuery query) 
        {
            return new AppWebQuery(new IQueryToken[] {new RawToken("all", "All()")}, QueryPlatform.Android, ((ITokenContainer)query).Tokens);
        }

        void GestureScrollToView(AppQuery within, AppQuery toQuery, double swipePercentage, int swipeSpeed, 
            bool withInertia, TimeSpan timeout)
        {
            var withinQuery = within ?? GestureScrollToWithinQueryFromDestination(toQuery);
            var appResults = _androidGestures.QueryGestureWait(withinQuery);
            
            if (!appResults.Any())
            {
                throw new Exception("Unable to find view to scroll within");
            }
            
            var withinElement = appResults.First();
            var withinRect = OnScreenRect(withinElement, withinQuery);
            var toAllQuery = AsAllQuery(toQuery);
            var viewConfig = _androidGestures.ViewConfiguration(withinQuery);

            Func<IRect> toQueryRect = () => _androidGestures.Query(toAllQuery).First().Rect;

            GestureScrollToInternal(
                timeout, toQueryRect, withinRect, viewConfig, swipePercentage, swipeSpeed, withInertia);
        }

        void GestureScrollToView(AppQuery within, AppWebQuery toAllQuery, double swipePercentage, int swipeSpeed,
            bool withInertia, TimeSpan timeout)
        {
            var withinElement = _androidGestures.Query(within).First();
            var withinRect = OnScreenRect(withinElement, within);
            var viewConfig = _androidGestures.ViewConfiguration(within);

            Func<IRect> toQueryRect = () => _androidGestures.Query(toAllQuery).First().Rect;

            GestureScrollToInternal(
                timeout, toQueryRect, withinRect, viewConfig, swipePercentage, swipeSpeed, withInertia);
        }

        void GestureScrollToInternal(TimeSpan timeout, Func<IRect> toQueryRect, IRect withinRect,
            ViewConfiguration viewConfig, double swipePercentage, int swipeSpeed, bool withInertia)
        {
            var maxWaitUtc = DateTime.UtcNow + timeout;
            var offset = (float)((1.0 - swipePercentage) / 2.0);
            var widthAdjustment = 1 - 2*offset;

            while (DateTime.UtcNow < maxWaitUtc)
            {
                var toRect = toQueryRect.Invoke();
                
                var offsetX = CalulateOffset(toRect.X, toRect.Width, withinRect.X, withinRect.Width);
                var panAmountX = Math.Min(withinRect.Width*widthAdjustment, Math.Abs(offsetX)) + viewConfig.ScaledTouchSlop + 1;
                var left = withinRect.X + withinRect.Width*offset;
                var right = left + panAmountX;

                var offsetY = CalulateOffset(toRect.Y, toRect.Height, withinRect.Y, withinRect.Height);
                var panAmountY = Math.Min(withinRect.Height*widthAdjustment, Math.Abs(offsetY)) + viewConfig.ScaledTouchSlop + 1;
                var up = withinRect.Y + withinRect.Height*offset;
                var down = up + panAmountY;

                float fromX = left, fromY = up, toX = left, toY = up;
                if (offsetX >= 0)
                {
                    fromX = left;
                    toX = right;
                }
                else if (offsetX < 0)
                {
                    fromX = right;
                    toX = left;
                }

                if (offsetY >= 0)
                {
                    fromY = up;
                    toY = down;
                }
                else if (offsetY < 0)
                {
                    fromY = down;
                    toY = up;
                }

                var absOffsetX = Math.Abs(offsetX);
                var absOffsetY = Math.Abs(offsetY);
                if (absOffsetX < toRect.Width / 2 && absOffsetX < withinRect.Width / 2 && absOffsetY < toRect.Height / 2 && absOffsetY < withinRect.Height / 2)
                {
                    break;
                }

                _androidGestures.DragViaGesture(fromX, fromY, toX, toY, swipeSpeed, withInertia);
            }
        }

        AppRect OnScreenRect(AppResult within, AppQuery withinQuery)
        {
            var rect = within.Rect;
            var ancestors = _androidGestures.Query(withinQuery.Parent());

            foreach (var ancestor in ancestors)
            {
                rect = Intersection(rect, ancestor.Rect);
            }

            return rect;
        }

        class Segment
        {
            readonly float _start;
            readonly float _extend;

            public Segment(float start, float extend)
            {
                _start = start;
                _extend = extend;
            }

            public float Extend { get { return _extend; } }
            public float Start { get { return _start; } }
        }

        AppRect Intersection(AppRect rect, IRect appRect)
        {
            Segment xw = Intersection(rect.X, rect.Width, appRect.X, appRect.Width);
            Segment yh = Intersection(rect.Y, rect.Height, appRect.Y, appRect.Height);
            var result = new AppRect { X = xw.Start, Width = xw.Extend, Y = yh.Start, Height = yh.Extend };
            return result;
        }

        Segment Intersection(float start1, float extend1, float start2, float extend2)
        {
            float end1 = start1 + extend1;
            float end2 = start2 + extend2;
            if (end1 < start2 || end2 < start1)
            {
                return new Segment(0f,0f);
            }
            var absStart = Math.Max(start1, start2);
            var absEnd = Math.Min(end1, end2);
            
            return new Segment(absStart, absEnd - absStart);
        }

        float CalulateOffset(float toStart, float toSize, float withinStart, float withinSize)
        {
            var offset = 0.0f;
            if (toSize > withinSize)
            {
                // Bring center to center
                offset = withinStart + withinSize / 2 - toStart - toSize / 2;
            }
            else
            {
                if (toStart < withinStart)
                {
                    offset = withinStart - toStart; // + 
                }
                else
                {
                    var toEnd = toStart + toSize;
                    var withinEnd = withinStart + withinSize;

                    if (toEnd > withinEnd)
                    {
                        offset = withinEnd - toEnd; // - 
                    }
                }
            }
            return offset;
        }

        public void ScrollTo(AppQuery toQuery, AppQuery within, ScrollStrategy strategy, double swipePercentage, 
            int swipeSpeed, bool withInertia, TimeSpan timeout)
        {
            if (_androidGestures.Query(toQuery).Any())
            {
                return;
            }

            var targetView = AsAllQuery(toQuery).Index(0);
            var targetViewExists = _androidGestures.QueryGestureWait(targetView).Any();

            if (!targetViewExists)
            {
                throw new Exception(string.Format("Unable to scrollTo, no view anywhere in the view hierarchy matches {0}.", _androidGestures.ToCodeString(toQuery)));
            }

            if (strategy == ScrollStrategy.Gesture) 
            {
                GestureScrollToView(within, toQuery, swipePercentage, swipeSpeed, withInertia, timeout);
            } 
            else 
            {
                ScrollToView(targetView);
            }
        }

        public void ScrollTo(AppWebQuery toQuery, ScrollStrategy strategy, double swipePercentage, int swipeSpeed,
            bool withInertia, TimeSpan timeout)
        {
            if (_androidGestures.Query(toQuery).Any())
            {
                return;
            }

            var targetView = AsAllQuery(toQuery);
            var targetViewExists = _androidGestures.QueryGestureWait(targetView).Any();

            if (!targetViewExists)
            {
                throw new Exception(string.Format("Unable to scrollTo, no view anywhere in the view hierarchy matches {0}.", _androidGestures.ToCodeString(toQuery)));
            }

            if (strategy == ScrollStrategy.Gesture) 
            {
                var within = GetParentFromWebQuery(toQuery);
                GestureScrollToView(within, targetView, swipePercentage, swipeSpeed, withInertia, timeout);
            } 
            else 
            {
                ScrollToView(targetView);
            }
        }
    }
}