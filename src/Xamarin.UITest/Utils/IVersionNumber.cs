namespace Xamarin.UITest.Utils
{
    /// <summary>
    /// An object representing a version
    /// </summary>
    public interface IVersionNumber
    {
        /// <summary>
        /// The version's major number
        /// </summary>
        int Major { get; }

        /// <summary>
        /// The version's minor number
        /// </summary>
        int Minor { get; }
    }
}