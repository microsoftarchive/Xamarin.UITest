using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Xamarin.UITest.Configuration;
using Xamarin.UITest.Queries;
using Xamarin.UITest.Repl;
using Xamarin.UITest.Shared;
using Xamarin.UITest.Shared.Artifacts;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Http;
using Xamarin.UITest.Shared.iOS;
using Xamarin.UITest.Shared.Logging;
using Xamarin.UITest.Shared.Queries;
using Xamarin.UITest.Shared.Resources;
using Xamarin.UITest.Shared.Screenshots;
using Xamarin.UITest.Utils;
using Xamarin.UITest.Utils.SpecFlow;
using Xamarin.UITest.XDB;
using Xamarin.UITest.XDB.Services;

namespace Xamarin.UITest.iOS
{
    /// <summary>
    /// Represents a running iOS application.
    /// </summary>
    public class iOSApp : IApp
    {
        // See https://github.com/calabash/calabash-ios-server/blob/develop/CHANGELOG.md
        readonly VersionNumber _minSupportedCalabashServer = new VersionNumber("0.21.1");
        readonly EmbeddedResourceLoader _resourceLoader = new EmbeddedResourceLoader();
        readonly WaitForHelper _waitForHelper;
        readonly DeviceConnectionInfo _deviceConnectionInfo;
        readonly IScreenshotTaker _screenshotTaker;
        readonly iOSGestures _gestures;
        readonly IExecutor _executor;
        readonly ErrorReporting _errorReporting;
        readonly iOSDevice _iosDevice;
        readonly ITestServer _testServer;
        readonly SharedApp _sharedApp;

        /// <summary>
        /// Main entry point for creating iOS applications. Should not be called directly
        /// but instead be invoked through the use of <see cref="ConfigureApp"/>.
        /// </summary>
        /// <param name="appConfiguration">
        /// The app configuration. Should be generated from <see cref="ConfigureApp"/>.
        /// </param>
        public iOSApp(IiOSAppConfiguration appConfiguration) : this(appConfiguration, null)
        {
        }

        internal iOSApp(IiOSAppConfiguration appConfiguration, IExecutor executor)
        {
            var factory = new DefaultiOSFactory();

            SharedApp.BuildLogger(appConfiguration.Debug, appConfiguration.LogDirectory);

            Log.VerifyInitialized();

            var processRunner = factory.BuildProcessRunner();
            _executor = executor ?? factory.BuildExecutor(processRunner);

            if (Shared.Processes.Platform.Instance.IsWindows)
            {
                throw new Exception("iOS tests are not supported on Windows.");
            }

            Log.Info("iOS test running Xamarin.UITest version: " + Assembly.GetExecutingAssembly().GetName().Version);

            ArtifactCleaner.PotentialCleanUp();

            if (!string.IsNullOrWhiteSpace(value: appConfiguration.DeviceAgentPathOverride))
            {
                IEnvironmentService environmentService = XdbServices.GetRequiredService<IEnvironmentService>();
                environmentService.DeviceAgentPathOverride = appConfiguration.DeviceAgentPathOverride;
            }

            if (!string.IsNullOrWhiteSpace(value: appConfiguration.IDBPathOverride))
            {
                IEnvironmentService environmentService = XdbServices.GetRequiredService<IEnvironmentService>();
                environmentService.IDBPathOverride = appConfiguration.IDBPathOverride;
            }

            _waitForHelper = new WaitForHelper(appConfiguration.WaitTimes.WaitForTimeout);

            var httpClient = new HttpClient(appConfiguration.DeviceUri);
            _testServer = new SharedTestServer(httpClient);

            if (appConfiguration.EnableScreenshots)
            {
                _screenshotTaker = new HttpScreenshotTaker(httpClient);
            }
            else
            {
                Log.Info("Skipping local screenshots. Can be enabled with EnableScreenshots() when configuring app.");
                _screenshotTaker = new NullScreenshotTaker();
            }

            var appLauncher = new iOSAppLauncher(processRunner, _executor, _resourceLoader);

            if (appConfiguration.StartAction == StartAction.ConnectToApp)
            {
                var connectResult = appLauncher.ConnectToApp(appConfiguration, httpClient);
                ValidateCalabashServerVersion(connectResult.CalabashDevice.CalabashServerVersion);
                _deviceConnectionInfo = connectResult.DeviceConnectionInfo;

                _gestures = new iOSGestures(
                    _deviceConnectionInfo,
                    connectResult.CalabashDevice,
                    _waitForHelper,
                    appConfiguration.WaitTimes);
                
                _sharedApp = new SharedApp(QueryPlatform.iOS, _gestures);
                _errorReporting = _sharedApp.ErrorReporting;
                _iosDevice = CreateiOSDevice(appConfiguration, connectResult, _gestures);
                return;
            }

            appConfiguration.Verify();

            var launchResult = appLauncher.LaunchApp(
                appConfiguration, 
                httpClient);
            _deviceConnectionInfo = launchResult.DeviceConnectionInfo;
            ValidateCalabashServerVersion(launchResult.CalabashDevice.CalabashServerVersion);

            _gestures = new iOSGestures(
                _deviceConnectionInfo,
                launchResult.CalabashDevice,
                _waitForHelper,
                appConfiguration.WaitTimes);

            _sharedApp = new SharedApp(QueryPlatform.iOS, _gestures);
            _errorReporting = _sharedApp.ErrorReporting;
            _iosDevice = CreateiOSDevice(appConfiguration, launchResult, _gestures);

            if (!appConfiguration.DisableSpecFlowIntegration)
            {
                SpecFlowIntegrator.CheckForSpecFlowAndLoadIntegration(launchResult.ArtifactFolder);
            }

            WaitFor(() => _gestures.Query(
                _sharedApp.Expand()).Any(), 
                timeout: appConfiguration.WaitTimes.WaitForTimeout);
        }

        iOSDevice CreateiOSDevice(
            IiOSAppConfiguration appConfiguration,
            LaunchAppResult launchResult, 
            iOSGestures gestures)
        {
            return new iOSDevice(
                appConfiguration.DeviceUri,
                launchResult.CalabashDevice, 
                _deviceConnectionInfo.DeviceIdentifier, 
                gestures);
        }

