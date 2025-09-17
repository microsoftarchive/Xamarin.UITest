namespace Xamarin.UITest.Queries.Tokens
{
    /// <summary>
    /// A query token. Part of a query for matching view elements for queries and gestures.
    /// </summary>
    public interface IQueryToken
    {
        /// <summary>
        /// Converts the query token to a Calabash query string.
        /// </summary>
        /// <param name="queryPlatform">The target query platform.</param>
        /// <returns>A valid Calabash query string.</returns>
        string ToQueryString(QueryPlatform queryPlatform);
        
        /// <summary>
        /// Returns a string representation of the code representing the query. Used for output. 
        /// </summary>
        /// <returns>A string representation of the query code.</returns>
        string ToCodeString();
    }
}