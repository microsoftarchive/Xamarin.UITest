using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Xamarin.UITest.Configuration;
using Xamarin.UITest.Queries;
using Xamarin.UITest.Repl;
using Xamarin.UITest.Shared;
using Xamarin.UITest.Shared.Android;
using Xamarin.UITest.Shared.Android.Commands;
using Xamarin.UITest.Shared.Android.Queries;
using Xamarin.UITest.Shared.Artifacts;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Logging;
using Xamarin.UITest.Shared.Screenshots;
using Xamarin.UITest.Queries.Tokens;
using Xamarin.UITest.Utils;
using Xamarin.UITest.Utils.SpecFlow;
using Xamarin.UITest.Shared.Dependencies;

namespace Xamarin.UITest.Android
{
    /// <summary>
    /// Represents a running Android application.
    /// </summary>
    public class AndroidApp : IApp
    {
        readonly WaitForHelper _waitForHelper;
        readonly IScreenshotTaker _screenshotTaker;
        readonly IExecutor _executor;
        readonly AndroidGestures _gestures;
        readonly AndroidConfig _androidConfig;
        readonly AndroidDevice _androidDevice;
        readonly ITestServer _testServer;
        readonly CommandAdbStartMonkey _monkeyStarter;
        readonly ErrorReporting _errorReporting;
        readonly SharedApp _sharedApp;

        /// <summary>
        /// Main entry point for creating Android applications. Should not be called directly
        /// but instead be invoked through the use of <see cref="ConfigureApp"/>.
        /// </summary>
        /// <param name="appConfiguration">
        /// The app configuration. Should be generated from <see cref="ConfigureApp"/>.
        /// </param>
        public AndroidApp(IAndroidAppConfiguration appConfiguration) : this (appConfiguration, null)
        {
        }

        internal AndroidApp(IAndroidAppConfiguration appConfiguration, IExecutor executor)
        {
            var factory = new DefaultAndroidFactory();

            SharedApp.BuildLogger(appConfiguration.Debug, appConfiguration.LogDirectory);

            Log.VerifyInitialized();

            _errorReporting = new ErrorReporting(QueryPlatform.Android);

            var processRunner = factory.BuildProcessRunner();
            _executor = executor ?? factory.BuildExecutor(processRunner);

            ArtifactCleaner.PotentialCleanUp();

            var appInitializer = new AndroidAppInitializer(appConfiguration, _executor, appConfiguration.WaitTimes);

            appInitializer.VerifyConfiguration();

            var deps = appInitializer.PrepareEnvironment();

            _screenshotTaker = deps.ScreenshotTaker;
            _gestures = deps.Gestures;
            _sharedApp = new SharedApp(QueryPlatform.Android, _gestures);
            _androidConfig = deps.Config;
            _androidDevice = deps.Device;
            _waitForHelper = deps.WaitForHelper;
            _testServer = deps.TestServer;
            _monkeyStarter = deps.MonkeyStarter;

            appConfiguration = deps.AppConfiguration;

            if (appConfiguration.StartAction == StartAction.ConnectToApp)
            {
                return;
            }

            var apkFiles = appInitializer.PrepareApkFiles(appConfiguration, deps.ArtifactFolder);

            if (apkFiles.AppApkFile != null)
            {
                deps.AppLifeCycle.EnsureInstalled(apkFiles.AppApkFile, apkFiles.TestServerApkFile);
                deps.AppLifeCycle.LaunchApp(apkFiles.AppApkFile, apkFiles.TestServerApkFile, appConfiguration.DeviceUri.Port);
            }
            else
            {
                deps.AppLifeCycle.EnsureInstalled(appConfiguration.InstalledAppPackageName, apkFiles.TestServerApkFile);
                deps.AppLifeCycle.LaunchApp(appConfiguration.InstalledAppPackageName, apkFiles.TestServerApkFile, appConfiguration.DeviceUri.Port);
            }

            if (!appConfiguration.DisableSpecFlowIntegration)
            {
                SpecFlowIntegrator.CheckForSpecFlowAndLoadIntegration(deps.ArtifactFolder);
            }

            _waitForHelper.WaitFor(() => _gestures.Query(_sharedApp.Expand()).Any(), timeout: appConfiguration.WaitTimes.WaitForTimeout);
        }

        /// <summary>
        /// Queries view objects using the fluent API. Defaults to only return view objects that are visible.
        /// </summary>
        /// <param name="query">
        /// Entry point for the fluent API to specify the element. If left as <c>null</c> returns all visible view
        /// objects.
        /// </param>
        /// <returns>An array representing the matched view objects.</returns>
        ///
        /// <example>
        /// <code>
        /// // Getting all TextViews on the screen. To ensure about element class use app.Repl() and tree command.
        /// AppResult[] textViews = app.Query(c => c.Class("AppCompatTextView"));
        /// </code>
        /// </example>
        public AppResult[] Query(Func<AppQuery, AppQuery> query = null)
        {
            return _errorReporting.With(() =>
                {
                    AppQuery appQuery = _sharedApp.Expand(query);
                    var results = _gestures.Query(appQuery);

                    Log.Info(string.Format("Query for {0} gave {1} results.", _sharedApp.ToCodeString(appQuery), results.Length));

                    return results;
                }, new[] { query });
        }

        /// <summary>
        /// Queries web view objects using the fluent API. Defaults to only return view objects that are visible.
        /// </summary>
        /// <param name="query">
        /// Entry point for the fluent API to specify the element. If left as <c>null</c> returns all visible view
        /// objects.
        /// </param>
        /// <returns>An array representing the matched view objects.</returns>
        ///
        /// <example>
        /// <code>
        /// // Getting all H1 elements in the second WebView
        /// AppWebResult[] headers = app.Query(c => c.WebView(1).Css("H1"));
        /// </code>
        /// </example>
        public AppWebResult[] Query(Func<AppQuery, AppWebQuery> query)
        {
            return _errorReporting.With(() =>
                {
                    AppWebQuery appWebQuery = _sharedApp.Expand(query);
                    var results = _gestures.Query(appWebQuery);

                    Log.Info(string.Format("Query for {0} gave {1} results.", _sharedApp.ToCodeString(appWebQuery), results.Length));

                    return results;
                }, new[] { query });
        }

        /// <summary>
        /// Queries view objects values using the fluent API.
        /// </summary>
        /// <param name="query">
        /// Entry point for the fluent API to specify the element. If left as <c>null</c> returns all visible view
        /// objects.
        /// </param>
        /// <returns>An array containing the values of the matched view objects.</returns>
        public T[] Query<T>(Func<AppQuery, AppTypedSelector<T>> query)
        {
            return _errorReporting.With(() =>
                {
                    AppTypedSelector<T> selector = _sharedApp.Expand(query);
                    var results = _gestures.Query(selector);

                    Log.Info(string.Format("Query for {0} gave {1} results.", _sharedApp.ToCodeString(selector), results == null ? "no" : results.Length.ToString()));

                    return results;
                }, new object[] { query });
        }

        /// <summary>
        /// Invokes Javascript on view objects using the fluent API.
        /// </summary>
        /// <param name="query">Entry point for the fluent API to specify the elements.</param>
        /// <returns>An array of strings representing the results.</returns>
        ///
        /// <example>
        /// <code>
        /// // Getting 'name' field on the web page.
        /// app.Query (c => c.WebView ().InvokeJS("return document.getElementById('name').value"));
        /// </code>
        /// </example>
        public string[] Query(Func<AppQuery, InvokeJSAppQuery> query)
        {
            return _errorReporting.With(() =>
                {
                    var selector = (IInvokeJSAppQuery)query(new AppQuery(QueryPlatform.Android));
                    string[] results = _gestures.InvokeJS(selector);
                    return results;
                }, new object[] { query });
        }

        /// <summary>
        /// Highlights the results of the query by making them flash. Specify view elements using the fluent API.
        /// Defaults to all view objects that are visible.
        /// </summary>
        /// <param name="query">
        /// Entry point for the fluent API to specify the elements. If left as <c>null</c> flashes all visible view
        /// objects.
        /// </param>
        /// <returns>An array representing the matched view objects.</returns>
        ///
        /// <example>
        /// <code>
        /// // Flashing all TextView elements on the screen
        /// app.Flash(c => c.Class("AppCompatTextView"));
        ///
        /// // Flashing all views on the screen
        /// app.Flash();
        /// </code>
        /// </example>
        public AppResult[] Flash(Func<AppQuery, AppQuery> query = null)
        {
            return _errorReporting.With(() =>
                {
                    AppQuery appQuery = _sharedApp.Expand(query);
                    var results = _gestures.Flash(appQuery);

                    Log.Info(string.Format("Flashing query for {0} gave {1} results.", _sharedApp.ToCodeString(appQuery), results.Length));

                    return results;
                }, new[] { query });
        }

        /// <summary>
        /// Enters text into the currently focused element.
        /// </summary>
        /// <param name="text">The text to enter.</param>
        ///
        /// <example>
        /// <code>
        /// // Tapping on EditText by android:id attribute and entering text.
        /// app.Tap(c => c.Id("inputField"));
        /// app.EnterText("Hello, world!");
        /// </code>
        /// </example>
        public void EnterText(string text)
        {
            _errorReporting.With(() =>
                {
                    Log.Info(string.Format("Entering text '{0}'.", text));
                    _gestures.KeyboardEnterText(text);
                }, new[] { text });
        }

        /// <summary>
        /// Enters text into a matching element that supports it.
        /// </summary>
        /// <param name="query">Entry point for the fluent API to specify the element.</param>
        /// <param name="text">The text to enter.</param>
        ///
        /// <example>
        /// <code>
        /// // Finding EditText by element class and entering text.
        /// app.EnterText(c => c.Class("AppCompatEditText"), "My text");
        ///
        /// // Finding EditText by android:id attribute and entering text. So, you don't need to tap firstly.
        /// app.EnterText(c => c.Id("inputText"), "message");
        /// </code>
        /// </example>
        public void EnterText(Func<AppQuery, AppQuery> query, string text)
        {
            _errorReporting.With(() =>
                {
                    AppQuery appQuery = _sharedApp.Expand(query);
                    var results = _gestures.QueryGestureWait(appQuery);
                    var first = _sharedApp.FirstWithLog(results, appQuery);

                    TapCoordinates(first.Rect.CenterX, first.Rect.CenterY);
                    _waitForHelper.WaitFor(IsKeyboardVisible, "Timed out waiting for keyboard to be shown.",
                        TimeSpan.FromSeconds(5));

                    _gestures.KeyboardEnterText(text);
                }, new object[] { query, text });
        }

        private bool IsKeyboardVisible()
        {
            return _executor.Execute(new QueryAdbKeyboardShown(_androidDevice.DeviceIdentifier));
        }

        /// <summary>
        /// Enters text into a matching element that supports it.
        /// </summary>
        /// <param name="query">Entry point for the fluent API to specify the element.</param>
        /// <param name="text">The text to enter.</param>
        ///
        /// <example>
        /// <code>
        /// // Entering text into the 'name' HTML input element
        /// app.EnterText(c => c.Css("input#name"), "John");
        /// </code>
        /// </example>
        public void EnterText(Func<AppQuery, AppWebQuery> query, string text)
        {
            _errorReporting.With(() =>
                {
                    AppWebQuery appWebQuery = _sharedApp.Expand(query);
                    var results = _gestures.QueryGestureWait(appWebQuery);

                    var first = _sharedApp.FirstWithLog(results, appWebQuery);
                    TapCoordinates(first.Rect.CenterX, first.Rect.CenterY);

                    bool hasCssToken = false;
                    // Try to make sure the element is selected before entering the text.
                    if (results.Length == 1 && ((ITokenContainer)appWebQuery).Tokens.Any(t => t is CssToken))
                    {
                        hasCssToken = true;
                        // Selector finds one element, try to fasttrack to KeyboardEnterText instead of waiting, by testing for focus.
                        var focusTokens = ((ITokenContainer)appWebQuery).Tokens.Select(AddFocus);
                        var focusQuery = new AppWebQuery(focusTokens, QueryPlatform.Android);
                        var appWebRect = results.First().Rect;
                        _waitForHelper.WaitForAnyOrDefault(() => _gestures.Query(focusQuery).Where(e => e.Rect.Equals(appWebRect)).ToArray(), null, TimeSpan.FromMilliseconds(1000), null, TimeSpan.FromDays(0));
                    }
                    else
                    {
                        Thread.Sleep(TimeSpan.FromMilliseconds(1000));
                    }
                    _waitForHelper.WaitFor(IsKeyboardVisible, "Timed out waiting for keyboard to be shown.",
                        TimeSpan.FromSeconds(5));

                    if (appWebQuery.Platform == QueryPlatform.Android && hasCssToken)
                    {
                         string cssSelector = ((CssToken)((ITokenContainer)appWebQuery).Tokens.Where(t => t is CssToken).First()).CssSelector;
                        _gestures.SetTextWebView(cssSelector, text);
                    }
                    else
                    {
                        _gestures.KeyboardEnterText(text);
                    }
                }, new object[] { query, text });
        }

