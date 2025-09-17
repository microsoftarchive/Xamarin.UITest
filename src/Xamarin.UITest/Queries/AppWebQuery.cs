using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.UITest.Queries.Tokens;
using Xamarin.UITest.Utils;

namespace Xamarin.UITest.Queries
{
    /// <summary>
    /// Fluent query API for specifying view elements predicates for web elements.
    /// </summary>
    public class AppWebQuery : ITokenContainer, IFluentInterface
    {
        readonly QueryPlatform _queryPlatform;
        readonly IQueryToken[] _tokens;

        /// <summary>
        /// Initial constructor for web element queries. Should not be called directly, but used as part of the fluent API in the app classes.
        /// </summary>
        /// <param name="initialTokens">The tokens of the existing <see cref="AppQuery"/>.</param>
        /// <param name="queryPlatform">The query target platform.</param>
        /// <param name="tokens">The additional tokens to add.</param>
        public AppWebQuery(IEnumerable<IQueryToken> initialTokens, QueryPlatform queryPlatform, params object[] tokens)
        {
            _queryPlatform = queryPlatform;

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

            _tokens = initialTokens
                .Concat(newTokens)
                .ToArray();
        }

        /// <summary>
        /// The tokens of the current query.
        /// </summary>
        IQueryToken[] ITokenContainer.Tokens
        {
            get { return _tokens; }
        }

        /// <summary>
        /// The target platform of the query. Useful when writing extensions methods for queries for platform differences.
        /// </summary>
        public QueryPlatform Platform
        {
           get { return _queryPlatform; }
        }

        /// <summary>
        /// Matches the nth element of the currently matched elements.
        /// </summary>
        /// <param name="index">The zero-based index of the element to match.</param>
        public AppWebQuery Index(int index)
        {
            return new AppWebQuery(_tokens, _queryPlatform, new WrappingToken(new IntPropertyToken("index", index), string.Format("Index({0})", index)));
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

            return string.Join(" ", _tokens.Select(x => x.ToQueryString(_queryPlatform)));
        }
    }
}