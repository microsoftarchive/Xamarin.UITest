using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.UITest.Android.Scroll;
using Xamarin.UITest.Queries;
using Xamarin.UITest.Queries.PlatformSpecific;
using Xamarin.UITest.Queries.Tokens;
using Xamarin.UITest.Shared;
using Xamarin.UITest.Shared.Http;
using Xamarin.UITest.Shared.Json;
using Xamarin.UITest.Shared.Logging;
using Xamarin.UITest.Utils;

namespace Xamarin.UITest.Android
{
    internal class AndroidGestures : IGestures
    {
        readonly HttpClient _httpClient;
        readonly WaitForHelper _waitForHelper;
        readonly TimeSpan _gestureWaitTimeout;
        readonly TimeSpan _gestureCompletionTimeout;
        readonly AndroidScroll _androidScroll;
        readonly SharedApp _sharedApp;


        public AndroidGestures(HttpClient httpClient, WaitForHelper waitForHelper, IWaitTimes waitTimes)
        {
            _httpClient = httpClient;
            _waitForHelper = waitForHelper;
            _gestureWaitTimeout = waitTimes.GestureWaitTimeout;
            _gestureCompletionTimeout = waitTimes.GestureCompletionTimeout;
            _androidScroll = new AndroidScroll(this, waitForHelper, waitTimes.GestureWaitTimeout);
            _sharedApp = new SharedApp(QueryPlatform.Android, this);
        }

        public AppResult[] Query(AppQuery query)
        {
            return Query<AndroidResult>(query, null, "query")
                    .Select(x => new AppResult(x))
                    .ToArray();
        }

        public AppResult[] Flash(AppQuery query)
        {
            return Query<AndroidResult>(query, null, "flash")
                .Select(x => new AppResult(x))
                .ToArray();
        }

        public AppResult[] QueryGestureWait(AppQuery query)
        {
            return _waitForHelper.WaitForAnyOrDefault(() => Query(query), new AppResult[0], _gestureWaitTimeout);
        }

        public void WaitForNoneAnimatingOrElapsed(TimeSpan? timeout = null)
        {
            try
            {
                _waitForHelper.WaitForStableResultOrElapsed(() => Query(new AppQuery(QueryPlatform.Android)), elapsedTimeout: timeout);
            }
            catch (Exception ex)
            {
                Log.Debug("Waiting for animations failed.", ex);
            }
        }

        public AppWebResult[] Query(AppWebQuery query)
        {
            return Query<AppWebResult>(query, null, "query");
        }

        public AppWebResult[] QueryGestureWait(AppWebQuery query)
        {
            return _waitForHelper.WaitForAnyOrDefault(() => Query(query), new AppWebResult[0], _gestureWaitTimeout);
        }

        public T[] QueryGestureWait<T>(AppQuery query, params object[] args)
        {
            return _waitForHelper.WaitForAnyOrDefault(() => Query<T>(query, args, "query"), new T[0], _gestureWaitTimeout);
        }

