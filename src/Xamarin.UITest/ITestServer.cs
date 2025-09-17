namespace Xamarin.UITest
{
    /// <summary>
    /// Provides access to the test server running on the device for advanced scenarios.
    /// </summary>
    public interface ITestServer
    {
        /// <summary>
        /// Performs an HTTP POST request to the test server.
        /// </summary>
        /// <param name="endpoint">The path of the request, which will be appended on the uri of the test server.</param>
        /// <param name="arguments">An object that will be serialized as json and posted to the test server.</param>
        /// <returns>The body of the the reponse.</returns>
        string Post(string endpoint, object arguments = null);

        /// <summary>
        /// Performs an HTTP PUT request to the test server.
        /// </summary>
        /// <param name="endpoint">The path of the request, which will be appended on the uri of the test server.</param>
        /// <param name = "data">The byte[] data to put.</param>
        /// <returns>The body of the the reponse.</returns>
        string Put(string endpoint, byte[] data);

        /// <summary>
        /// Performs an HTTP GET request to the test server.
        /// </summary>
        /// <param name="endpoint">The path of the request, which will be appended on the uri of the test server.</param>
        /// <returns>The body of the the reponse.</returns>
        string Get(string endpoint);
    }
}