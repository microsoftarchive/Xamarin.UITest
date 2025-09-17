using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.UITest.Queries;
using Xamarin.UITest.Queries.PlatformSpecific;
using Xamarin.UITest.Shared;
using Xamarin.UITest.Shared.Http;
using Xamarin.UITest.Shared.Json;
using Xamarin.UITest.Shared.Queries;
using Xamarin.UITest.Shared.Extensions;
using Xamarin.UITest.Utils;
using Xamarin.UITest.Shared.Logging;
using Xamarin.UITest.XDB;
using Xamarin.UITest.XDB.Services;
using Xamarin.UITest.XDB.Enums;
using Xamarin.UITest.XDB.Entities;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.UITest.XDB.Services.OSX.IDB;
using Xamarin.UITest.XDB.Services.OSX;

namespace Xamarin.UITest.iOS
{
    internal class iOSGestures : IGestures, ILocationSimulation
    {
        readonly ICalabashConnection _connection;
        readonly bool _useXDB;
        readonly string _deviceAddress;
        readonly string _deviceIdentifier;
        readonly iOSCalabashDevice _device;
        readonly WaitForHelper _waitForHelper;
        readonly TimeSpan _gestureWaitTimeout;
        readonly TimeSpan _gestureCompletionTimeout;
        readonly iOSRectTransformer _rectTransformer;
        readonly SharedApp _sharedApp;

        IiOSDeviceAgentService _deviceAgent;
        IiOSDeviceAgentService _DeviceAgent
        {
            get
            {
                return _deviceAgent = _deviceAgent ?? XdbServices.GetRequiredService<IiOSDeviceAgentService>();
            }
        }

        public iOSGestures(
            DeviceConnectionInfo deviceConnectionInfo,
            iOSCalabashDevice device,
            WaitForHelper waitForHelper,
            IWaitTimes waitTimes)
        {
            _connection = deviceConnectionInfo.Connection;
            _deviceAddress = deviceConnectionInfo.DeviceAddress;
            _deviceIdentifier = deviceConnectionInfo.DeviceIdentifier;
            _useXDB = deviceConnectionInfo.UseXDB;
            _device = device;
            _waitForHelper = waitForHelper;
            _gestureWaitTimeout = waitTimes.GestureWaitTimeout;
            _gestureCompletionTimeout = waitTimes.GestureCompletionTimeout;
            _rectTransformer = new iOSRectTransformer(device.GetScreenSize(), device.iOSVersion);
            _sharedApp = new SharedApp(QueryPlatform.iOS, this);
        }

        public AppResult[] Query(AppQuery query)
        {
            var result = InternalQuery<iOSResult>(query, null, "query");

            ScrollDirection direction = new ScrollDirection();
            switch (result.HomeButtonOrientation)
            {
                case "right":
                    direction = ScrollDirection.Right;
                    break;
                case "left":
                    direction = ScrollDirection.Left;
                    break;
                case "up":
                    direction = ScrollDirection.Up;
                    break;
                case "down":
                    direction = ScrollDirection.Down;
                    break;
                default:
                    throw new Exception("Unknown home button direction: " + result.HomeButtonOrientation);
            }

            return result.Results
                .Select(x => _rectTransformer.TransformRect(x, direction))
                .Select(x => new AppResult(x))
                .ToArray();
        }

        public ScrollDirection GetHomeButtonOrientation()
        {
            Func<AppQuery, AppQuery> windowQuery = e => e.All().Index(0);
            var result = InternalQuery<iOSResult>(_sharedApp.Expand(windowQuery), null, "query");

            ScrollDirection direction = new ScrollDirection();
            switch (result.HomeButtonOrientation)
            {
                case "right":
                    direction = ScrollDirection.Right;
                    break;
                case "left":
                    direction = ScrollDirection.Left;
                    break;
                case "up":
                    direction = ScrollDirection.Up;
                    break;
                case "down":
                    direction = ScrollDirection.Down;
                    break;
                default:
                    throw new Exception("Unknown home button direction: " + result.HomeButtonOrientation);
            }

            return direction;
        }

        public TreeElement[] Dump()
        {
            var result = _connection.Dump();

            var dumpElement = JsonConvert.DeserializeObject<DumpElement>(result.Contents, new JsonSerializerSettings { MaxDepth = null });

            var treeElement = dumpElement.ToTreeElement(true);

            if (treeElement == null)
            {
                Array.Empty<TreeElement>();
            }

            return new[] { treeElement };
        }

