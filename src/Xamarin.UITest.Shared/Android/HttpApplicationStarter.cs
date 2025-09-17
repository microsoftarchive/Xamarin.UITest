using System;
using System.Net;
using Newtonsoft.Json;
using Xamarin.UITest.Shared.Http;

namespace Xamarin.UITest.Shared.Android
{
    public class HttpApplicationStarter
    {
        const string ReadyEndpoint = "/ready";
        const string ExpectedReadyResult = "true";
        const string PingEndpoint = "/ping";
        const string ExpectedPingResult = "pong";
        const string StartApplicationEndpoint = "/start-application";
        const string ExpectedOutcome = "SUCCESS";

        readonly HttpClient httpClient;

        public HttpApplicationStarter(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public void Execute(string intentJson = "{\"intent\":null }")
        {
            httpClient.PostUntilExpectedResultReceived(PingEndpoint, intentJson, ExpectedPingResult, 100);

            httpClient.PostUntilExpectedResultReceived(ReadyEndpoint, intentJson, ExpectedReadyResult, 300);

            var result = httpClient.Post(StartApplicationEndpoint, intentJson);

            string outcome = string.Empty;

            if (result.StatusCode != (int)HttpStatusCode.OK)
            {
                throw new Exception($"Starting application failed with contents '{result.Contents}'");
            }

            outcome = JsonConvert.DeserializeAnonymousType(result.Contents, new { Outcome = outcome }).Outcome;

            if (outcome == null)
            {
                throw new Exception($"Starting application failed due to unexpected response: {result.Contents}");
            }

            if (outcome != ExpectedOutcome)
            {
                throw new Exception($"Starting application failed with outcome '{outcome}'. Expected '{ExpectedOutcome}'.");
            }
        }
    }
}

