using Xamarin.UITest.Shared.Http;
using System;

namespace Xamarin.UITest.Utils
{
    internal class SharedTestServer : ITestServer
    {
        readonly HttpClient _httpClient;

        public SharedTestServer(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Performs an HTTP POST request to the test server.
        /// </summary>
        /// <param name="endpoint">The path of the request, which will be appended on the uri of the test server.</param>
        /// <param name="arguments">An object that will be serialized as json and posted to the test server.</param>
        /// <returns>The body of the the reponse.</returns>
        public string Post(string endpoint, object arguments = null)
        {
            var result = _httpClient.Post(endpoint, arguments);
            return result.Contents;
        }

        /// <summary>
        /// Performs an HTTP GET request to the test server.
        /// </summary>
        /// <param name="endpoint">The path of the request, which will be appended on the uri of the test server.</param>
        /// <returns>The body of the the reponse.</returns>
        public string Get(string endpoint)
        {
            var result = _httpClient.Get(endpoint);
            return result.Contents;
        }

        /// <summary>
        /// Performs an HTTP PUT request to the test server.
        /// </summary>
        /// <param name="endpoint">The path of the request, which will be appended on the uri of the test server.</param>
        /// <param name = "data">The byte[] data to put.</param>
        /// <returns>The body of the the reponse.</returns>
        public string Put(string endpoint, byte[] data)
        {
            var result = _httpClient.Put(endpoint, data);
            return result.Contents;
        }
    }
}