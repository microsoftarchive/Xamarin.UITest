namespace Xamarin.UITest.Configuration
{

    /// <summary>
    /// Enum for controlling whether or not to clear app data.
    /// </summary>
    public enum AppDataMode
    {
        /// <summary>
        /// Default behavior depending on platform.
        /// </summary>
        Auto,

        /// <summary>
        /// Always clear app data.
        /// </summary>
        Clear,

        /// <summary>
        /// Never clear app data.
        /// </summary>
        DoNotClear,
    }
}