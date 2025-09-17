using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Xamarin.UITest.Queries.Tokens;
using Xamarin.UITest.Shared.Extensions;
using Xamarin.UITest.Shared.Queries;
using Xamarin.UITest.Utils;

namespace Xamarin.UITest.Queries
{
    /// <summary>
    /// Fluent query API for specifying view elements to match for queries and gestures.
    /// </summary>
    public class AppQuery : ITokenContainer, IFluentInterface
    {
        readonly QueryPlatform _queryPlatform;
        readonly IQueryToken[] _tokens;
        readonly InvokeHelper _invokeHelper = new InvokeHelper();

        /// <summary>
        /// Initial constructor for queries. Should not be called directly, but used as part of the fluent API in the app classes.
        /// </summary>
        /// <param name="queryPlatform">The query target platform.</param>
        public AppQuery(QueryPlatform queryPlatform)
        {
            _queryPlatform = queryPlatform;
            _tokens = new IQueryToken[0];
        }

        AppQuery(QueryPlatform queryPlatform, params object[] tokens)
        {
            _queryPlatform = queryPlatform;
            _tokens = tokensFromObjectArray(tokens).ToArray();
        }

        IQueryToken[] ITokenContainer.Tokens
        {
            get { return _tokens; }
        }

        List<IQueryToken> tokensFromObjectArray(params object[] tokens)
        {
            var newTokens = new List<IQueryToken>();

            foreach (var token in tokens)
            {
                if (token is IQueryToken)
                {
                    newTokens.Add((IQueryToken)token);
                }
                else if (token is string)
                {
                    newTokens.Add(new RawToken((string)token));
                }
                else
                {
                    throw new ArgumentException(string.Format("Unknown token type: {0}. Supported token types are {1} and {2}.",
                        token.GetType().FullName,
                        typeof(string).FullName,
                        typeof(IQueryToken).FullName));
                }
            }

            return newTokens;
        }

        /// <summary>
        /// Constructor for adding more tokens to an existing query in an immutable fashion. Takes the tokens from the old query plus the additional tokens.
        /// </summary>
        /// <param name="appQuery">The existing query.</param>
        /// <param name="tokens">The new query tokens.</param>
        public AppQuery(AppQuery appQuery, params object[] tokens)
        {
            _queryPlatform = appQuery._queryPlatform;

            var newTokens = tokensFromObjectArray(tokens);

            _tokens = appQuery._tokens
                .Concat(newTokens)
                .ToArray();
        }


        /// <summary>
        /// Matches a button. 
        /// For Android: An element that has class (or inherits from) <c>android.widget.Button</c>. 
        /// For iOS: An element with class <c>UIButton</c>.
        /// </summary>
        /// <param name="marked">Optional argument for matching using marked classification. See <see cref="Marked"/> for more.</param>
        public AppQuery Button(string marked = null)
        {
            return new AppQuery(this, new ButtonToken(new SingleQuoteEscapedString(marked)));
        }

        /// <summary>
        /// Matches a TextField. 
        /// For Android: An element that has class (or inherits from) <c>android.widget.EditText</c>. 
        /// For iOS: An element with class <c>UITextField</c>.
        /// </summary>
        /// <param name="marked">Optional argument for matching using marked classification. See <see cref="Marked"/> for more.</param>
        public AppQuery TextField(string marked = null)
        {
            string classname = QueryPlatform == QueryPlatform.Android ? "android.widget.EditText" : "UITextField";

            if (string.IsNullOrWhiteSpace(marked))
            {
                return ComposedAppQuery(new AppQuery(QueryPlatform).ClassFull(classname), "TextField()");
            }
            else
            {
                return ComposedAppQuery(new AppQuery(QueryPlatform).ClassFull(classname).Marked(
                    new SingleQuoteEscapedString(marked).ToString()),
                    string.Format("TextField(\"{0}\")", marked)
                );
            }
        }

