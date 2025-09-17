namespace Xamarin.UITest.Configuration
{
    /// <summary>
    /// The desired state of the app after test initialization. Either the test framework can start the app or connect to an already running app in more advanced scenarios.
    /// </summary>
    public enum StartAction
    {
        /// <summary>
        /// Start the app after successful configuration.
        /// </summary>
        LaunchApp,

        /// <summary>
        /// Use the given configuration to connect to an already running app.
        /// </summary>
        ConnectToApp,
    }
}