        /// <summary>
        /// Gets Application's main <see cref="UIElement"/>.
        /// Uses DeviceAgent for that.
        /// </summary>
        /// <returns></returns>
        public UIElement GetApplicationUIElement()
        {
            WaitForNoneAnimatingOrElapsed();

            UIElement topUIElement = null;
            int maxAttempts = 5;
            for (int retry = 0; retry < maxAttempts; retry++)
            {
                // Sometimes right after user interface interaction DeviceAgent is not able to get elements hierarchy and returns null.
                topUIElement = _DeviceAgent.DumpElements(_deviceAddress).GetAwaiter().GetResult();
                if (topUIElement != null) break;
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }

            return topUIElement;
        }

        /// <summary>
        /// Dumps all elements using DeviceAgent's tree request.
        /// Comparing to calabash-ios-server dump request this one prints all elements which are present on the screen.
        /// It let us print system elements such as alerts, system keyboards, system date pickers and so on.
        /// </summary>
        /// <returns><see cref="TreeElement"/> array with all elements which are present on screen.</returns>
        public TreeElement[] DumpWithDeviceAgent()
        {
            TreeElement treeElement = GetApplicationUIElement().ToTreeElement();

            if (treeElement == null)
            {
                Array.Empty<TreeElement>();
            }
            return new[] { treeElement };
        }

        TreeElement BuildTree(AppResult element, Dictionary<string, AppResult[]> childrenDictionary)
        {
            var children = childrenDictionary[element.Description]
                .Select(x => BuildTree(x, childrenDictionary))
                .ToArray();

            return new TreeElement(element.Id, element.Label, element.Text, element.Class, children, true);
        }

        public AppResult[] Flash(AppQuery query)
        {
            var result = InternalQuery<string>(query, null, "flash");

            if (!result.Results.Any())
            {
                return new AppResult[0];
            }

            ScrollDirection direction = new ScrollDirection();
            switch (result.HomeButtonOrientation)
            {
                case "right":
                    direction = ScrollDirection.Right;
                    break;
                case "left":
                    direction = ScrollDirection.Left;
                    break;
                case "up":
                    direction = ScrollDirection.Up;
                    break;
                case "down":
                    direction = ScrollDirection.Down;
                    break;
                default:
                    throw new Exception("Unknown home button direction: " + result.HomeButtonOrientation);
            }

            return result.Results
                .Select(x => new JsonTranslator().Deserialize<iOSResult>(x))
                .Select(x => _rectTransformer.TransformRect(x, direction))
                .Select(x => new AppResult(x))
                .ToArray();
        }

        public AppResult[] QueryGestureWait(AppQuery query)
        {
            WaitForNoneAnimatingOrElapsed();
            return _waitForHelper.WaitForAnyOrDefault(() => Query(query), new AppResult[0], _gestureWaitTimeout);
        }

        public AppWebResult[] QueryGestureWait(AppWebQuery query)
        {
            WaitForNoneAnimatingOrElapsed();
            return _waitForHelper.WaitForAnyOrDefault(() => Query(query), new AppWebResult[0], _gestureWaitTimeout);
        }

        public AppWebResult[] Query(AppWebQuery query)
        {
            var result = InternalQuery<AppWebResult>(query, null, "query");
            return result.Results;
        }

        public T[] Query<T>(AppQuery query, object[] args)
        {
            return InternalQuery<T>(query, args, "query").Results;
        }

        public T[] Query<T>(AppTypedSelector<T> typedQuery)
        {
            return Query<T>((IAppTypedSelector)typedQuery);
        }

        InternalResult<T> InternalQuery<T>(AppQuery query, object[] args, string methodName)
        {
            if (_useXDB)
            {
                _DeviceAgent.DismissSpringboardAlertsAsync(_deviceAddress).Wait();
            }

            string queryString = query.ToString();

            args = args ?? new object[0];

            var arguments = new
            {
                query = queryString,
                operation = new { method_name = methodName, arguments = args }
            };

            var result = _connection.Map(arguments);
            var responseJObject = JObject.Parse(result.Contents);

            if (!IsSuccessfulCalabashResult(result))
            {
                throw new Exception(string.Format("Query for '{0}' failed with output: {1}{2}", queryString, Environment.NewLine, result.Contents));
            }

            var jArray = (JArray)responseJObject["results"];
            var results = jArray.Select(x => x.ToString()).ToArray();

            //there is a logical error in calabash-ios-server: status_bar_orientation returns the device's home button position
            var orientationValue = (JValue)responseJObject["status_bar_orientation"];
            var orientation = orientationValue.Value<string>();

            if (typeof(T) == typeof(string))
            {
                return new InternalResult<T>(results as T[], orientation);
            }

            return new InternalResult<T>(results
                .Select(x => new JsonTranslator().Deserialize<T>(x))
                .ToArray(), orientation);
        }

