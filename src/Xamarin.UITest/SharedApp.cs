using System;
using Xamarin.UITest.Queries;
using Xamarin.UITest.Queries.Tokens;
using Xamarin.UITest.Shared.Logging;
using Xamarin.UITest.Utils;
using System.Linq;
using Xamarin.UITest.Shared;

namespace Xamarin.UITest
{
    internal class SharedApp
    {
        readonly QueryPlatform _platform;
        readonly IGestures _gestures;
        readonly ErrorReporting _errorReporting;

        public SharedApp(QueryPlatform platform, IGestures gestures)
        {
            _platform = platform;
            _gestures = gestures;
            _errorReporting = new ErrorReporting(_platform);
        }

        public ErrorReporting ErrorReporting
        {
            get { return _errorReporting; }
        }

        public void LogWithWithinQuery(string message, Func<AppQuery, AppQuery> withinQuery)
        {
            var expanded = ExpandIfNotNull(withinQuery);
            Log.Info(AppendToMessageIfNotNotNull(message, "withinQuery:", expanded));
        }

        public void LogWithToQuery(string message, Func<AppQuery, AppWebQuery> toQuery)
        {
            var expanded = Expand(toQuery);
            Log.Info(AppendToMessageIfNotNotNull(message, expanded));
        }

        public void LogWithToQueryWithinQuery(string message, Func<AppQuery, AppQuery> toQuery, Func<AppQuery, AppQuery> withinQuery)
        {
            var expandedTo = ExpandIfNotNull(toQuery);
            var expandedWithin = ExpandIfNotNull(withinQuery);
            var m = AppendToMessageIfNotNotNull(message, expandedTo);
            Log.Info(AppendToMessageIfNotNotNull(m, "withinQuery:", expandedWithin));
        }

        public void LogWithToQueryWithinQuery(string message, Func<AppQuery, AppWebQuery> toQuery, Func<AppQuery, AppQuery> withinQuery)
        {
            var expandedTo = Expand(toQuery);
            var expandedWithin = ExpandIfNotNull(withinQuery);
            var m = AppendToMessageIfNotNotNull(message, expandedTo);
            Log.Info(AppendToMessageIfNotNotNull(m, "withinQuery:", expandedWithin));
        }

        public string AppendToMessageIfNotNotNull(string message, string prefix, ITokenContainer container)
        {
            if (container != null)
            {
                return String.Format("{0} {1} {2}", message, prefix, TokenCodePrinter.ToCodeString(container));
            }
            return message;
        }

        public string AppendToMessageIfNotNotNull(string message, ITokenContainer container)
        {
            if (container != null)
            {
                return String.Format("{0} {1}", message, TokenCodePrinter.ToCodeString(container));
            }
            return message;
        }

        public AppQuery ExpandIfNotNull(Func<AppQuery, AppQuery> query)
        {
            return query == null ? null : Expand(query);
        }

        public AppQuery Expand(Func<AppQuery, AppQuery> query = null)
        {
            if (query == null)
            {
                return new AppQuery(_platform);
            }
            return query(new AppQuery(_platform));
        }

        public AppWebQuery Expand(Func<AppQuery, AppWebQuery> query)
        {
            return query(new AppQuery(_platform));
        }

        public AppTypedSelector<T> Expand<T>(Func<AppQuery, AppTypedSelector<T>> typedQuery)
        {
            return typedQuery(new AppQuery(_platform));
        }

        public string ToCodeString(ITokenContainer container)
        {
            return TokenCodePrinter.ToCodeString(container);
        }

        public Func<AppQuery, AppQuery> AsMarkedQuery(string marked)
        {
            return marked == null ? (Func<AppQuery, AppQuery>)null : (c => c.Marked(marked));
        }

        public T FirstWithLog<T>(T[] results, ITokenContainer tokenContainer)
        {
            if (!results.Any())
            {
                throw new Exception(
                    $"Unable to find element. Query for {ToCodeString(tokenContainer)} gave no results.");
            }

            Log.Info(results.Length == 1
                ? $"Using element matching {ToCodeString(tokenContainer)}."
                : $"Using first element ({results.Length} total) matching {ToCodeString(tokenContainer)}.");

            return results.First();
        }

        public void ScrollUp(double swipePercentage, int swipeSpeed, Func<AppQuery, AppQuery> withinQuery = null,
            ScrollStrategy strategy = ScrollStrategy.Auto, bool withInertia = true)
        {
            LogWithWithinQuery("Scrolling up", withinQuery);
            Log.Info("Scrolling up with query.");

            _gestures.Scroll(ExpandIfNotNull(withinQuery), ScrollDirection.Up, strategy, swipePercentage, swipeSpeed,
                withInertia);

            _gestures.WaitForNoneAnimatingOrElapsed();
        }


