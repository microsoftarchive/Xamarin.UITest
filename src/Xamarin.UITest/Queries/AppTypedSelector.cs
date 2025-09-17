using System;
using Xamarin.UITest.Queries.Tokens;
using Xamarin.UITest.Utils;

namespace Xamarin.UITest.Queries
{
    /// <summary>
    /// Fluent query API for specifying the type in property and invoke result.
    /// </summary>
    /// <typeparam name="T">type</typeparam>
    public class AppTypedSelector<T> : IFluentInterface, ITokenContainer, IAppTypedSelector
    {
        readonly AppQuery _appQuery;
        readonly object[] _queryParams;
        private readonly bool _explicitlyRequestedValue;
        readonly InvokeHelper _invokeHelper = new InvokeHelper();

        /// <summary>
        /// Constructor for selectors. Should not be called directly, but used as part of the fluent API in the Property class and for Invoke.
        /// </summary>
        /// <param name="appQuery">The query for property</param>
        /// <param name="queryParams">The parameters passed to the query.</param>
        /// <param name="explicitlyRequestedValue"></param>
        public AppTypedSelector(AppQuery appQuery, object[] queryParams, bool explicitlyRequestedValue = false)
        {
            _appQuery = appQuery;
            _queryParams = queryParams;
            _explicitlyRequestedValue = explicitlyRequestedValue;
        }

        object[] IAppTypedSelector.QueryParams
        {
            get { return _queryParams; }
        }

        AppQuery IAppTypedSelector.AppQuery
        {
            get { return _appQuery; }
        }

        /// <summary>
        /// The tokens of the current query.
        /// </summary>
        IQueryToken[] ITokenContainer.Tokens
        {
            get { return ((ITokenContainer)_appQuery).Tokens; }
        }


        /// <summary>
        /// The value of the query was explicitly requested.
        /// </summary>
        bool IAppTypedSelector.ExplicitlyRequestedValue
        {
            get { return _explicitlyRequestedValue; }
        }

        /// <summary>
        /// Converts the string into it's Calabash query equivalent.
        /// </summary>
        public override string ToString()
        {
            return _appQuery.ToString();
        }

        /// <summary>
        /// Invokes a method on the view elements matched by the query. Can be chained to invoke methods on the results.
        /// </summary>
        /// <param name="methodName">The name of the method.</param>
        public AppTypedSelector<object> Invoke(string methodName)
        {
            return _invokeHelper.Invoke(this, methodName, new object[] {});
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
        /// Extracts the result of the query as a given type.
        /// </summary>
        /// <typeparam name="TResult">The expected result type of the query.</typeparam>
        public AppTypedSelector<TResult> Value<TResult>()
        {
            if (_explicitlyRequestedValue)
            {
                throw new Exception(string.Format("Value must never be called more that once, inspect query that starts with: {0}", TokenCodePrinter.ToCodeString(this)));
            }
            return new AppTypedSelector<TResult>(new AppQuery(_appQuery, new ValueToken<TResult>()), _queryParams, true);
        }
    }
}