        /// <summary>
        /// Matches a Switch. 
        /// For Android: An element that inherits from <c>android.widget.CompoundButton</c>. 
        /// For iOS: An element with class <c>UISwitch</c>.
        /// </summary>
        /// <param name="marked">Optional argument for matching using marked classification. See <see cref="Marked"/> for more.</param>
        public AppQuery Switch(string marked = null)
        {
            string classname = QueryPlatform == QueryPlatform.Android ? "android.widget.CompoundButton" : "UISwitch";

            if (string.IsNullOrWhiteSpace(marked))
            {
                return ComposedAppQuery(new AppQuery(QueryPlatform).ClassFull(classname), "Switch()");
            }
            else
            {
                return ComposedAppQuery(new AppQuery(QueryPlatform).ClassFull(classname).Marked(
                    new SingleQuoteEscapedString(marked).ToString()),
                    string.Format("Switch(\"{0}\")",
                    marked)
                );
            }
        }

        /// <summary>
        /// Matches element class.
        /// For Android (no '.' in className): An element that has a class name of the given value (case insensitive).
        /// For Android ('.'s in className): An element which has a class (or super class) fully qualified name that matches the value.
        /// For iOS (first char lowercase): An element that has the class (or super class) name of the given value prepended with "UI". Example: <c>button</c> becomes <c>UIButton</c>.
        /// For iOS (first char uppercase): An element that has the class (or super class) name of the given value.
        /// </summary>
        /// <param name="className">The class name to match.</param>
        public AppQuery Class(string className)
        {
            return new AppQuery(this, new RawToken(className, string.Format("Class(\"{0}\")", className)));
        }

        /// <summary>
        /// Matches element class.
        /// For Android (no '.' in className): An element that has a class name of the given value (case insensitive).
        /// For Android ('.'s in className): An element which has a class (or super class) fully qualified name that matches the value.
        /// For iOS: An element that has the class (or super class) name of the given value.
        /// </summary>
        /// <param name="className">The class name to match.</param>
        public AppQuery ClassFull(string className)
        {
            if (QueryPlatform == QueryPlatform.iOS)
            {
                return new AppQuery(this, new WrappingToken(new StringPropertyToken<SingleQuoteEscapedString>(
                    "view",
                    new SingleQuoteEscapedString(className)),
                    string.Format("ClassFull(\"{0}\")", className)
                ));
            }

            return new AppQuery(this, new RawToken(className, string.Format("ClassFull(\"{0}\")", className)));
        }

        /// <summary>
        /// Matches common values. 
        /// For Android: An element with the given value as either <c>id</c>, <c>contentDescription</c> or <c>text</c>.
        /// For iOS: An element with the given value as either <c>accessibilityLabel</c> or <c>accessibilityIdentifier</c>.
        /// </summary>
        /// <param name="text">The value to match.</param>
        public AppQuery Marked(string text)
        {
            var appQuery = this;

            if (!_tokens.Any())
            {
                appQuery = new AppQuery(appQuery, new HiddenToken("*"));
            }

            return new AppQuery(appQuery, new WrappingToken(new StringPropertyToken<SingleQuoteEscapedString>(
                "marked",
                new SingleQuoteEscapedString(text)),
                string.Format("Marked(\"{0}\")", text)
            ));
        }

        /// <summary>
        /// Matches element id. 
        /// For Android: An element with the given value as <c>id</c>.
        /// For iOS: An element with the given value as <c>accessibilityIdentifier</c>.
        /// </summary>
        /// <param name="id">The value to match.</param>
        public AppQuery Id(string id)
        {
            var appQuery = this;

            if (!_tokens.Any())
            {
                appQuery = new AppQuery(appQuery, new HiddenToken("*"));
            }

            return new AppQuery(
                appQuery, 
                new WrappingToken(new StringPropertyToken<SingleQuoteEscapedString>(
                    "id",
                    new SingleQuoteEscapedString(id)),
                    $"Id(\"{id}\")"));
        }

        /// <summary>
        /// Matches element id. 
        /// For Android: An element with the given value as <c>id</c>.  Allows properties of 
        /// an Android App project's `Resource.Id` to be used in `Id()` queries.
        /// For iOS: An element with the string version of the given value as
        /// <c>accessibilityIdentifier</c>.
        /// </summary>
        /// <param name="id">The value to match.</param>
        public AppQuery Id(int id)
        {
            var appQuery = this;

            if (!_tokens.Any())
            {
                appQuery = new AppQuery(appQuery, new HiddenToken("*"));
            }

            return new AppQuery(
                appQuery, 
                new WrappingToken(new StringPropertyToken<SingleQuoteEscapedString>(
                    "id",
                    new SingleQuoteEscapedString(id.ToString())),
                    $"Id({id})"));
        }

