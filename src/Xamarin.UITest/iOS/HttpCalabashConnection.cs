using System;
using Xamarin.UITest.Shared.Http;

namespace Xamarin.UITest.iOS
{
    internal class HttpCalabashConnection : ICalabashConnection
    {
        readonly HttpClient _httpClient;

        public HttpCalabashConnection(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public HttpResult Map(object arguments)
        {
            return _httpClient.Post("/map", arguments);
        }

        public HttpResult Location(object arguments)
        {
            return _httpClient.Post("/location", arguments);
        }

        public HttpResult UIA(string command)
        {
            var arguments = new {
                command = command
            };

            return _httpClient.Post("/uia", arguments);
        }

        public HttpResult Condition(object condition)
        {
            return _httpClient.Post("condition", condition);
        }

        public HttpResult Backdoor(object condition)
        {
            return _httpClient.Post("backdoor", condition);
        }

        public HttpResult Dump()
        {
            return _httpClient.Get("dump");
        }

        public HttpResult Version()
        {
            return _httpClient.Get("/version", ExceptionPolicy.CatchServerHttpErrors, TimeSpan.FromSeconds(30));
        }

        public HttpResult Suspend(double seconds)
        {
            return _httpClient.Post("/suspend", new { duration = seconds });
        }

        public HttpResult Exit()
        {
            var arguments = new
            {
                post_resign_active_delay = 0.4,
                post_will_terminate_delay = 0.4,
                exit_code = 0
            };

            return _httpClient.Post("/exit", arguments, ExceptionPolicy.CatchServerHttpErrors, TimeSpan.FromSeconds(1));
        }

        public HttpResult ClearText()
        {
            return _httpClient.Get("clearText");
        }

    }
}