using Xamarin.UITest.Queries.Tokens;

namespace Xamarin.UITest.Queries
{
    /// <summary>
    /// Helper interface for exposing tokens from the fluent query API without cluttering the fluent API itself (when using explicit interface implementation). 
    /// </summary>
	public interface ITokenContainer
	{
        /// <summary>
        /// The tokens of the current query.
        /// </summary>
        IQueryToken[] Tokens { get; }
	}
}