using System.Linq;
using Xamarin.UITest.Queries.Tokens;
using Xamarin.UITest.Shared.Extensions;
using System.Collections.Generic;
using System;

namespace Xamarin.UITest.Queries
{
    internal class InvokeHelper
    {
        public AppTypedSelector<object> Invoke(AppQuery appQuery, string methodName, object[] arguments)
        {
            return AppTypedSelector(appQuery, appQuery, null, methodName, arguments, false);
        }

        public AppTypedSelector<object> Invoke(IAppTypedSelector selector, string methodName, object[] arguments)
        {
            return AppTypedSelector(selector.AppQuery, selector, selector.QueryParams, methodName, arguments, selector.ExplicitlyRequestedValue);
        }

        AppTypedSelector<object> AppTypedSelector(AppQuery appQuery, ITokenContainer tokenContainer, object[] queryParams, string methodName, object[] arguments, bool explicitlyRequestedValue)
        {
            queryParams = queryParams ?? new object[0];
            arguments = arguments ?? new object[0];

            if (!tokenContainer.Tokens.Any())
            {
                appQuery = new AppQuery(appQuery, new HiddenToken("*"));
            }

            var strArgs = string.Join(", ", new[] {methodName}.Concat(arguments).Select(x => x.Stringify()));
            appQuery = new AppQuery(appQuery, new RawToken(string.Empty, string.Format("Invoke({0})", strArgs)));

            if (appQuery.QueryPlatform == QueryPlatform.Android)
            {
                return new AppTypedSelector<object>(appQuery, queryParams.Concat(new object[] { new { method_name = methodName, arguments = arguments } }).ToArray());
            }

            var methodNameAndArgs = new object[] { methodName }.Concat(arguments).ToArray();
            var paramArgs = new List<object>();

            if (methodNameAndArgs.Length == 1)
            {
                paramArgs.Add(methodName);
				queryParams = queryParams.Concat (paramArgs).ToArray ();
            }
            else if (methodNameAndArgs.Length % 2 != 0)
            {
                throw new Exception("Invoking an iOS selector requires either 0 or an uneven number of arguments (they have to match up pairwise including method name)."); 
            }
            else
            {
                for (var i = 0; i < methodNameAndArgs.Length; i += 2)
                {
                    paramArgs.Add(new Dictionary<object, object> { { methodNameAndArgs[i], methodNameAndArgs[i + 1 ] } });
                }

				/* Invocations with arguments on iOS require a nested array. E.g. [[ {foo: bar}, {baz: qux} ]] */
				queryParams = new object[] { queryParams.Concat(paramArgs).ToArray() };
            }

			return new AppTypedSelector<object>(appQuery, queryParams, explicitlyRequestedValue);
        }
    }
}