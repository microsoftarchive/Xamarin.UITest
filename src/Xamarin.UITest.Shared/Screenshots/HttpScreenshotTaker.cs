using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Xamarin.UITest.Shared.Http;
using Xamarin.UITest.Shared.Logging;

namespace Xamarin.UITest.Shared.Screenshots
{
    internal class HttpScreenshotTaker(HttpClient httpClient) : IScreenshotTaker
    {
        private readonly HttpClient _httpClient = httpClient;
        private int _counter = 1;

        public FileInfo Screenshot(string title)
        {
            var screenshotName = string.Format("screenshot-{0}.png", _counter);

            var screenshotPath = new FileInfo(screenshotName).FullName;

            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                _httpClient.GetBinaryFile("/screenshot", screenshotPath);
            }
            catch (UnauthorizedAccessException)
            {
                var testDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                screenshotPath = Path.Combine(testDir, screenshotName);

                try
                {
                    _httpClient.GetBinaryFile("/screenshot", screenshotPath);
                }
                catch (UnauthorizedAccessException)
                {
                    var msg = $"Access to the path {screenshotPath} is denied. You may need to set the "
                        + "working directory, which can be done using System.IO.Directory.SetCurrentDirectory().";

                    throw new UnauthorizedAccessException(msg);
                }
            }

            stopwatch.Stop();

            Log.Info(
                "Took screenshot.",
                new { stopwatch.ElapsedMilliseconds, Path = screenshotPath, Title = title });

            _counter += 1;
            return new FileInfo(screenshotPath);
        }
    }
}