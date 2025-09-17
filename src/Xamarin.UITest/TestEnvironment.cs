namespace Xamarin.UITest
{
    /// <summary>
    /// Contains runtime information about the current test environment.
    /// </summary>
    public static class TestEnvironment
    {
        /// <summary>
        /// Provides the current runtime platform. Useful for complicated test setup scenarios.
        /// </summary>
        public static TestPlatform Platform
        {
            get 
            {
                return TestPlatform.Local; 
            }
        }
    }

    /// <summary>
    /// Enum containing the various different test platforms.
    /// </summary>
    public enum TestPlatform
    {
        /// <summary>
        /// A local test run.
        /// </summary>
        Local,
    }
}