        /// <summary>
        /// Changes the query to return sibling elements of the currently matched ones.
        /// </summary>
        /// <param name="className">Optional class name of elements to match.</param>
        public AppQuery Sibling(string className = null)
        {
            return new AppQuery(this, new DirectionalToken(className, "sibling", "Sibling"));
        }

        /// <summary>
        /// Changes the query to return the n'th sibling element of the currently matched ones.
        /// </summary>
        /// <param name="index">The zero-based index of the sibling to return.</param>
        /// <returns></returns>
        public AppQuery Sibling(int index)
        {
            return ComposedAppQuery(new AppQuery(QueryPlatform).Sibling().Index(index), string.Format("Sibling({0})", index));
        }

        /// <summary>
        /// Changes the query to return descendant elements of the currently matched ones.
        /// </summary>
        /// <param name="className">Optional class name of elements to match.</param>
        public AppQuery Descendant(string className = null)
        {
            return new AppQuery(this, new DirectionalToken(className, "descendant", "Descendant"));
        }

        /// <summary>
        /// Changes the query to return the n'th descendant element of the currently matched ones.
        /// </summary>
        /// <param name="index">The zero-based index of the descendant to return.</param>
        /// <returns></returns>
        public AppQuery Descendant(int index)
        {
            return ComposedAppQuery(new AppQuery(QueryPlatform).Descendant().Index(index), string.Format("Descendant({0})", index));
        }

        /// <summary>
        /// Changes the query to return parent elements of the currently matched ones.
        /// </summary>
        /// <param name="className">Optional class name of elements to match.</param>
        public AppQuery Parent(string className = null)
        {
            return new AppQuery(this, new DirectionalToken(className, "parent", "Parent"));
        }

        /// <summary>
        /// Changes the query to return the n'th parent element of the currently matched ones.
        /// </summary>
        /// <param name="index">The zero-based index of the parent to return.</param>
        /// <returns></returns>
        public AppQuery Parent(int index)
        {
            return ComposedAppQuery(new AppQuery(QueryPlatform).Parent().Index(index), string.Format("Parent({0})", index));
        }

        /// <summary>
        /// Changes the query to return child elements of the currently matched ones.
        /// </summary>
        /// <param name="className">Optional class name of elements to match.</param>
        public AppQuery Child(string className = null)
        {
            return new AppQuery(this, new DirectionalToken(className, "child", "Child"));
        }

        /// <summary>
        /// Changes the query to return the n'th child element of the currently matched ones.
        /// </summary>
        /// <param name="index">The zero-based index of the child to return.</param>
        /// <returns></returns>
        public AppQuery Child(int index)
        {
            return ComposedAppQuery(new AppQuery(QueryPlatform).Child().Index(index), string.Format("Child({0})", index));
        }

        /// <summary>
        /// Changes the query to return all elements instead of just the visible ones.
        /// </summary>
        /// <param name="className">Optional class name of elements to match.</param>
        public AppQuery All(string className = null)
        {
            return new AppQuery(this, new AllToken(className));
        }

        /// <summary>
        /// Matches element text. 
        /// </summary>
        /// <param name="text">The value to match.</param>
        public AppQuery Text(string text)
        {
            var appQuery = this;

            if (!_tokens.Any())
            {
                appQuery = new AppQuery(appQuery, new HiddenToken("*"));
            }

            return new AppQuery(appQuery, new WrappingToken(new StringPropertyToken<SingleQuoteEscapedString>(
                "text",
                new SingleQuoteEscapedString(text)),
                string.Format("Text(\"{0}\")", text))
            );
        }

        /// <summary>
        /// Matches the nth element of the currently matched elements.
        /// </summary>
        /// <param name="index">The zero-based index of the element to match.</param>
        public AppQuery Index(int index)
        {
            var appQuery = this;

            if (!_tokens.Any())
            {
                appQuery = new AppQuery(appQuery, new HiddenToken("*"));
            }

            return new AppQuery(appQuery, new WrappingToken(new IntPropertyToken("index", index), string.Format("Index({0})", index)));
        }