        private IQueryToken AddFocus(IQueryToken queryToken)
        {
            if (queryToken is CssToken)
            {
                return new CssToken(((CssToken)queryToken).CssSelector + ":focus");
            }
            return queryToken;
        }

        /// <summary>
        /// Clears text from a matching element that supports it.
        /// </summary>
        /// <param name="query">Entry point for the fluent API to specify the element.</param>
        ///
        /// <example>
        /// <code>
        /// // Clearing EditText with android:id = "inputField"
        /// app.ClearText(c => c.Id("inputField"))
        /// </code>
        /// </example>
        public void ClearText(Func<AppQuery, AppQuery> query)
        {
            _errorReporting.With(() =>
                {
                    AppQuery appQuery = _sharedApp.Expand(query);
                    var results = _gestures.QueryGestureWait(appQuery);

                    var first = _sharedApp.FirstWithLog(results, appQuery);
                    _gestures.TapCoordinates(first.Rect.CenterX, first.Rect.CenterY);
                    _gestures.ClearText();
                }, new object[] { query });
        }

        /// <summary>
        /// Clears text from a matching element that supports it.
        /// </summary>
        /// <param name="query">Entry point for the fluent API to specify the element.</param>
        ///
        /// <example>
        /// <code>
        /// // Clearing text in the 'name' HTML input element
        /// app.ClearText(c => c.Css("input#name"));
        /// </code>
        /// </example>
        public void ClearText(Func<AppQuery, AppWebQuery> query)
        {
            _errorReporting.With(() =>
                {
                    AppWebQuery appWebQuery = _sharedApp.Expand(query);
                    var results = _gestures.QueryGestureWait(appWebQuery);

                    var first = _sharedApp.FirstWithLog(results, appWebQuery);
                    _gestures.TapCoordinates(first.Rect.CenterX, first.Rect.CenterY);
                    _gestures.ClearText();
                }, new object[] { query });
        }

        /// <summary>
        /// Clears text from the currently focused element.
        /// </summary>
        ///
        /// <example>
        /// <code>
        /// // Tapping on the EditText with android:id = "input" and clearing text.
        /// app.Tap(c => c.Id("input"))
        /// app.ClearText()
        /// </code>
        /// </example>
        public void ClearText()
        {
            _errorReporting.With(() =>
                {
                    _gestures.ClearText();
                });
        }

        /// <summary>
        /// Performs a tap / touch gesture on the matched element. If multiple elements are matched, the first one
        /// will be used.
        /// </summary>
        /// <param name="query">Entry point for the fluent API to specify the element.</param>
        ///
        /// <example>
        /// <code>
        /// // Tapping on CALCULATE button
        /// app.Tap(c => c.Marked("CALCULATE"));
        ///
        /// // Tapping on EditText with android:id="@+id/inputBill"
        /// app.Tap(c => c.Id("inputBill"));
        ///
        /// // Tapping on the first appropriate element with class AppCompatButton. You can check a class of
        /// // element by using app.Repl() and tree command.
        /// app.Tap(c => c.Class("AppCompatButton"));
        /// </code>
        /// </example>
        public void Tap(Func<AppQuery, AppQuery> query)
        {
            _errorReporting.With(() =>
                {
                    _gestures.WaitForNoneAnimatingOrElapsed();
                    AppQuery appQuery = _sharedApp.Expand(query);
                    var results = _gestures.QueryGestureWait(appQuery);
                    var first = _sharedApp.FirstWithLog(results, appQuery);

                    TapCoordinates(first.Rect.CenterX, first.Rect.CenterY);
                }, new[] { query });
        }

        /// <summary>
        /// Performs a tap / touch gesture on the matched element. If multiple elements are matched, the first one
        /// will be used.
        /// </summary>
        /// <param name="query">Entry point for the fluent API to specify the element.</param>
        public void Tap(Func<AppQuery, AppWebQuery> query)
        {
            _errorReporting.With(() =>
                {
                    _gestures.WaitForNoneAnimatingOrElapsed();
                    AppWebQuery appWebQuery = _sharedApp.Expand(query);
                    var results = _gestures.QueryGestureWait(appWebQuery);
                    var first = _sharedApp.FirstWithLog(results, appWebQuery);

                    TapCoordinates(first.Rect.CenterX, first.Rect.CenterY);
                }, new[] { query });
        }

        /// <summary>
        /// Performs a tap / touch gesture on the given coordinates.
        /// </summary>
        /// <param name="x">The x coordinate to tap.</param>
        /// <param name="y">The y coordinate to tap.</param>
        public void TapCoordinates(float x, float y)
        {
            _errorReporting.With(() =>
                {
                    Log.Info("Tapping coordinates [ " + x + ", " + y + " ].");
                    _gestures.TapCoordinates(x, y);
                }, new object[] { x, y });
        }

        /// <summary>
        /// Performs a continuous touch gesture on the matched element. If multiple elements are matched, the first
        /// one will be used.
        /// </summary>
        /// <param name="query">Entry point for the fluent API to specify the element.</param>
        public void TouchAndHold(Func<AppQuery, AppQuery> query)
        {
            _errorReporting.With(() =>
                {
                    AppQuery appQuery = _sharedApp.Expand(query);
                    var results = _gestures.QueryGestureWait(appQuery);

                    var first = _sharedApp.FirstWithLog(results, appQuery);

                    _gestures.TouchAndHoldCoordinates(first.Rect.CenterX, first.Rect.CenterY);
                }, new[] { query });
        }

        /// <summary>
        /// Performs a continuous touch gesture on the given coordinates.
        /// </summary>
        /// <param name="x">The x coordinate to touch.</param>
        /// <param name="y">The y coordinate to touch.</param>
        public void TouchAndHoldCoordinates(float x, float y)
        {
            _errorReporting.With(() =>
                {
                    Log.Info("Touch and holding coordinates [ " + x + ", " + y + " ].");
                    _gestures.TouchAndHoldCoordinates(x, y);
                }, new object[] { x, y });
        }

        private void LogDrop<T>(T[] fromResults, T[] toResults, ITokenContainer fromQuery, ITokenContainer toQuery, float toX, float toY, DropLocation placement)
        {
            if (!toResults.Any())
            {
                throw new Exception(String.Format("Unable to drop element. Query for {0} gave no results", toQuery));
            }

            if (toResults.Length == 1)
            {
                if (placement == DropLocation.Above || placement == DropLocation.Below)
                    Log.Info(string.Format("Dropping element matching {0} {4} element matching {1} at coordinates [ {2} , {3} ]", fromQuery, toQuery, toX, toY, placement));
                else
                    Log.Info(string.Format("Dropping element matching {0} {4} of element matching {1} at coordinates [ {2} , {3} ]", fromQuery, toQuery, toX, toY, placement));
            }
            if (toResults.Length > 1)
            {
                if (placement == DropLocation.Above || placement == DropLocation.Below)
                    Log.Info(string.Format("Dropping element matching {0} {4} first element matching {1} (of {2} total) at coordinates [ {3} , {4} ]", fromQuery, toQuery, toResults.Length, toX, toY));
                else
                    Log.Info(string.Format("Dropping element matching {0} {4} of first element matching {1} (of {2} total) at coordinates [ {3} , {4} ]", fromQuery, toQuery, toResults.Length, toX, toY));
            }
        }

        /// <summary>
        /// Performs a long touch on an item, followed by dragging the item to a second item and dropping it
        /// </summary>
        /// <param name="from">The query of the item to be dragged</param>
        /// <param name="to">The query of the location for the item to be dropped</param>
        public void DragAndDrop(
            Func<AppQuery, AppQuery> from,
            Func<AppQuery, AppQuery> to)
        {
            _errorReporting.With(() =>
                {
                    DragAndDropInner(from, to);
                }, new Object[] { from, to });
        }

        /// <summary>
        /// Performs a long touch on an item, followed by dragging the item to a second item and dropping it
        /// </summary>
        /// <param name="from">Marked selector of the from element.</param>
        /// <param name="to">Marked selector of the to element.</param>
        public void DragAndDrop(string from, string to)
        {
            _errorReporting.With(() =>
                {
                    DragAndDropInner(_sharedApp.AsMarkedQuery(from), _sharedApp.AsMarkedQuery(to));
                }, new Object[] { from, to });
        }

        /// <summary>
        /// Performs a long touch on an item, followed by dragging the item to a second item and dropping it
        /// </summary>
        /// <param name="from">The query of the item to be dragged</param>
        /// <param name="to">The query of the location for the item to be dropped</param>
        /// <param name="placement">
        /// The placement of the drop (on top, above, below, left, right) relative to the to query
        /// </param>
        /// <param name="holdTime">Time to hold on the from query</param>
        /// <param name="hangTime">Time to hold above the to query</param>
        /// <param name="steps">The number of steps desired to drag the item, higher for a slower drag</param>
        /// <param name="afterStepAction">Action to perform after each step</param>
        public void DragAndDrop(
            Func<AppQuery, AppQuery> from,
            Func<AppQuery, AppQuery> to,
            DropLocation placement = DropLocation.OnTop,
            TimeSpan? holdTime = null,
            TimeSpan? hangTime = null,
            int steps = 1,
            Action afterStepAction = null)
        {
            _errorReporting.With(() =>
                {
                    DragAndDropInner(from, to, placement, holdTime, hangTime, steps, afterStepAction);
                }, new Object[] { from, to, placement, holdTime, hangTime, steps, afterStepAction });
        }

        void DragAndDropInner(
            Func<AppQuery, AppQuery> from,
            Func<AppQuery, AppQuery> to,
            DropLocation placement = DropLocation.OnTop,
            TimeSpan? holdTime = null,
            TimeSpan? hangTime = null,
            int steps = 1,
            Action afterStepAction = null)
        {
            if (steps < 0)
            {
                throw new ArgumentException("steps must be zero or positive.");
            }

            var fromQuery = _sharedApp.Expand(from);
            var fromResults = _gestures.QueryGestureWait(fromQuery);

            _sharedApp.FirstWithLog(fromResults, fromQuery);

            var fromX = fromResults.First().Rect.CenterX;
            var fromY = fromResults.First().Rect.CenterY;

            using (var monkeyConnection = new MonkeyConnection(_monkeyStarter, _executor, _waitForHelper, _gestures))
            {
                _waitForHelper.ExecuteAndWait(
                    // ReSharper disable once AccessToDisposedClosure
                    () => monkeyConnection.SendCommand($"touch down {fromX} {fromY}"),
                    holdTime.GetValueOrDefault(TimeSpan.FromSeconds(2)));

                var toQuery = _sharedApp.Expand(to);
                var toResults = _gestures.QueryGestureWait(toQuery);

                if (!toResults.Any())
                {
                    throw new Exception($"Unable to drop element. Query for {toQuery} gave no results");
                }

                var toX = toResults.First().Rect.CenterX;
                var toY = toResults.First().Rect.CenterY;

                switch (placement)
                {
                    case DropLocation.Above:
                    {
                        toY = toResults.First().Rect.Y;
                        break;
                    }
                    case DropLocation.Below:
                    {
                        toY = toResults.First().Rect.Y + toResults.First().Rect.Height;
                        break;
                    }
                    case DropLocation.Left:
                    {
                        toX = toResults.First().Rect.X;
                        break;
                    }
                    case DropLocation.Right:
                    {
                        toX = toResults.First().Rect.X + toResults.First().Rect.Width;
                        break;
                    }
                }

                LogDrop(fromResults, toResults, fromQuery, toQuery, toX, toY, placement);

                var xDelta = (toX - fromX);
                var yDelta = (toY - fromY);

                for (float i = 1; i <= steps - 1; i++)
                {
                    var xPosition = (int)(fromX + (i*xDelta)/steps);
                    var yPosition = (int)(fromY + (i*yDelta)/steps);

                    monkeyConnection.SendCommand($"touch move {xPosition} {yPosition}");
                    afterStepAction?.Invoke();
                }
                monkeyConnection.SendCommand($"touch move {(int)toX} {(int)toY}");

                Thread.Sleep(hangTime.GetValueOrDefault(TimeSpan.FromSeconds(2)));
                monkeyConnection.SendCommand($"touch up {toX} {toY}");
            }
        }

