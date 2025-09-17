using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Threading;
using Xamarin.UITest.Shared.Extensions;
using Xamarin.UITest.Shared.Json;
using Xamarin.UITest.Shared.Logging;
using System.Threading.Tasks;

namespace Xamarin.UITest.Shared.Http
{
    public class HttpClient
    {
        static readonly System.Net.Http.HttpClient _HttpClient = new System.Net.Http.HttpClient()
        {
            Timeout = Timeout.InfiniteTimeSpan
        };

        static readonly TimeSpan _DefaultHttpTimeout = TimeSpan.FromSeconds(100);

        readonly Uri _baseUri;
        readonly Dictionary<string, string> _extraHeaders;
        readonly JsonTranslator _jsonTranslator = new JsonTranslator();
        readonly MultipartFormDataClient _formDataClient;

        public HttpClient(Uri baseUri = null, Dictionary<string, string> extraHeaders = null)
        {
            _extraHeaders = extraHeaders ?? new Dictionary<string, string>();
            _baseUri = baseUri ?? new Uri("http://127.0.0.1:80");
            _formDataClient = new MultipartFormDataClient(_baseUri, _extraHeaders);
        }

        public HttpResult Get(string endpoint, ExceptionPolicy exceptionPolicy = ExceptionPolicy.Throw, TimeSpan? timeOut = null)
        {
            return Request("GET", endpoint, exceptionPolicy, timeOut);
        }

        public HttpResult Delete(string endpoint, ExceptionPolicy exceptionPolicy = ExceptionPolicy.Throw, TimeSpan? timeOut = null)
        {
            return Request("DELETE", endpoint, exceptionPolicy, timeOut);
        }

        HttpResult Request(string method, string endpoint, ExceptionPolicy exceptionPolicy = ExceptionPolicy.Throw, TimeSpan? timeOut = null)
        {
            var url = _baseUri.Combine(endpoint);

            string methodName = method.ToUpperInvariant();
            HttpMethod httpMethod = new HttpMethod(methodName);
            using (HttpRequestMessage request = new HttpRequestMessage(httpMethod, url))
            {
                request.Headers.Add("ContentType", "application/json;charset=utf-8");

                foreach (var extraHeader in _extraHeaders)
                {
                    request.Headers.Add(extraHeader.Key, extraHeader.Value);
                }

                Log.Debug("HTTP " + methodName, new { Url = url });

                try
                {
                    HttpResponseMessage response = null;

                    try
                    {
                        try
                        {
                            var cts = new CancellationTokenSource();
                            cts.CancelAfter(timeOut.HasValue ? timeOut.Value : _DefaultHttpTimeout);
                            response = _HttpClient.SendAsync(request, cts.Token).Result;
                        }
                        catch (AggregateException ex)
                        {
                            if (ex.InnerExceptions.Count == 1)
                            {
                                throw ex.InnerExceptions.First();
                            }

                            throw;
                        }

                        if (response.IsSuccessStatusCode)
                        {
                            HttpResult result = new HttpResult((int)response.StatusCode, response.Content.ReadAsStringAsync().Result);
                            Log.Debug(methodName + " Complete", result);
                            return result;
                        }
                        else
                        {
                            return HandleHttpError(methodName, response, exceptionPolicy);
                        }
                    }
                    finally
                    {
                        if (response != null)
                        {
                            response.Dispose();
                        }
                    }
                }
                catch (TaskCanceledException e) // Timeout
                {
                    return HandleHttpError(methodName, e, exceptionPolicy);
                }
            }
        }

        public HttpResult PostUntilExpectedResultReceived(string endpoint, string arguments, string expectedContent, int retries)
        {
            HttpResult httpResult = null;
            int counter = 0;
            string errorMessage = $"Post to endpoint '{endpoint}' failed after {retries} retries.";

            while (counter < retries)
            {
                try
                {
                    httpResult = Post(endpoint, arguments);

                    if (httpResult?.Contents == expectedContent)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    var retryExceptions = new[]
                    {
                        WebExceptionStatus.ReceiveFailure,
                        WebExceptionStatus.ConnectionClosed,
                        WebExceptionStatus.ConnectFailure,
                        WebExceptionStatus.KeepAliveFailure,
                        WebExceptionStatus.UnknownError, // Thrown on Linux if adb has not completed its port forward
                    };
                    if (ShouldRetryOnWebExceptionStatus(e, retryExceptions))
                    {
                        counter++;
                        Thread.Sleep(TimeSpan.FromMilliseconds(100));
                        continue;
                    }
                    throw;
                }
            }

            if (httpResult == null)
            {
                throw new Exception($"{errorMessage} No http result received");
            }

            if (httpResult.Contents != expectedContent)
            {
                var message = $"{errorMessage} Actual content {httpResult.Contents} did not match expectation {expectedContent}.";
                throw new Exception(message);
            }

            return httpResult;
        }

        public HttpResult Post(string endpoint, object arguments, ExceptionPolicy exceptionPolicy = ExceptionPolicy.Throw, TimeSpan? timeOut = null)
        {
            var encodedArgs = arguments == null ? null : _jsonTranslator.Serialize(arguments);
            return Post(endpoint, encodedArgs, exceptionPolicy, timeOut);
        }