        /// <summary>
        /// Queries view objects using the fluent API. Defaults to only return view objects that are visible.
        /// </summary>
        /// <param name="query">
        /// Entry point for the fluent API to specify the element. If left as <c>null</c> returns all visible view 
        /// objects.
        /// </param>
        /// <returns>An array representing the matched view objects.</returns>
        public AppResult[] Query(Func<AppQuery, AppQuery> query = null)
        {
            return _errorReporting.With(() =>
                {
                    AppQuery appQuery = _sharedApp.Expand(query);
                    var results = _gestures.Query(appQuery);

                    Log.Info($"Query for {_sharedApp.ToCodeString(appQuery)} gave {results.Length} results.");

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
        public AppWebResult[] Query(Func<AppQuery, AppWebQuery> query)
        {
            return _errorReporting.With(() =>
                {
                    query = query ?? (x => x.Css("*"));
                    AppWebQuery appWebQuery = _sharedApp.Expand(query);
                    var results = _gestures.Query(appWebQuery);

                    Log.Info($"Query for {_sharedApp.ToCodeString(appWebQuery)} gave {results.Length} results.");

                    return results;
                }, new[] { query });
        }

        /// <summary>
        /// Queries properties on view objects using the fluent API. 
        /// </summary>
        /// <param name="query">Entry point for the fluent API to specify the type of the property.</param>
        /// <typeparam name="T">The type of the property.</typeparam>
        public T[] Query<T>(Func<AppQuery, AppTypedSelector<T>> query)
        {
            return _errorReporting.With(() =>
                {
                    AppTypedSelector<T> selector = _sharedApp.Expand(query);
                    var results = _gestures.Query(selector);

                    Log.Info($"Query for {_sharedApp.ToCodeString(selector)} gave {results.Length} results.");

                    return results;
                }, new[] { query });
        }

        /// <summary>
        /// Queries view objects using the fluent API. Defaults to only return view objects that are visible.
        /// </summary>
        /// <param name="query">
        /// Entry point for the fluent API to specify the element. If left as <c>null</c> returns all visible view
        /// objects.
        /// </param>
        /// <returns>An array representing the matched view objects.</returns>
        public string[] Query(Func<AppQuery, InvokeJSAppQuery> query)
        {
            return _errorReporting.With(() =>
                {
                    IInvokeJSAppQuery invokeJsAppQuery = query(new AppQuery(QueryPlatform.iOS));
                    var results = _gestures.InvokeJS(invokeJsAppQuery);
                    return results;
                }, new[] { query });
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
        public AppResult[] Flash(Func<AppQuery, AppQuery> query = null)
        {
            return _errorReporting.With(() =>
                {
                    AppQuery appQuery = _sharedApp.Expand(query);
                    var results = _gestures.Flash(appQuery);

                    Log.Info(
                        $"Flashing query for {_sharedApp.ToCodeString(appQuery)} gave {results.Length} results.");

                    return results;

                }, new[] { query });
        }

        /// <summary>
        /// Enters text into the currently focused element. Will fail if no keyboard is visible.
        /// </summary>
        /// <param name="text">The text to enter.</param>
        public void EnterText(string text)
        {
            _errorReporting.With(() =>
                {
                    _gestures.WaitForNoneAnimatingOrElapsed();

                    if (!_gestures.IsKeyboardVisible())
                    {
                        throw new Exception("No keyboard is visible for text entry.");
                    }

                    Log.Info(string.Format("Entering text '{0}'.", text));
                    _gestures.EnterText(new SingleQuoteEscapedString(text));
                }, new[] { text });
        }

        /// <summary>
        /// Enters text into a matching element that supports it. 
        /// </summary>
        /// <param name="query">Entry point for the fluent API to specify the element.</param>
        /// <param name="text">The text to enter.</param>
        public void EnterText(Func<AppQuery, AppQuery> query, string text)
        {
            _errorReporting.With(() =>
                {
                    AppQuery appQuery = _sharedApp.Expand(query);
                    var results = _gestures.QueryGestureWait(appQuery);

                    if (!results.Any())
                    {
                        throw new Exception(
                            $"Unable to enter text. Query for {_sharedApp.ToCodeString(appQuery)} gave no results.");
                    }

                    var centerX = results.First().Rect.CenterX;
                    var centerY = results.First().Rect.CenterY;

                    if (results.Length == 1)
                    {
                        Log.Info(
                            string.Format("Entering text '{3}' in element matching {0} at coordinates [ {1}, {2} ].",
                                _sharedApp.ToCodeString(appQuery), centerX, centerY, text));
                    }
                    else
                    {
                        Log.Info(
                            string.Format(
                                "Entering text '{4}' in first element ({1} total) matching {0} at coordinates [ {2}, {3} ]. ",
                                _sharedApp.ToCodeString(appQuery), results.Length, centerX, centerY, text));
                    }

                    _gestures.TapCoordinates(centerX, centerY);
                    _waitForHelper.WaitFor(_gestures.IsKeyboardVisible, "Timed out waiting for keyboard.",
                        TimeSpan.FromSeconds(5));

                    _gestures.EnterText(new SingleQuoteEscapedString(text));
                }, new object[] { query, text });

        }

        /// <summary>
        /// Enters text into a matching element that supports it. 
        /// </summary>
        /// <param name="query">Entry point for the fluent API to specify the element.</param>
        /// <param name="text">The text to enter.</param>
        public void EnterText(Func<AppQuery, AppWebQuery> query, string text)
        {
            _errorReporting.With(() =>
                {
                    AppWebQuery appWebQuery = _sharedApp.Expand(query);
                    var results = _gestures.QueryGestureWait(appWebQuery);

                    if (!results.Any())
                    {
                        throw new Exception(string.Format("Unable to enter text. Query for {0} gave no results.",
                            _sharedApp.ToCodeString(appWebQuery)));
                    }

                    var centerX = results.First().Rect.CenterX;
                    var centerY = results.First().Rect.CenterY;

                    if (results.Length == 1)
                    {
                        Log.Info(
                            string.Format("Entering text '{3}' in element matching {0} at coordinates [ {1}, {2} ].",
                                _sharedApp.ToCodeString(appWebQuery), centerX, centerY, text));
                    }
                    else
                    {
                        Log.Info(
                            string.Format(
                                "Entering text '{4}' in first element ({1} total) matching {0} at coordinates [ {2}, {3} ]. ",
                                _sharedApp.ToCodeString(appWebQuery), results.Length, centerX, centerY, text));
                    }

                    _gestures.TapCoordinates(centerX, centerY);
                    _waitForHelper.WaitFor(_gestures.IsKeyboardVisible, "Timed out waiting for keyboard.",
                        TimeSpan.FromSeconds(5));

                    _gestures.EnterText(new SingleQuoteEscapedString(text));
                }, new object[] { query, text });
        }

        /// <summary>
        /// Clears text from a matching element that supports it. 
        /// </summary>
        /// <param name="query">Entry point for the fluent API to specify the element.</param>
        public void ClearText(Func<AppQuery, AppQuery> query)
        {
            _errorReporting.With(() =>
            {
                var appQuery = _sharedApp.Expand(query);
                var results = _gestures.QueryGestureWait(appQuery);

                if (!results.Any())
                {
                    throw new Exception(string.Format("Unable to clear text. Query for {0} gave no results.",
                        _sharedApp.ToCodeString(appQuery)));
                }

                if (!_gestures.IsKeyboardVisible())
                {
                    var centerX = results.First().Rect.CenterX;
                    var centerY = results.First().Rect.CenterY;
                    _gestures.TapCoordinates(centerX, centerY);
                    _waitForHelper.WaitFor(
                        _gestures.IsKeyboardVisible,
                        "Timed out waiting for keyboard.",
                        TimeSpan.FromSeconds(5)
                    );
                }
                else
                {
                    var firstResponder = _gestures.Query(new AppQuery(QueryPlatform.iOS)
                                                   .Property("isFirstResponder", true))
                                                   .First();
                    // Keyboard is visible, but there is no firstResponder.
                    // This unlikely case is not something we can handle.
                    if (firstResponder == null)
                    {
                        throw new Exception("A keyboard is visible, but no view is the first responder");
                    }

                    var responderNeedsToChange = true;

                    foreach (var match in results)
                    {
                        if (firstResponder.Description.Equals(match.Description))
                        {
                            // Query references view that is already the first responder
                            responderNeedsToChange = false;
                            break;
                        }
                    }

                    if (responderNeedsToChange)
                    {
                        var centerX = results.First().Rect.CenterX;
                        var centerY = results.First().Rect.CenterY;
                        _gestures.TapCoordinates(centerX, centerY);
                        _waitForHelper.WaitFor(_gestures.IsKeyboardVisible,
                                               "Timed out waiting for keyboard.",
                                               TimeSpan.FromSeconds(5));
                    }
                }

                Log.Info("Clearing text in element");
                _sharedApp.FirstWithLog(results, appQuery);

                ClearText();
            }, new object[] { query });
        }

        /// <summary>
        /// Clears text from a matching element that supports it. 
        /// </summary>
        /// <param name="query">Entry point for the fluent API to specify the element.</param>
        public void ClearText(Func<AppQuery, AppWebQuery> query)
        {
            _errorReporting.With(() =>
                {
                    AppWebQuery appQuery = _sharedApp.Expand(query);
                    var results = _gestures.QueryGestureWait(appQuery);

                    Log.Info("Clearing text in element");
                    _sharedApp.FirstWithLog(results, appQuery);

                    _gestures.ClearText(appQuery);
                }, new object[] { query });
        }

        /// <summary>
        /// Clears text from the currently focused element.
        /// </summary>
        public void ClearText()
        {
            _errorReporting.With(() =>
                {
                    if (!_gestures.IsKeyboardVisible())
                    {
                        throw new Exception("No keyboard is shown, unable to clear text.");
                    }

                    var result = _deviceConnectionInfo.Connection.ClearText();
                    var responseJObject = JObject.Parse(result.Contents);

                    if ((string) responseJObject["outcome"] != "SUCCESS")
                    {
                        throw new Exception("Failed to clear text: " + responseJObject["reason"]);
                    }
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
                    _gestures.PressEnter();
                });
        }

        /// <summary>
        /// Changes the device orientation to portrait mode.
        /// </summary>
        public void SetOrientationPortrait()
        {
            _errorReporting.With(() =>
                {
                    _gestures.WaitForNoneAnimatingOrElapsed();
                    Log.Info("Setting orientation to portrait.");
                    _gestures.SetOrientationPortrait();
                });
        }



        /// <summary>
        /// Changes the device orientation to landscape mode.
        /// </summary>
        public void SetOrientationLandscape()
        {
            _errorReporting.With(() =>
                {
                    _gestures.WaitForNoneAnimatingOrElapsed();
                    Log.Info("Setting orientation to landscape.");
                    _gestures.SetOrientationLandscape();
                });
        }

        /// <summary>
        /// Performs a tap / touch gesture on the matched element. If multiple elements are matched, the first one will
        /// be used.
        /// </summary>
        /// <param name="query">Entry point for the fluent API to specify the element.</param>
        public void Tap(Func<AppQuery, AppQuery> query)
        {
            _errorReporting.With(() =>
                {
                    AppQuery appQuery = _sharedApp.Expand(query);
                    var results = _gestures.QueryGestureWait(appQuery);

                    if (!results.Any())
                    {
                        throw new Exception(
                            $"Unable to tap element. Query for {_sharedApp.ToCodeString(appQuery)} gave no results.");
                    }

                    var centerX = results.First().Rect.CenterX;
                    var centerY = results.First().Rect.CenterY;

                    if (results.Length == 1)
                    {
                        Log.Info(string.Format("Tapping element matching {0} at coordinates [ {1}, {2} ].",
                                _sharedApp.ToCodeString(appQuery), centerX, centerY));
                    }
                    else
                    {
                        Log.Info(
                            string.Format("Tapping first element ({1} total) matching {0} at coordinates [ {2}, {3} ]. ",
                                _sharedApp.ToCodeString(appQuery), results.Length, centerX, centerY));
                    }

                    _gestures.TapCoordinates(centerX, centerY);
                }, new object[] { query });
        }

        /// <summary>
        /// Performs a tap / touch gesture on the matched element. If multiple elements are matched, the first one will
        /// be used.
        /// </summary>
        /// <param name="query">Entry point for the fluent API to specify the element.</param>
        public void Tap(Func<AppQuery, AppWebQuery> query)
        {
            _errorReporting.With(() =>
                {
                    AppWebQuery appWebQuery = _sharedApp.Expand(query);
                    var results = _gestures.QueryGestureWait(appWebQuery);

                    if (!results.Any())
                    {
                        throw new Exception(string.Format("Unable to tap element. Query for {0} gave no results.", 
                            _sharedApp.ToCodeString(appWebQuery)));
                    }

                    var centerX = results.First().Rect.CenterX;
                    var centerY = results.First().Rect.CenterY;

                    if (results.Length == 1)
                    {
                        Log.Info(string.Format("Tapping element matching {0} at coordinates [ {1}, {2} ].",
                                _sharedApp.ToCodeString(appWebQuery), centerX, centerY));
                    }
                    else
                    {
                        Log.Info(
                            string.Format("Tapping first element ({1} total) matching {0} at coordinates [ {2}, {3} ]. ",
                                _sharedApp.ToCodeString(appWebQuery), results.Length, centerX, centerY));
                    }

                    _gestures.TapCoordinates(centerX, centerY);
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
                    Log.Info("Touching coordinates [ " + x + ", " + y + " ].");
                    _gestures.TapCoordinates(x, y);
                }, new object[] { x, y });
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
            ScrollDirection homeButtonOrientation = _gestures.GetHomeButtonOrientation();
            switch (homeButtonOrientation)
            {
                case ScrollDirection.Right:
                    _sharedApp.SwipeUpToDown(swipePercentage, swipeSpeed, withInertia);
                    break;
                case ScrollDirection.Left:
                    _sharedApp.SwipeDownToUp(swipePercentage, swipeSpeed, withInertia);
                    break;
                case ScrollDirection.Up:
                    _sharedApp.SwipeRightToLeft(swipePercentage, swipeSpeed, withInertia);
                    break;
                case ScrollDirection.Down:
                    _sharedApp.SwipeLeftToRight(swipePercentage, swipeSpeed, withInertia);
                    break;
                default:
                    throw new Exception(string.Format("SwipeLeftToRight: Unable to swipe in direction {0}", homeButtonOrientation));
            }
        }

        /// <summary>
        /// Performs a left to right swipe gesture on the matching element. If multiple elements are matched, the first
        /// one will be used.
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
            ScrollDirection homeButtonOrientation = _gestures.GetHomeButtonOrientation();
            switch (homeButtonOrientation)
            {
                case ScrollDirection.Right:
                    _sharedApp.SwipeUpToDown(marked, swipePercentage, swipeSpeed, withInertia);
                    break;
                case ScrollDirection.Left:
                    _sharedApp.SwipeDownToUp(marked, swipePercentage, swipeSpeed, withInertia);
                    break;
                case ScrollDirection.Up:
                    _sharedApp.SwipeRightToLeft(marked, swipePercentage, swipeSpeed, withInertia);
                    break;
                case ScrollDirection.Down:
                    _sharedApp.SwipeLeftToRight(marked, swipePercentage, swipeSpeed, withInertia);
                    break;
                default:
                    throw new Exception(string.Format("SwipeLeftToRight: Unable to swipe in direction {0}", homeButtonOrientation));
            }
        }

        /// <summary>
        /// Performs a left to right swipe gesture on the matching element. If multiple elements are matched, the first
        /// one will be used.
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

            ScrollDirection homeButtonOrientation = _gestures.GetHomeButtonOrientation();
            switch (homeButtonOrientation)
            {
                case ScrollDirection.Right:
                    _sharedApp.SwipeUpToDown(query, swipePercentage, swipeSpeed, withInertia);
                    break;
                case ScrollDirection.Left:
                    _sharedApp.SwipeDownToUp(query, swipePercentage, swipeSpeed, withInertia);
                    break;
                case ScrollDirection.Up:
                    _sharedApp.SwipeRightToLeft(query, swipePercentage, swipeSpeed, withInertia);
                    break;
                case ScrollDirection.Down:
                    _sharedApp.SwipeLeftToRight(query, swipePercentage, swipeSpeed, withInertia);
                    break;
                default:
                    throw new Exception(string.Format("SwipeLeftToRight: Unable to swipe in direction {0}", homeButtonOrientation));
            }
        }

        /// <summary>
        /// Performs a left to right swipe gesture on the matching element. If multiple elements are matched, the first
        /// one will be used.
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
            ScrollDirection homeButtonOrientation = _gestures.GetHomeButtonOrientation();
            switch (homeButtonOrientation)
            {
                case ScrollDirection.Right:
                    _sharedApp.SwipeUpToDown(query, swipePercentage, swipeSpeed, withInertia);
                    break;
                case ScrollDirection.Left:
                    _sharedApp.SwipeDownToUp(query, swipePercentage, swipeSpeed, withInertia);
                    break;
                case ScrollDirection.Up:
                    _sharedApp.SwipeRightToLeft(query, swipePercentage, swipeSpeed, withInertia);
                    break;
                case ScrollDirection.Down:
                    _sharedApp.SwipeLeftToRight(query, swipePercentage, swipeSpeed, withInertia);
                    break;
                default:
                    throw new Exception(string.Format("SwipeLeftToRight: Unable to swipe in direction {0}", homeButtonOrientation));
            }
        }

        /// <summary>
        /// Performs a right to left swipe gesture.
        /// </summary>
        /// <param name="swipePercentage">How far across the screen to swipe (from 0.0 to 1.0).</param> 
        /// <param name="swipeSpeed">The speed of the gesture.</param> 
        /// <param name="withInertia">Whether swipes should cause inertia.</param>
        public void SwipeRightToLeft(
            double swipePercentage = UITestConstants.DefaultSwipePercentage, 
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed, 
            bool withInertia = true)
        {
            ScrollDirection homeButtonOrientation = _gestures.GetHomeButtonOrientation();
            switch (homeButtonOrientation)
            {
                case ScrollDirection.Right:
                    _sharedApp.SwipeDownToUp(swipePercentage, swipeSpeed, withInertia);
                    break;
                case ScrollDirection.Left:
                    _sharedApp.SwipeUpToDown(swipePercentage, swipeSpeed, withInertia);
                    break;
                case ScrollDirection.Up:
                    _sharedApp.SwipeLeftToRight(swipePercentage, swipeSpeed, withInertia);
                    break;
                case ScrollDirection.Down:
                    _sharedApp.SwipeRightToLeft(swipePercentage, swipeSpeed, withInertia);
                    break;
                default:
                    throw new Exception(string.Format("SwipeRightToLeft: Unable to swipe in direction {0}", homeButtonOrientation));
            }
        }

        /// <summary>
        /// Performs a right to left swipe gesture on the matching element. If multiple elements are matched, the first 
        /// one will be used.
        /// </summary>
        /// <param name="marked">
        /// Marked selector to match. See <see cref="AppQuery.Marked" /> for more information.
        /// </param>
        /// <param name="swipePercentage">How far across the element to swipe (from 0.0 to 1.0).</param> 
        /// <param name="swipeSpeed">The speed of the gesture.</param> 
        /// <param name="withInertia">Whether swipes should cause inertia.</param>
        public void SwipeRightToLeft(
            string marked, 
            double swipePercentage = UITestConstants.DefaultSwipePercentage, 
            int swipeSpeed = UITestConstants.DefaultSwipeSpeed, 
            bool withInertia = true)
        {
            ScrollDirection homeButtonOrientation = _gestures.GetHomeButtonOrientation();
            switch (homeButtonOrientation)
            {
                case ScrollDirection.Right:
                    _sharedApp.SwipeDownToUp(marked, swipePercentage, swipeSpeed, withInertia);
                    break;
                case ScrollDirection.Left:
                    _sharedApp.SwipeUpToDown(marked, swipePercentage, swipeSpeed, withInertia);
                    break;
                case ScrollDirection.Up:
                    _sharedApp.SwipeLeftToRight(marked, swipePercentage, swipeSpeed, withInertia);
                    break;
                case ScrollDirection.Down:
                    _sharedApp.SwipeRightToLeft(marked, swipePercentage, swipeSpeed, withInertia);
                    break;
                default:
                    throw new Exception(string.Format("SwipeRightToLeft: Unable to swipe in direction {0}", homeButtonOrientation));
            }
        }

        /// <summary>
        /// Performs a right to left swipe gesture on the matching element. If multiple elements are matched, the first
        ///  one will be used.
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
            ScrollDirection homeButtonOrientation = _gestures.GetHomeButtonOrientation();
            switch (homeButtonOrientation)
            {
                
                case ScrollDirection.Right:
                    _sharedApp.SwipeDownToUp(query, swipePercentage, swipeSpeed, withInertia);
                    break;
                case ScrollDirection.Left:
                    _sharedApp.SwipeUpToDown(query, swipePercentage, swipeSpeed, withInertia);
                    break;
                case ScrollDirection.Up:
                    _sharedApp.SwipeLeftToRight(query, swipePercentage, swipeSpeed, withInertia);
                    break;
                case ScrollDirection.Down:
                    _sharedApp.SwipeRightToLeft(query, swipePercentage, swipeSpeed, withInertia);
                    break;
                default:
                    throw new Exception(string.Format("SwipeRightToLeft: Unable to swipe in direction {0}", homeButtonOrientation));
            }
        }

