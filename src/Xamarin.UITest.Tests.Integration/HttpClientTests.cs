using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Xamarin.UITest.Shared.Extensions;
using Xamarin.UITest.Shared.Http;

namespace Xamarin.UITest.Tests.Integration
{
    class HttpClientTests
    {
        Uri _testServerUri;

        HttpClient _httpClient;
        HttpEchoServer _echoServer;

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            int echoServerPort;
            var portIncrement = 0;

            while (true)
            {
                echoServerPort = 8747 + portIncrement;

                try
                {
                    _echoServer = new HttpEchoServer(echoServerPort);
                }
                catch (Exception ex)
                {
                    if (!(ex is SocketException))
                    {
                        throw ex;
                    }
                    if (portIncrement > 20)
                    {
                        throw ex;
                    }

                    portIncrement++;
                }

                break;
            }

            _testServerUri = new Uri($"http://127.0.0.1:{echoServerPort}");

            _httpClient = new HttpClient(
                _testServerUri, 
                new Dictionary<string, string> {{"My-Header", "My-Header-value"}});
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            _echoServer.Terminate();
        }

        [Test]
        public void GetTest()
        {
            var endpoint = "/get";

            var response = _httpClient.Get(endpoint);

            Assert.AreEqual(200, response.StatusCode);

            var result = JObject.Parse(response.Contents);
            
            Assert.AreEqual(_testServerUri.Combine(endpoint), result["url"].Value<string>());

            Assert.AreEqual(
                "My-Header-value",
                result["headers"].Value<JObject>()["My-Header"].Value<string>());

        }

        [Test]
        public void GetErrorTest()
        {
            var endpoint = "/status/500";

            Assert.Throws<WebException>(delegate { _httpClient.Get(endpoint); });
        }

        [Test]
        public void DeleteTest()
        {
            var endpoint = "/delete";

            var response = _httpClient.Delete(endpoint);

            Assert.AreEqual(200, response.StatusCode);

            var result = JObject.Parse(response.Contents);

            Assert.AreEqual(_testServerUri.Combine(endpoint), result["url"].Value<string>());

            Assert.AreEqual(
                "My-Header-value",
                result["headers"].Value<JObject>()["My-Header"].Value<string>());
        }

        [Test]
        public void DeleteErrorTest()
        {
            var endpoint = "/status/500";

            Assert.Throws<WebException>(delegate { _httpClient.Delete(endpoint); });
        }

        [Test]
        public void PostObjectTest()
        {
            var endpoint = "/post";

            var arguments = new
                {
                    query = "my_query",
                    operation = new { method_name = "my_method", args = new object[] {} }
                };

            var response = _httpClient.Post(endpoint, arguments);

            Assert.AreEqual(200, response.StatusCode);

            var result = JObject.Parse(response.Contents);

            Assert.AreEqual(_testServerUri.Combine(endpoint), result["url"].Value<string>());

            Assert.AreEqual(
                "{\"query\":\"my_query\",\"operation\":{\"method_name\":\"my_method\",\"args\":[]}}",
                result["data"].Value<string>());

            Assert.AreEqual(
                "My-Header-value",
                result["headers"].Value<JObject>()["My-Header"].Value<string>());
        }

        [Test]
        public void PostStringTest()
        {
            var endpoint = "/post";

            var response = _httpClient.Post(
                endpoint, 
                "{\"query\":\"my_query\",\"operation\":{\"method_name\":\"my_method\",\"args\":[]}}");

            Assert.AreEqual(200, response.StatusCode);

            var result = JObject.Parse(response.Contents);

            Assert.AreEqual(_testServerUri.Combine(endpoint), result["url"].Value<string>());

            Assert.AreEqual(
                "{\"query\":\"my_query\",\"operation\":{\"method_name\":\"my_method\",\"args\":[]}}",
                result["data"].Value<string>());

            Assert.AreEqual(
                "My-Header-value",
                result["headers"].Value<JObject>()["My-Header"].Value<string>());
        }

        [Test]
        public void PostNullTest()
        {
            var endpoint = "/post";

            var response = _httpClient.Post(endpoint, null);

            Assert.AreEqual(200, response.StatusCode);

            var result = JObject.Parse(response.Contents);

            Assert.AreEqual(_testServerUri.Combine(endpoint), result["url"].Value<string>());

            Assert.IsFalse(
                result["data"].HasValues);

            Assert.AreEqual(
                "My-Header-value",
                result["headers"].Value<JObject>()["My-Header"].Value<string>());
        }

        [Test]
        public void PostErrorTest()
        {
            var endpoint = "/status/500";

            Assert.Throws<WebException>(delegate { _httpClient.Post(endpoint, null); });
        }

        [Test]
        public void PutTest()
        {
            var endpoint = "/put";

            var dataString = "my data";
            var dataBytes = new byte[dataString.Length * sizeof(char)];
            Buffer.BlockCopy(dataString.ToCharArray(), 0, dataBytes, 0, dataBytes.Length);

            var response = _httpClient.Put(endpoint, dataBytes);

            Assert.AreEqual(200, response.StatusCode);

            JObject result;

            try
            {
                result = JObject.Parse(response.Contents);
            }
            catch
            {
                throw new Exception("Unexpected response from EchoServer: " + response.Contents);
            }

            Assert.AreEqual(_testServerUri.Combine(endpoint), result["url"].Value<string>());

            Assert.AreEqual("m\u0000y\u0000 \u0000d\u0000a\u0000t\u0000a\u0000", result["data"].Value<string>());

            Assert.AreEqual(
                "My-Header-value",
                result["headers"].Value<JObject>()["My-Header"].Value<string>());
        }

