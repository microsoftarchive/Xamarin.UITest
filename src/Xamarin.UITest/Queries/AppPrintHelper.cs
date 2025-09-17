using System;
using Newtonsoft.Json;
using Xamarin.UITest.Shared.Logging;
using Xamarin.UITest.Utils;

namespace Xamarin.UITest.Queries
{
    /// <summary>
    /// A fluent API for making test output easier. Mirrored query methods from the app classes that print directly to <see cref="System.Console"/>.
    /// </summary>
    public class AppPrintHelper : IFluentInterface
    {
        readonly IApp _app;
        readonly IGestures _gestures;

        internal AppPrintHelper(IApp app, IGestures gestures)
        {
            _app = app;
            _gestures = gestures;
        }

        /// <summary>
        /// Prints all the visible view elements to <see cref="System.Console"/>.
        /// </summary>
        public void Visible()
        {
            Query(c => c);
        }

        /// <summary>
        /// Prints the view elements matched by <c>query</c> to <see cref="System.Console"/>.
        /// </summary>
        /// <param name="query">Entry point for the fluent API to specify the element. If left as <c>null</c> returns all visible view objects.</param>
        public void Query(Func<AppQuery, AppQuery> query = null)
        {
            Log.Info(ToPrettyString(_app.Query(query)));
        }

        /// <summary>
        /// Prints the view elements matched by <c>query</c> to <see cref="System.Console"/>.
        /// </summary>
        /// <param name="query">Entry point for the fluent API to specify the element. If left as <c>null</c> returns all visible view objects.</param>
        public void Query(Func<AppQuery, AppWebQuery> query)
        {
            Log.Info(ToPrettyString(_app.Query(query)));
        }

        /// <summary>
        /// Prints the properties matched by <c>query</c> to <see cref="System.Console"/>.
        /// </summary>
        /// <param name="typedSelector">Entry point for the fluent API to specify the property.</param>
        public void Query<T>(Func<AppQuery, AppTypedSelector<T>> typedSelector)
        {
            Log.Info(ToPrettyString(_app.Query(typedSelector)));
        }

        /// <summary>
        /// Prints a tree of the visible view elements.
        /// </summary>
        /// <param name="console">Output to console in color instead of default logger.</param>
        public void Tree(bool console = false)
        {
            var treeHelper = new TreePrintHelper(_gestures);

            if (console)
            {
                treeHelper.PrintTree(new ConsoleTreePrinter());
            }
            else
            {
                treeHelper.PrintTree(new LoggerTreePrinter());
            }
        }

        /// <summary>
        /// Prints a tree of all elements of current application.
        /// Also prints iOS elements such as system keyboards, alerts and so on.
        /// </summary>
        /// <param name="console">Output to console in color instead of default logger.</param>
        public void AllElements(bool console = false)
        {
            var treeHelper = new TreePrintHelper(_gestures);

            if (console)
            {
                treeHelper.PrintTreeWithDeviceAgent(new ConsoleTreePrinter());
            }
            else
            {
                treeHelper.PrintTreeWithDeviceAgent(new LoggerTreePrinter());
            }
        }

        static string ToPrettyString(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }
    }
}