        T[] Query<T>(IAppTypedSelector typedSelector)
        {
            string queryString = typedSelector.ToString();

            var arguments = new
            {
                query = queryString,
                operation = new { method_name = "query", arguments = typedSelector.QueryParams }
            };

            var result = _connection.Map(arguments);
            var responseJObject = JObject.Parse(result.Contents);

            if (!IsSuccessfulCalabashResult(result))
            {
                throw new Exception(string.Format("Query for '{0}' failed with output: {1}{2}", queryString, Environment.NewLine, result.Contents));
            }

            var jArray = (JArray)responseJObject["results"];

            // Booleans are returned as "0" and "1" on IOS
            if (typeof(T) == typeof(bool))
            {
                return new JsonTranslator().DeserializeArray<string>(jArray).Select(b => (b != "0")).ToArray() as T[];
            }

            return new JsonTranslator().DeserializeArray<T>(jArray);
        }


        InternalResult<T> InternalQuery<T>(AppWebQuery query, object[] args, string methodName)
        {
            string queryString = query.ToString();

            args = args ?? new object[0];

            var arguments = new
            {
                query = queryString,
                operation = new { method_name = methodName, arguments = args }
            };

            var result = _connection.Map(arguments);
            var responseJObject = JObject.Parse(result.Contents);

            if (!IsSuccessfulCalabashResult(result))
            {
                throw new Exception(string.Format("Query for '{0}' failed with output: {1}{2}", queryString, Environment.NewLine, result.Contents));
            }

            var jArray = (JArray)responseJObject["results"];
            var results = jArray.Select(x => x.ToString()).ToArray();

            //there is a logical error in calabash-ios-server: status_bar_orientation returns the device's home button position
            var orientationValue = (JValue)responseJObject["status_bar_orientation"];
            var orientation = orientationValue.Value<string>();

            if (typeof(T) == typeof(string))
            {
                return new InternalResult<T>(results as T[], orientation);
            }

            return new InternalResult<T>(results
                .Select(x => new JsonTranslator().Deserialize<T>(x))
                .ToArray(), orientation);
        }

        public void WaitForNoneAnimatingOrElapsed(TimeSpan? timeout = null)
        {
            timeout = timeout ?? TimeSpan.FromSeconds(3);

            _waitForHelper.WaitForOrElapsed(() =>
            {
                var responseTimeout = (int)(timeout.Value.Seconds / 2);
                var result = _connection.Condition(new { condition = "NONE_ANIMATING", query = "view", timeout = responseTimeout });

                var responseJObject = JObject.Parse(result.Contents);
                var outcome = (string)responseJObject["outcome"];

                return string.Equals("SUCCESS", outcome, StringComparison.InvariantCultureIgnoreCase);
            }, timeout: timeout);
        }

        public void DismissKeyboard()
        {
            //Find the first responder and tell it to resign
            Query(new AppQuery(QueryPlatform.iOS).Property("isFirstResponder", true).Invoke("resignFirstResponder"));
        }

        public void SetInputViewPickerWheelValue(int pickerIndex, int wheelIndex, string value)
        {
            _DeviceAgent.SetInputViewPickerWheelValueAsync(_deviceAddress, pickerIndex, wheelIndex, value).Wait();
        }

        public List<UIElement> GetPickers(string typeOfPickers)
        {
            return _sharedApp.ErrorReporting.With(() =>
            {
                string pickerInfo = InvokeDeviceAgentQuery(new { type = typeOfPickers }).GetAwaiter().GetResult().ToString();

                Dictionary<string, List<UIElement>> pickerProperties = JsonConvert.DeserializeObject<Dictionary<string, List<UIElement>>>(pickerInfo);
                return pickerProperties["result"];
            });
        }

        public void EnterText(SingleQuoteEscapedString text)
        {
            if (_useXDB)
            {
                _DeviceAgent.EnterTextAsync(_deviceAddress, text.UnescapedString).Wait();
            }
            else
            {
                var fixBackslashesforUIA = text.UnescapedString.EscapeBackslashes().EscapeSingleQuotes().ToString();
                var command = string.Format(CultureInfo.InvariantCulture, "uia.typeString('{0}')", fixBackslashesforUIA);
                var result = _connection.UIA(command);
                ValidateSuccessfulCalabashResult(result);
            }
        }