        public void ScrollDown(double swipePercentage, int swipeSpeed, Func<AppQuery, AppQuery> withinQuery = null,
            ScrollStrategy strategy = ScrollStrategy.Auto, bool withInertia = true)
        {
            LogWithWithinQuery("Scrolling down", withinQuery);

            _gestures.Scroll(ExpandIfNotNull(withinQuery), ScrollDirection.Down, strategy, swipePercentage,
                swipeSpeed, withInertia);

            _gestures.WaitForNoneAnimatingOrElapsed();
        }

        public void ScrollLeft(double swipePercentage, int swipeSpeed, Func<AppQuery, AppQuery> withinQuery = null,
            ScrollStrategy strategy = ScrollStrategy.Auto, bool withInertia = true)
        {
            LogWithWithinQuery("Scrolling left", withinQuery);

            _gestures.Scroll(ExpandIfNotNull(withinQuery), ScrollDirection.Left, strategy, swipePercentage,
                swipeSpeed, withInertia);

            _gestures.WaitForNoneAnimatingOrElapsed();
        }

        public void ScrollRight(double swipePercentage, int swipeSpeed, Func<AppQuery, AppQuery> withinQuery = null,
            ScrollStrategy strategy = ScrollStrategy.Auto, bool withInertia = true)
        {
            LogWithWithinQuery("Scrolling right", withinQuery);

            _gestures.Scroll(ExpandIfNotNull(withinQuery), ScrollDirection.Right, strategy, swipePercentage,
                swipeSpeed, withInertia);

            _gestures.WaitForNoneAnimatingOrElapsed();
        }

        public void ScrollUpTo(Func<AppQuery, AppWebQuery> toQuery, double swipePercentage, int swipeSpeed,
            Func<AppQuery, AppQuery> withinQuery = null, ScrollStrategy strategy = ScrollStrategy.Auto,
            bool withInertia = true, TimeSpan? timeout = null)
        {
            LogWithToQueryWithinQuery("Scrolling up to", toQuery, withinQuery);

            _gestures.ScrollTo(Expand(toQuery), ExpandIfNotNull(withinQuery), ScrollDirection.Up, strategy,
                swipePercentage, swipeSpeed, withInertia, timeout);
        }

        public void ScrollUpTo(Func<AppQuery, AppQuery> toQuery, double swipePercentage, int swipeSpeed,
            Func<AppQuery, AppQuery> withinQuery = null, ScrollStrategy strategy = ScrollStrategy.Auto,
            bool withInertia = true, TimeSpan? timeout = null)
        {
            LogWithToQueryWithinQuery("Scrolling up to", toQuery, withinQuery);

            _gestures.ScrollTo(Expand(toQuery), ExpandIfNotNull(withinQuery), ScrollDirection.Up, strategy,
                swipePercentage, swipeSpeed, withInertia, timeout);
        }


        public void ScrollDownTo(Func<AppQuery, AppQuery> toQuery, double swipePercentage, int swipeSpeed,
            Func<AppQuery, AppQuery> withinQuery = null, ScrollStrategy strategy = ScrollStrategy.Auto,
            bool withInertia = true, TimeSpan? timeout = null)
        {
            LogWithToQueryWithinQuery("Scrolling down to", toQuery, withinQuery);

            _gestures.ScrollTo(Expand(toQuery), ExpandIfNotNull(withinQuery), ScrollDirection.Down, strategy,
                swipePercentage, swipeSpeed, withInertia, timeout);
        }

        public void ScrollDownTo(Func<AppQuery, AppWebQuery> toQuery, double swipePercentage, int swipeSpeed,
            Func<AppQuery, AppQuery> withinQuery = null, ScrollStrategy strategy = ScrollStrategy.Auto,
            bool withInertia = true, TimeSpan? timeout = null)
        {
            LogWithToQueryWithinQuery("Scrolling down to", toQuery, withinQuery);

            _gestures.ScrollTo(Expand(toQuery), ExpandIfNotNull(withinQuery), ScrollDirection.Down, strategy,
                swipePercentage, swipeSpeed, withInertia, timeout);
        }

        private Func<AppQuery, AppQuery> WindowQuery()
        {
            if (_platform == QueryPlatform.Android)
            {
                // If this starts failing because we can't guarantee the order of elements
                // we could try: `return e => e.Id("content").Index(0);`
                return e => e.All().Index(0);
            }

            return e => e.Class("UIWindow").Index(0);
        }
        //to right
        public void SwipeLeftToRight(double swipePercentage, int swipeSpeed, bool withInertia)
        {
            SwipeLeftToRight(WindowQuery(), swipePercentage, swipeSpeed, withInertia);
        }

        public void SwipeLeftToRight(string marked, double swipePercentage, int swipeSpeed, bool withInertia)
        {
            SwipeLeftToRight(AsMarkedQuery(marked), swipePercentage, swipeSpeed, withInertia);
        }

