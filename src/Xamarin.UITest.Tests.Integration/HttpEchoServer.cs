using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;

namespace Xamarin.UITest.Tests.Integration
{
    class HttpEchoServer
    {
        CancellationTokenSource _cancellationTokenSource;
        HttpListener _httpListener;
        string _baseUri;

        public HttpEchoServer(int port)
        {
            _baseUri = $"http://127.0.0.1:{port}/";

            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(_baseUri);
            _httpListener.Start();

            Task.Run(() =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        try
                        {
                            HandleRequest(_httpListener.GetContext());
                        }
                        catch (HttpListenerException)
                        {
                        }
                    }
                },
                token);
        }

        void HandleRequest(HttpListenerContext context)
        {
            Thread.Sleep(100);
            try
            {
                HandleRequestInner(context);
            }
            catch
            {
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                context.Response.OutputStream.Close();
            }    
        }

        void HandleRequestInner(HttpListenerContext context)
        {
            if (context.Request.RawUrl.StartsWith("/status/", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = int.Parse(context.Request.RawUrl.Substring(8));
            }

            if (context.Request.RawUrl.Equals("/image/png", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.ContentType = "image/png";
                Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("Xamarin.UITest.Tests.Integration.test-cloud-logo.png")
                    .CopyTo(context.Response.OutputStream);
                context.Response.OutputStream.Close();
                return;
            }

            if (new [] {"GET", "POST", "DELETE", "PUT"}.Any(
                method => context.Request.RawUrl.ToUpper() == $"/{method}" && context.Request.HttpMethod != method))
            {
                context.Response.StatusCode = 500;
                context.Response.OutputStream.Close();
                return;
            }

            dynamic result = new ExpandoObject();

            result.url = context.Request.Url;

            context.Response.ContentType = "application/json";

            result.headers = new Dictionary<string, string>();

            foreach (var key in context.Request.Headers.AllKeys)
            {
                result.headers.Add(key, context.Request.Headers[key]);
            }

            if (context.Request.RawUrl.Equals("/put", StringComparison.OrdinalIgnoreCase)
                || context.Request.RawUrl.Equals("/post", StringComparison.OrdinalIgnoreCase))
            {
                if (context.Request.ContentType != null 
                    && context.Request.ContentType.StartsWith("multipart/form-data; boundary="))
                {
                    result.form = new Dictionary<string, object>();
                    result.files = new Dictionary<string, string>();

                    var boundary = context.Request.ContentType.Substring(30);
                    var reader = new MultipartReader(boundary, context.Request.InputStream);
                    while (true)
                    {
                        var section = reader.ReadNextSectionAsync().Result;
                        if (section == null)
                        {
                            break;
                        }

                        var field = section.ContentDisposition.Split('"')[1];

                        if (section.ContentType == "application/octet-stream")
                        {
                            result.files.Add(field, StreamToString(section.Body));
                        }
                        else
                        {
                            if (field.EndsWith("[]"))
                            {
                                if (!result.form.ContainsKey(field))
                                {
                                    result.form.Add(field, new List<string>());
                                }

                                ((List<string>)result.form[field]).Add(StreamToString(section.Body));
                            }
                            else
                            {
                                result.form.Add(field, StreamToString(section.Body));
                            }
                        }
                    }
                }
                else
                {
                    result.data = StreamToString(context.Request.InputStream);
                }
            }

            using (var writer = new StreamWriter(context.Response.OutputStream))
            {
                writer.Write(JsonConvert.SerializeObject(result));
            }

            context.Response.OutputStream.Close();
        }

        static string StreamToString(Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }
        
        public void Terminate()
        {
            _httpListener.Stop();
            _cancellationTokenSource.Cancel();
        }
    }
}
