using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using Xamarin.UITest.Shared.Extensions;
using Xamarin.UITest.Shared.Json;
using Xamarin.UITest.Shared.Logging;

namespace Xamarin.UITest.Shared.Http
{
    internal class MultipartFormDataClient
    {
        readonly Uri _baseUri;
        readonly Dictionary<string, string> _extraHeaders;
        readonly JsonTranslator _jsonTranslator = new JsonTranslator();

        internal MultipartFormDataClient(Uri baseUri = null, Dictionary<string, string> extraHeaders = null)
        {
            _extraHeaders = extraHeaders ?? new Dictionary<string, string>();
            _baseUri = baseUri ?? new Uri("http://127.0.0.1:80");
        }

        internal HttpResult PostMultipartWithRetry(string endpoint, Dictionary<string, object> parameters, IUploadProgressReporter progressReporter = null, ExceptionPolicy exceptionPolicy = ExceptionPolicy.Throw, TimeSpan? timeOut = null, int numberOfRetries = 5)
        {
            var retry = numberOfRetries;
            Exception latestException;
            do
            {
                try
                {
                    return PostMultipart(endpoint, parameters, progressReporter, exceptionPolicy, timeOut);
                }
                catch (Exception e)
                {
                    latestException = e;
                }
            } while (retry-- > 0);
            throw latestException;
        }


        internal HttpResult PostMultipart(string endpoint, Dictionary<string, object> parameters, IUploadProgressReporter progressReporter = null, ExceptionPolicy exceptionPolicy = ExceptionPolicy.Throw, TimeSpan? timeOut = null)
        {
            var uri = _baseUri.Combine(endpoint);
// We will suppress the SYSLIB0014 warning for now.
// TODO: Migrate from HttpWebRequest to HttpClient.
#pragma warning disable SYSLIB0014
            var request = (HttpWebRequest)WebRequest.Create(uri);
#pragma warning restore SYSLIB0014

            if (timeOut != null)
            {
                request.Timeout = (int)timeOut.Value.TotalMilliseconds;
            }

            foreach (var extraHeader in _extraHeaders)
            {
                request.Headers[extraHeader.Key] = extraHeader.Value;
            }

            var boundary = String.Format("--{0}--", DateTime.Now.Ticks.ToString("x"));
            request.ContentType = string.Format("multipart/form-data; boundary={0}", boundary);

            Log.Debug("HTTP POST (multi-part)", new { Url = uri, Arguments = _jsonTranslator.Serialize(parameters) });

            request.Method = "POST";
            request.KeepAlive = true;
            request.AllowWriteStreamBuffering = false;
            request.ContentLength = GetContentLength(parameters, boundary);
            request.Credentials = CredentialCache.DefaultCredentials;

            if (parameters != null && parameters.Count > 0)
            {
                using (var requestStream = request.GetRequestStream())
                {
                    WriteToStream(parameters, requestStream, GetBoundaryBytes(boundary), GetTrailerBytes(boundary), progressReporter);
                }
            }

            try
            {
                var result = GetResponseViaRequest(request);
                Log.Debug("POST (multi-part) Complete", result);
                return result;
            }
            catch (WebException ex)
            {
                var response = ex.Response as HttpWebResponse;

                if (response == null)
                {
                    Log.Debug("POST (multi-part) Failed. No HttpWebResponse");
                    throw;
                }

                if (exceptionPolicy != ExceptionPolicy.CatchServerHttpErrors)
                {
                    Log.Debug("POST (multi-part) Failed", new { StatusCode = (int)response.StatusCode, StatusDescription = response.StatusDescription, ResponseHeaders = response.Headers.ToString() });
                    throw;
                }

                var result = GetResponseAsResult(response);
                Log.Debug("POST (multi-part) Complete", result);
                return result;
            }
        }


        long GetContentLength(Dictionary<string, object> parameters, string boundary)
        {
            using (var countingStream = new ByteCountingStream())
            {
                WriteToStream(parameters, countingStream, GetBoundaryBytes(boundary), GetTrailerBytes(boundary));
                return countingStream.Length;
            }
        }

        HttpResult GetResponseViaRequest(HttpWebRequest request)
        {
            using (var response = request.GetResponse() as HttpWebResponse)
            {
                return GetResponseAsResult(response);
            }
        }

        HttpResult GetResponseAsResult(HttpWebResponse response)
        {
            using (var responseStream = response.GetResponseStream())
            {
                using (var streamReader = new StreamReader(responseStream))
                {
                    var statusCode = (int)response.StatusCode;
                    var contents = streamReader.ReadToEnd();
                    return new HttpResult(statusCode, contents);
                }
            }
        }