        public void SwipeLeftToRight(Func<AppQuery, AppQuery> query, double swipePercentage, int swipeSpeed,
            bool withInertia)
        {
            _errorReporting.With(
                () => Swipe(query, ScrollDirection.Right, swipePercentage, swipeSpeed, withInertia),
                new object[] { query, swipePercentage, swipeSpeed, withInertia });
        }

        public void SwipeLeftToRight(Func<AppQuery, AppWebQuery> query, double swipePercentage, int swipeSpeed,
            bool withInertia)
        {
            _errorReporting.With(
                () => Swipe(query, ScrollDirection.Right, swipePercentage, swipeSpeed, withInertia),
                new object[] { query, swipePercentage, swipeSpeed, withInertia });
        }
        //to left
        public void SwipeRightToLeft(double swipePercentage, int swipeSpeed, bool withInertia)
        {
            SwipeRightToLeft(WindowQuery(), swipePercentage, swipeSpeed, withInertia);
        }

        public void SwipeRightToLeft(Func<AppQuery, AppQuery> query, double swipePercentage, int swipeSpeed,
            bool withInertia)
        {
            _errorReporting.With(
                () => Swipe(query, ScrollDirection.Left, swipePercentage, swipeSpeed, withInertia),
                new object[] { query, swipePercentage, swipeSpeed, withInertia });
        }

        public void SwipeRightToLeft(Func<AppQuery, AppWebQuery> query, double swipePercentage, int swipeSpeed,
            bool withInertia)
        {
            _errorReporting.With(
              () => Swipe(query, ScrollDirection.Left, swipePercentage, swipeSpeed, withInertia),
              new object[] { query, swipePercentage, swipeSpeed, withInertia });
        }

        public void SwipeRightToLeft(string marked, double swipePercentage, int swipeSpeed, bool withInertia)
        {
            SwipeRightToLeft(AsMarkedQuery(marked), swipePercentage, swipeSpeed, withInertia);
        }

        //to up
        public void SwipeDownToUp(double swipePercentage, int swipeSpeed, bool withInertia)
        {
            SwipeDownToUp(WindowQuery(), swipePercentage, swipeSpeed, withInertia);
        }

        public void SwipeDownToUp(Func<AppQuery, AppQuery> query, double swipePercentage, int swipeSpeed,
            bool withInertia)
        {
            _errorReporting.With(
                () => Swipe(query, ScrollDirection.Up, swipePercentage, swipeSpeed, withInertia),
                new object[] { query, swipePercentage, swipeSpeed, withInertia });
        }

        public void SwipeDownToUp(Func<AppQuery, AppWebQuery> query, double swipePercentage, int swipeSpeed,
            bool withInertia)
        {
            _errorReporting.With(
              () => Swipe(query, ScrollDirection.Up, swipePercentage, swipeSpeed, withInertia),
              new object[] { query, swipePercentage, swipeSpeed, withInertia });
        }

        public void SwipeDownToUp(string marked, double swipePercentage, int swipeSpeed, bool withInertia)
        {
            SwipeDownToUp(AsMarkedQuery(marked), swipePercentage, swipeSpeed, withInertia);
        }

        //to down
        public void SwipeUpToDown(double swipePercentage, int swipeSpeed, bool withInertia)
        {
            SwipeUpToDown(WindowQuery(), swipePercentage, swipeSpeed, withInertia);
        }

        public void SwipeUpToDown(Func<AppQuery, AppQuery> query, double swipePercentage, int swipeSpeed,
            bool withInertia)
        {
            _errorReporting.With(
                () => Swipe(query, ScrollDirection.Down, swipePercentage, swipeSpeed, withInertia),
                new object[] { query, swipePercentage, swipeSpeed, withInertia });
        }

        public void SwipeUpToDown(Func<AppQuery, AppWebQuery> query, double swipePercentage, int swipeSpeed,
            bool withInertia)
        {
            _errorReporting.With(
              () => Swipe(query, ScrollDirection.Down, swipePercentage, swipeSpeed, withInertia),
              new object[] { query, swipePercentage, swipeSpeed, withInertia });
        }

        public void SwipeUpToDown(string marked, double swipePercentage, int swipeSpeed, bool withInertia)
        {
            SwipeUpToDown(AsMarkedQuery(marked), swipePercentage, swipeSpeed, withInertia);
        }

        public void Swipe(Func<AppQuery, AppQuery> query, ScrollDirection direction, double swipePercentage, int swipeSpeed,
            bool withInertia)
        {
            _errorReporting.With(() =>
                {
                    Swipe(Expand(query), direction, swipePercentage, swipeSpeed, withInertia);
                }, new object[] { query, direction, swipePercentage, swipeSpeed, withInertia });
        }

