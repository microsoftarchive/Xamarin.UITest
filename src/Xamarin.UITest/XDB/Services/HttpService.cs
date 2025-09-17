using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.UITest.XDB.Entities;
using Xamarin.UITest.XDB.Exceptions.Net;

namespace Xamarin.UITest.XDB.Services
{
    class HttpService : IHttpService, IDisposable
    {
        static readonly TimeSpan _defaultHttpTimeout = TimeSpan.FromSeconds(120);

        ILoggerService _loggerService;

        Lazy<HttpClient> _lazyHttpClient = new Lazy<HttpClient>(() =>
        {
            var client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );

            return client;
        });

        HttpClient _httpClient { get { return _lazyHttpClient.Value; } }

        public HttpService(ILoggerService loggerService)
        {
            _loggerService = loggerService;
        }

        public async Task<IHttpResult<T>> DeleteAsync<T>(
            string url,
            TimeSpan? timeout = null,
            int attempts = 1,
            TimeSpan? retryInterval = null,
            bool errorIfUnavailable = true,
            bool logErrors = true)
        {
            var eventId = _loggerService.GetEventId();

            _loggerService.LogDebug($"HTTP DELETE {url}", eventId);

            var result = await RequestAsync<T>(
                eventId,
                timeout,
                attempts,
                retryInterval,
                errorIfUnavailable,
                logErrors,
                async (token) =>
                {
                    return await _httpClient.DeleteAsync(url, token).ConfigureAwait(false);
                }).ConfigureAwait(false);

            _loggerService.LogDebug($"HTTP DELETE complete", eventId);

            return result;
        }

        public async Task<IHttpResult<T>> GetAsync<T>(
            string url,
            TimeSpan? timeout = null,
            int attempts = 1,
            TimeSpan? retryInterval = null,
            bool errorIfUnavailable = true,
            bool logErrors = true)
        {
            var eventId = _loggerService.GetEventId();

            _loggerService.LogDebug($"HTTP GET {url}", eventId);

            var result = await RequestAsync<T>(
                eventId,
                timeout,
                attempts,
                retryInterval,
                errorIfUnavailable,
                logErrors,
                async (token) =>
                {
                    return await _httpClient.GetAsync(url, token).ConfigureAwait(false);
                }).ConfigureAwait(false);

            _loggerService.LogDebug($"HTTP GET complete", eventId);

            return result;
        }

        public async Task<IHttpResult<T>> PostAsync<T>(
            string url,
            TimeSpan? timeout = null,
            int attempts = 1,
            TimeSpan? retryInterval = null,
            bool errorIfUnavailable = true,
            bool logErrors = true)
        {
            var eventId = _loggerService.GetEventId();

            _loggerService.LogDebug($"HTTP POST {url}", eventId);

            var result = await RequestAsync<T>(
                eventId,
                timeout,
                attempts,
                retryInterval,
                errorIfUnavailable,
                logErrors,
                async (token) =>
                {
                    return await _httpClient.PostAsync(url, null, token).ConfigureAwait(false);
                }).ConfigureAwait(false);

            _loggerService.LogDebug($"HTTP POST complete", eventId);

            return result;
        }

        public async Task<IHttpResult<T>> PostAsJsonAsync<T>(
            string url,
            object payload,
            TimeSpan? timeout = null,
            int attempts = 1,
            TimeSpan? retryInterval = null,
            bool errorIfUnavailable = true,
            bool logErrors = true)
        {
            var eventId = _loggerService.GetEventId();

            _loggerService.LogDebug($"HTTP POST {url}", eventId, payload);

            var result = await RequestAsync<T>(
                eventId,
                timeout,
                attempts,
                retryInterval,
                errorIfUnavailable,
                logErrors,
                async (token) =>
                {
                    return await _httpClient.PostAsJsonAsync(url, payload, token).ConfigureAwait(false);
                }).ConfigureAwait(false);

            _loggerService.LogDebug($"HTTP POST complete", eventId);

            return result;
        }

        public void Dispose()
        {
            if (_lazyHttpClient.IsValueCreated)
            {
                _lazyHttpClient.Value.Dispose();
            }
        }

        async Task<IHttpResult<T>> RequestAsync<T>(
            int eventId,
            TimeSpan? timeout,
            int attempts,
            TimeSpan? retryInterval,
            bool errorIfUnavailable,
            bool logErrors,
            Func<CancellationToken, Task<HttpResponseMessage>> request)
        {
            for (var i = 1; i <= attempts; i++)
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(timeout.HasValue ? timeout.Value : _defaultHttpTimeout);

                try
                {
                    using (var response = await ExecuteRequestAsync(request, cts.Token).ConfigureAwait(false))
                    {
                        _loggerService.LogDebug("HTTP request succeeded", eventId);

                        T content = default(T);

                        try
                        {
                            content = await response.Content.ReadAsAsync<T>().ConfigureAwait(false);
                        }
                        catch (System.Exception ex)
                        {
                            try
                            {
                                throw new UnexpectedResponseException(
                                    await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                            }
                            catch (System.Exception)
                            {
                                throw ex;
                            }
                        }

                        return new HttpResult<T>
                        {
                            StatusCode = response.StatusCode,
                            Content = content
                        };
                    }
                }
                catch (Exception ex)
                {
                    if (i >= attempts)
                    {
                        if (!errorIfUnavailable)
                        {
                            return new HttpResult<T>
                            {
                                StatusCode = HttpStatusCode.RequestTimeout,
                                Content = default(T)
                            };
                        }

                        if (logErrors)
                        {
                            _loggerService.LogError("HTTP request failed, retry limit hit", eventId, ex);
                        }
                        throw;
                    }

                    if (logErrors)
                    {
                        _loggerService.LogDebug($"HTTP request failed: {ex.GetType()}: {ex.Message}", eventId);
                    }

                    if (retryInterval.HasValue)
                    {
                        Thread.Sleep((int)retryInterval.Value.TotalMilliseconds);
                    }
                }
            }

            return null; // We should _never_ get here
        }

        async Task<HttpResponseMessage> ExecuteRequestAsync(
            Func<CancellationToken, Task<HttpResponseMessage>> request, CancellationToken token)
        {
            try
            {
                return await request(token).ConfigureAwait(false);
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Count == 1
                    && ex.InnerExceptions.First() is ObjectDisposedException)
                {
                    // TODO: Remove/improve this workaround for suspected mono HttpClient issue
                    // https://xamarin.atlassian.net/browse/TCFW-262
                    {
                        return await request(token).ConfigureAwait(false);
                    }
                }

                throw;
            }
        }
    }
}