        /// <summary>
        /// Performs two quick tap / touch gestures on the matched element. If multiple elements are matched, the
        /// first one will be used.
        /// </summary>
        /// <param name="query">Entry point for the fluent API to specify the element.</param>
        public void DoubleTap(Func<AppQuery, AppQuery> query)
        {
            _errorReporting.With(() =>
                {
                    AppQuery appQuery = _sharedApp.Expand(query);
                    var results = _gestures.QueryGestureWait(appQuery);

                    if (!results.Any())
                    {
                        throw new Exception(string.Format(
                                "Unable to double tap on element. Query for {0} gave no results.", _sharedApp.ToCodeString(appQuery)));
                    }

                    var centerX = results.First().Rect.CenterX;
                    var centerY = results.First().Rect.CenterY;

                    if (results.Length == 1)
                    {
                        Log.Info(string.Format("Double tapping on element matching {0} at coordinates [ {1}, {2} ].", _sharedApp.ToCodeString(appQuery), centerX, centerY));
                    }
                    else
                    {
                        Log.Info(
                            string.Format(
                                "Double tapping on first element ({1} total) matching {0} at coordinates [ {2}, {3} ]. ", _sharedApp.ToCodeString(appQuery), results.Length, centerX, centerY));
                    }

                    _gestures.DoubleTapCoordinates(centerX, centerY);
                }, new[] { query });
        }

        /// <summary>
        /// Performs two quick tap / touch gestures on the matched element. If multiple elements are matched, the
        /// first one will be used.
        /// This version is specifically for queries on web views
        /// </summary>
        /// <param name="query">Entry point for the fluent API to specify the element.</param>
        public void DoubleTap(Func<AppQuery, AppWebQuery> query)
        {
            _errorReporting.With(() =>
                {
                    AppWebQuery appWebQuery = _sharedApp.Expand(query);
                    var results = _gestures.QueryGestureWait(appWebQuery);
                    var first = _sharedApp.FirstWithLog(results, appWebQuery);

                    _gestures.DoubleTapCoordinates(first.Rect.CenterX, first.Rect.CenterY);
                }, new object[] { query });
        }

        /// <summary>
        /// Performs a quick double tap / touch gesture on the given coordinates.
        /// </summary>
        /// <param name="x">The x coordinate to touch.</param>
        /// <param name="y">The y coordinate to touch.</param>
        public void DoubleTapCoordinates(float x, float y)
        {
            _errorReporting.With(() =>
                {
                    _gestures.WaitForNoneAnimatingOrElapsed();
                    Log.Info("Double tapping coordinates [ " + x + ", " + y + " ].");
                    _gestures.DoubleTapCoordinates(x, y);
                }, new object[] { x, y });
        }

        /// <summary>
        /// Performs a pinch gestures on the matched element to zoom the view in. If multiple elements are matched,
        /// the first one will be used.
        /// </summary>
        /// <param name="query">Entry point for the fluent API to specify the element.</param>
        /// <param name="duration">The <see cref="TimeSpan"/> duration of the pinch gesture.</param>
        public void PinchToZoomIn(Func<AppQuery, AppQuery> query, TimeSpan? duration = null)
        {
            _errorReporting.With(() =>
                {
                    AppQuery appQuery = _sharedApp.Expand(query);
                    var results = _gestures.QueryGestureWait(appQuery);

                    var first = _sharedApp.FirstWithLog(results, appQuery);

                    _gestures.PinchToZoomCoordinates(first.Rect.CenterX, first.Rect.CenterY, AndroidGestures.PinchDirection.Open, duration);

                }, new object[] { query, duration });
        }

        /// <summary>
        /// Performs a pinch gestures to zoom the view in on the given coordinates.
        /// </summary>
        /// <param name="x">The x coordinate of the center of the pinch.</param>
        /// <param name="y">The y coordinate of the center of the pinch.</param>
        /// <param name="duration">The <see cref="TimeSpan"/> duration of the pinch gesture.</param>
        public void PinchToZoomInCoordinates(float x, float y, TimeSpan? duration = null)
        {
            _errorReporting.With(() =>
                {
                    _gestures.WaitForNoneAnimatingOrElapsed();
                    Log.Info("Pinching to zoom in on coordinates [ " + x + ", " + y + " ].");
                    _gestures.PinchToZoomCoordinates(x, y, AndroidGestures.PinchDirection.Open, duration);
                }, new object[] { x, y, duration });
        }

        /// <summary>
        /// Performs a pinch gestures on the matched element to zoom the view out. If multiple elements are matched,
        /// the first one will be used.
        /// </summary>
        /// <param name="query">Entry point for the fluent API to specify the element.</param>
        /// <param name="duration">The <see cref="TimeSpan"/> duration of the pinch gesture.</param>
        public void PinchToZoomOut(Func<AppQuery, AppQuery> query, TimeSpan? duration = null)
        {
            _errorReporting.With(() =>
                {
                    AppQuery appQuery = _sharedApp.Expand(query);
                    var results = _gestures.QueryGestureWait(appQuery);

                    var first = _sharedApp.FirstWithLog(results, appQuery);

                    _gestures.PinchToZoomCoordinates(first.Rect.CenterX, first.Rect.CenterY, AndroidGestures.PinchDirection.Close, duration);
                }, new object[] { query, duration });
        }

        /// <summary>
        /// Performs a pinch gestures to zoom the view in on the given coordinates.
        /// </summary>
        /// <param name="x">The x coordinate of the center of the pinch.</param>
        /// <param name="y">The y coordinate of the center of the pinch.</param>
        /// <param name="duration">The <see cref="TimeSpan"/> duration of the pinch gesture.</param>
        public void PinchToZoomOutCoordinates(float x, float y, TimeSpan? duration = null)
        {
            _errorReporting.With(() =>
                {
                    _gestures.WaitForNoneAnimatingOrElapsed();
                    Log.Info("Pinching to zoom out on coordinates [ " + x + ", " + y + " ].");
                    _gestures.PinchToZoomCoordinates(x, y, AndroidGestures.PinchDirection.Close, duration);
                }, new object[] { x, y, duration });
        }

        /// <summary>
        /// Performs a continuous drag gesture between 2 points.
        /// </summary>
        /// <param name="fromX">The x coordinate to start dragging from.</param>
        /// <param name="fromY">The y coordinate to start dragging from.</param>
        /// <param name="toX">The x coordinate to drag to.</param>
        /// <param name="toY">The y coordinate to drag to.</param>
        public void DragCoordinates(float fromX, float fromY, float toX, float toY)
        {
            _errorReporting.With(() =>
                {
                    Log.Info("Dragging from [ " + fromX + ", " + fromY + " ] to [ " + toX + ", " + toY + " ].");
                    _gestures.DragCoordinates(fromX, fromY, toX, toY);
                }, new object[] { fromX, fromY, toX, toY });
        }

        /// <summary>
        /// Performs a left to right swipe gesture.
        /// </summary>
        /// <param name="swipePercentage">How far across the screen to swipe (from 0.0 to 1.0).</param>
        /// <param name="swipeSpeed">The speed of the gesture.</param>
        /// <param name="withInertia">Whether swipes should cause inertia.</param>
        public void SwipeLeftToRight(
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            bool withInertia = true)
        {
            _sharedApp.SwipeLeftToRight(swipePercentage, swipeSpeed, withInertia);
        }

        /// <summary>
        /// Performs a left to right swipe gesture on the matching element. If multiple elements are matched, the
        /// first one will be used.
        /// </summary>
        /// <param name="marked">
        /// Marked selector to match. See <see cref="AppQuery.Marked" /> for more information.
        /// </param>
        /// <param name="swipePercentage">How far across the element to swipe (from 0.0 to 1.0).</param>
        /// <param name="swipeSpeed">The speed of the gesture.</param>
        /// <param name="withInertia">Whether swipes should cause inertia.</param>
        public void SwipeLeftToRight(
            string marked,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            bool withInertia = true)
        {
            _sharedApp.SwipeLeftToRight(marked, swipePercentage, swipeSpeed, withInertia);
        }

        /// <summary>
        /// Performs a left to right swipe gesture on the matching element. If multiple elements are matched, the
        /// first one will be used.
        /// </summary>
        /// <param name="query">Entry point for the fluent API to specify the element.</param>
        /// <param name="swipePercentage">How far across the element to swipe (from 0.0 to 1.0).</param>
        /// <param name="swipeSpeed">The speed of the gesture.</param>
        /// <param name="withInertia">Whether swipes should cause inertia.</param>
        public void SwipeLeftToRight(
            Func<AppQuery, AppQuery> query,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            bool withInertia = true)
        {
            _sharedApp.SwipeLeftToRight(query, swipePercentage, swipeSpeed, withInertia);
        }

        /// <summary>
        /// Performs a left to right swipe gesture on the matching element. If multiple elements are matched, the
        /// first one will be used.
        /// </summary>
        /// <param name="query">Entry point for the fluent API to specify the element.</param>
        /// <param name="swipePercentage">How far across the element to swipe (from 0.0 to 1.0).</param>
        /// <param name="swipeSpeed">The speed of the gesture.</param>
        /// <param name="withInertia">Whether swipes should cause inertia.</param>
        public void SwipeLeftToRight(
            Func<AppQuery, AppWebQuery> query,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            bool withInertia = true)
        {
            _sharedApp.SwipeLeftToRight(query, swipePercentage, swipeSpeed, withInertia);
        }

        /// <summary>
        /// Performs a right to left swipe gesture.
        /// </summary>
        /// <param name="swipePercentage">How far across the screen to swipe (from 0.0 to 1.0).</param>
        /// <param name="swipeSpeed">The speed of the gesture.</param>
        /// <param name="withInertia">Whether swipes should cause inertia.</param>
        public void SwipeRightToLeft(double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed, bool withInertia = true)
        {
            _sharedApp.SwipeRightToLeft(swipePercentage, swipeSpeed, withInertia);
        }

        /// <summary>
        /// Performs a right to left swipe gesture on the matching element. If multiple elements are matched, the
        /// first one will be used.
        /// </summary>
        /// <param name="marked">
        /// Marked selector to match. See <see cref="AppQuery.Marked" /> for more information.
        /// </param>
        /// <param name="swipePercentage">How far across the element to swipe (from 0.0 to 1.0).</param>
        /// <param name="swipeSpeed">The speed of the gesture.</param>
        /// <param name="withInertia">Whether swipes should cause inertia.</param>
        public void SwipeRightToLeft(string marked, double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed, bool withInertia = true)
        {
            _sharedApp.SwipeRightToLeft(marked, swipePercentage, swipeSpeed, withInertia);
        }

        /// <summary>
        /// Performs a right to left swipe gesture on the matching element. If multiple elements are matched, the
        /// first one will be used.
        /// </summary>
        /// <param name="query">Entry point for the fluent API to specify the element.</param>
        /// <param name="swipePercentage">How far across the element to swipe (from 0.0 to 1.0).</param>
        /// <param name="swipeSpeed">The speed of the gesture.</param>
        /// <param name="withInertia">Whether swipes should cause inertia.</param>
        public void SwipeRightToLeft(
            Func<AppQuery, AppQuery> query,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            bool withInertia = true)
        {
            _sharedApp.SwipeRightToLeft(query, swipePercentage, swipeSpeed, withInertia);
        }

        /// <summary>
        /// Performs a left to right swipe gesture on the matching element. If multiple elements are matched, the
        /// first one will be used.
        /// </summary>
        /// <param name="query">Entry point for the fluent API to specify the element.</param>
        /// <param name="swipePercentage">How far across the element to swipe (from 0.0 to 1.0).</param>
        /// <param name="swipeSpeed">The speed of the gesture.</param>
        /// <param name="withInertia">Whether swipes should cause inertia.</param>
        public void SwipeRightToLeft(
            Func<AppQuery, AppWebQuery> query,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            bool withInertia = true)
        {
            _sharedApp.SwipeRightToLeft(query, swipePercentage, swipeSpeed, withInertia);
        }

        /// <summary>
        /// Scrolls up on the first element matching query.
        /// </summary>
        /// <param name="withinQuery">
        /// Entry point for the fluent API to specify the what element to scroll within.
        /// </param>
        /// <param name="strategy">Strategy for scrolling element.</param>
        /// <param name="swipePercentage">
        /// How far across the element to swipe (from 0.0 to 1.0).  Ignored for programmatic scrolling.
        /// </param>
        /// <param name="swipeSpeed">The speed of the gesture.  Ignored for programmatic scrolling.</param>
        /// <param name="withInertia">Whether swipes should cause inertia.  Ignored for programmatic scrolling.</param>
        public void ScrollUp(
            Func<AppQuery, AppQuery> withinQuery = null,
            ScrollStrategy strategy = ScrollStrategy.Auto,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            bool withInertia = true)
        {
            _errorReporting.With(() =>
                {
                    _sharedApp.ScrollUp(swipePercentage, swipeSpeed, withinQuery, strategy, withInertia);
                }, new object[] { withinQuery, strategy, swipePercentage, swipeSpeed, withInertia });
        }

