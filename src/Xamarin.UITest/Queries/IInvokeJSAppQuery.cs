namespace Xamarin.UITest.Queries
{
    /// <summary>
    /// Helper interface for exposing property from the fluent query API without cluttering the fluent API itself (when using explicit interface implementation). 
    /// </summary>
    interface IInvokeJSAppQuery : ITokenContainer
    {
        string Javascript { get; }
        AppQuery AppQuery { get; }
    }
}