        AppQuery ComposedAppQuery(AppQuery query, string codeString)
        {
            return new AppQuery(this, new QueryToken(query, codeString));
        }

        /// <summary>
        /// Matches WebViews
        /// </summary>
        public AppQuery WebView()
        {
            AppQuery classFull = new AppQuery(QueryPlatform).ClassFull(QueryPlatform == QueryPlatform.iOS ? "UIWebView" : "android.webkit.WebView");
            return ComposedAppQuery(classFull, "WebView()");
        }

        /// <summary>
        /// Matches the nth WebView
        /// </summary>
        /// <param name="index">The zero-based index of the webview to return.</param>
        public AppQuery WebView(int index)
        {
            AppQuery classFull = new AppQuery(QueryPlatform).ClassFull(QueryPlatform == QueryPlatform.iOS ? "UIWebView" : "android.webkit.WebView").Index(index);
            return ComposedAppQuery(classFull, string.Format("WebView({0})", index));
        }

        /// <summary>
        /// A raw Calabash selector. Allows for string based Calabash queries.
        /// </summary>
        /// <param name="calabashQuery">The Calabash query to match.</param>
        public AppQuery Raw(string calabashQuery)
        {
            return new AppQuery(this, calabashQuery);
        }

        /// <summary>
        /// A raw Calabash selector. Allows for string based Calabash queries.
        /// </summary>
        /// <param name="calabashQuery">The Calabash query to match.</param>
        /// <param name="arg1">A raw argument to pass to the Calabash query.</param>
        public AppTypedSelector<string> Raw(string calabashQuery, object arg1)
        {
            return InternalRaw(calabashQuery, new[] { arg1 });
        }

        /// <summary>
        /// A raw Calabash selector. Allows for string based Calabash queries.
        /// </summary>
        /// <param name="calabashQuery">The Calabash query to match.</param>
        /// <param name="arg1">A raw argument to pass to the Calabash query.</param>
        /// <param name="arg2">A raw argument to pass to the Calabash query.</param>
        public AppTypedSelector<string> Raw(string calabashQuery, object arg1, object arg2)
        {
            return InternalRaw(calabashQuery, new[] { arg1, arg2 });
        }

        /// <summary>
        /// A raw Calabash selector. Allows for string based Calabash queries.
        /// </summary>
        /// <param name="calabashQuery">The Calabash query to match.</param>
        /// <param name="arg1">A raw argument to pass to the Calabash query.</param>
        /// <param name="arg2">A raw argument to pass to the Calabash query.</param>
        /// <param name="arg3">A raw argument to pass to the Calabash query.</param>
        public AppTypedSelector<string> Raw(string calabashQuery, object arg1, object arg2, object arg3)
        {
            return InternalRaw(calabashQuery, new[] { arg1, arg2, arg3 });
        }

        /// <summary>
        /// A raw Calabash selector. Allows for string based Calabash queries.
        /// </summary>
        /// <param name="calabashQuery">The Calabash query to match.</param>
        /// <param name="arg1">A raw argument to pass to the Calabash query.</param>
        /// <param name="arg2">A raw argument to pass to the Calabash query.</param>
        /// <param name="arg3">A raw argument to pass to the Calabash query.</param>
        /// <param name="arg4">A raw argument to pass to the Calabash query.</param>
        public AppTypedSelector<string> Raw(string calabashQuery, object arg1, object arg2, object arg3, object arg4)
        {
            return InternalRaw(calabashQuery, new[] { arg1, arg2, arg3, arg4 });
        }

        /// <summary>
        /// A raw Calabash selector. Allows for string based Calabash queries.
        /// </summary>
        /// <param name="calabashQuery">The Calabash query to match.</param>
        /// <param name="arg1">A raw argument to pass to the Calabash query.</param>
        /// <param name="arg2">A raw argument to pass to the Calabash query.</param>
        /// <param name="arg3">A raw argument to pass to the Calabash query.</param>
        /// <param name="arg4">A raw argument to pass to the Calabash query.</param>
        /// <param name="arg5">A raw argument to pass to the Calabash query.</param>
        public AppTypedSelector<string> Raw(string calabashQuery, object arg1, object arg2, object arg3, object arg4, object arg5)
        {
            return InternalRaw(calabashQuery, new[] { arg1, arg2, arg3, arg4, arg5 });
        }