        /// <summary>
        /// Scrolls down on the first element matching query.
        /// </summary>
        /// <param name="withinQuery">
        /// Entry point for the fluent API to specify the what element to scroll within.
        /// </param>
        /// <param name="strategy">Strategy for scrolling element.</param>
        /// <param name="swipePercentage">
        /// How far across the element to swipe (from 0.0 to 1.0).  Ignored for programmatic scrolling.
        /// </param>
        /// <param name="swipeSpeed">The speed of the gesture.  Ignored for programmatic scrolling.</param>
        /// <param name="withInertia">Whether swipes should cause inertia.  Ignored for programmatic scrolling.</param>
        public void ScrollDown(
            Func<AppQuery, AppQuery> withinQuery = null,
            ScrollStrategy strategy = ScrollStrategy.Auto,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            bool withInertia = true)
        {
            _errorReporting.With(() =>
                {
                    _sharedApp.ScrollDown(swipePercentage, swipeSpeed, withinQuery, strategy, withInertia);
                }, new object[] { withinQuery, strategy, swipePercentage, swipeSpeed, withInertia });
        }

        /// <summary>
        /// Scrolls left on the first element matching query.
        /// </summary>
        /// <param name="withinQuery">
        /// Entry point for the fluent API to specify the what element to scroll within.
        /// </param>
        /// <param name="strategy">Strategy for scrolling element.</param>
        /// <param name="swipePercentage">
        /// How far across the element to swipe (from 0.0 to 1.0).  Ignored for programmatic scrolling.
        /// </param>
        /// <param name="swipeSpeed">The speed of the gesture.  Ignored for programmatic scrolling.</param>
        /// <param name="withInertia">Whether swipes should cause inertia.  Ignored for programmatic scrolling.</param>
        public void ScrollLeft(
            Func<AppQuery, AppQuery> withinQuery = null,
            ScrollStrategy strategy = ScrollStrategy.Auto,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            bool withInertia = true)
        {
            _errorReporting.With(() =>
                {
                    _sharedApp.ScrollLeft(swipePercentage, swipeSpeed, withinQuery, strategy, withInertia);
                }, new object[] { withinQuery, strategy, swipePercentage, swipeSpeed, withInertia });
        }

        /// <summary>
        /// Scrolls right on the first element matching query.
        /// </summary>
        /// <param name="withinQuery">
        /// Entry point for the fluent API to specify the what element to scroll within.
        /// </param>
        /// <param name="strategy">Strategy for scrolling element.</param>
        /// <param name="swipePercentage">
        /// How far across the element to swipe (from 0.0 to 1.0).  Ignored for programmatic scrolling.
        /// </param>
        /// <param name="swipeSpeed">The speed of the gesture.  Ignored for programmatic scrolling.</param>
        /// <param name="withInertia">Whether swipes should cause inertia.  Ignored for programmatic scrolling.</param>
        public void ScrollRight(
            Func<AppQuery, AppQuery> withinQuery = null,
            ScrollStrategy strategy = ScrollStrategy.Auto,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            bool withInertia = true)
        {
            _errorReporting.With(() =>
                {
                    _sharedApp.ScrollRight(swipePercentage, swipeSpeed, withinQuery, strategy, withInertia);
                }, new object[] { withinQuery, strategy, swipePercentage, swipeSpeed, withInertia });
        }

        /// <summary>
        /// Scroll up until an element that matches the <c>toQuery</c> is shown on the screen.
        /// </summary>
        /// <param name="toQuery">Entry point for the fluent API to specify the element to bring on screen.</param>
        /// <param name="withinQuery">Entry point for the fluent API to specify what element to scroll within.</param>
        /// <param name="strategy">Strategy for scrolling element.</param>
        /// <param name="swipePercentage">
        /// How far across the element to swipe (from 0.0 to 1.0).  Ignored for programmatic scrolling.
        /// </param>
        /// <param name="swipeSpeed">The speed of the gesture.  Ignored for programmatic scrolling.</param>
        /// <param name="withInertia">Whether swipes should cause inertia.  Ignored for programmatic scrolling.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before failing.</param>
        public void ScrollUpTo(
            Func<AppQuery, AppQuery> toQuery,
            Func<AppQuery, AppQuery> withinQuery = null,
            ScrollStrategy strategy = ScrollStrategy.Auto,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            bool withInertia = true,
            TimeSpan? timeout = null)
        {
            _errorReporting.With(() =>
                {
                    _sharedApp.ScrollUpTo(toQuery, swipePercentage, swipeSpeed, withinQuery, strategy, withInertia,
                        timeout);
                }, new object[] { toQuery, withinQuery, strategy, swipePercentage, swipeSpeed, withInertia, timeout });
        }

        /// <summary>
        /// Scroll up until an element that matches the <c>toQuery</c> is shown on the screen.
        /// </summary>
        /// <param name="toQuery">Entry point for the fluent API to specify the element to bring on screen.</param>
        /// <param name="withinQuery">Entry point for the fluent API to specify what element to scroll within.</param>
        /// <param name="strategy">Strategy for scrolling element.</param>
        /// <param name="swipePercentage">
        /// How far across the element to swipe (from 0.0 to 1.0).  Ignored for programmatic scrolling.
        /// </param>
        /// <param name="swipeSpeed">The speed of the gesture.  Ignored for programmatic scrolling.</param>
        /// <param name="withInertia">Whether swipes should cause inertia.  Ignored for programmatic scrolling.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before failing.</param>
        public void ScrollUpTo(
            Func<AppQuery, AppWebQuery> toQuery,
            Func<AppQuery, AppQuery> withinQuery = null,
            ScrollStrategy strategy = ScrollStrategy.Auto,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            bool withInertia = true,
            TimeSpan? timeout = null)
        {
            _errorReporting.With(() =>
                {
                    _sharedApp.ScrollUpTo(toQuery, swipePercentage, swipeSpeed, withinQuery, strategy, withInertia,
                        timeout);
                }, new object[] { toQuery, withinQuery, strategy, swipePercentage, swipeSpeed, withInertia, timeout });
        }

        /// <summary>
        /// Scroll down until an element that matches the <c>toQuery</c> is shown on the screen.
        /// </summary>
        /// <param name="toQuery">Entry point for the fluent API to specify the element to bring on screen.</param>
        /// <param name="withinQuery">Entry point for the fluent API to specify what element to scroll within.</param>
        /// <param name="strategy">Strategy for scrolling element.</param>
        /// <param name="swipePercentage">
        /// How far across the element to swipe (from 0.0 to 1.0).  Ignored for programmatic scrolling.
        /// </param>
        /// <param name="swipeSpeed">The speed of the gesture.  Ignored for programmatic scrolling.</param>
        /// <param name="withInertia">Whether swipes should cause inertia.  Ignored for programmatic scrolling.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before failing.</param>
        public void ScrollDownTo(
            Func<AppQuery, AppQuery> toQuery,
            Func<AppQuery, AppQuery> withinQuery = null,
            ScrollStrategy strategy = ScrollStrategy.Auto,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            bool withInertia = true,
            TimeSpan? timeout = null)
        {
            _errorReporting.With(() =>
                {
                    _sharedApp.ScrollDownTo(toQuery, swipePercentage, swipeSpeed, withinQuery, strategy, withInertia,
                        timeout);
                }, new object[] { toQuery, withinQuery, strategy, swipePercentage, swipeSpeed, withInertia, timeout });
        }

        /// <summary>
        /// Scroll down until an element that matches the <c>toQuery</c> is shown on the screen.
        /// </summary>
        /// <param name="toQuery">Entry point for the fluent API to specify the element to bring on screen.</param>
        /// <param name="withinQuery">Entry point for the fluent API to specify what element to scroll within.</param>
        /// <param name="strategy">Strategy for scrolling element.</param>
        /// <param name="swipePercentage">
        /// How far across the element to swipe (from 0.0 to 1.0).  Ignored for programmatic scrolling.
        /// </param>
        /// <param name="swipeSpeed">The speed of the gesture.  Ignored for programmatic scrolling.</param>
        /// <param name="withInertia">Whether swipes should cause inertia.  Ignored for programmatic scrolling.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before failing.</param>
        public void ScrollDownTo(
            Func<AppQuery, AppWebQuery> toQuery,
            Func<AppQuery, AppQuery> withinQuery = null,
            ScrollStrategy strategy = ScrollStrategy.Auto,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            bool withInertia = true,
            TimeSpan? timeout = null)
        {
            _errorReporting.With(() =>
                {
                    _sharedApp.ScrollDownTo(toQuery, swipePercentage, swipeSpeed, withinQuery, strategy, withInertia,
                        timeout);
                }, new object[] { toQuery, withinQuery, strategy, swipePercentage, swipeSpeed, withInertia, timeout });
        }

        /// <summary>
        /// Scroll left until an element that matches the <c>toQuery</c> is shown on the screen.
        /// </summary>
        /// <param name="toQuery">Entry point for the fluent API to specify the element to bring on screen.</param>
        /// <param name="withinQuery">Entry point for the fluent API to specify what element to scroll within.</param>
        /// <param name="strategy">Strategy for scrolling element.</param>
        /// <param name="swipePercentage">
        /// How far across the element to swipe (from 0.0 to 1.0).  Ignored for programmatic scrolling.
        /// </param>
        /// <param name="swipeSpeed">The speed of the gesture.  Ignored for programmatic scrolling.</param>
        /// <param name="withInertia">Whether swipes should cause inertia.  Ignored for programmatic scrolling.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before failing.</param>
        public void ScrollLeftTo(
            Func<AppQuery, AppQuery> toQuery,
            Func<AppQuery, AppQuery> withinQuery = null,
            ScrollStrategy strategy = ScrollStrategy.Auto,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            bool withInertia = true,
            TimeSpan? timeout = null)
        {
            _errorReporting.With(() =>
                {
                    _sharedApp.LogWithToQueryWithinQuery("Scrolling left to", toQuery, withinQuery);

                    _gestures.ScrollTo(_sharedApp.Expand(toQuery), _sharedApp.ExpandIfNotNull(withinQuery),
                        ScrollDirection.Left, strategy, swipePercentage, swipeSpeed, withInertia, timeout);
                }, new object[] { toQuery, withinQuery, strategy, swipePercentage, swipeSpeed, withInertia, timeout });
        }

        /// <summary>
        /// Scroll left until an element that matches the <c>toQuery</c> is shown on the screen.
        /// </summary>
        /// <param name="toQuery">Entry point for the fluent API to specify the element to bring on screen.</param>
        /// <param name="withinQuery">Entry point for the fluent API to specify what element to scroll within.</param>
        /// <param name="strategy">Strategy for scrolling element.</param>
        /// <param name="swipePercentage">
        /// How far across the element to swipe (from 0.0 to 1.0).  Ignored for programmatic scrolling.
        /// </param>
        /// <param name="swipeSpeed">The speed of the gesture.  Ignored for programmatic scrolling.</param>
        /// <param name="withInertia">Whether swipes should cause inertia.  Ignored for programmatic scrolling.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before failing.</param>
        public void ScrollLeftTo(
            Func<AppQuery, AppWebQuery> toQuery,
            Func<AppQuery, AppQuery> withinQuery = null,
            ScrollStrategy strategy = ScrollStrategy.Auto,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            bool withInertia = true,
            TimeSpan? timeout = null)
        {
            _errorReporting.With(() =>
                {
                    _sharedApp.LogWithToQueryWithinQuery("Scrolling left to", toQuery, withinQuery);

                    _gestures.ScrollTo(_sharedApp.Expand(toQuery), _sharedApp.ExpandIfNotNull(withinQuery),
                        ScrollDirection.Left, strategy, swipePercentage, swipeSpeed, withInertia, timeout);
                }, new object[] { toQuery, withinQuery, strategy, swipePercentage, swipeSpeed, withInertia, timeout });
        }

        /// <summary>
        /// Scroll right until an element that matches the <c>toQuery</c> is shown on the screen.
        /// </summary>
        /// <param name="toQuery">Entry point for the fluent API to specify the element to bring on screen.</param>
        /// <param name="withinQuery">Entry point for the fluent API to specify what element to scroll within.</param>
        /// <param name="strategy">Strategy for scrolling element.</param>
        /// <param name="swipePercentage">
        /// How far across the element to swipe (from 0.0 to 1.0).  Ignored for programmatic scrolling.
        /// </param>
        /// <param name="swipeSpeed">The speed of the gesture.  Ignored for programmatic scrolling.</param>
        /// <param name="withInertia">Whether swipes should cause inertia.  Ignored for programmatic scrolling.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before failing.</param>
        public void ScrollRightTo(
            Func<AppQuery, AppQuery> toQuery,
            Func<AppQuery, AppQuery> withinQuery = null,
            ScrollStrategy strategy = ScrollStrategy.Auto,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            bool withInertia = true,
            TimeSpan? timeout = null)
        {
            _errorReporting.With(() =>
                {
                    _sharedApp.LogWithToQueryWithinQuery("Scrolling right to", toQuery, withinQuery);

                    _gestures.ScrollTo(_sharedApp.Expand(toQuery), _sharedApp.ExpandIfNotNull(withinQuery),
                        ScrollDirection.Right, strategy, swipePercentage, swipeSpeed, withInertia, timeout);
                }, new object[] { toQuery, withinQuery, strategy, swipePercentage, swipeSpeed, withInertia, timeout });
        }