        public void ClearText(AppWebQuery query)
        {
            string queryString = query.ToString();

            var arguments = new
            {
                query = queryString,
                operation = new { method_name = "setText", arguments = new[] { "" } }
            };

            var result = _connection.Map(arguments);
            IsSuccessfulCalabashResult(result);
        }

        public void PressEnter()
        {
            if (_useXDB)
            {
                _DeviceAgent.EnterTextAsync(_deviceAddress, "\n").Wait();
            }
            else
            {
                var command = "uia.enter()";
                var result = _connection.UIA(command);
                ValidateSuccessfulCalabashResult(result);
            }
        }

        public bool IsKeyboardVisible()
        {
            if (_device.iOSVersion.Major < 16)
            {
                return Query(new AppQuery(QueryPlatform.iOS).Class("UIKBKeyplaneView")).Any();
            }
            var keyboard = new
            {
                type = "Keyboard"
            };

            return _sharedApp.ErrorReporting.With(() =>
            {

                //the output should contain {"result":[] } if there is no keyboard. Otherwise "result" will not be empty
                var keyboardInfo = InvokeDeviceAgentQuery(keyboard).GetAwaiter().GetResult().ToString();

                var keyboardProperties = JsonConvert.DeserializeObject<Dictionary<string, List<UIElement>>>(keyboardInfo);

                return keyboardProperties["result"].Any();
            }, new[] { keyboard });
        }

        public void TapCoordinates(float x, float y)
        {
            if (_useXDB)
            {
                _DeviceAgent.TouchAsync(_deviceAddress, new PointF(x, y)).Wait();
            }
            else
            {
                var command = string.Format(CultureInfo.InvariantCulture, "uia.tapOffset('{{:x {0} :y {1}}}')", x, y);
                var result = _connection.UIA(command);
                ValidateSuccessfulCalabashResult(result);
            }
        }

        public object InvokeUia(string script)
        {
            if (_useXDB)
            {
                throw new Exception("InvokeUia is not supported when using DeviceAgent");
            }
            else
            {
                var result = _connection.UIA(script);

                var responseJObject = JObject.Parse(result.Contents);

                if (!IsSuccessfulCalabashResult(result))
                {
                    if (responseJObject["results"] != null && responseJObject["results"][0] != null && responseJObject["results"][0]["value"] != null)
                    {
                        var error = responseJObject["results"][0]["value"].ToString();
                        throw new Exception(string.Format("UIA failed with output: {0}{1}", Environment.NewLine, error));
                    }

                    throw new Exception(string.Format("UIA failed with output: {0}{1}", Environment.NewLine, result));
                }

                var jArray = (JArray)responseJObject["results"];

                if (jArray.Count < 1)
                {
                    return null;
                }

                var value = jArray[0]["value"];

                return value == null ? null : value.ToString();
            }
        }

        public Task<object> InvokeDeviceAgentQuery(object query)
        {
            if (!_useXDB)
            {
                throw new Exception("DeviceAgent is not in use");
            }

            return _DeviceAgent.QueryAsync(_deviceAddress, query);
        }

        public Task InvokeDeviceAgentGesture(string gesture, object options = null, object specifiers = null)
        {
            if (!_useXDB)
            {
                throw new Exception("DeviceAgent is not in use");
            }

            return _DeviceAgent.GestureAsync(_deviceAddress, gesture, options, specifiers);
        }

        public void DismissSpringboardAlerts()
        {
            if (!_useXDB)
            {
                throw new Exception("DeviceAgent is not in use");
            }

            _DeviceAgent.DismissSpringboardAlertsAsync(_deviceAddress).Wait();
        }

        public void SwipeCoordinates(int fromX, int toX, int fromY, int toY, bool withInertia, TimeSpan duration)
        {
            if (_device.iOSVersion.Major < 10 && !withInertia)
            {
                throw new Exception("'withInertia = false' cannot be used prior to iOS10");
            }

            Func<AppQuery, AppQuery> windowQuery = e => e.All().Index(0);
            var window = QueryGestureWait(_sharedApp.Expand(windowQuery)).First();
            float windowWidth = window.Rect.Width;
            float windowHeight = window.Rect.Height;

            /*
             * UIAutomation dragInsideWithOptions() works in percentages, not pixels. 
             */

            float fromXPercent, toXPercent, fromYPercent, toYPercent;
            fromXPercent = fromX / windowWidth;
            toXPercent = toX / windowWidth;
            fromYPercent = fromY / windowHeight;
            toYPercent = toY / windowHeight;

            WaitForNoneAnimatingOrElapsed();
            NotifyLogIfSimulatorSwipeOrScroll();

            if (_useXDB)
            {
                _DeviceAgent.DragAsync(
                    _deviceAddress,
                    new PointF(fromX, fromY),
                    new PointF(toX, toY),
                    duration,
                    null,
                    withInertia).Wait();
            }
            else
            {
                var command = string.Format(
                    "target.dragInsideWithOptions({{startOffset:{{x:{0}, y:{2}}}, endOffset:{{x:{1}, y:{3}}}, duration:{4}}})",
                    fromXPercent,
                    toXPercent,
                    fromYPercent,
                    toYPercent,
                    duration.TotalSeconds);

                var result = _connection.UIA(command);
                ValidateSuccessfulCalabashResult(result);
            }
        }