        public void Swipe(Func<AppQuery, AppWebQuery> query, ScrollDirection direction, double swipePercentage, int swipeSpeed,
            bool withInertia)
        {
            _errorReporting.With(() =>
                {
                    Swipe(Expand(query), direction, swipePercentage, swipeSpeed, withInertia);
                }, new object[] { query, direction, swipePercentage, swipeSpeed, withInertia });
        }

        void PerformSwipe(IRect target, ScrollDirection direction, double swipePercentage, int swipeSpeed, bool withInertia, string codeString)
        {
            var centerX = (int)target.CenterX;
            var centerY = (int)target.CenterY;
            var targetWidth = target.Width;
            var targetHeight = target.Height;

            Log.Info(string.Format("Swiping {3} on first element matching {0} at coordinates [ {1}, {2} ]. ", codeString, centerX, centerY, direction));

            int startX, endX, startY, endY;
            startX = endX = centerX;
            startY = endY = centerY;
            var xOffset = (int)((swipePercentage / 2.0f) * targetWidth);
            var yOffset = (int)((swipePercentage / 2.0f) * targetHeight);

            bool percentTooBig = false;
            switch (direction)
            {
                case ScrollDirection.Right: // left to right
                    startX = (centerX - xOffset);
                    endX = (centerX + xOffset);
                    percentTooBig = (startX <= target.X || endX >= targetWidth + target.X);
                    break;
                case ScrollDirection.Left: // right to left
                    startX = (centerX + xOffset);
                    endX = (centerX - xOffset);
                    percentTooBig = (endX <= target.X || startX >= targetWidth + target.X);
                    break;
                case ScrollDirection.Up: // down to up
                    startY = (centerY + yOffset);
                    endY = (centerY - yOffset);
                    percentTooBig = (endY <= target.Y || startY >= targetHeight + target.Y);
                    break;
                case ScrollDirection.Down: // up to down
                    startY = (centerY - yOffset);
                    endY = (centerY + yOffset);
                    percentTooBig = (startY <= target.Y || endY >= targetHeight + target.Y);
                    break;
                default:
                    throw new Exception(string.Format("Unable to swipe in direction {0}", direction));
            }

            if (percentTooBig)
            {
                throw new Exception(string.Format(
                    "Invalid swipe coordinates ({0}, {1}) to ({2}, {3}).{4}Try setting swipePercentage smaller than {5}.",
                    startX, startY, endX, endY, Environment.NewLine, swipePercentage));
            }

            Log.Info(string.Format("Swiping from ({0}, {1}) to ({2}, {3})", startX, startY, endX, endY));

            var duration = CalculateDurationForSwipe(startX, startY, endX, endY, swipeSpeed);

            _gestures.SwipeCoordinates(startX, endX, startY, endY, withInertia, TimeSpan.FromMilliseconds(duration));
        }

        public void Swipe(AppQuery appQuery, ScrollDirection direction, double swipePercentage, int swipeSpeed,
            bool withInertia)
        {
            var results = _gestures.QueryGestureWait(appQuery);
            if (!results.Any())
            {
                throw new Exception(string.Format("Unable to swipe {0}. Query for {1} gave no results.", direction, ToCodeString(appQuery)));
            }

            var target = results.First();
            PerformSwipe(target.Rect, direction, swipePercentage, swipeSpeed, withInertia, ToCodeString(appQuery));
        }

        public void Swipe(AppWebQuery appQuery, ScrollDirection direction, double swipePercentage, int swipeSpeed,
            bool withInertia)
        {
            var results = _gestures.QueryGestureWait(appQuery);
            if (!results.Any())
            {
                throw new Exception(string.Format("Unable to swipe {0}. Query for {1} gave no results.", direction, ToCodeString(appQuery)));
            }

            var target = results.First();
            PerformSwipe(target.Rect, direction, swipePercentage, swipeSpeed, withInertia, ToCodeString(appQuery));
        }

        public int CalculateDurationForSwipe(int startX, int startY, int endX, int endY, int pixelsPerSecond)
        {
            var distance = Math.Sqrt(Math.Pow(startX - endX, 2) + Math.Pow(startY - endY, 2));

            return (int)(distance / (pixelsPerSecond / 1000.0));
        }

        public static void BuildLogger(bool debug, string logDirectory)
        {
            var consoleLogConsumer = new ConsoleLogConsumer(debug, false);

            var fileLogConsumer = new FileLogConsumer(logDirectory);

            var logger = new LoggerFacade(consoleLogConsumer, fileLogConsumer, new TraceSourceLogConsumer());

            consoleLogConsumer.Consume(
                new LogEntry($"Full log file: {fileLogConsumer.LogPath}", LogLevel.Info, 0));

            Log.Initialize(logger);
        }
    }
}