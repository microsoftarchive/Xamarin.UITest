using System;
using System.Linq;
using System.Text;
using Xamarin.UITest.Queries.Tokens;
using Xamarin.UITest.Utils;

namespace Xamarin.UITest.Queries
{
    /// <summary>
    /// Fluent query API for invoking javascipt on Webviews.
    /// </summary>
    public class InvokeJSAppQuery : IFluentInterface, IInvokeJSAppQuery
    {
        private readonly AppQuery _appQuery;
        private readonly string _javascript;
        private readonly IQueryToken[] _tokens;

        /// <summary>
        /// Initial constructor. Should not be called directly, but used as part of the fluent API in the app classes.
        /// </summary>
        public InvokeJSAppQuery(AppQuery appQuery, string javascript)
        {
            _javascript = javascript;
            _appQuery = appQuery;
            IQueryToken[] x = { new WrappingToken(new HiddenToken(string.Empty), string.Format("InvokeJS(\"{0}\")", javascript)) };
            _tokens = ((ITokenContainer)appQuery).Tokens.Concat(x).ToArray();
        }

        string IInvokeJSAppQuery.Javascript
        {
            get { return _javascript; } 
        }

        AppQuery IInvokeJSAppQuery.AppQuery
        {
            get { return _appQuery; } 
        }

        /// <summary>
        /// The tokens of the current query.
        /// </summary>
        IQueryToken[] ITokenContainer.Tokens
        {
            get { return _tokens; }
        }
    }
}