        void WriteToStream(Dictionary<string, object> parameters, Stream stream, byte[] boundaryBytes, byte[] trailerBytes, IUploadProgressReporter progressReporter = null)
        {
            foreach (KeyValuePair<string, object> pair in parameters)
            {
                if (pair.Value == null)
                {
                    continue;
                }

                if (pair.Value is FormFile)
                {
                    stream.Write(boundaryBytes, 0, boundaryBytes.Length);
                    WriteFile(pair.Value as FormFile, pair.Key, stream, progressReporter);
                }
                else if (pair.Value is object[])
                {
                    string name = pair.Key + "[]";
                    var objs = pair.Value as object[];

                    foreach (var obj in objs)
                    {
                        stream.Write(boundaryBytes, 0, boundaryBytes.Length);

                        if (obj is FormFile)
                        {
                            WriteFile(obj as FormFile, name, stream, progressReporter);
                        }
                        else if (obj is string)
                        {
                            WriteValue(name, obj as string, stream);
                        }
                    }
                }
                else if (pair.Value is Dictionary<string, object>)
                {
                    var dictionary = pair.Value as Dictionary<string, object>;

                    foreach (var innerPair in dictionary)
                    {
                        if (innerPair.Value is string)
                        {
                            var name = string.Format("{0}[{1}]", pair.Key, innerPair.Key);
                            var value = innerPair.Value as string;

                            stream.Write(boundaryBytes, 0, boundaryBytes.Length);
                            WriteValue(name, value, stream);
                        }
                        else if (innerPair.Value is string[])
                        {
                            var name = string.Format("{0}[{1}][]", pair.Key, innerPair.Key);
                            var value = innerPair.Value as string[];

                            foreach (var s in value)
                            {
                                stream.Write(boundaryBytes, 0, boundaryBytes.Length);
                                WriteValue(name, s, stream);
                            }
                        }
                        else if (innerPair.Value is Dictionary<string, Dictionary<string, string[]>>)
                        {
                            var value = innerPair.Value as Dictionary<string, Dictionary<string, string[]>>;

                            foreach (var pair1 in value)
                            {
                                foreach (var pair2 in pair1.Value)
                                {
                                    var name = string.Format("{0}[{1}][{2}][{3}][]", pair.Key, innerPair.Key, pair1.Key, pair2.Key);

                                    foreach (var s in pair2.Value)
                                    {
                                        stream.Write(boundaryBytes, 0, boundaryBytes.Length);
                                        WriteValue(name, s, stream);
                                    }
                                }
                            }
                        }
                        else
                        {
                            throw new Exception("Unable to write unknown type: " + innerPair.Value.GetType().FullName);
                        }
                    }
                }
                else
                {
                    stream.Write(boundaryBytes, 0, boundaryBytes.Length);
                    WriteValue(pair.Key, pair.Value.ToString(), stream);
                }
            }

            stream.Write(trailerBytes, 0, trailerBytes.Length);

            stream.Flush();
        }

        byte[] GetBoundaryBytes(string boundary)
        {
            return Encoding.ASCII.GetBytes(string.Format("\r\n--{0}\r\n", boundary));
        }

        byte[] GetTrailerBytes(string boundary)
        {
            return Encoding.ASCII.GetBytes(string.Format("\r\n--{0}--\r\n", boundary));
        }

        void WriteValue(string name, string value, Stream requestStream)
        {
            string data = string.Format("Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}", name, value);
            byte[] bytes = Encoding.UTF8.GetBytes(data);

            requestStream.Write(bytes, 0, bytes.Length);
        }

        void WriteFile(FormFile file, string name, Stream requestStream, IUploadProgressReporter progressReporter)
        {
            var fileName = Path.GetFileName(file.FilePath);

            if (progressReporter != null)
            {
                progressReporter.UploadStart(fileName);
            }

            try
            {
                var header = string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n", name, file.Name, file.ContentType);
                var bytes = Encoding.UTF8.GetBytes(header);
                requestStream.Write(bytes, 0, bytes.Length);
                var buffer = new byte[32768];

                using (var fileStream = File.OpenRead(file.FilePath))
                {
                    int bytesRead, totalBytesWritten = 0;

                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        requestStream.Write(buffer, 0, bytesRead);
                        totalBytesWritten += bytesRead;

                        if (progressReporter != null)
                        {
                            progressReporter.UploadProgress(fileName, totalBytesWritten, fileStream.Length);
                        }
                    }
                }
            }
            catch (Exception)
            {
                if (progressReporter != null)
                {
                    progressReporter.UploadError(fileName);
                }
                throw;
            }

            if (progressReporter != null)
            {
                progressReporter.UploadComplete(fileName);
            }
        }
    }
}