        /// <summary>
        /// A raw Calabash selector. Allows for string based Calabash queries.
        /// </summary>
        /// <param name="calabashQuery">The Calabash query to match.</param>
        /// <param name="arg1">A raw argument to pass to the Calabash query.</param>
        /// <param name="arg2">A raw argument to pass to the Calabash query.</param>
        /// <param name="arg3">A raw argument to pass to the Calabash query.</param>
        /// <param name="arg4">A raw argument to pass to the Calabash query.</param>
        /// <param name="arg5">A raw argument to pass to the Calabash query.</param>
        /// <param name="arg6">A raw argument to pass to the Calabash query.</param>
        public AppTypedSelector<string> Raw(string calabashQuery, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6)
        {
            return InternalRaw(calabashQuery, new[] { arg1, arg2, arg3, arg4, arg5, arg6 });
        }

        AppTypedSelector<string> InternalRaw(string calabashQuery, object[] args)
        {
            var argsString = string.Join(", ", args.Select(x => x.Stringify()));
            var codeString = string.Format("Raw(\"{0}\", {1})", calabashQuery, argsString);

            if (QueryPlatform == QueryPlatform.Android)
            {
                args = args.Select(MapAndroidRawArguments).ToArray();
            }

            return new AppTypedSelector<string>(new AppQuery(this, new WrappingToken(new RawToken(calabashQuery), codeString)), args);
        }

        static object MapAndroidRawArguments(object argument)
        {
            if (argument == null || !argument.GetType().IsAnonymousType())
            {
                return argument;
            }

            var propertyDescriptors = TypeDescriptor.GetProperties(argument)
                .OfType<PropertyDescriptor>()
                .ToArray();

            if (propertyDescriptors.Length != 1)
            {
                return argument;
            }

            var propertyDescriptor = propertyDescriptors.Single();
            var methodName = propertyDescriptor.Name;

            var value = propertyDescriptor.GetValue(argument);

            if (value != null && value.GetType().IsArray)
            {
                var arguments = ((IEnumerable)value).Cast<object>().ToArray();
                return new { method_name = methodName, arguments = arguments };
            }

            return new { method_name = methodName, arguments = new[] { value } };
        }

        /// <summary>
        /// Invokes a method on the view elements matched by the query. Can be chained to invoke methods on the results.
        /// </summary>
        /// <param name="methodName">The name of the method.</param>
        public AppTypedSelector<object> Invoke(string methodName)
        {
            return _invokeHelper.Invoke(this, methodName, new object[] { });
        }

        /// <summary>
        /// Invokes a method on the view elements matched by the query. Can be chained to invoke methods on the results.
        /// </summary>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="arg1">The 1st parameter.</param>
        public AppTypedSelector<object> Invoke(string methodName, object arg1)
        {
            return _invokeHelper.Invoke(this, methodName, new object[] { arg1 });
        }

        /// <summary>
        /// Invokes a method on the view elements matched by the query. Can be chained to invoke methods on the results.
        /// </summary>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="arg1">The 1st parameter.</param>
        /// <param name="arg2">The 2nd parameter.</param>
        public AppTypedSelector<object> Invoke(string methodName, object arg1, object arg2)
        {
            return _invokeHelper.Invoke(this, methodName, new object[] { arg1, arg2 });
        }

        /// <summary>
        /// Invokes a method on the view elements matched by the query. Can be chained to invoke methods on the results.
        /// </summary>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="arg1">The 1st parameter.</param>
        /// <param name="arg2">The 2nd parameter.</param>
        /// <param name="arg3">The 3rd parameter.</param>
        public AppTypedSelector<object> Invoke(string methodName, object arg1, object arg2, object arg3)
        {
            return _invokeHelper.Invoke(this, methodName, new object[] { arg1, arg2, arg3 });
        }