        void NotifyLogIfSimulatorSwipeOrScroll()
        {
            if (_device.IsSimulator && _device.iOSVersion.Major < 9)
            {
                Log.Info("Swipe / scroll is broken on simulators running iOS v8 or earlier for some views due to an Apple bug. " + Environment.NewLine
                    + "The best workaround is to test using a simulator running iOS v9.0 or later or to test on a physical device." + Environment.NewLine
                    + "If scrolling UIATableView or UIAScrollViews, a better solution is often to use ScrollTo:" + Environment.NewLine
                    + "app.ScrollTo(\"MyControl\");");
            }
        }

        public void ScrollTo(string marked)
        {
            NotifyLogIfSimulatorSwipeOrScroll();

            var scrollViews = Query(new AppQuery(QueryPlatform.iOS).Class("UIScrollView"));

            if (!scrollViews.Any())
            {
                throw new Exception($"Unable to locate any scrollable views");
            }

            for (var i = 0; i < scrollViews.Length; i++)
            {
                var result = _connection.Map(
                    new
                    {
                        query = $"UIScrollView index:{i}",
                        operation = new
                        {
                            method_name = "scrollToMark",
                            arguments = new object[] { marked, false }
                        }
                    });

                if (!IsSuccessfulCalabashResult(result))
                {
                    throw new Exception(
                        $"Scroll to '{marked} failed with output: {Environment.NewLine}{result.Contents}");
                }

                if (((JArray)JObject.Parse(result.Contents)["results"]).Any(r => r != null))
                {
                    return;
                }
            }

            throw new Exception($"Unable to find element Marked(\"{marked}\")");
        }

        public void Scroll(AppQuery withQuery, ScrollDirection direction, ScrollStrategy strategy,
            double swipePercentage, int swipeSpeed, bool withInertia)
        {
            if (strategy == ScrollStrategy.Auto)
            {
                strategy = ScrollStrategy.Programmatically;
            }

            if (strategy == ScrollStrategy.Gesture)
            {
                GestureBasedScroll(withQuery, direction, swipePercentage, swipeSpeed, withInertia);
            }
            else
            {
                ProgrammaticScroll(withQuery, direction);
            }
        }

        void ProgrammaticScroll(AppQuery withQuery, ScrollDirection direction)
        {
            if (withQuery == null)
            {
                withQuery = FindScrollableTarget();
            }

            if (withQuery == null)
            {
                throw new Exception("Unable to determine what view to programmatically scroll, try specifying view in withinQuery");
            }

            var queryString = withQuery.ToString();

            var arguments = new
            {
                query = queryString,
                operation = new { method_name = "scroll", arguments = new[] { direction.ToString().ToLowerInvariant() } }
            };

            var result = _connection.Map(arguments);
            if (!IsSuccessfulCalabashResult(result))
            {
                throw new Exception(string.Format("Scroll within '{0}' failed with output: {1}{2}", queryString, Environment.NewLine, result.Contents));
            }
        }