        public HttpResult Post(string endpoint, string arguments, ExceptionPolicy exceptionPolicy = ExceptionPolicy.Throw, TimeSpan? timeOut = null)
        {
            if (arguments != null)
            {
                var contents = new StringContent(arguments, System.Text.Encoding.UTF8, "application/json");
                return SendData(endpoint, "POST", contents, exceptionPolicy, timeOut);
            }
            else
            {
                return SendData(endpoint, "POST", null, exceptionPolicy, timeOut);
            }
        }

        public HttpResult Put(string endpoint, ExceptionPolicy exceptionPolicy = ExceptionPolicy.Throw, TimeSpan? timeOut = null)
        {
            return Put(endpoint, new byte[0], exceptionPolicy, timeOut);
        }

        public HttpResult Put(string endpoint, byte[] data, ExceptionPolicy exceptionPolicy = ExceptionPolicy.Throw, TimeSpan? timeOut = null)
        {
            return SendData(endpoint, "PUT", new ByteArrayContent(data), exceptionPolicy, timeOut);
        }

        public HttpResult SendData(string endpoint, string method, HttpContent content, ExceptionPolicy exceptionPolicy, TimeSpan? timeOut)
        {
            var methodName = method.ToUpperInvariant();

            using (var request = CreateHttpRequest(endpoint, methodName, content))
            {
                try
                {
                    HttpResponseMessage response = null;

                    try
                    {
                        try
                        {
                            var cts = new CancellationTokenSource();
                            cts.CancelAfter(timeOut.HasValue ? timeOut.Value : _DefaultHttpTimeout);

                            try
                            {
                                response = _HttpClient.SendAsync(request, cts.Token).Result;
                            }
                            catch (AggregateException ex)
                            {
                                // We have seen ObjectDisposedExceptions when using mono. Retrying succeeds.
                                var isObjectDisposed = ex.InnerExceptions.First() is ObjectDisposedException;

                                // Could be a transient network issue
                                var isHttpRequestException = ex.InnerExceptions.First() is HttpRequestException;

                                if (isObjectDisposed || isHttpRequestException)
                                {
                                    var cts2 = new CancellationTokenSource();
                                    cts2.CancelAfter(timeOut.HasValue ? timeOut.Value : _DefaultHttpTimeout);

                                    // content may be disposed: https://github.com/dotnet/corefx/issues/1794
                                    // in that case we will get an ObjectDisposedException here
                                    try
                                    {
                                        using (var request2 = CreateHttpRequest(endpoint, method, content))
                                        {
                                            response = _HttpClient.SendAsync(request2, cts2.Token).Result;
                                        }
                                    }
                                    catch (AggregateException secondEx)
                                    {
                                        var isObjectDisposed2 = secondEx.InnerExceptions.First() is ObjectDisposedException;

                                        if (isObjectDisposed2)
                                        {
                                            throw ex;
                                        }

                                        throw;
                                    }
                                }
                                else
                                {
                                    throw;
                                }
                            }
                        }
                        catch (AggregateException ex)
                        {
                            if (ex.InnerExceptions.Count == 1)
                            {
                                throw ex.InnerExceptions.First();
                            }

                            throw;
                        }

                        if (response.IsSuccessStatusCode)
                        {
                            String contents = response.Content.ReadAsStringAsync().Result;
                            Log.Debug(methodName + " Complete", new { response.StatusCode, StatusDescription = response.ReasonPhrase, ResponseHeaders = response.Headers.ToString(), Contents = contents });
                            return new HttpResult((int)response.StatusCode, contents);
                        }
                        else
                        {
                            return HandleHttpError(methodName, response, exceptionPolicy);
                        }
                    }
                    finally
                    {
                        if (response != null)
                        {
                            response.Dispose();
                        }
                    }
                }
                catch (TaskCanceledException e) // Timeout
                {
                    return HandleHttpError(methodName, e, exceptionPolicy);
                }
            }
        }