        /// <summary>
        /// Invokes a method on the view elements matched by the query. Can be chained to invoke methods on the results.
        /// </summary>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="arg1">The 1st parameter.</param>
        /// <param name="arg2">The 2nd parameter.</param>
        /// <param name="arg3">The 3rd parameter.</param>
        /// <param name="arg4">The 4th parameter.</param>
        public AppTypedSelector<object> Invoke(string methodName, object arg1, object arg2, object arg3, object arg4)
        {
            return _invokeHelper.Invoke(this, methodName, new object[] { arg1, arg2, arg3, arg4 });
        }

        /// <summary>
        /// Invokes a method on the view elements matched by the query. Can be chained to invoke methods on the results.
        /// </summary>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="arg1">The 1st parameter.</param>
        /// <param name="arg2">The 2nd parameter.</param>
        /// <param name="arg3">The 3rd parameter.</param>
        /// <param name="arg4">The 4th parameter.</param>
        /// <param name="arg5">The 5th parameter.</param>
        public AppTypedSelector<object> Invoke(string methodName, object arg1, object arg2, object arg3, object arg4, object arg5)
        {
            return _invokeHelper.Invoke(this, methodName, new object[] { arg1, arg2, arg3, arg4, arg5 });
        }

        /// <summary>
        /// Invokes javascript on the view elements matched by the query. If view elements other than WebViews are encountered, the execution will halt and an Exception will be thrown.
        /// </summary>
        /// <param name="javascript">The javascript to invoke on the views</param>
        /// <returns></returns>
        public InvokeJSAppQuery InvokeJS(string javascript)
        {
            return new InvokeJSAppQuery(this, javascript);
        }

        /// <summary>
        /// Matches elements in web views matching the given css selector. Must be used on web view elements. If used alone, will default to <c>android.webkit.WebView</c> for Android and <c>UIWebView</c> for iOS.
        /// </summary>
        /// <param name="cssSelector">The css selector to match.</param>
        public AppWebQuery Css(string cssSelector)
        {
            return WebViewQuery(new CssToken(cssSelector));
        }

        /// <summary>
        /// Matches a Frame/IFrame, allowing subsequent Css queries to execute within that frame. Must be used on web view elements. 
        /// If used alone, will default to <c>android.webkit.WebView</c> for Android and <c>UIWebView</c> for iOS.
        /// </summary>
        /// <param name="cssSelector">The css selector to match. Should refer to an html Frame/IFrame</param>
        public AppQuery Frame(string cssSelector)
        {
            return FrameAppQuery(new CssToken(cssSelector));
        }

        /// <summary>
        /// Matches elements in web views matching the given XPath selector. Must be used on web view elements. If used alone, will default to <c>android.webkit.WebView</c> for Android and <c>UIWebView</c> for iOS.
        /// </summary>
        /// <param name="xPathSelector">The css selector to match.</param>
        public AppWebQuery XPath(string xPathSelector)
        {
            var codeString = string.Format("XPath(\"{0}\")", xPathSelector);
            var token = new WrappingToken(new StringPropertyToken<XPathSingleQuoteWorkAroundString>(
                "xpath",
                new XPathSingleQuoteWorkAroundString(xPathSelector)),
                codeString
            );

            return WebViewQuery(token);
        }

        AppQuery FrameAppQuery(CssToken token)
        {
            var appQuery = this;
            var allToken = _tokens.LastOrDefault() as AllToken;

            if (allToken != null && allToken._className == null)
            {
                appQuery = WebViewAppQueryWithAllToken(appQuery);
                appQuery = new AppQuery(appQuery.QueryPlatform, appQuery._tokens.Where(t => t != allToken).ToArray());
            }

            if (!_tokens.Any())
            {
                appQuery = WebViewAppQuery(appQuery);
            }
            return CssAppQuery(appQuery, token);
        }

        AppQuery CssAppQuery(AppQuery appQuery, CssToken token)
        {
            return new AppQuery(appQuery, token);
        }

        AppQuery WebViewAppQuery(AppQuery appQuery)
        {
            if (QueryPlatform == QueryPlatform.Android)
            {
                appQuery = new AppQuery(appQuery, new HiddenToken("android.webkit.WebView"));
            }
            else
            {
                appQuery = new AppQuery(appQuery, new HiddenToken("UIWebView"));
            }
            return appQuery;
        }