        void GestureBasedScroll(AppQuery withinQuery, ScrollDirection scrollDirection, double swipePercentage,
            int swipeSpeed, bool withInertia)
        {
            var direction = scrollDirection.Opposite();

            if (withinQuery == null)
            {
                withinQuery = _sharedApp.Expand(e => e.All().Index(0)); //Get window
            }

            ScrollDirection homeButtonOrientation = GetHomeButtonOrientation();
        
            switch (homeButtonOrientation)
            {
                case ScrollDirection.Right:
                    switch (direction)
                    {
                        case ScrollDirection.Right:
                            direction = ScrollDirection.Down;
                            break;
                        case ScrollDirection.Left:
                            direction = ScrollDirection.Up;
                            break;
                        case ScrollDirection.Up:
                            direction = ScrollDirection.Right;
                            break;
                        case ScrollDirection.Down:
                            direction = ScrollDirection.Left;
                            break;
                        default:
                            throw new Exception(string.Format("GestureBasedScroll: Unable to swipe in direction {0}", homeButtonOrientation));
                    }
                    
                    break;
                case ScrollDirection.Left:
                    switch (direction)
                    {
                        case ScrollDirection.Right:
                            direction = ScrollDirection.Up;
                            break;
                        case ScrollDirection.Left:
                            direction = ScrollDirection.Down;
                            break;
                        case ScrollDirection.Up:
                            direction = ScrollDirection.Left;
                            break;
                        case ScrollDirection.Down:
                            direction = ScrollDirection.Right;
                            break;
                        default:
                            throw new Exception(string.Format("GestureBasedScroll: Unable to swipe in direction {0}", homeButtonOrientation));
                    }
                    break;

                case ScrollDirection.Up:
                    switch (direction)
                    {
                        case ScrollDirection.Right:
                            direction = ScrollDirection.Left;
                            break;
                        case ScrollDirection.Left:
                            direction = ScrollDirection.Right;
                            break;
                        case ScrollDirection.Up:
                            direction = ScrollDirection.Down;
                            break;
                        case ScrollDirection.Down:
                            direction = ScrollDirection.Up;
                            break;
                        default:
                            throw new Exception(string.Format("GestureBasedScroll: Unable to swipe in direction {0}", homeButtonOrientation));
                    }
                    break;
                case ScrollDirection.Down:
                    break;
                default:
                    throw new Exception(string.Format("GestureBasedScroll: Unable to swipe in direction {0}", homeButtonOrientation));
            }

            _sharedApp.Swipe(withinQuery, direction, swipePercentage, swipeSpeed, withInertia);
        }

        public TimeSpan GetScrollTimeout(TimeSpan? timeout)
        {
            return timeout ?? _gestureCompletionTimeout;
        }

        public void ScrollTo(AppQuery toQuery, AppQuery withinQuery, ScrollDirection direction,
            ScrollStrategy strategy, double swipePercentage, int swipeSpeed, bool withInertia,
            TimeSpan? timeout = null)
        {
            var actualTimeout = GetScrollTimeout(timeout);

            ScrollToInternal(
                () => Query(toQuery).Any(),
                withinQuery,
                direction,
                strategy,
                swipePercentage,
                swipeSpeed,
                withInertia,
                actualTimeout);
        }

        public void ScrollTo(AppWebQuery toQuery, AppQuery withinQuery, ScrollDirection direction,
            ScrollStrategy strategy, double swipePercentage, int swipeSpeed, bool withInertia,
            TimeSpan? timeout = null)
        {
            var actualTimeout = GetScrollTimeout(timeout);

            ScrollToInternal(
                () => Query(toQuery).Any(),
                withinQuery,
                direction,
                strategy,
                swipePercentage,
                swipeSpeed,
                withInertia,
                actualTimeout);
        }

        void ScrollToInternal(Func<bool> toFound, AppQuery withinQuery, ScrollDirection direction,
            ScrollStrategy strategy, double swipePercentage, int swipeSpeed, bool withInertia,
            TimeSpan timeout)
        {
            var maxWaitUtc = DateTime.UtcNow + timeout;

            bool found = toFound();

            while (!found)
            {
                Scroll(withinQuery, direction, strategy, swipePercentage, swipeSpeed, withInertia);
                found = toFound();

                if (found)
                {
                    break;
                }

                if (DateTime.UtcNow > maxWaitUtc)
                {
                    throw new Exception("Timeout before element was found");
                }
            }
        }