        /// <summary>
        /// Scroll right until an element that matches the <c>toQuery</c> is shown on the screen.
        /// </summary>
        /// <param name="toQuery">Entry point for the fluent API to specify the element to bring on screen.</param>
        /// <param name="withinQuery">Entry point for the fluent API to specify what element to scroll within.</param>
        /// <param name="strategy">Strategy for scrolling element.</param>
        /// <param name="swipePercentage">
        /// How far across the element to swipe (from 0.0 to 1.0).  Ignored for programmatic scrolling.
        /// </param>
        /// <param name="swipeSpeed">The speed of the gesture.  Ignored for programmatic scrolling.</param>
        /// <param name="withInertia">Whether swipes should cause inertia.  Ignored for programmatic scrolling.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before failing.</param>
        public void ScrollRightTo(
            Func<AppQuery, AppWebQuery> toQuery,
            Func<AppQuery, AppQuery> withinQuery = null,
            ScrollStrategy strategy = ScrollStrategy.Auto,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            bool withInertia = true,
            TimeSpan? timeout = null)
        {
            _errorReporting.With(() =>
                {
                    _sharedApp.LogWithToQueryWithinQuery("Scrolling right to", toQuery, withinQuery);

                    _gestures.ScrollTo(_sharedApp.Expand(toQuery), _sharedApp.ExpandIfNotNull(withinQuery),
                        ScrollDirection.Right, strategy, swipePercentage, swipeSpeed, withInertia, timeout);
                }, new object[] { toQuery, withinQuery, strategy, swipePercentage, swipeSpeed, withInertia, timeout });
        }

        /// <summary>
        /// Scroll the matching element so that its bottom child element is visible. If multiple elements are matched,
        /// the first one will be used.
        /// </summary>
        /// <param name="withinQuery">Entry point for the fluent API to specify the elements.</param>
        /// <param name="strategy">Strategy for scrolling element.</param>
        /// <param name="swipePercentage">
        /// How far across the element to swipe (from 0.0 to 1.0).  Ignored for programmatic scrolling.
        /// </param>
        /// <param name="swipeSpeed">The speed of the gesture.  Ignored for programmatic scrolling.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before failing.</param>
        public void ScrollToVerticalEnd(
            Func<AppQuery, AppQuery> withinQuery = null,
            ScrollStrategy strategy = ScrollStrategy.Auto,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            TimeSpan? timeout = null)
        {
            _errorReporting.With(() =>
                {
                    _sharedApp.LogWithWithinQuery("Scrolling to vertical end", withinQuery);

                    _gestures.ScrollToEnd(_sharedApp.ExpandIfNotNull(withinQuery), ScrollDirection.Down, strategy,
                        swipePercentage, swipeSpeed, timeout);
                }, new object[] { withinQuery, strategy, swipePercentage, swipeSpeed, timeout });
        }

        /// <summary>
        /// Scroll the matching element so that its top child element is visible. If multiple elements are matched,
        /// the first one will be used.
        /// <param name="withinQuery">Entry point for the fluent API to specify the elements.</param>
        /// <param name="strategy">Strategy for scrolling element.</param>
        /// <param name="swipePercentage">
        /// How far across the element to swipe (from 0.0 to 1.0).  Ignored for programmatic scrolling.
        /// </param>
        /// <param name="swipeSpeed">The speed of the gesture.  Ignored for programmatic scrolling.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before failing.</param>
        /// </summary>
        public void ScrollToVerticalStart(
            Func<AppQuery, AppQuery> withinQuery = null,
            ScrollStrategy strategy = ScrollStrategy.Auto,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            TimeSpan? timeout = null)
        {
            _errorReporting.With(() =>
                {
                    _sharedApp.LogWithWithinQuery("Scrolling to vertical start", withinQuery);

                    _gestures.ScrollToStart(_sharedApp.ExpandIfNotNull(withinQuery), ScrollDirection.Up, strategy,
                        swipePercentage, swipeSpeed, timeout);
                }, new object[] { withinQuery, strategy, swipePercentage, swipeSpeed, timeout });
        }

        /// <summary>
        /// Scroll the matching element so that its rightmost child element is visible. If multiple elements are
        /// matched, the first one will be used.
        /// </summary>
        /// <param name="withinQuery">Entry point for the fluent API to specify the elements.</param>
        /// <param name="strategy">Strategy for scrolling element.</param>
        /// <param name="swipePercentage">
        /// How far across the element to swipe (from 0.0 to 1.0).  Ignored for programmatic scrolling.
        /// </param>
        /// <param name="swipeSpeed">The speed of the gesture.  Ignored for programmatic scrolling.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before failing.</param>
        public void ScrollToHorizontalEnd(
            Func<AppQuery, AppQuery> withinQuery = null,
            ScrollStrategy strategy = ScrollStrategy.Auto,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            TimeSpan? timeout = null)
        {
            _errorReporting.With(() =>
                {
                    _sharedApp.LogWithWithinQuery("Scrolling to horizontal end", withinQuery);

                    _gestures.ScrollToEnd(_sharedApp.ExpandIfNotNull(withinQuery), ScrollDirection.Right, strategy,
                        swipePercentage, swipeSpeed, timeout);
                }, new object[] { withinQuery, strategy, swipePercentage, swipeSpeed, timeout });
        }

        /// <summary>
        /// Scroll the matching element so that its leftmost child element is visible. If multiple elements are
        /// matched, the first one will be used.
        /// <param name="withinQuery">Entry point for the fluent API to specify the elements.</param>
        /// <param name="strategy">Strategy for scrolling element.</param>
        /// <param name="swipePercentage">
        /// How far across the element to swipe (from 0.0 to 1.0).  Ignored for programmatic scrolling.
        /// </param>
        /// <param name="swipeSpeed">The speed of the gesture.  Ignored for programmatic scrolling.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before failing.</param>
        /// </summary>
        public void ScrollToHorizontalStart(
            Func<AppQuery, AppQuery> withinQuery = null,
            ScrollStrategy strategy = ScrollStrategy.Auto,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            TimeSpan? timeout = null)
        {
            _errorReporting.With(() =>
                {
                    _sharedApp.LogWithWithinQuery("Scrolling to horizontal start", withinQuery);

                    _gestures.ScrollToStart(_sharedApp.ExpandIfNotNull(withinQuery), ScrollDirection.Left, strategy,
                        swipePercentage, swipeSpeed, timeout);
                }, new object[] { withinQuery, strategy, swipePercentage, swipeSpeed, timeout });
        }


        /// <summary>
        /// Scroll until an element that matches the <c>toQuery</c> is shown on the screen.
        /// </summary>
        /// <param name="toQuery">Entry point for the fluent API to specify the element to bring on screen.</param>
        /// <param name="withinQuery">Entry point for the fluent API to specify what element to scroll within.</param>
        /// <param name="strategy">Strategy for scrolling element.</param>
        /// <param name="swipePercentage">
        /// How far across the element to swipe (from 0.0 to 1.0).  Ignored for programmatic scrolling.
        /// </param>
        /// <param name="swipeSpeed">The speed of the gesture.  Ignored for programmatic scrolling.</param>
        /// <param name="withInertia">Whether swipes should cause inertia.  Ignored for programmatic scrolling.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before failing.</param>
        public void ScrollTo(
            Func<AppQuery, AppQuery> toQuery,
            Func<AppQuery, AppQuery> withinQuery = null,
            ScrollStrategy strategy = ScrollStrategy.Auto,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            bool withInertia = true,
            TimeSpan? timeout = null)
        {
            _errorReporting.With(() =>
                {
                    _sharedApp.LogWithToQueryWithinQuery("Scrolling to", toQuery, withinQuery);

                    if (strategy != ScrollStrategy.Gesture)
                    {
                        Log.Info("Not using withinQuery for Programmatically and Auto strategy");
                    }

                    _gestures.ScrollTo(_sharedApp.Expand(toQuery), _sharedApp.ExpandIfNotNull(withinQuery), strategy,
                        swipePercentage, swipeSpeed, withInertia, timeout);
                }, new object[] { toQuery,  withinQuery, strategy, swipePercentage, swipeSpeed, withInertia, timeout });
        }

        /// <summary>
        /// Scroll until an element that matches the <c>toQuery</c> is shown on the screen.
        /// </summary>
        /// <param name="toQuery">Entry point for the fluent API to specify the element to bring on screen.</param>
        /// <param name="strategy">Strategy for scrolling element.</param>
        /// <param name="swipePercentage">
        /// How far across the screen to swipe (from 0.0 to 1.0).  Ignored for programmatic scrolling.
        /// </param>
        /// <param name="swipeSpeed">The speed of the gesture.  Ignored for programmatic scrolling.</param>
        /// <param name="withInertia">Whether swipes should cause inertia.  Ignored for programmatic scrolling.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before failing.</param>
        public void ScrollTo(
            Func<AppQuery, AppWebQuery> toQuery,
            ScrollStrategy strategy = ScrollStrategy.Auto,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            bool withInertia = true,
            TimeSpan? timeout = null)
        {
            _errorReporting.With(() =>
                {
                    _sharedApp.LogWithToQuery("Scrolling to", toQuery);

                    _gestures.ScrollTo(_sharedApp.Expand(toQuery), strategy, swipePercentage, swipeSpeed, withInertia,
                        timeout);

                }, new object[] { toQuery, strategy, swipePercentage, swipeSpeed, withInertia, timeout });
        }

        /// <summary>
        /// Presses the back button of the device.
        /// </summary>
        public void Back()
        {
            _errorReporting.With(() =>
                {
                    Log.Info("Pressing back button.");
                    _gestures.PressKey("KEYCODE_BACK");
                });
        }

        /// <summary>
        /// Presses the volume up button on the device.
        /// </summary>
        public void PressVolumeUp()
        {
            _errorReporting.With(() =>
                {
                    Log.Info("Pressing volume up button.");
                    _gestures.PressKey(0x00000018);
                });
        }

        /// <summary>
        /// Presses the volume down button on the device.
        /// </summary>
        public void PressVolumeDown()
        {
            _errorReporting.With(() =>
                {
                    Log.Info("Pressing volume down button.");
                    _gestures.PressKey(0x00000019);
                });
        }

        /// <summary>
        /// Presses the menu button of the device.
        /// </summary>
        public void PressMenu()
        {
            _errorReporting.With(() =>
                {
                    Log.Info("Pressing menu button.");
                    _gestures.PressKey("KEYCODE_MENU");
                });
        }

        /// <summary>
        /// Presses the enter key in the app.
        /// </summary>
        public void PressEnter()
        {
            _errorReporting.With(() =>
                {
                    Log.Info("Pressing enter.");
                    _gestures.PressKey("KEYCODE_ENTER");
                });
        }

        /// <summary>
        /// Presses the user action in the app.
        /// </summary>
        /// <param name="action">
        /// Use Action <c>action</c>, if null then the default action for the focused element is used.
        /// </param>
        public void PressUserAction(UserAction? action = null)
        {
            _errorReporting.With(() =>
                {
                    if (action.HasValue)
                    {
                        string actionAsString = GetUserActionAsString(action.Value);
                        Log.Info(string.Format("Pressing user action: {0}", actionAsString));
                        _gestures.PressUserAction(actionAsString);
                    }
                    else
                    {
                        Log.Info("Pressing default user action.");
                        _gestures.PressUserAction(null);
                    }
                });
        }

        string GetUserActionAsString(UserAction action)
        {
            switch (action)
            {
                case UserAction.Done:
                    return "done";
                case UserAction.Go:
                    return "go";
                case UserAction.Next:
                    return "next";
                case UserAction.None:
                    return "none";
                case UserAction.Normal:
                    return "normal";
                case UserAction.Previous:
                    return "previous";
                case UserAction.Search:
                    return "search";
                case UserAction.Send:
                    return "send";
                case UserAction.Unspecified:
                    return "unspecified";
                default:
                    throw new Exception("Unable to map action to string");
            }
        }

        /// <summary>
        /// Hides keyboard if present
        /// </summary>
        public void DismissKeyboard()
        {
            _errorReporting.With(() =>
                {
                    Log.Info("Attempting to dismiss keyboard");
                    _gestures.DismissKeyboard();
                });
        }


        /// <summary>
        /// Changes the current activity orientation to portrait mode.
        /// </summary>
        public void SetOrientationPortrait()
        {
            _errorReporting.With(() =>
                {
                    Log.Info("Setting orientation to portrait.");
                    _gestures.SetOrientationPortrait();
                });
        }

        /// <summary>
        /// Changes the current activity orientation to landscape mode.
        /// </summary>
        public void SetOrientationLandscape()
        {
            _errorReporting.With(() =>
                {
                    Log.Info("Setting orientation to landscape.");
                    _gestures.SetOrientationLandscape();
                });
        }

