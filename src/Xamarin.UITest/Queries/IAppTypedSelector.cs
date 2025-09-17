namespace Xamarin.UITest.Queries
{
    /// <summary>
    /// Helper interface for exposing property from the fluent query API without cluttering the fluent API itself (when using explicit interface implementation). 
    /// </summary>
    public interface IAppTypedSelector : ITokenContainer
    {
        /// <summary>
        /// The query parameters.
        /// </summary>
        object[] QueryParams { get; }
        
        /// <summary>
        /// The app query.
        /// </summary>
        AppQuery AppQuery { get; }

        /// <summary>
        /// The value of the query was explicitly requested.
        /// </summary>
        bool ExplicitlyRequestedValue { get; }
    }
}