        AppQuery FindScrollableTarget()
        {
            AppQuery matching = null;
            AppQuery[] posibilites =
                {
                    new AppQuery(QueryPlatform.iOS).Class("UIScrollView").Index(0),
                };

            try
            {
                _waitForHelper.WaitFor(() =>
                    {
                        foreach (var posibility in posibilites)
                        {
                            var results = Query(posibility);

                            if (!results.Any())
                            {
                                continue;
                            }
                            matching = posibility;
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


        public void DoubleTapCoordinates(float x, float y)
        {
            if (_useXDB)
            {
                _DeviceAgent.DoubleTouchAsync(_deviceAddress, new PointF(x, y)).Wait();
            }
            else
            {
                var command = string.Format(CultureInfo.InvariantCulture, "uia.doubleTapOffset('{{:x {0} :y {1}}}')", x, y);

                var result = _connection.UIA(command);
                ValidateSuccessfulCalabashResult(result);
            }
        }

        public void TwoFingerTapCoordinates(float x, float y)
        {
            if (_useXDB)
            {
                _DeviceAgent.TwoFingerTouchAsync(_deviceAddress, new PointF(x, y)).Wait();
            }
            else
            {
                var command = string.Format(CultureInfo.InvariantCulture, "uia.twoFingerTapOffset('{{:x {0} :y {1}}}')", x, y);

                var result = _connection.UIA(command);
                ValidateSuccessfulCalabashResult(result);
            }
        }

        public void TouchAndHoldCoordinates(float x, float y, TimeSpan? duration = null)
        {
            duration = duration ?? TimeSpan.FromSeconds(1);

            if (_useXDB)
            {
                _DeviceAgent.TouchAndHoldAsync(_deviceAddress, new PointF(x, y), duration).Wait();
            }
            else
            {
                var command = string.Format(
                    CultureInfo.InvariantCulture,
                    "uia.touchHoldOffset('{2}', '{{:x {0} :y {1}}}')",
                    x,
                    y,
                    duration.Value.TotalSeconds
                );

                var result = _connection.UIA(command);
                ValidateSuccessfulCalabashResult(result);
            }
        }

        public void FlickCoordinates(float fromX, float fromY, float toX, float toY)
        {
            if (_useXDB)
            {
                _DeviceAgent.FlickAsync(_deviceAddress, new PointF(fromX, fromY), new PointF(toY, toY)).Wait();
            }
            else
            {
                var command = string.Format(CultureInfo.InvariantCulture, "uia.flickOffset('{{:x {0} :y {1}}}', '{{:x {2} :y {3}}}')", fromX, fromY, toX, toY);

                var result = _connection.UIA(command);
                ValidateSuccessfulCalabashResult(result);
            }
        }

        public void DragCoordinates(float fromX, float fromY, float toX, float toY, TimeSpan? duration = null, TimeSpan? holdTime = null)
        {
            var actualDuration = duration.GetValueOrDefault(TimeSpan.FromMilliseconds(750));

            if (_useXDB)
            {
                _DeviceAgent.DragAsync(
                    _deviceAddress,
                    new PointF(fromX, fromY),
                    new PointF(toX, toY),
                    actualDuration,
                    holdTime).Wait();
            }
            else
            {
                var totalSeconds = actualDuration.TotalSeconds;

                var command = string.Format(CultureInfo.InvariantCulture, "uia.panOffset('{{:x {0} :y {1}}}', '{{:x {2} :y {3}}}', '{{:duration {4}}}')", fromX, fromY, toX, toY, totalSeconds);

                var result = _connection.UIA(command);
                ValidateSuccessfulCalabashResult(result);
            }
        }

        public void PinchToZoomInCoordinates(float x, float y, TimeSpan? duration = null)
        {
            var totalSeconds = duration.GetValueOrDefault(TimeSpan.FromMilliseconds(500)).TotalSeconds;

            if (_useXDB)
            {
                _DeviceAgent.PinchAsync(_deviceAddress, new PointF(x, y), PinchDirection.In, 18, duration).Wait();
            }
            else
            {
                var command = string.Format(CultureInfo.InvariantCulture, "uia.pinchOffset(':in', '{{:x {0} :y {1}}}', '{{:duration {2}}}')", x, y, totalSeconds);

                var result = _connection.UIA(command);
                ValidateSuccessfulCalabashResult(result);
            }
        }

        public void PinchToZoomOutCoordinates(float x, float y, TimeSpan? duration = null)
        {
            var totalSeconds = duration.GetValueOrDefault(TimeSpan.FromMilliseconds(500)).TotalSeconds;

            if (_useXDB)
            {
                _DeviceAgent.PinchAsync(_deviceAddress, new PointF(x, y), PinchDirection.Out, 12, duration).Wait();
            }
            else
            {
                var command = string.Format(CultureInfo.InvariantCulture, "uia.pinchOffset(':out', '{{:x {0} :y {1}}}', '{{:duration {2}}}')", x, y, totalSeconds);

                var result = _connection.UIA(command);
                ValidateSuccessfulCalabashResult(result);
            }
        }

        public void SetOrientationPortrait()
        {
            if (_useXDB)
            {
                _DeviceAgent.SetOrientationAsync(_deviceAddress, DeviceOrientation.Portrait).Wait();
            }
            else
            {
                var command = "target.setDeviceOrientation(UIA_DEVICE_ORIENTATION_PORTRAIT)";
                var result = _connection.UIA(command);
                ValidateSuccessfulCalabashResult(result);
            }

            WaitForNoneAnimatingOrElapsed();
        }

        public void SetOrientationLandscape()
        {
            if (_useXDB)
            {
                _DeviceAgent.SetOrientationAsync(_deviceAddress, DeviceOrientation.LandscapeLeft).Wait();
            }
            else
            {
                var command = "target.setDeviceOrientation(UIA_DEVICE_ORIENTATION_LANDSCAPELEFT)";

                var result = _connection.UIA(command);
                ValidateSuccessfulCalabashResult(result);
            }

            WaitForNoneAnimatingOrElapsed();
        }

        public void PressVolumeUp()
        {
            if (_useXDB)
            {
                _DeviceAgent.VolumeAsync(_deviceAddress, VolumeDirection.Up).Wait();
            }
            else
            {
                var command = "target.clickVolumeUp()";
                var result = _connection.UIA(command);
                ValidateSuccessfulCalabashResult(result);
            }
        }

        public void PressVolumeDown()
        {
            if (_useXDB)
            {
                _DeviceAgent.VolumeAsync(_deviceAddress, VolumeDirection.Down).Wait();
            }
            else
            {
                var command = "target.clickVolumeDown()";
                var result = _connection.UIA(command);
                ValidateSuccessfulCalabashResult(result);
            }
        }

        public void SetGpsCoordinates(double latitude, double longitude)
        {
            IIDBService idbService = XdbServices.GetRequiredService<IIDBService>();
            idbService.SetLocation(new UDID(UDID: _deviceIdentifier), new LatLong(latitude, longitude));
        }

        void ValidateSuccessfulCalabashResult(HttpResult result)
        {
            if (!IsSuccessfulCalabashResult(result))
            {
                throw new Exception(string.Format("Underlying query failed with output: {0}{1}", Environment.NewLine, result.Contents));
            }
        }

        bool IsSuccessfulCalabashResult(HttpResult result)
        {
            if (result.Contents.IsNullOrWhiteSpace())
            {
                return false;
            }

            var responseJObject = JObject.Parse(result.Contents);

            var outcome = (string)responseJObject["outcome"];

            if (!string.Equals("SUCCESS", outcome, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            var jArray = responseJObject["results"] as JArray;

            if (jArray != null)
            {
                if (jArray.Count == 1 && jArray[0] is JObject && jArray[0]["status"] != null && jArray[0]["status"].Value<string>() == "error")
                {
                    return false;
                }
            }

            return true;
        }

        class InternalResult<T>
        {
            readonly T[] _results;
            readonly string _homeButtonOrientation;

            public T[] Results
            {
                get { return _results; }
            }

            public string HomeButtonOrientation
            {
                get { return _homeButtonOrientation; }
            }

            public InternalResult(T[] results, string homeButtonOrientation)
            {
                _homeButtonOrientation = homeButtonOrientation;
                _results = results;
            }
        }

        public string[] InvokeJS(IInvokeJSAppQuery invokeJsAppQuery)
        {
            QueryGestureWait(invokeJsAppQuery.AppQuery);

            var results = Query(invokeJsAppQuery.AppQuery.Invoke("calabashStringByEvaluatingJavaScript", invokeJsAppQuery.Javascript).Value<string>());
            if (results.Any(s => s.Equals("*****")))
            {
                string message = String.Format(
@"One or more views maching {0} does not support JavaScript evaluation.

Result of query:

{1}

'*****' means the view does not respond to 'calabashStringByEvaluatingJavaScript:'

", invokeJsAppQuery.AppQuery, results);
                throw new Exception(message);
            }
            return results;
        }

        public void SetSliderValue(AppQuery withQuery, double value)
        {
            var queryString = withQuery.Index(0).ToString();

            var arguments = new
            {
                query = queryString,
                operation = new { method_name = "changeSlider", arguments = new[] { value.ToString(CultureInfo.InvariantCulture) } }
            };

            var result = _connection.Map(arguments);
            if (!IsSuccessfulCalabashResult(result))
            {
                throw new Exception(string.Format("Set Slider Value'{0}' failed with output: {1}{2}", queryString, Environment.NewLine, result.Contents));
            }
            var responseJObject = JObject.Parse(result.Contents);
            var jArray = (JArray)responseJObject["results"];
            if (jArray.Count == 0 || jArray.Count == 1 && !jArray[0].HasValues)
            {
                throw new Exception(string.Format("Set Slider Value'{0}' failed.  No matching slider found.", queryString));
            }
        }

        public void SendAppToBackground(double seconds)
        {
            var result = _connection.Suspend(seconds);

            if (!IsSuccessfulCalabashResult(result))
            {
                throw new Exception(string.Format("SendAppToBackground failed with output: {0}{1}", Environment.NewLine,
                    result.Contents));
            }
        }
    }
}