        /// <summary>
        /// Generic wait function that will repeatly call the <c>predicate</c> function until it returns <c>true</c>.
        /// Throws a <see cref="TimeoutException"/> if the predicate is not fullfilled within the time limit.
        /// </summary>
        /// <param name="predicate">Predicate function that should return <c>true</c> when waiting is complete.</param>
        /// <param name="timeoutMessage">The message used in the <see cref="TimeoutException"/>.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before failing.</param>
        /// <param name="retryFrequency">The <see cref="TimeSpan"/> to wait between each call to the predicate.</param>
        /// <param name="postTimeout">
        /// The final <see cref="TimeSpan"/> to wait after the predicate returns <c>true</c>.
        /// </param>
        public void WaitFor(Func<bool> predicate, string timeoutMessage = "Timed out waiting...",
            TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
        {
            _errorReporting.With(() => _waitForHelper.WaitFor(predicate, timeoutMessage, timeout, retryFrequency, postTimeout), new object[] {
                    predicate,
                    timeoutMessage,
                    timeout,
                    retryFrequency,
                    postTimeout
                });
        }

        /// <summary>
        /// Wait function that will repeatly query the app until a matching element is found. Throws a
        /// <see cref="TimeoutException"/> if no element is found within the time limit.
        /// </summary>
        /// <param name="query">Entry point for the fluent API to specify the element.</param>
        /// <param name="timeoutMessage">The message used in the <see cref="TimeoutException"/>.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before failing.</param>
        /// <param name="retryFrequency">The <see cref="TimeSpan"/> to wait between each query call to the app.</param>
        /// <param name="postTimeout">
        /// The final <see cref="TimeSpan"/> to wait after the element has been found.
        /// </param>
        /// <returns>An array representing the matched view objects.</returns>
        ///
        /// <example>
        /// <code>
        /// // Waiting for EditText with hint attribute "Your text here..."
        /// app.WaitForElement(c => c.Marked("Your text here..."));
        /// </code>
        /// </example>
        public AppResult[] WaitForElement(
            Func<AppQuery, AppQuery> query,
            string timeoutMessage = "Timed out waiting for element...",
            TimeSpan? timeout = null,
            TimeSpan? retryFrequency = null,
            TimeSpan? postTimeout = null)
        {
            return _errorReporting.With(() =>
                {
                    var appQuery = _sharedApp.Expand(query);
                    Log.Info("Waiting for element matching " + _sharedApp.ToCodeString(appQuery) + ".");

                    return _waitForHelper.WaitForAny(() => _gestures.Query(appQuery), timeoutMessage, timeout, retryFrequency, postTimeout);
                }, new object[] { query, timeoutMessage, timeout, retryFrequency, postTimeout });
        }

        /// <summary>
        /// Wait function that will repeatly query the app until a matching element is no longer found. Throws a
        /// <see cref="TimeoutException"/> if the element is visible at the end of the time limit.
        /// </summary>
        /// <param name="query">Entry point for the fluent API to specify the element.</param>
        /// <param name="timeoutMessage">The message used in the <see cref="TimeoutException"/>.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before failing.</param>
        /// <param name="retryFrequency">The <see cref="TimeSpan"/> to wait between each query call to the app.</param>
        /// <param name="postTimeout">
        /// The final <see cref="TimeSpan"/> to wait after the element is no longer visible.
        /// </param>
        public void WaitForNoElement(
            Func<AppQuery, AppQuery> query,
            string timeoutMessage = "Timed out waiting for no element...",
            TimeSpan? timeout = null,
            TimeSpan? retryFrequency = null,
            TimeSpan? postTimeout = null)
        {
            _errorReporting.With(() =>
                {
                    var appQuery = _sharedApp.Expand(query);
                    Log.Info("Waiting for no element matching " + _sharedApp.ToCodeString(appQuery) + ".");

                    _waitForHelper.WaitFor(() => !_gestures.Query(appQuery).Any(), timeoutMessage, timeout, retryFrequency, postTimeout);
                }, new object[] { query, timeoutMessage, timeout, retryFrequency, postTimeout });
        }

        /// <summary>
        /// Wait function that will repeatly query the app until a matching element is found. Throws a
        /// <see cref="TimeoutException"/> if no element is found within the time limit.
        /// </summary>
        /// <param name="query">Entry point for the fluent API to specify the element.</param>
        /// <param name="timeoutMessage">The message used in the <see cref="TimeoutException"/>.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before failing.</param>
        /// <param name="retryFrequency">The <see cref="TimeSpan"/> to wait between each query call to the app.</param>
        /// <param name="postTimeout">
        /// The final <see cref="TimeSpan"/> to wait after the element has been found.
        /// </param>
        /// <returns>An array representing the matched view objects.</returns>
        ///
        /// <example>
        /// <code>
        /// // Creating a lambda query
        /// Func&lt;AppQuery, AppWebQuery&gt; lambda = c =&gt; c.Class("WKWebView").Css("input");
        ///
        /// // Waiting for input field on the WebView
        /// app.WaitForElement(lambda,
        ///          "Didn't see input elements.",
        ///          new TimeSpan(0, 0, 0, 30, 0));
        /// </code>
        /// </example>
        public AppWebResult[] WaitForElement(
            Func<AppQuery, AppWebQuery> query,
            string timeoutMessage = "Timed out waiting for element...",
            TimeSpan? timeout = null,
            TimeSpan? retryFrequency = null,
            TimeSpan? postTimeout = null)
        {
            return _errorReporting.With(() =>
                {
                    var appQuery = _sharedApp.Expand(query);
                    Log.Info("Waiting for element matching " + _sharedApp.ToCodeString(appQuery) + ".");

                    return _waitForHelper.WaitForAny(() => _gestures.Query(appQuery), timeoutMessage, timeout, retryFrequency, postTimeout);
                }, new object[] { query, timeoutMessage, timeout, retryFrequency, postTimeout });
        }

        /// <summary>
        /// Wait function that will repeatly query the app until a matching element is no longer found. Throws a
        /// <see cref="TimeoutException"/> if the element is visible at the end of the time limit.
        /// </summary>
        /// <param name="query">Entry point for the fluent API to specify the element.</param>
        /// <param name="timeoutMessage">The message used in the <see cref="TimeoutException"/>.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before failing.</param>
        /// <param name="retryFrequency">The <see cref="TimeSpan"/> to wait between each query call to the app.</param>
        /// <param name="postTimeout">
        /// The final <see cref="TimeSpan"/> to wait after the element is no longer visible.
        /// </param>
        public void WaitForNoElement(
            Func<AppQuery, AppWebQuery> query,
            string timeoutMessage = "Timed out waiting for no element...",
            TimeSpan? timeout = null,
            TimeSpan? retryFrequency = null,
            TimeSpan? postTimeout = null)
        {
            _errorReporting.With(() =>
                {
                    var appQuery = _sharedApp.Expand(query);
                    Log.Info("Waiting for no element matching " + _sharedApp.ToCodeString(appQuery) + ".");

                    _waitForHelper.WaitFor(() => !_gestures.Query(appQuery).Any(), timeoutMessage, timeout, retryFrequency, postTimeout);
                }, new object[] { query, timeoutMessage, timeout, retryFrequency, postTimeout });
        }

        /// <summary>
        /// Takes a screenshot of the app in it's current state. When executed locally, the <see cref="FileInfo"/> returned is the file the screenshot has
        /// been saved to.
        /// </summary>
        /// <param name="title">The title of screenshot, used as step name.</param>
        /// <returns>The screenshot file.</returns>
        public FileInfo Screenshot(string title)
        {
            return _errorReporting.With(() =>
                {
                    return _screenshotTaker.Screenshot(title);
                }, new[] { title });
        }

        /// <summary>
        /// Contains helper methods for outputting the result of queries instead of resorting to
        /// <see cref="System.Console"/>.
        /// </summary>
        public AppPrintHelper Print
        {
            get { return new AppPrintHelper(this, _gestures); }
        }

        /// <summary>
        /// Starts an interactive REPL (Read-Eval-Print-Loop) for app exploration and pauses test execution until
        /// it is closed.
        /// </summary>
        public void Repl()
        {
            _errorReporting.With(() =>
                {
                    var deviceUrl = Device.DeviceUri.ToString();

                    var sdkPath = new AndroidSdkFinder().GetTools().GetSdkPath();

                    var replStarter = new ReplStarter();
                    replStarter.RunAndroidRepl(Assembly.GetExecutingAssembly(), deviceUrl, Device.DeviceIdentifier, sdkPath);
                });
        }

        /// <summary>
        /// Invokes a method on the app's main activity. For Xamarin apps, methods must be exposed using attributes
        /// as shown below.
        ///
        /// Android example in activity:
        ///
        /// <code>
        /// [Export]
        /// public string MyInvokeMethod(string arg)
        /// {
        ///     return "uitest";
        /// }
        /// </code>
        /// </summary>
        /// <param name="methodName">The name of the method to invoke.</param>
        /// <param name="argument">The argument to pass to the method.</param>
        /// <returns>The result of the invocation.</returns>
        public object Invoke(string methodName, object argument = null)
        {
            return _errorReporting.With(() =>
                {
                    return _gestures.Invoke(methodName, argument == null ? null : new object[] { argument });
                }, new[] { methodName, argument });
        }

        /// <summary>
        /// Invokes a method on the app's main activity. For Xamarin apps, methods must be exposed using attributes
        /// as shown below.
        ///
        /// Android example in activity:
        ///
        /// <code>
        /// [Export]
        /// public string MyInvokeMethod(string arg, string arg2)
        /// {
        ///     return "uitest";
        /// }
        /// </code>
        /// </summary>
        /// <param name="methodName">The name of the method to invoke.</param>
        /// <param name="arguments">An array of arguments to pass to the method.</param>
        /// <returns>The result of the invocation.</returns>
        public object Invoke(string methodName, object[] arguments)
        {
            return _errorReporting.With(() =>
                {
                    return _gestures.Invoke(methodName, arguments);
                }, new object[] { methodName, arguments });
        }

        /// <summary>
        /// Runtime information and control of the currently running device.
        /// </summary>
        IDevice IApp.Device
        {
            get { return _androidDevice; }
        }

        /// <summary>
        /// Runtime information and control of the currently running device.
        /// </summary>
        public AndroidDevice Device
        {
            get { return _androidDevice; }
        }

        /// <summary>
        /// Allows HTTP access to the test server running on the device.
        /// </summary>
        public ITestServer TestServer
        {
            get { return _testServer; }
        }

        /// <summary>
        /// Queries view objects using the fluent API. Defaults to only return view objects that are visible.
        /// </summary>
        /// <param name="marked">
        /// Marked selector to match. See <see cref="AppQuery.Marked" /> for more information.
        /// </param>
        /// <returns>An array representing the matched view objects.</returns>
        public AppResult[] Query(string marked)
        {
            return Query(_sharedApp.AsMarkedQuery(marked));
        }

        /// <summary>
        /// Highlights the results of the query by making them flash. Specify view elements using marked string.
        /// </summary>
        /// <param name="marked">
        /// Marked selector to match. See <see cref="AppQuery.Marked" /> for more information.
        /// </param>
        /// <returns>An array representing the matched view objects.</returns>
        public AppResult[] Flash(string marked)
        {
            return Flash(_sharedApp.AsMarkedQuery(marked));
        }

        /// <summary>
        /// Enters text into a matching element that supports it.
        /// </summary>
        /// <param name="marked">
        /// Marked selector to match. See <see cref="AppQuery.Marked" /> for more information.
        /// </param>
        /// <param name="text">The text to enter.</param>
        ///
        /// <example>
        /// <code>
        /// // Entering text by android:hint="My hint..." attribute in EditText
        /// app.EnterText("My hint...", "Text for input");
        /// </code>
        /// </example>
        public void EnterText(string marked, string text)
        {
            EnterText(_sharedApp.AsMarkedQuery(marked), text);
        }

        /// <summary>
        /// Clears text from a matching element that supports it.
        /// </summary>
        /// <param name="marked">
        /// Marked selector to match. See <see cref="AppQuery.Marked" /> for more information.
        /// </param>
        ///
        /// <example>
        /// <code>
        /// // Clearing text field by text it has
        /// app.ClearText("text in the EditText")
        /// </code>
        /// </example>
        public void ClearText(string marked)
        {
            ClearText(_sharedApp.AsMarkedQuery(marked));
        }

        /// <summary>
        /// Performs a tap / touch gesture on the matched element. If multiple elements are matched, the first one
        /// will be used.
        /// </summary>
        /// <param name="marked">
        /// Marked selector to match. See <see cref="AppQuery.Marked" /> for more information.
        /// </param>
        public void Tap(string marked)
        {
            Tap(_sharedApp.AsMarkedQuery(marked));
        }

        /// <summary>
        /// Performs a continuous touch gesture on the matched element. If multiple elements are matched, the first
        /// one will be used.
        /// </summary>
        /// <param name="marked">
        /// Marked selector to match. See <see cref="AppQuery.Marked" /> for more information.
        /// </param>
        public void TouchAndHold(string marked)
        {
            TouchAndHold(_sharedApp.AsMarkedQuery(marked));
        }

        /// <summary>
        /// Performs two quick tap / touch gestures on the matched element. If multiple elements are matched, the
        /// first one will be used.
        /// </summary>
        /// <param name="marked">
        /// Marked selector to match. See <see cref="AppQuery.Marked" /> for more information.
        /// </param>
        public void DoubleTap(string marked)
        {
            DoubleTap(_sharedApp.AsMarkedQuery(marked));
        }

        /// <summary>
        /// Performs a pinch gestures on the matched element to zoom the view in. If multiple elements are matched,
        /// the first one will be used.
        /// </summary>
        /// <param name="marked">
        /// Marked selector to match. See <see cref="AppQuery.Marked" /> for more information.
        /// </param>
        /// <param name="duration">The <see cref="TimeSpan"/> duration of the pinch gesture.</param>
        public void PinchToZoomIn(string marked, TimeSpan? duration = null)
        {
            PinchToZoomIn(_sharedApp.AsMarkedQuery(marked));
        }

        /// <summary>
        /// Performs a pinch gestures on the matched element to zoom the view out. If multiple elements are matched,
        /// the first one will be used.
        /// </summary>
        /// <param name="marked">
        /// Marked selector to match. See <see cref="AppQuery.Marked" /> for more information.
        /// </param>
        /// <param name="duration">The <see cref="TimeSpan"/> duration of the pinch gesture.</param>
        public void PinchToZoomOut(string marked, TimeSpan? duration = null)
        {
            PinchToZoomIn(_sharedApp.AsMarkedQuery(marked), duration);
        }

        /// <summary>
        /// Wait function that will repeatly query the app until a matching element is no longer found. Throws a
        /// <see cref="TimeoutException"/> if the element is visible at the end of the time limit.
        /// </summary>
        /// <param name="marked">
        /// Marked selector to match. See <see cref="AppQuery.Marked" /> for more information.
        /// </param>
        /// <param name="timeoutMessage">The message used in the <see cref="TimeoutException"/>.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before failing.</param>
        /// <param name="retryFrequency">The <see cref="TimeSpan"/> to wait between each query call to the app.</param>
        /// <param name="postTimeout">
        /// The final <see cref="TimeSpan"/> to wait after the element is no longer visible.
        /// </param>
        public void WaitForNoElement(
            string marked,
            string timeoutMessage = "Timed out waiting for no element...",
            TimeSpan? timeout = null,
            TimeSpan? retryFrequency = null,
            TimeSpan? postTimeout = null)
        {
            WaitForNoElement(_sharedApp.AsMarkedQuery(marked), timeoutMessage, timeout, retryFrequency, postTimeout);
        }

        /// <summary>
        /// Wait function that will repeatly query the app until a matching element is found. Throws a
        /// <see cref="TimeoutException"/> if no element is found within the time limit.
        /// </summary>
        /// <param name="marked">
        /// Marked selector to match. See <see cref="AppQuery.Marked" /> for more information.
        /// </param>
        /// <param name="timeoutMessage">The message used in the <see cref="TimeoutException"/>.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before failing.</param>
        /// <param name="retryFrequency">The <see cref="TimeSpan"/> to wait between each query call to the app.</param>
        /// <param name="postTimeout">
        /// The final <see cref="TimeSpan"/> to wait after the element has been found.
        /// </param>
        /// <returns>An array representing the matched view objects.</returns>
        public AppResult[] WaitForElement(
            string marked,
            string timeoutMessage = "Timed out waiting for element...",
            TimeSpan? timeout = null,
            TimeSpan? retryFrequency = null,
            TimeSpan? postTimeout = null)
        {
            return WaitForElement(_sharedApp.AsMarkedQuery(marked), timeoutMessage, timeout, retryFrequency, postTimeout);
        }

        /// <summary>
        /// Performs a long touch on an item, followed by dragging the item to a second item and dropping it
        /// </summary>
        /// <param name="fromMarked">
        /// Marked selector for the item to be dragged. See <see cref="AppQuery.Marked" /> for more information.
        /// </param>
        /// <param name="toMarked">
        /// Marked selector for the location for the item to be dropped. See <see cref="AppQuery.Marked" /> for more
        /// information.
        /// </param>
        /// <param name="placement">
        /// The placement of the drop (on top, above, below, left, right) relative to the to query
        /// </param>
        /// <param name="holdTime">Time to hold on the from query</param>
        /// <param name="hangTime">Time to hold above the to query</param>
        /// <param name="steps">The number of steps desired to drag the item, higher for a slower drag</param>
        /// <param name="afterStepAction">Action to perform after each step</param>
        public void DragAndDrop(
            string fromMarked,
            string toMarked,
            DropLocation placement = DropLocation.OnTop,
            TimeSpan? holdTime = null,
            TimeSpan? hangTime = null,
            int steps = 1,
            Action afterStepAction = null)
        {
            DragAndDrop(_sharedApp.AsMarkedQuery(fromMarked), _sharedApp.AsMarkedQuery(toMarked), placement, holdTime, hangTime, steps, afterStepAction);
        }

        /// <summary>
        /// Scrolls up on the first element matching query.
        /// </summary>
        /// <param name="withinMarked">
        /// Marked selector to select what element to scroll within. See <see cref="AppQuery.Marked" /> for more
        /// information.
        /// </param>
        /// <param name="strategy">Strategy for scrolling element.</param>
        /// <param name="swipePercentage">
        /// How far across the element to swipe (from 0.0 to 1.0).  Ignored for programmatic scrolling.
        /// </param>
        /// <param name="swipeSpeed">The speed of the gesture.  Ignored for programmatic scrolling.</param>
        /// <param name="withInertia">Whether swipes should cause inertia.  Ignored for programmatic scrolling.</param>
        public void ScrollUp(
            string withinMarked,
            ScrollStrategy strategy = ScrollStrategy.Auto,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            bool withInertia = true)
        {
            ScrollUp(_sharedApp.AsMarkedQuery(withinMarked), strategy, swipePercentage, swipeSpeed, withInertia);
        }

        /// <summary>
        /// Scrolls down on the first element matching query.
        /// </summary>
        /// <param name="withinMarked">
        /// Marked selector to select what element to scroll within. See <see cref="AppQuery.Marked" /> for more
        /// information.
        /// </param>
        /// <param name="strategy">Strategy for scrolling element.</param>
        /// <param name="swipePercentage">
        /// How far across the element to swipe (from 0.0 to 1.0).  Ignored for programmatic scrolling.
        /// </param>
        /// <param name="swipeSpeed">The speed of the gesture.  Ignored for programmatic scrolling.</param>
        /// <param name="withInertia">Whether swipes should cause inertia.  Ignored for programmatic scrolling.</param>
        public void ScrollDown(
            string withinMarked,
            ScrollStrategy strategy = ScrollStrategy.Auto,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            bool withInertia = true)
        {
            ScrollDown(_sharedApp.AsMarkedQuery(withinMarked), strategy, swipePercentage, swipeSpeed, withInertia);
        }

        /// <summary>
        /// Scrolls left on the first element matching query.
        /// </summary>
        /// <param name="withinMarked">
        /// Marked selector to select what element to scroll within. See <see cref="AppQuery.Marked" /> for more
        /// information.
        /// </param>
        /// <param name="strategy">Strategy for scrolling element.</param>
        /// <param name="swipePercentage">
        /// How far across the element to swipe (from 0.0 to 1.0).  Ignored for programmatic scrolling.
        /// </param>
        /// <param name="swipeSpeed">The speed of the gesture.  Ignored for programmatic scrolling.</param>
        /// <param name="withInertia">Whether swipes should cause inertia.  Ignored for programmatic scrolling.</param>
        public void ScrollLeft(
            string withinMarked,
            ScrollStrategy strategy = ScrollStrategy.Auto,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            bool withInertia = true)
        {
            ScrollLeft(_sharedApp.AsMarkedQuery(withinMarked), strategy, swipePercentage, swipeSpeed, withInertia);
        }

        /// <summary>
        /// Scrolls right on the first element matching query.
        /// </summary>
        /// <param name="withinMarked">
        /// Marked selector to select what element to scroll within. See <see cref="AppQuery.Marked" /> for more
        /// information.
        /// </param>
        /// <param name="strategy">Strategy for scrolling element.</param>
        /// <param name="swipePercentage">
        /// How far across the element to swipe (from 0.0 to 1.0).  Ignored for programmatic scrolling.
        /// </param>
        /// <param name="swipeSpeed">The speed of the gesture.  Ignored for programmatic scrolling.</param>
        /// <param name="withInertia">Whether swipes should cause inertia.  Ignored for programmatic scrolling.</param>
        public void ScrollRight(
            string withinMarked,
            ScrollStrategy strategy = ScrollStrategy.Auto,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            bool withInertia = true)
        {
            ScrollRight(_sharedApp.AsMarkedQuery(withinMarked), strategy, swipePercentage, swipeSpeed, withInertia);
        }

        /// <summary>
        /// Scroll up until an element that matches the <c>toMarked</c> is shown on the screen.
        /// </summary>
        /// <param name="toMarked">
        /// Marked selector to select what element to bring on screen. See <see cref="AppQuery.Marked" /> for more
        /// information.
        /// </param>
        /// <param name="withinMarked">
        /// Marked selector to select what element to scroll within. See <see cref="AppQuery.Marked" /> for more
        /// information.
        /// </param>
        /// <param name="strategy">Strategy for scrolling element.</param>
        /// <param name="swipePercentage">
        /// How far across the element to swipe (from 0.0 to 1.0).  Ignored for programmatic scrolling.
        /// </param>
        /// <param name="swipeSpeed">The speed of the gesture.  Ignored for programmatic scrolling.</param>
        /// <param name="withInertia">Whether swipes should cause inertia.  Ignored for programmatic scrolling.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before failing.</param>
        public void ScrollUpTo(
            string toMarked,
            string withinMarked = null,
            ScrollStrategy strategy = ScrollStrategy.Auto,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            bool withInertia = true,
            TimeSpan? timeout = null)
        {
            ScrollUpTo(_sharedApp.AsMarkedQuery(toMarked), _sharedApp.AsMarkedQuery(withinMarked), strategy,
                swipePercentage, swipeSpeed, withInertia, timeout);
        }

        /// <summary>
        /// Scroll up until an element that matches the <c>toMarked</c> is shown on the screen.
        /// </summary>
        /// <param name="toQuery">Entry point for the fluent API to specify the element to bring on screen.</param>
        /// <param name="withinMarked">
        /// Marked selector to select what element to scroll within. See <see cref="AppQuery.Marked" /> for more
        /// information.
        /// </param>
        /// <param name="strategy">Strategy for scrolling element.</param>
        /// <param name="swipePercentage">
        /// How far across the element to swipe (from 0.0 to 1.0).  Ignored for programmatic scrolling.
        /// </param>
        /// <param name="swipeSpeed">The speed of the gesture.  Ignored for programmatic scrolling.</param>
        /// <param name="withInertia">Whether swipes should cause inertia.  Ignored for programmatic scrolling.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before failing.</param>
        public void ScrollUpTo(
            Func<AppQuery, AppWebQuery> toQuery,
            string withinMarked,
            ScrollStrategy strategy = ScrollStrategy.Auto,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            bool withInertia = true,
            TimeSpan? timeout = null)
        {
            ScrollUpTo(toQuery, _sharedApp.AsMarkedQuery(withinMarked), strategy, swipePercentage, swipeSpeed,
                withInertia, timeout);
        }

        /// <summary>
        /// Scroll down until an element that matches the <c>toMarked</c> is shown on the screen.
        /// </summary>
        /// <param name="toMarked">
        /// Marked selector to select what element to bring on screen. See <see cref="AppQuery.Marked" /> for more
        /// information.
        /// </param>
        /// <param name="withinMarked">
        /// Marked selector to select what element to scroll within. See <see cref="AppQuery.Marked" /> for more
        /// information.
        /// </param>
        /// <param name="strategy">Strategy for scrolling element.</param>
        /// <param name="swipePercentage">
        /// How far across the element to swipe (from 0.0 to 1.0).  Ignored for programmatic scrolling.
        /// </param>
        /// <param name="swipeSpeed">The speed of the gesture.  Ignored for programmatic scrolling.</param>
        /// <param name="withInertia">Whether swipes should cause inertia.  Ignored for programmatic scrolling.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before failing.</param>
        public void ScrollDownTo(
            string toMarked,
            string withinMarked = null,
            ScrollStrategy strategy = ScrollStrategy.Auto,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            bool withInertia = true,
            TimeSpan? timeout = null)
        {
            ScrollDownTo(_sharedApp.AsMarkedQuery(toMarked), _sharedApp.AsMarkedQuery(withinMarked), strategy,
                swipePercentage, swipeSpeed, withInertia, timeout);
        }

        /// <summary>
        /// Scroll down until an element that matches the <c>toMarked</c> is shown on the screen.
        /// </summary>
        /// <param name="toQuery">Entry point for the fluent API to specify the element to bring on screen.</param>
        /// <param name="withinMarked">
        /// Marked selector to select what element to scroll within. See <see cref="AppQuery.Marked" /> for more
        /// information.
        /// </param>
        /// <param name="strategy">Strategy for scrolling element.</param>
        /// <param name="swipePercentage">
        /// How far across the element to swipe (from 0.0 to 1.0).  Ignored for programmatic scrolling.
        /// </param>
        /// <param name="swipeSpeed">The speed of the gesture.  Ignored for programmatic scrolling.</param>
        /// <param name="withInertia">Whether swipes should cause inertia.  Ignored for programmatic scrolling.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before failing.</param>
        public void ScrollDownTo(
            Func<AppQuery, AppWebQuery> toQuery,
            string withinMarked,
            ScrollStrategy strategy = ScrollStrategy.Auto,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            bool withInertia = true,
            TimeSpan? timeout = null)
        {
            ScrollDownTo(toQuery, _sharedApp.AsMarkedQuery(withinMarked), strategy, swipePercentage, swipeSpeed,
                withInertia, timeout);
        }

        /// <summary>
        /// Scroll left until an element that matches the <c>toMarked</c> is shown on the screen.
        /// </summary>
        /// <param name="toMarked">
        /// Marked selector to select what element to bring on screen. See <see cref="AppQuery.Marked" /> for more
        /// information.
        /// </param>
        /// <param name="withinMarked">
        /// Marked selector to select what element to scroll within. See <see cref="AppQuery.Marked" /> for more
        /// information.
        /// </param>
        /// <param name="strategy">Strategy for scrolling element.</param>
        /// <param name="swipePercentage">
        /// How far across the element to swipe (from 0.0 to 1.0).  Ignored for programmatic scrolling.
        /// </param>
        /// <param name="swipeSpeed">The speed of the gesture.  Ignored for programmatic scrolling.</param>
        /// <param name="withInertia">Whether swipes should cause inertia.  Ignored for programmatic scrolling.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before failing.</param>
        public void ScrollLeftTo(
            string toMarked,
            string withinMarked = null,
            ScrollStrategy strategy = ScrollStrategy.Auto,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            bool withInertia = true,
            TimeSpan? timeout = null)
        {
            ScrollLeftTo(_sharedApp.AsMarkedQuery(toMarked), _sharedApp.AsMarkedQuery(withinMarked), strategy,
                swipePercentage, swipeSpeed, withInertia, timeout);
        }

        /// <summary>
        /// Scroll left until an element that matches the <c>toMarked</c> is shown on the screen.
        /// </summary>
        /// <param name="toQuery">Entry point for the fluent API to specify the element to bring on screen.</param>
        /// <param name="withinMarked">
        /// Marked selector to select what element to scroll within. See <see cref="AppQuery.Marked" /> for more
        /// information.
        /// </param>
        /// <param name="strategy">Strategy for scrolling element.</param>
        /// <param name="swipePercentage">
        /// How far across the element to swipe (from 0.0 to 1.0).  Ignored for programmatic scrolling.
        /// </param>
        /// <param name="swipeSpeed">The speed of the gesture.  Ignored for programmatic scrolling.</param>
        /// <param name="withInertia">Whether swipes should cause inertia.  Ignored for programmatic scrolling.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before failing.</param>
        public void ScrollLeftTo(
            Func<AppQuery, AppWebQuery> toQuery,
            string withinMarked,
            ScrollStrategy strategy = ScrollStrategy.Auto,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            bool withInertia = true, TimeSpan? timeout = null)
        {
            ScrollLeftTo(toQuery, _sharedApp.AsMarkedQuery(withinMarked), strategy, swipePercentage, swipeSpeed,
                withInertia, timeout);
        }

        /// <summary>
        /// Scroll right until an element that matches the <c>toMarked</c> is shown on the screen.
        /// </summary>
        /// <param name="toMarked">
        /// Marked selector to select what element to bring on screen. See <see cref="AppQuery.Marked" /> for more
        /// information.
        /// </param>
        /// <param name="withinMarked">
        /// Marked selector to select what element to scroll within. See <see cref="AppQuery.Marked" /> for more
        /// information.
        /// </param>
        /// <param name="strategy">Strategy for scrolling element.</param>
        /// <param name="swipePercentage">
        /// How far across the element to swipe (from 0.0 to 1.0).  Ignored for programmatic scrolling.
        /// </param>
        /// <param name="swipeSpeed">The speed of the gesture.  Ignored for programmatic scrolling.</param>
        /// <param name="withInertia">Whether swipes should cause inertia.  Ignored for programmatic scrolling.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before failing.</param>
        public void ScrollRightTo(
            string toMarked,
            string withinMarked = null,
            ScrollStrategy strategy = ScrollStrategy.Auto,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            bool withInertia = true,
            TimeSpan? timeout = null)
        {
            ScrollRightTo(_sharedApp.AsMarkedQuery(toMarked), _sharedApp.AsMarkedQuery(withinMarked), strategy,
                swipePercentage, swipeSpeed, withInertia, timeout);
        }

        /// <summary>
        /// Scroll right until an element that matches the <c>toMarked</c> is shown on the screen.
        /// </summary>
        /// <param name="toQuery">Entry point for the fluent API to specify the element to bring on screen.</param>
        /// <param name="withinMarked">
        /// Marked selector to select what element to scroll within. See <see cref="AppQuery.Marked" /> for more
        /// information.
        /// </param>
        /// <param name="strategy">Strategy for scrolling element.</param>
        /// <param name="swipePercentage">
        /// How far across the element to swipe (from 0.0 to 1.0).  Ignored for programmatic scrolling.
        /// </param>
        /// <param name="swipeSpeed">The speed of the gesture.  Ignored for programmatic scrolling.</param>
        /// <param name="withInertia">Whether swipes should cause inertia.  Ignored for programmatic scrolling.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before failing.</param>
        public void ScrollRightTo(
            Func<AppQuery, AppWebQuery> toQuery,
            string withinMarked,
            ScrollStrategy strategy = ScrollStrategy.Auto,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            bool withInertia = true,
            TimeSpan? timeout = null)
        {
            ScrollRightTo(toQuery, _sharedApp.AsMarkedQuery(withinMarked), strategy, swipePercentage, swipeSpeed,
                withInertia, timeout);
        }

        /// <summary>
        /// Scroll the matching element so that its bottom child element is visible. If multiple elements are matched,
        /// the first one will be used.
        /// </summary>
        /// <param name="withinMarked">
        /// Marked selector to select what element to scroll within. See <see cref="AppQuery.Marked" /> for more
        /// information.
        /// </param>
        /// <param name="strategy">Strategy for scrolling element.</param>
        /// <param name="swipePercentage">
        /// How far across the element to swipe (from 0.0 to 1.0).  Ignored for programmatic scrolling.
        /// </param>
        /// <param name="swipeSpeed">The speed of the gesture.  Ignored for programmatic scrolling.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before failing.</param>
        public void ScrollToVerticalEnd(
            string withinMarked,
            ScrollStrategy strategy = ScrollStrategy.Auto,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            TimeSpan? timeout = null)
        {
            ScrollToVerticalEnd(_sharedApp.AsMarkedQuery(withinMarked), strategy, swipePercentage, swipeSpeed,
                timeout);
        }

        /// <summary>
        /// Scroll the matching element so that its top child element is visible. If multiple elements are matched,
        /// the first one will be used.
        /// <param name="withinMarked">
        /// Marked selector to select what element to scroll within. See <see cref="AppQuery.Marked" /> for more
        /// information.
        /// </param>
        /// <param name="strategy">Strategy for scrolling element.</param>
        /// <param name="swipePercentage">
        /// How far across the element to swipe (from 0.0 to 1.0).  Ignored for programmatic scrolling.
        /// </param>
        /// <param name="swipeSpeed">The speed of the gesture.  Ignored for programmatic scrolling.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before failing.</param>
        /// </summary>
        public void ScrollToVerticalStart(
            string withinMarked,
            ScrollStrategy strategy = ScrollStrategy.Auto,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            TimeSpan? timeout = null)
        {
            ScrollToVerticalStart(_sharedApp.AsMarkedQuery(withinMarked), strategy, swipePercentage, swipeSpeed,
                timeout);
        }

        /// <summary>
        /// Scroll the matching element so that its rightmost child element is visible. If multiple elements are
        /// matched, the first one will be used.
        /// </summary>
        /// <param name="withinMarked">
        /// Marked selector to select what element to scroll within. See <see cref="AppQuery.Marked" /> for more
        /// information.
        /// </param>
        /// <param name="strategy">Strategy for scrolling element.</param>
        /// <param name="swipePercentage">
        /// How far across the element to swipe (from 0.0 to 1.0).  Ignored for programmatic scrolling.
        /// </param>
        /// <param name="swipeSpeed">The speed of the gesture.  Ignored for programmatic scrolling.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before failing.</param>
        public void ScrollToHorizontalEnd(
            string withinMarked,
            ScrollStrategy strategy = ScrollStrategy.Auto,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            TimeSpan? timeout = null)
        {
            ScrollToHorizontalEnd(_sharedApp.AsMarkedQuery(withinMarked), strategy, swipePercentage, swipeSpeed,
                timeout);
        }

        /// <summary>
        /// Scroll the matching element so that its leftmost child element is visible. If multiple elements are
        /// matched, the first one will be used.
        /// <param name="withinMarked">
        /// Marked selector to select what element to scroll within. See <see cref="AppQuery.Marked" /> for more
        /// information.
        /// </param>
        /// <param name="strategy">Strategy for scrolling element.</param>
        /// <param name="swipePercentage">
        /// How far across the element to swipe (from 0.0 to 1.0).  Ignored for programmatic scrolling.
        /// </param>
        /// <param name="swipeSpeed">The speed of the gesture.  Ignored for programmatic scrolling.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before failing.</param>
        /// </summary>
        public void ScrollToHorizontalStart(
            string withinMarked,
            ScrollStrategy strategy = ScrollStrategy.Auto,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            TimeSpan? timeout = null)
        {
            ScrollToHorizontalStart(_sharedApp.AsMarkedQuery(withinMarked), strategy, swipePercentage, swipeSpeed,
                timeout);
        }

        /// <summary>
        /// Scroll until an element that matches the <c>toMarked</c> is shown on the screen.
        /// </summary>
        /// <param name="toMarked">
        /// Marked selector to select what element to bring on screen. See <see cref="AppQuery.Marked" /> for more
        /// information.
        /// </param>
        /// <param name="withinMarked">
        /// Marked selector to select what element to scroll within. See <see cref="AppQuery.Marked" /> for more
        /// information.
        /// </param>
        /// <param name="strategy">Strategy for scrolling element.</param>
        /// <param name="swipePercentage">
        /// How far across the element to swipe (from 0.0 to 1.0).  Ignored for programmatic scrolling.
        /// </param>
        /// <param name="swipeSpeed">The speed of the gesture.  Ignored for programmatic scrolling.</param>
        /// <param name="withInertia">Whether swipes should cause inertia.  Ignored for programmatic scrolling.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before failing.</param>
        public void ScrollTo(
            string toMarked,
            string withinMarked = null,
            ScrollStrategy strategy = ScrollStrategy.Auto,
            double swipePercentage = UITestConstants.DefaultSwipePercentage,
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed,
            bool withInertia = true,
            TimeSpan? timeout = null)
        {
            ScrollTo(_sharedApp.AsMarkedQuery(toMarked), _sharedApp.AsMarkedQuery(withinMarked), strategy,
                swipePercentage, swipeSpeed, withInertia, timeout);
        }

        /// <summary>
        /// Sets the value of a slider element that matches <c>marked</c>.
        /// </summary>
        /// <param name="marked">Marked selector of the slider element to update.</param>
        /// <param name="value">The value to set the slider to.</param>
        public void SetSliderValue(string marked, double value)
        {
            SetSliderValue(_sharedApp.AsMarkedQuery(marked), value);
        }

        /// <summary>
        /// Sets the value of a slider element that matches <c>query</c>.
        /// </summary>
        /// <param name="query">Entry point for the fluent API to specify the element.</param>
        /// <param name="value">The value to set the slider to.</param>
        public void SetSliderValue(Func<AppQuery, AppQuery> query, double value)
        {
            _errorReporting.With(() =>
                {
                    AppQuery appQuery = _sharedApp.Expand(query);
                    var results = _gestures.QueryGestureWait(appQuery);
                    _sharedApp.FirstWithLog(results, appQuery);

                    var invokeQuery = appQuery.Invoke("setProgress", (int)Math.Round(value));

                    Log.Info("Updating the value of a slider element");

                    _gestures.Query(invokeQuery);
                }, new object[] { query });
        }
    }
}