        /// <summary>
        /// Performs a right to left swipe gesture on the matching element. If multiple elements are matched, the first
        ///  one will be used.
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
            ScrollDirection homeButtonOrientation = _gestures.GetHomeButtonOrientation();
            switch (homeButtonOrientation)
            {
                case ScrollDirection.Right:
                    _sharedApp.SwipeDownToUp(query, swipePercentage, swipeSpeed, withInertia);
                    break;
                case ScrollDirection.Left:
                    _sharedApp.SwipeUpToDown(query, swipePercentage, swipeSpeed, withInertia);
                    break;
                case ScrollDirection.Up:
                    _sharedApp.SwipeLeftToRight(query, swipePercentage, swipeSpeed, withInertia);
                    break;
                case ScrollDirection.Down:
                    _sharedApp.SwipeRightToLeft(query, swipePercentage, swipeSpeed, withInertia);
                    break;
                default:
                    throw new Exception(string.Format("SwipeRightToLeft: Unable to swipe in direction {0}", homeButtonOrientation));
            }
            
        }

        /// <summary>
        /// Scroll until an element that matches the <c>toMarked</c> is shown on the screen. 
        /// </summary>
        /// <param name="toMarked">
        /// Marked selector to select what element to bring on screen. See <see cref="AppQuery.Marked" /> for more
        ///  information.
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
            _errorReporting.With(() =>
                {
                    if (!string.IsNullOrEmpty(withinMarked))
                    {
                        throw new Exception("ScrollTo on iOS does not support withinMarked queries yet.");
                    }

                    if (strategy == ScrollStrategy.Gesture)
                    {
                        throw new Exception("ScrollTo on iOS does not support Gesture queries yet.");
                    }

                    Log.Info("Scrolling to element matching Marked(\"" + toMarked + "\").");
                    _gestures.ScrollTo(toMarked);
                }, new object[] { toMarked, withinMarked, strategy, swipePercentage, swipeSpeed, withInertia, timeout });
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