        [Test]
        public void PutNullTest()
        {
            var endpoint = "/put";

            var response = _httpClient.Put(endpoint);

            Assert.AreEqual(200, response.StatusCode);

            var result = JObject.Parse(response.Contents);

            Assert.AreEqual(_testServerUri.Combine(endpoint), result["url"].Value<string>());

            Assert.IsFalse(
                result["data"].HasValues);

            Assert.AreEqual(
                "My-Header-value",
                result["headers"].Value<JObject>()["My-Header"].Value<string>());
        }

        [Test]
        public void PutErrorTest()
        {
            var endpoint = "/status/500";

            Assert.Throws<WebException>(delegate { _httpClient.Put(endpoint); });
        }

        [Test]
        public void GetBinaryFileTest()
        {
            var endpoint = "/image/png";

            var myFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".png");

            var response = _httpClient.GetBinaryFile(endpoint, myFile);

            Assert.AreEqual(200, response.StatusCode);

            var bmp = new Bitmap(response.File.FullName);

            Assert.IsTrue(bmp.Width > 0);
        }

        [Test]
        public void GetBinaryFileErrorTest()
        {
            var endpoint = "/status/500";

            Assert.Throws<WebException>(delegate { _httpClient.GetBinaryFile(endpoint, "dummy.txt"); });
        }

        [Test]
        public void PostMultipartTest()
        {
            PostMultipart((endPoint, parameters) => _httpClient.PostMultipart(endPoint, parameters));
        }

        [Test]
        public void PostMultipartWithRetryTest()
        {
            PostMultipart((endPoint, parameters) => _httpClient.PostMultipartWithRetry(endPoint, parameters));
        }

        public void PostMultipart(Func<string, Dictionary<string, object>, HttpResult> postMethod)
        {
            var endpoint = "/post";

            var myFile1 = Path.GetTempFileName();
            File.WriteAllText(myFile1, "my first file contents");

            var myFile2 = Path.GetTempFileName();
            File.WriteAllText(myFile2, "my second file contents");

            var parameters = new Dictionary<string, object>
            {
                { "param1", "param1 value" },
                { "param2", new FormFile(new FileInfo(myFile1)) },
                { "param3", new object[] { "1", "2", new FormFile(new FileInfo(myFile2)) } },
                {
                    "param4", new Dictionary<string, object>
                    {
                        { "innerParam1", "innerParam1 value" },
                        { "innerParam2", new [] { "one", "two", "three" } },
                        {
                            "innerParam3", new Dictionary<string, Dictionary<string, string[]>>
                            {
                                {
                                    "nested1", new Dictionary<string, string[]>
                                    {
                                        { "nestedChild1", new [] { "one", "two" } }
                                    }
                                }
                            }
                        }
                    }
                },

            };

            var response = postMethod(endpoint, parameters);

            Assert.AreEqual(200, response.StatusCode);

            var result = JObject.Parse(response.Contents);

            Assert.AreEqual(_testServerUri.Combine(endpoint), result["url"].Value<string>());

            var form = result["form"].Value<JObject>();

            Assert.AreEqual(
                "param1 value",
                form["param1"].Value<string>());

            Assert.AreEqual(
                "my first file contents",
                result["files"].Value<JObject>()["param2"].Value<string>());

            var param3 = form["param3[]"].Value<JArray>();

            Assert.AreEqual(2, param3.Count);
            Assert.AreEqual("1", param3[0].Value<string>());

            Assert.AreEqual(
                "my second file contents",
                result["files"].Value<JObject>()["param3[]"].Value<string>());

            Assert.AreEqual("innerParam1 value", form["param4[innerParam1]"].Value<string>());

            var innerParam2 = form["param4[innerParam2][]"].Value<JArray>();

            Assert.AreEqual(3, innerParam2.Count);
            Assert.AreEqual("one", innerParam2[0].Value<string>());

            var nestedChild1 = form["param4[innerParam3][nested1][nestedChild1][]"].Value<JArray>();

            Assert.AreEqual(2, nestedChild1.Count);
            Assert.AreEqual("one", nestedChild1[0].Value<string>());

            var contentType = result["headers"].Value<JObject>()["Content-Type"].Value<string>();

            Assert.IsTrue(contentType.StartsWith("multipart/form-data; boundary=--", StringComparison.InvariantCulture));

            Assert.AreEqual(
                "My-Header-value",
                result["headers"].Value<JObject>()["My-Header"].Value<string>());
        }

        [Test]
        public void PostMultipartErrorTest()
        {
            var endpoint = "/status/500";

            var parameters = new Dictionary<string, object>
            {
                {"param1", "param1 value"}
            };

            Assert.Throws<WebException>(delegate { _httpClient.PostMultipart(endpoint, parameters); });
        }

        [Test]
        public void PostMultipartWithRetryErrorTest()
        {
            var endpoint = "/status/500";

            var parameters = new Dictionary<string, object>
            {
                {"param1", "param1 value"}
            };

            Assert.Throws<WebException>(delegate { _httpClient.PostMultipartWithRetry(endpoint, parameters); });
        }
    }
}