        AppQuery WebViewAppQueryWithAllToken(AppQuery appQuery)
        {
            if (QueryPlatform == QueryPlatform.Android)
            {
                appQuery = new AppQuery(appQuery, new WrappingToken(new AllToken("android.webkit.WebView"), "All()"));
            }
            else
            {
                appQuery = new AppQuery(appQuery, new WrappingToken(new AllToken("UIWebView"), "All()"));
            }
            return appQuery;
        }

        AppWebQuery WebViewQuery(IQueryToken token)
        {
            var appQuery = this;
            var allToken = _tokens.LastOrDefault() as AllToken;

            if (allToken != null && allToken._className == null)
            {
                appQuery = WebViewAppQueryWithAllToken(appQuery);
                return new AppWebQuery(appQuery._tokens.Where(t => t != allToken).ToArray(), appQuery._queryPlatform, token);
            }

            if (!_tokens.Any())
            {
                appQuery = WebViewAppQuery(appQuery);
            }
            return new AppWebQuery(appQuery._tokens, appQuery._queryPlatform, token);
        }

        /// <summary>
        /// Matches a property or getter method value on the element. 
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value to match.</param>
        public AppQuery Property(string propertyName, string value)
        {
            var appQuery = this;

            if (!_tokens.Any())
            {
                appQuery = new AppQuery(appQuery, new HiddenToken("*"));
            }

            return new AppQuery(appQuery, new StringPropertyToken<SingleQuoteEscapedString>(propertyName, new SingleQuoteEscapedString(value)));
        }

        /// <summary>
        /// Matches a property or getter method value on the element. 
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value to match.</param>
        public AppQuery Property(string propertyName, int value)
        {
            var appQuery = this;

            if (!_tokens.Any())
            {
                appQuery = new AppQuery(appQuery, new HiddenToken("*"));
            }

            return new AppQuery(appQuery, new IntPropertyToken(propertyName, value));
        }

        /// <summary>
        /// Matches a property or getter method value on the element. 
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value to match.</param>
        public AppQuery Property(string propertyName, bool value)
        {
            var appQuery = this;

            if (!_tokens.Any())
            {
                appQuery = new AppQuery(appQuery, new HiddenToken("*"));
            }

            return new AppQuery(appQuery, new BoolPropertyToken(propertyName, value));
        }

        /// <summary>
        /// Allows for further filtering on a given property value.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        public PropertyAppQuery Property(string propertyName)
        {
            var appQuery = this;

            if (!_tokens.Any())
            {
                appQuery = new AppQuery(appQuery, new HiddenToken("*"));
            }

            return new PropertyAppQuery(QueryPlatform, appQuery, propertyName);
        }

        /// <summary>
        /// The target platform of the query. Useful when writing extensions methods for queries for platform differences.
        /// </summary>
        public QueryPlatform QueryPlatform
        {
            get { return _queryPlatform; }
        }

        /// <summary>
        /// Converts the string into it's Calabash query equivalent.
        /// </summary>
        public override string ToString()
        {
            if (!_tokens.Any())
            {
                return "*";
            }

            var tokens = _tokens.Select(x => x.ToQueryString(_queryPlatform));
            var deduplicatedTokens = RemoveConsecutiveDuplicatesOf(tokens, "*");
            return string.Join(" ", deduplicatedTokens);
        }

        internal IEnumerable<string> RemoveConsecutiveDuplicatesOf(IEnumerable<string> tokens, string target = "*")
        {
            string lastSeen = "";
            return tokens.SelectMany(t => Regex.Matches(t, @"[^ ]*['][^'\\]*(\\.)*[^'\\]*['][^ ]*|[^ ]+")
                            .Cast<Match>()).Select(m => m.Value).Where(t =>
                              {
                                  if (string.IsNullOrEmpty(t))
                                  {
                                      return false;
                                  }

                                  var trimed = t.Trim();
                                  var duplicate = trimed == target && lastSeen == target;
                                  lastSeen = t;
                                  return !duplicate;
                              });
        }
    }
}