        T[] Query<T>(ITokenContainer query, object[] args, string methodName)
        {
            string queryString = query.ToString();

            args = args ?? new object[0];

            var arguments = new
            {
                query = queryString,
                operation = new { method_name = methodName, arguments = args }
            };

            var result = _httpClient.Post("/map", arguments);
            var responseJObject = JObject.Parse(result.Contents);

            var outcome = (string)responseJObject["outcome"];

            if (!string.Equals("SUCCESS", outcome, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception(string.Format("Query for {0} failed with outcome: {1}{2}{3}", ToCodeString(query), outcome, Environment.NewLine, result));
            }

            var jArray = (JArray)responseJObject["results"];
            var results = jArray.Select(x => x.ToString()).ToArray();

            if (typeof(T) == typeof(string))
            {
                return results as T[];
            }

            return results
                    .Select(x => new JsonTranslator().Deserialize<T>(x))
                    .ToArray();
        }

        public T[] Query<T>(AppTypedSelector<T> selector)
        {
            var arguments = new
            {
                query = selector.ToString(),
                operation = new { method_name = "query", arguments = ((IAppTypedSelector)selector).QueryParams }
            };

            var result = _httpClient.Post("/map", arguments);
            var responseJObject = JObject.Parse(result.Contents);

            var outcome = (string)responseJObject["outcome"];

            if (!string.Equals("SUCCESS", outcome, StringComparison.InvariantCultureIgnoreCase))
            {
                var reason = (string)responseJObject["reason"];
                bool jsonSerialisationError = (reason != null && reason.Contains("Could not convert result to json"));

                // Ignore json serialisation errors, unless the user specifically requested the value.
                if (!((IAppTypedSelector)selector).ExplicitlyRequestedValue && jsonSerialisationError)
                {
                    Log.Info(string.Format("Ignoring failed json serialisation of result of Query for {0}. Value was never requested", ToCodeString(selector)));
                    return null;
                }
                throw new Exception(string.Format("Query for {0} failed with outcome: {1}{2}{3}", ToCodeString(selector), outcome, Environment.NewLine, result));
            }

            var jArray = (JArray)responseJObject["results"];
            return new JsonTranslator().DeserializeArray<T>(jArray);
        }

        public void DismissKeyboard()
        {
            var arguments = new { command = "hide_soft_keyboard" };
            _httpClient.Post("/", arguments);
        }

        public void KeyboardEnterText(string text)
        {
            var arguments = new { command = "keyboard_enter_text", arguments = new[] { text } };
            _httpClient.Post("/", arguments);
        }

        public void SetTextWebView(string cssSelector, string text)
        {
            var arguments = new { command = "enter_text_by_selector", arguments = new[] { cssSelector, text } };
            _httpClient.Post("/", arguments);
        }

        public void TapCoordinates(float x, float y)
        {
            var arguments = new { command = "touch_coordinate", arguments = new[] { x, y } };
            _httpClient.Post("/", arguments);
        }

        public void DoubleTapCoordinates(float x, float y)
        {
            var arguments = new { command = "double_tap_coordinate", arguments = new[] { x, y } };
            _httpClient.Post("/", arguments);
        }

        public enum PinchDirection
        {
            Open,
            Close
        };

        public void PinchToZoomCoordinates(float x, float y, PinchDirection direction, TimeSpan? duration)
        {
            var fingerSize = 40.0f;

            var pxResult = PerformAction("dp_to_device_pixel", new[] { "40.0" }).Contents;

            var pxObject = (Dictionary<string, object>)JsonConvert.DeserializeObject(pxResult, typeof(Dictionary<string, object>));

            float pinchDistance;
            if (!float.TryParse((string)pxObject["message"], NumberStyles.Any, CultureInfo.InvariantCulture, out pinchDistance))
            {
                throw new Exception(string.Format("Failed to parse {0}", pxObject["message"]));
            }

            if (pinchDistance == 0)
            {
                pinchDistance = 40.0f;
            }
            else
            {
                fingerSize = pinchDistance;
            }

            var screen = QueryGestureWait(new AppQuery(QueryPlatform.Android).Id("content")).First().Rect;

            x = Math.Max(screen.X + fingerSize, Math.Min(screen.X + screen.Width - fingerSize, x));
            y = Math.Max(screen.Y + fingerSize, Math.Min(screen.Y + screen.Height - fingerSize, y));

            var totalSeconds = duration.GetValueOrDefault(TimeSpan.FromMilliseconds(500)).TotalSeconds;

            var finger1A = new Point(x - fingerSize, y - fingerSize);
            var finger2A = new Point(x + fingerSize, y + fingerSize);

            var finger1Distance = Math.Min(pinchDistance,
                Math.Min(finger1A.X - screen.X, finger1A.Y - screen.Y));
            var finger2Distance = Math.Min(pinchDistance,
                Math.Min((screen.X + screen.Width) - finger2A.X, (screen.Y + screen.Height) - finger2A.Y));

            if (finger1Distance < pinchDistance)
            {
                finger2Distance += (pinchDistance - finger1Distance);
            }

            if (finger2Distance < pinchDistance)
            {
                finger1Distance += (pinchDistance - finger2Distance);
            }

            var finger1B = new Point(finger1A.X - finger1Distance, finger1A.Y - finger1Distance);
            var finger2B = new Point(finger2A.X + finger2Distance, finger2A.Y + finger2Distance);

            if (direction == PinchDirection.Open)
            {
                Pinch(finger1A, finger2A, finger1B, finger2B, totalSeconds);
            }
            else
            {
                Pinch(finger1B, finger2B, finger1A, finger2A, totalSeconds);
            }
        }

        private void Pinch(Point finger1Start, Point finger2Start, Point finger1End, Point finger2End, double time)
        {
            var zoomGesture = new MultiTouchGesture(
                new Gesture[] {
                    new Gesture(new Touch[]
                        {
                            new Touch(0.2, time, (int)finger1Start.X, (int)finger1Start.Y),
                            new Touch(0.2, 0, (int)finger1End.X, (int)finger1End.Y)
                        }),
                    new Gesture(new Touch[]
                        {
                            new Touch(0.2, time, (int)finger2Start.X, (int)finger2Start.Y),
                            new Touch(0.2, 0, (int)finger2End.X, (int)finger2End.Y)
                        })
            });
            _httpClient.Post("/gesture", zoomGesture);
        }

        public void SwipeCoordinates(int fromX, int toX, int fromY, int toY, bool withInertia, TimeSpan duration)
        {
            var swipeGesture = new AndroidGestures.MultiTouchGesture(
                    new AndroidGestures.Gesture[]
                    {
                    new AndroidGestures.Gesture(new AndroidGestures.Touch[]
                        {
                            new AndroidGestures.Touch(0, duration.TotalSeconds, fromX, fromY),
                            new AndroidGestures.Touch(withInertia ? 0 : 0.2, 0, toX, toY)
                        })
                    });

            _httpClient.Post("/gesture", swipeGesture);
        }

        HttpResult InvokeAction(string action, object[] arguments)
        {
            // InvokeAction("foo", null) does not place argument in an array, but use null directly
            var actionArgs = arguments ?? new object[] {
                null
            };
            var actionPostArgs = new
            {
                command = action,
                arguments = actionArgs
            };
            return _httpClient.Post("/", actionPostArgs);
        }

        static void FailIfNotSuccess(string action, JObject responseJObject, params object[] actionArgs)
        {
            var success = (Boolean)responseJObject["success"];
            if (!success)
            {
                throw new Exception(string.Format("Action {0} with arguments {1} failed with the following message {2}", action, actionArgs, responseJObject["message"]));
            }
        }

        public HttpResult PerformAction(string action, params object[] arguments)
        {
            var result = InvokeAction(action, arguments);
            var responseJObject = JObject.Parse(result.Contents);
            FailIfNotSuccess(action, responseJObject, arguments);
            return result;
        }

        T PerformAction<T>(string action, params object[] arguments)
        {
            var result = InvokeAction(action, arguments);
            var responseJObject = JObject.Parse(result.Contents);
            FailIfNotSuccess(action, responseJObject);
            var messageWithJson = (string)responseJObject["message"];

            return new JsonTranslator().Deserialize<T>(messageWithJson);
        }

        public ViewConfiguration ViewConfiguration(AppQuery query)
        {
            return PerformAction<ViewConfiguration>("view_configuration", query.ToString());
        }

        public void Pan(AppQuery query, ScrollDirection direction, double swipePercentage, int swipeSpeed,
            bool withInertia)
        {
            query = query ?? new AppQuery(QueryPlatform.Android);

            var results = QueryGestureWait(query.Index(0));

            if (results.Length == 0)
            {
                throw new Exception(string.Format("Unable to scroll, no element were found by query: {0}", ToCodeString(query)));
            }

            var rect = results.Single().Rect;
            var indentPercentage = (float)((1.0 - swipePercentage) / 2);

            if (direction == ScrollDirection.Down || direction == ScrollDirection.Up)
            {
                var indent = rect.Height * indentPercentage;
                if (direction == ScrollDirection.Down)
                {
                    DragViaGesture(rect.CenterX, rect.Y + rect.Height - indent, rect.CenterX, rect.Y + indent,
                        swipeSpeed, withInertia);
                }
                else
                {
                    DragViaGesture(rect.CenterX, rect.Y + indent, rect.CenterX, rect.Y + rect.Height - indent,
                        swipeSpeed, withInertia);
                }
            }
            else
            {
                var indent = rect.Width * indentPercentage;
                if (direction == ScrollDirection.Left)
                {
                    DragViaGesture(rect.X + indent, rect.CenterY, rect.X + rect.Width - indent, rect.CenterY,
                        swipeSpeed, withInertia);
                }
                else
                {
                    DragViaGesture(rect.X + rect.Width - indent, rect.CenterY, rect.X + indent, rect.CenterY,
                        swipeSpeed, withInertia);
                }
            }
        }

        public void DragViaGesture(float fromX, float fromY, float toX, float toY, int swipeSpeed, bool withInertia)
        {
            var startX = (int)Math.Round(fromX);
            var startY = (int)Math.Round(fromY);
            var endX = (int)Math.Round(toX);
            var endY = (int)Math.Round(toY);

            var duration = _sharedApp.CalculateDurationForSwipe(startX, startY, endX, endY, swipeSpeed);

            SwipeCoordinates(startX, endX, startY, endY, withInertia, TimeSpan.FromMilliseconds(duration));
        }

        public void ClearText()
        {

            var setSelectionArguments = new { command = "set_selection", arguments = new[] { -1, -1 } };
            _httpClient.Post("/", setSelectionArguments);

            Thread.Sleep(500);

            var deleteSurroundingTextArguments = new { command = "delete_surrounding_text", arguments = new[] { -1, 0 } };
            _httpClient.Post("/", deleteSurroundingTextArguments);
        }

        public TimeSpan GetScrollTimeout(TimeSpan? timeout)
        {
            return timeout ?? _gestureCompletionTimeout;
        }

        public void Scroll(AppQuery withinQuery, ScrollDirection direction, ScrollStrategy strategy,
            double swipePercentage, int swipeSpeed, bool withInertia = true)
        {
            IScrollInteraction interaction = _androidScroll.GetScrollableInteraction(
                withinQuery, strategy, direction, swipePercentage, swipeSpeed, withInertia, GetScrollTimeout(null));

            interaction.Scroll(direction);
        }

        public void ScrollToEnd(AppQuery withinQuery, ScrollDirection direction, ScrollStrategy strategy,
            double swipePercentage, int swipeSpeed, TimeSpan? timeout)
        {
            IScrollInteraction view = _androidScroll.GetScrollableInteraction(
                withinQuery, strategy, direction, swipePercentage, swipeSpeed, true, GetScrollTimeout(timeout));

            view.ScrollToEnd();
        }

        public void ScrollToStart(AppQuery withinQuery, ScrollDirection direction, ScrollStrategy strategy,
            double swipePercentage, int swipeSpeed, TimeSpan? timeout)
        {
            IScrollInteraction view = _androidScroll.GetScrollableInteraction(
                withinQuery, strategy, direction, swipePercentage, swipeSpeed, true, GetScrollTimeout(timeout));

            view.ScrollToStart();
        }

        public void ScrollTo(AppQuery to, AppQuery within, ScrollDirection scrollDirection, ScrollStrategy strategy,
            double swipePercentage, int swipeSpeed, bool withInertia = true, TimeSpan? timeout = null)
        {
            var actualTimeout = GetScrollTimeout(timeout);

            IScrollInteraction interaction = _androidScroll.GetScrollableInteraction(
                within, strategy, scrollDirection, swipePercentage, swipeSpeed, withInertia, actualTimeout, to);

            ScrollToInternal(() => Query(to).Any(), interaction, scrollDirection, actualTimeout);
        }

        public void ScrollTo(AppWebQuery to, AppQuery within, ScrollDirection scrollDirection,
            ScrollStrategy strategy, double swipePercentage, int swipeSpeed, bool withInertia = true,
            TimeSpan? timeout = null)
        {
            var actualTimeout = GetScrollTimeout(timeout);

            IScrollInteraction interaction = _androidScroll.GetScrollableInteraction(
                within, strategy, scrollDirection, swipePercentage, swipeSpeed, withInertia, actualTimeout, to);

            ScrollToInternal(() => Query(to).Any(), interaction, scrollDirection, actualTimeout);
        }

        void ScrollToInternal(Func<bool> toFound, IScrollInteraction interaction, ScrollDirection scrollDirection, TimeSpan timeout)
        {
            var maxWaitUtc = DateTime.UtcNow + timeout;

            bool found = toFound();

            while (!found)
            {
                var stop = !interaction.Scroll(scrollDirection);
                found = toFound();

                if (found)
                {
                    break;
                }

                if (stop)
                {
                    throw new Exception(string.Format("Unable to scroll view found by {0} any further", ToCodeString(interaction.Query())));
                }
                if (DateTime.UtcNow > maxWaitUtc)
                {
                    throw new Exception("Timeout before element was found");
                }
            }
        }

        public void ScrollTo(AppQuery toQuery, AppQuery within, ScrollStrategy strategy, double swipePercentage,
            int swipeSpeed, bool withInertia = true, TimeSpan? timeout = null)
        {
            _androidScroll.ScrollTo(toQuery, within, strategy, swipePercentage, swipeSpeed, withInertia,
                GetScrollTimeout(timeout));
        }

        public void ScrollTo(AppWebQuery toQuery, ScrollStrategy strategy, double swipePercentage, int swipeSpeed,
            bool withInertia = true, TimeSpan? timeout = null)
        {
            _androidScroll.ScrollTo(toQuery, strategy, swipePercentage, swipeSpeed, withInertia,
                GetScrollTimeout(timeout));
        }

        public TreeElement[] Dump()
        {
            var result = _httpClient.Get("dump");
            var dumpElement = JsonConvert.DeserializeObject<DumpElement>(result.Contents, new JsonSerializerSettings { MaxDepth = null });
            var treeElement = dumpElement.ToTreeElement(true);

            if (treeElement == null)
            {
                return Array.Empty<TreeElement>();
            }

            return new[] { treeElement };
        }

        public TreeElement[] DumpWithDeviceAgent()
        {
            throw new NotImplementedException("DumpWithDeviceAgent method only implemented in iOS part. Use Dump method instead for Android.");
        }

        public void PressKey(int keycode)
        {
            var arguments = new { command = "press_key", arguments = new[] { keycode } };
            _httpClient.Post("/", arguments);
        }

        public void PressKey(string keycode)
        {
            var arguments = new { command = "press_key", arguments = new[] { keycode } };
            var result = _httpClient.Post("/", arguments);

            var resultContentObject = new
            {
                Message = string.Empty,
                Success = false
            };

            var resultContent = JsonConvert.DeserializeAnonymousType(result.Contents, resultContentObject);

            if (resultContent == null)
            {
                throw new Exception($"Unexpected content from Test Server: {result.Contents}");
            }

            if (resultContent.Success == false)
            {
                var message = $"{nameof(PressKey)} for keycode:'{keycode}' was unsucessful: {resultContent.Message}";
                throw new Exception(message);
            }
        }

        public void PressUserAction(string action)
        {
            var arguments = new { command = "press_user_action_button", arguments = action == null ? new string[] { } : new[] { action } };

            _httpClient.Post("/", arguments);
        }

        public void DragCoordinates(float fromX, float fromY, float toX, float toY)
        {
            var arguments = new { command = "drag_coordinates", arguments = new[] { fromX, fromY, toX, toY } };
            _httpClient.Post("/", arguments);
        }

        public void DragPercentages(float fromX, float fromY, float toX, float toY, int stepCount)
        {
            var arguments = new { command = "drag", arguments = new object[] { fromX, fromY, toX, toY, stepCount } };
            _httpClient.Post("/", arguments);
        }

        public void TouchAndHoldCoordinates(float x, float y)
        {
            var arguments = new { command = "long_press_coordinate", arguments = new[] { x, y } };
            _httpClient.Post("/", arguments);
        }

        public void SetOrientationPortrait()
        {
            var arguments = new { command = "set_activity_orientation", arguments = new[] { "portrait" } };
            _httpClient.Post("/", arguments);
        }

        public void SetOrientationLandscape()
        {
            var arguments = new { command = "set_activity_orientation", arguments = new[] { "landscape" } };
            _httpClient.Post("/", arguments);
        }

        public string ToCodeString(ITokenContainer container)
        {
            return TokenCodePrinter.ToCodeString(container);
        }

        public string Invoke(string methodName, object[] arguments = null)
        {
            arguments = arguments ?? new object[] { };

            var data = new { method_name = methodName, arguments = arguments };
            var httpResult = _httpClient.Post("/backdoor", data);

            var responseJObject = JObject.Parse(httpResult.Contents);

            var outcome = (string)responseJObject["outcome"];
            var result = (string)responseJObject["result"];

            if (!string.Equals("SUCCESS", outcome, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception(string.Format("Invoke for {0} failed with outcome: {1}{2}{3}", methodName, outcome, Environment.NewLine, result));
            }

            return result;
        }

        public void SetGpsCoordinates(double latitude, double longitude)
        {
            PerformAction("set_gps_coordinates", latitude, longitude);
        }

        public string[] InvokeJS(IInvokeJSAppQuery selector)
        {
            QueryGestureWait(selector.AppQuery);

            var arguments = new
            {
                query = selector.AppQuery.ToString(),
                operation = new { method_name = "execute-javascript" },
                javascript = selector.Javascript
            };

            var result = _httpClient.Post("/map", arguments);
            var responseJObject = JObject.Parse(result.Contents);

            var outcome = (string)responseJObject["outcome"];

            if (!string.Equals("SUCCESS", outcome, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception(string.Format("InvokeJS for {0} failed with outcome: {1}{2}{3}", ToCodeString(selector), outcome, Environment.NewLine, result));
            }

            var jArray = (JArray)responseJObject["results"];
            return jArray.Select(x => x.ToString()).ToArray();
        }

        class Touch
        {
            public readonly string query_string;
            public readonly int offset_x;
            public readonly int offset_y;
            public readonly double wait;
            public readonly double time;
            public readonly int x;
            public readonly int y;
            public readonly bool release;

            public Touch(Func<AppQuery, AppQuery> query, double wait = 0.2, double time = 1, int x = 50, int y = 50, int offsetX = 0, int offsetY = 0, bool release = false)
            {
                this.query_string = query == null ? null : query(new AppQuery(QueryPlatform.Android)).ToString();
                this.time = time;
                this.x = x;
                this.y = y;
                this.wait = wait;
                offset_x = offsetX;
                offset_y = offsetY;
                this.release = release;
            }

            public Touch(double wait, double time, int x, int y)
            {
                this.query_string = null;
                this.time = time;
                this.x = 0;
                this.y = 0;
                this.wait = wait;
                this.offset_x = x;
                this.offset_y = y;
                this.release = false;
            }
        }

        class Gesture
        {
            public readonly AndroidGestures.Touch[] touches;

            public Gesture(Touch[] touches)
            {
                this.touches = touches;
            }
        }

        class MultiTouchGesture
        {
            public readonly AndroidGestures.Gesture[] gestures;
            public readonly double query_timeout;

            public MultiTouchGesture(Gesture[] gestures, double queryTimeoutInSec = 5.0)
            {
                this.gestures = gestures;
                query_timeout = queryTimeoutInSec;
            }
        }

        public enum SwipeDirection { Right, Left };

    }

    internal class Point
    {
        internal float X { get; set; }
        internal float Y { get; set; }

        internal Point(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }
    }
}