        public BinaryResult GetBinaryFile(string endpoint, string fileName, string queryString = null, ExceptionPolicy exceptionPolicy = ExceptionPolicy.Throw)
        {
            var url = _baseUri.Combine(endpoint);

            HttpMethod httpMethod = new HttpMethod("GET");
            using (HttpRequestMessage request = new HttpRequestMessage(httpMethod, url))
            {
                request.Headers.Add("ContentType", "application/json;charset=utf-8");

                foreach (var extraHeader in _extraHeaders)
                {
                    request.Headers.Add(extraHeader.Key, extraHeader.Value);
                }

                Log.Debug("HTTP GET BINARY", new { Url = url });

                HttpResponseMessage response = null;

                try
                {
                    try
                    {
                        response = _HttpClient.SendAsync(request).Result;
                    }
                    catch (AggregateException ex)
                    {
                        if (ex.InnerExceptions.Count == 1)
                        {
                            throw ex.InnerExceptions.First();
                        }

                        throw;
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        using (var responseStream = response.Content.ReadAsStreamAsync().Result)
                        {
                            using (var fileStream = File.OpenWrite(fileName))
                            {
                                responseStream.CopyTo(fileStream);

                                fileStream.Flush();
                            }

                            var fileInfo = new FileInfo(fileName);
                            Log.Debug("GET BINARY Complete", new { FileName = fileName, FileSize = fileInfo.Length });

                            var headers = response.Headers.ToDictionary(pair => pair.Key, pair => String.Join(" ", pair.Value.ToArray()));
                            return new BinaryResult((int)response.StatusCode, fileInfo, headers);
                        }
                    }
                    else
                    {
                        if (exceptionPolicy != ExceptionPolicy.CatchServerHttpErrors)
                        {
                            string message = "GET BINARY Failed";
                            Log.Debug(message, new { StatusCode = (int)response.StatusCode, StatusDescription = response.ReasonPhrase, ResponseHeaders = response.Headers.ToString() });
                            throw new WebException(message);
                        }

                        return new BinaryResult(0, null, null);
                    }
                }
                finally
                {
                    if (response != null)
                    {
                        response.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// WARNING!
        ///
        /// Use PostMultipartWithRetry with caution!
        ///
        /// It uses HttpWebRequest which is known to have race conditions. Take care when using in multi threaded scenarios especially.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="parameters"></param>
        /// <param name="progressReporter"></param>
        /// <param name="exceptionPolicy"></param>
        /// <param name="timeOut"></param>
        /// <param name="numberOfRetries"></param>
        /// <returns></returns>
        public HttpResult PostMultipartWithRetry(string endpoint, Dictionary<string, object> parameters, IUploadProgressReporter progressReporter = null, ExceptionPolicy exceptionPolicy = ExceptionPolicy.Throw, TimeSpan? timeOut = null, int numberOfRetries = 5)
        {
            return _formDataClient.PostMultipartWithRetry(endpoint, parameters, progressReporter, exceptionPolicy, timeOut, numberOfRetries);
        }

        /// <summary>
        /// WARNING!
        ///
        /// Use PostMultipart with caution!
        ///
        /// It uses HttpWebRequest which is known to have race conditions. Take care when using in multi threaded scenarios especially.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="parameters"></param>
        /// <param name="progressReporter"></param>
        /// <param name="exceptionPolicy"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public HttpResult PostMultipart(string endpoint, Dictionary<string, object> parameters, IUploadProgressReporter progressReporter = null, ExceptionPolicy exceptionPolicy = ExceptionPolicy.Throw, TimeSpan? timeOut = null)
        {
            return _formDataClient.PostMultipart(endpoint, parameters, progressReporter, exceptionPolicy, timeOut);
        }

        HttpResult HandleHttpError(string method, HttpResponseMessage response, ExceptionPolicy exceptionPolicy)
        {
            var debugInfo = new
            {
                StatusCode = (int)response.StatusCode,
                StatusDescription = response.ReasonPhrase,
                ResponseHeaders = response.Headers.ToString(),
                Content = response.Content.ReadAsStringAsync().Result
            };

            if (exceptionPolicy != ExceptionPolicy.CatchServerHttpErrors)
            {
                Log.Debug(method + " Failed", debugInfo);
                throw new WebException(method + " Failed");
            }

            Log.Debug(method + " Complete", debugInfo);
            return new HttpResult(debugInfo.StatusCode, debugInfo.StatusDescription);
        }

        HttpResult HandleHttpError(string method, Exception exception, ExceptionPolicy exceptionPolicy)
        {
            var debugInfo = new
            {
                StatusCode = 0,
                StatusDescription = exception.Message,
                ResponseHeaders = "",
                Content = ""
            };

            if (exceptionPolicy != ExceptionPolicy.CatchServerHttpErrors)
            {
                Log.Debug(method + " Failed", debugInfo);
                throw new WebException(method + " Failed");
            }

            Log.Debug(method + " Complete", debugInfo);
            return new HttpResult(debugInfo.StatusCode, debugInfo.StatusDescription);
        }

        HttpRequestMessage CreateHttpRequest(string endpoint, string method, HttpContent content)
        {
            var url = _baseUri.Combine(endpoint);

            var request = new HttpRequestMessage(new HttpMethod(method), url);

            foreach (var extraHeader in _extraHeaders)
            {
                request.Headers.Add(extraHeader.Key, extraHeader.Value);
            }

            Log.Debug("HTTP " + method, new { Url = url });

            if (content != null)
            {
                request.Content = content;
            }

            return request;
        }

        bool ShouldRetryOnWebExceptionStatus(Exception ex, IList<WebExceptionStatus> status)
        {
            var webException = ex is WebException ?
                ex as WebException : //Mono
                ex.InnerException as WebException; //.NET

            //Hack to get around VS 4 Mac beta channel weirdness.
            //The returned exception no longer includes the InnerException as a WebException.
            var httpException = ex as HttpRequestException;
            var innerIOException = ex.InnerException as IOException;

            if (httpException != null && innerIOException != null)
            {
                return true;
            }

            if (webException != null && status.Contains(webException.Status))
            {
                return true;
            }

            return false;
        }
    }
}