                    if (!results.Any())
                    {
                        throw new Exception(
                            string.Format("Unable to touch and hold on element. Query for {0} gave no results.",
                                _sharedApp.ToCodeString(appQuery)));
                    }

                    var centerX = results.First().Rect.CenterX;
                    var centerY = results.First().Rect.CenterY;

                    if (results.Length == 1)
                    {
                        Log.Info(
                            string.Format("Touching and holding on element matching {0} at coordinates [ {1}, {2} ].",
                                _sharedApp.ToCodeString(appQuery), centerX, centerY));
                    }
                    else
                    {
                        Log.Info(
                            string.Format(
                                "Touch and holding on first element ({1} total) matching {0} at coordinates [ {2}, {3} ]. ",
                                _sharedApp.ToCodeString(appQuery), results.Length, centerX, centerY));
                    }

                    _gestures.TouchAndHoldCoordinates(centerX, centerY);
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
                    _gestures.WaitForNoneAnimatingOrElapsed(); 
                    Log.Info("Touching and holding coordinates [ " + x + ", " + y + " ].");
                    _gestures.TouchAndHoldCoordinates(x, y);
                }, new object[] { x, y });
        }

        /// <summary>
        /// Performs two quick tap / touch gestures on the matched element. If multiple elements are matched, the first
        /// one will be used.
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
                        throw new Exception(string.Format("Unable to double tap on element. Query for {0} gave no results.", 
                                _sharedApp.ToCodeString(appQuery)));
                    }

                    var centerX = results.First().Rect.CenterX;
                    var centerY = results.First().Rect.CenterY;

                    if (results.Length == 1)
                    {
                        Log.Info(string.Format("Double tapping on element matching {0} at coordinates [ {1}, {2} ].",
                                _sharedApp.ToCodeString(appQuery), centerX, centerY));
                    }
                    else
                    {
                        Log.Info(
                            string.Format(
                                "Double tapping on first element ({1} total) matching {0} at coordinates [ {2}, {3} ]. ",
                                _sharedApp.ToCodeString(appQuery), results.Length, centerX, centerY));
                    }

                    _gestures.DoubleTapCoordinates(centerX, centerY);
                }, new[] { query });
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
        /// Performs a tap / touch gestures with 2 fingers on the matched element. If multiple elements are matched,
        ///  the first one will be used.
        /// </summary>
        /// <param name="query">Entry point for the fluent API to specify the element.</param>
        public void TwoFingerTap(Func<AppQuery, AppQuery> query)
        {
            _errorReporting.With(() =>
                {
                    AppQuery appQuery = _sharedApp.Expand(query);
                    var results = _gestures.QueryGestureWait(appQuery);

                    if (!results.Any())
                    {
                        throw new Exception(
                            string.Format("Unable to two finger tap on element. Query for {0} gave no results.",
                                _sharedApp.ToCodeString(appQuery)));
                    }

                    var centerX = results.First().Rect.CenterX;
                    var centerY = results.First().Rect.CenterY;

                    if (results.Length == 1)
                    {
                        Log.Info(string.Format(
                                "Two finger tapping on element matching {0} at coordinates [ {1}, {2} ].",
                                _sharedApp.ToCodeString(appQuery), centerX, centerY));
                    }
                    else
                    {
                        Log.Info(
                            string.Format(
                                "Two finger tapping on first element ({1} total) matching {0} at coordinates [ {2}, {3} ]. ",
                                _sharedApp.ToCodeString(appQuery), results.Length, centerX, centerY));
                    }

                    _gestures.TwoFingerTapCoordinates(centerX, centerY);
                }, new[] { query });
        }

        /// <summary>
        /// Performs a tap / touch gesture with 2 fingers on the given coordinates.
        /// </summary>
        /// <param name="x">The x coordinate to touch.</param>
        /// <param name="y">The y coordinate to touch.</param>
        public void TwoFingerTapCoordinates(float x, float y)
        {
            _errorReporting.With(() =>
                {
                    _gestures.WaitForNoneAnimatingOrElapsed();
                    Log.Info("Two finger tapping coordinates [ " + x + ", " + y + " ].");
                    _gestures.TwoFingerTapCoordinates(x, y);
                }, new object[] { x, y });
        }

        /// <summary>
        /// Performs a quick continuous flick gesture between 2 points.
        /// </summary>
        /// <param name="fromX">The x coordinate to start from.</param>
        /// <param name="fromY">The y coordinate to start from.</param>
        /// <param name="toX">The x coordinate to end at.</param>
        /// <param name="toY">The y coordinate to end at.</param>
        public void FlickCoordinates(float fromX, float fromY, float toX, float toY)
        {
            _errorReporting.With(() =>
                {
                    _gestures.WaitForNoneAnimatingOrElapsed();
                    Log.Info("Flicking from [ " + fromX + ", " + fromY + " ] to [ " + toX + ", " + toY + " ].");
                    _gestures.FlickCoordinates(fromX, fromY, toX, toY);
                }, new object[] { fromX, fromY, toX, toY });
        }

        /// <summary>
        /// Performs a continuous drag gesture between 2 points.
        /// </summary>
        /// <param name="fromX">The x coordinate to start from.</param>
        /// <param name="fromY">The y coordinate to start from.</param>
        /// <param name="toX">The x coordinate to end at.</param>
        /// <param name="toY">The y coordinate to end at.</param>
        public void DragCoordinates(float fromX, float fromY, float toX, float toY)
        {
            DragCoordinates(fromX, fromY, toX, toY, null, null);
        }

        /// <summary>
        /// Performs a continuous drag gesture between 2 points.
        /// </summary>
        /// <param name="fromX">The x coordinate to start from.</param>
        /// <param name="fromY">The y coordinate to start from.</param>
        /// <param name="toX">The x coordinate to end at.</param>
        /// <param name="toY">The y coordinate to end at.</param>
        /// <param name="duration">The <see cref="TimeSpan"/> duration of the pan gesture not including the hold time.</param>
        /// <param name="holdTime">The <see cref="TimeSpan"/> duration to hold the initial press before starting the pan gesture.</param>
        public void DragCoordinates(float fromX, float fromY, float toX, float toY, TimeSpan? duration, TimeSpan? holdTime)
        {
            _errorReporting.With(() =>
                {
                    _gestures.WaitForNoneAnimatingOrElapsed();
                    Log.Info("Dragging from [ " + fromX + ", " + fromY + " ] to [ " + toX + ", " + toY + " ].");
                    _gestures.DragCoordinates(fromX, fromY, toX, toY, duration, holdTime);
                }, new object[] { fromX, fromY, toX, toY, duration, holdTime });
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

                    if (!results.Any())
                    {
                        throw new Exception(
                            string.Format("Unable to pinch to zoom in on element. Query for {0} gave no results.",
                                _sharedApp.ToCodeString(appQuery)));
                    }

                    var centerX = results.First().Rect.CenterX;
                    var centerY = results.First().Rect.CenterY;

                    if (results.Length == 1)
                    {
                        Log.Info(
                            string.Format("Pinching to zoom in on element matching {0} at coordinates [ {1}, {2} ].",
                                _sharedApp.ToCodeString(appQuery), centerX, centerY));
                    }
                    else
                    {
                        Log.Info(
                            string.Format(
                                "Pinching to zoom in on first element ({1} total) matching {0} at coordinates [ {2}, {3} ]. ",
                                _sharedApp.ToCodeString(appQuery), results.Length, centerX, centerY));
                    }

                    _gestures.PinchToZoomInCoordinates(centerX, centerY, duration);
                }, new object[] { query, duration });
        }

        /// <summary>
        /// Performs a pinch gestures to zoom the view in on the given coordinates.
        /// </summary>
        /// <param name="x">The x coordinate of the center of the pinch.</param>
        /// <param name="y">The y coordinate of the center of the pinch.</param>
        /// <param name="duration">The <see cref="TimeSpan"/> duration of the pinch gesture.</param>
        public void PinchToZoomInCoordinates(float x, float y, TimeSpan? duration)
        {
            _errorReporting.With(() =>
                {
                    _gestures.WaitForNoneAnimatingOrElapsed();
                    Log.Info("Pinching to zoom in on coordinates [ " + x + ", " + y + " ].");
                    _gestures.PinchToZoomInCoordinates(x, y, duration);
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

                    if (!results.Any())
                    {
                        throw new Exception(
                            string.Format("Unable to pinch to zoom out on element. Query for {0} gave no results.",
                                _sharedApp.ToCodeString(appQuery)));
                    }

                    var centerX = results.First().Rect.CenterX;
                    var centerY = results.First().Rect.CenterY;

                    if (results.Length == 1)
                    {
                        Log.Info(
                            string.Format("Pinching to zoom out on element matching {0} at coordinates [ {1}, {2} ].",
                                _sharedApp.ToCodeString(appQuery), centerX, centerY));
                    }
                    else
                    {
                        Log.Info(
                            string.Format(
                                "Pinching to zoom out on first element ({1} total) matching {0} at coordinates [ {2}, {3} ]. ",
                                _sharedApp.ToCodeString(appQuery), results.Length, centerX, centerY));
                    }

                    _gestures.PinchToZoomOutCoordinates(centerX, centerY, duration);
                }, new object[] { query, duration });
        }

        /// <summary>
        /// Performs a pinch gestures to zoom the view in on the given coordinates.
        /// </summary>
        /// <param name="x">The x coordinate of the center of the pinch.</param>
        /// <param name="y">The y coordinate of the center of the pinch.</param>
        /// <param name="duration">The <see cref="TimeSpan"/> duration of the pinch gesture.</param>
        public void PinchToZoomOutCoordinates(float x, float y, TimeSpan? duration)
        {
            _errorReporting.With(() =>
                {
                    _gestures.WaitForNoneAnimatingOrElapsed();
                    Log.Info("Pinching to zoom out on coordinates [ " + x + ", " + y + " ].");
                    _gestures.PinchToZoomOutCoordinates(x, y, duration);
                }, new object[] { x, y, duration });
        }

        /// <summary>
        /// Dismisses the keyboard if present
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
        /// Presses the volume up button on the device.
        /// </summary>
        public void PressVolumeUp()
        {
            _errorReporting.With(() =>
                {
                    Log.Info("Pressing volume up.");
                    _gestures.PressVolumeUp();
                });
        }

        /// <summary>
        /// Presses the volume down button on the device.
        /// </summary>
        public void PressVolumeDown()
        {
            _errorReporting.With(() =>
                {
                    Log.Info("Pressing volume down.");
                    _gestures.PressVolumeDown();
                });
        }

        /// <summary>
        /// Sends the app to background for the specified time span.
        /// </summary>
        /// <param name="time">The time for the app to be in the background.</param>
        public void SendAppToBackground(TimeSpan time)
        {
            _errorReporting.With(() =>
                {
                    Log.Info("Sending app to background.");

                    _gestures.SendAppToBackground(time.TotalSeconds);

                }, new object[] { time });
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

                    return _waitForHelper.WaitForAny(() => _gestures.Query(appQuery), timeoutMessage, timeout,
                        retryFrequency, postTimeout);
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

                    _waitForHelper.WaitFor(() => !_gestures.Query(appQuery).Any(), timeoutMessage, timeout, retryFrequency,
                        postTimeout);
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
                    _gestures.WaitForNoneAnimatingOrElapsed();
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

                    var replStarter = new ReplStarter();

                    replStarter.RuniOSRepl(
                        Assembly.GetExecutingAssembly(), 
                        deviceUrl,
                        Device.DeviceIdentifier,
                        _deviceConnectionInfo.UseXDB);
                });
        }

        /// <summary>
        /// Invokes a method on the app's app delegate. For Xamarin apps, methods must be exposed using attributes 
        /// as shown below.
        /// 
        /// iOS example in app delegate:
        /// 
        /// <code>
        /// [Export("myInvokeMethod:")]
        /// public NSString MyInvokeMethod(NSString arg)
        /// {
        ///     return new NSString("uitest");
        /// }
        /// </code>
        /// 
        /// </summary>
        /// <param name="methodName">The name of the method to invoke.</param>
        /// <param name="argument">The argument to pass to the method.</param>
        /// <returns>The result of the invocation.</returns>
        public object Invoke(string methodName, object argument = null)
        {
            return _errorReporting.With(() =>
                {
                    return InvokeInner(methodName, argument == null ? null : new object[] { argument });
                }, new[] { methodName, argument });
        }

        /// <summary>
        /// Invokes a method on the app's app delegate. For Xamarin apps, methods must be exposed using attributes 
        /// as shown below.
        /// 
        /// iOS example in app delegate:
        /// 
        /// <code>
        /// [Export("myInvokeMethod:")]
        /// public NSString MyInvokeMethod(NSString arg, NSString arg2)
        /// {
        ///     return new NSString("uitest");
        /// }
        /// </code>
        /// 
        /// </summary>
        /// <param name="methodName">The name of the method to invoke.</param>
        /// <param name="arguments">An array of arguments to pass to the method.</param>
        /// <returns>The result of the invocation.</returns>
        public object Invoke(string methodName, object[] arguments)
        {
            return _errorReporting.With(() =>
                {
                    return InvokeInner(methodName, arguments);
                }, new object[] { methodName, arguments });
        }
         
        object InvokeInner(string methodName, object[] arguments = null)
        {
            var data = new
                {
                    selector = methodName,
                    arguments = arguments == null ? new object[] {} : arguments,
                };

            var result = _deviceConnectionInfo.Connection.Backdoor(data);

            var responseJObject = JObject.Parse(result.Contents);

            if ((string)responseJObject["outcome"] != "SUCCESS")
            {
                throw new Exception("Invocation failed: " + responseJObject["reason"]);
            }

            return responseJObject["result"];
        }

        /// <summary>
        /// Invokes raw UIA javascript.
        /// </summary>
        /// <param name="script">The automation script.</param>
        /// <returns>The result of the script.</returns>
        public object InvokeUia(string script)
        {
            return _errorReporting.With(() =>
                {
                    return _gestures.InvokeUia(script);
                }, new [] { script });
        }

        /// <summary>
        /// Invokes the Device Agent query.
        /// </summary>
        /// <returns>The results of the query.</returns>
        /// <param name="parameters">The parameters of the query.</param>
        public object InvokeDeviceAgentQuery(object parameters)
        {
            return _errorReporting.With(() =>
            {
                return _gestures.InvokeDeviceAgentQuery(parameters).GetAwaiter().GetResult();
            }, new[] { parameters });
        }

        /// <summary>
        /// Invokes the Device Agent gesture.
        /// </summary>
        /// <param name="gesture">The gesture to perform.</param>
        /// <param name="options">The gesture options.</param>
        /// <param name="specifiers">The gesture specifiers.</param>
        public void InvokeDeviceAgentGesture(string gesture, object options = null, object specifiers = null)
        {
            _errorReporting.With(() =>
            {
                _gestures.InvokeDeviceAgentGesture(gesture, options, specifiers).GetAwaiter().GetResult();
            }, new[] { gesture, options, specifiers });
        }

        /// <summary>
        /// Uses Device Agent to dismiss springboard alerts.
        /// </summary>
        public void DismissSpringboardAlerts()
        {
            _errorReporting.With(() =>
            {
                _gestures.DismissSpringboardAlerts();
            });
        }

        /// <summary>
        /// Runtime information and control of the currently running device.
        /// </summary>
        IDevice IApp.Device
        {
            get { return _iosDevice; }
        }

        /// <summary>
        /// Navigate back on the device. 
        /// </summary>
        public void Back()
        {
            _errorReporting.With(() =>
                {
                    Log.Info("Navigating back in application");
                    if (_iosDevice.OSVersion.Major >= 14)
                    {
                        WaitForElement(x => x.ClassFull("_UIModernBarButton"));
                        Tap(x => x.ClassFull("_UIModernBarButton"));
                        return;
                    }
                    else if (_iosDevice.OSVersion.Major >= 11)
                    {
                        var query = Query(x => x.ClassFull("_UINavigationItemButtonView"));
                        if (query.Any())
                        {
                            Tap(x => x.ClassFull("_UINavigationItemButtonView"));
                            return;
                        }

                        Tap(x => x.ClassFull("_UIBackButtonContainerView"));
                    }
                    else
                    {
                        Tap(x => x.ClassFull("UINavigationItemButtonView"));
                    }
                });
        }

        /// <summary>
        /// Allows HTTP access to the test server running on the device.
        /// </summary>
        public ITestServer TestServer
        {
            get { return _testServer; }
        }

        /// <summary>
        /// Runtime information and control of the currently running device.
        /// </summary>
        public iOSDevice Device
        {
            get { return _iosDevice; }
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
                }, new object[] { toQuery, withinQuery, strategy, swipePercentage, swipeSpeed, timeout });
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
                    _sharedApp.ScrollDownTo(toQuery, swipePercentage, swipeSpeed, withinQuery, strategy, 
                        withInertia, timeout);
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
                    _sharedApp.ScrollDownTo(toQuery, swipePercentage, swipeSpeed, withinQuery, strategy,
                        withInertia, timeout);
                }, new object[] { toQuery, withinQuery, strategy, swipePercentage, swipeSpeed, withInertia, timeout });
        }

        /// <summary>
        /// Scroll down until an element that matches the <c>toMarked</c> is shown on the screen. 
        /// </summary>
        /// <param name="toQuery">Entry point for the fluent API to specify the element to bring on screen.</param>
        /// <param name="withinMarked">
        /// Marked selector to select what element to scroll within. See <see cref="AppQuery.Marked" /> for more information.
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
        /// Performs a tap / touch gestures with 2 fingers on the matched element. If multiple elements are matched, 
        /// the first one will be used.
        /// </summary>
        /// <param name="marked">
        /// Marked selector to match. See <see cref="AppQuery.Marked" /> for more information.
        /// </param>
        public void TwoFingerTap(string marked)
        {
            TwoFingerTap(_sharedApp.AsMarkedQuery(marked));
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

                    Log.Info("Updating the value of a slider element");
                    _sharedApp.FirstWithLog(results, appQuery);

                    _gestures.SetSliderValue(appQuery, value);
                }, new object[] { query });
        }

        /// <summary>
        /// Sets the value of a wheel of a input view picker (that appears instead of a keyboard).
        /// </summary>
        /// <param name="pickerIndex">Index of picker to interact with.</param>
        /// <param name="wheelIndex">Index of wheel of a picker. Starts from 0.</param>
        /// <param name="value">Value to set.</param>
        public void SetInputViewPickerWheelValue(int pickerIndex, int wheelIndex, string value)
        {
            _errorReporting.With(() =>
            {
                _gestures.SetInputViewPickerWheelValue(pickerIndex, wheelIndex, value);
            }, new object[] { pickerIndex, wheelIndex, value });
        }

        /// <summary>
        /// Gets pickers that present on screen.
        /// </summary>
        /// <returns>List of pickers.</returns>
        public List<UIElement> GetPickers()
        {
            return _errorReporting.With(() =>
            {
                return _gestures.GetPickers(typeOfPickers: "Picker");
            });
        }

        /// <summary>
        /// Gets date pickers that present on screen.
        /// </summary>
        /// <returns>List of date pickers.</returns>
        public List<UIElement> GetDatePickers()
        {
            return _errorReporting.With(() =>
            {
                return _gestures.GetPickers(typeOfPickers: "DatePicker");
            });
        }

        /// <summary>
        /// Drags the from element to the to element.
        /// </summary>
        /// <param name="from">Entry point for the fluent API to specify the from element.</param>
        /// <param name="to">Entry point for the fluent API to specify the to element.</param>
        public void DragAndDrop(Func<AppQuery, AppQuery> from, Func<AppQuery, AppQuery> to) 
        {
            _errorReporting.With(() => 
                {
                    DragAndDropInner(from, to);
                }, new object[] { from, to });
        }

        /// <summary>
        /// Drags the from element to the to element.
        /// </summary>
        /// <param name="from">Marked selector of the from element.</param>
        /// <param name="to">Marked selector of the to element.</param>
        public void DragAndDrop(string from, string to) 
        {
            _errorReporting.With(() => 
                {
                    DragAndDropInner(_sharedApp.AsMarkedQuery(from), _sharedApp.AsMarkedQuery(to));
                }, new object[] { from, to });
        }

        /// <summary>
        /// Drags the from element to the to element.
        /// </summary>
        /// <param name="from">Entry point for the fluent API to specify the from element.</param>
        /// <param name="to">Entry point for the fluent API to specify the to element.</param>
        /// <param name="duration">The <see cref="TimeSpan"/> duration of the pan gesture not including the hold time.</param>
        /// <param name="holdTime">The <see cref="TimeSpan"/> duration to hold the initial press before starting the pan gesture.</param>
        public void DragAndDrop(Func<AppQuery, AppQuery> from, Func<AppQuery, AppQuery> to, 
            TimeSpan? duration = null, TimeSpan? holdTime = null)
        {
            _errorReporting.With(() => 
                {
                    DragAndDropInner(from, to, duration, holdTime);
                }, new object[] { from, to, duration, holdTime });
        }

        void DragAndDropInner(Func<AppQuery, AppQuery> from, Func<AppQuery, AppQuery> to, 
                              TimeSpan? duration = null, TimeSpan? holdTime = null)
        {
            AppQuery fromQuery = _sharedApp.Expand(from);
            var fromResults = _gestures.QueryGestureWait(fromQuery);
            var fromRect = _sharedApp.FirstWithLog(fromResults, fromQuery).Rect;

            AppQuery toQuery = _sharedApp.Expand(to);
            var toResults = _gestures.QueryGestureWait(toQuery);
            var toRect = _sharedApp.FirstWithLog(toResults, toQuery).Rect;

            _gestures.DragCoordinates(
                fromRect.CenterX, 
                fromRect.CenterY,
                toRect.CenterX,
                toRect.CenterY,
                duration.HasValue ? duration : TimeSpan.FromSeconds(1),
                holdTime.HasValue ? holdTime : TimeSpan.FromSeconds(0.5));
        }

        void ValidateCalabashServerVersion(VersionNumber serverVersion)
        {
            Log.Info("Test server version: " + serverVersion);

            if (serverVersion < _minSupportedCalabashServer)
            {
                throw new Exception(string.Concat(
                    "calabash-ios-server version ",
                    _minSupportedCalabashServer,
                    " or later is required. Version ",
                    serverVersion,
                    " was detected."));
            }
        }
    }
}
