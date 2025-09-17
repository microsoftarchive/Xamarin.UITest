using System;
using System.IO;
using System.Reflection;
using Xamarin.UITest.Shared.Android.Commands;
using Xamarin.UITest.Shared.Artifacts;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Logging;
using Xamarin.UITest.Shared.Resources;
using Xamarin.UITest.Shared.Screenshots;

namespace Xamarin.UITest.Android
{
    internal class JavaScreenshotTaker : IScreenshotTaker
    {
        readonly string _deviceSerial;
        readonly IExecutor _executor;
        readonly string _screenshotTakerJarPath;
        int _counter = 1;

        public JavaScreenshotTaker(ArtifactFolder artifactFolder, string deviceSerial, IExecutor executor)
        {
            _deviceSerial = deviceSerial;
            _executor = executor;
            _screenshotTakerJarPath = artifactFolder.CreateArtifact("screenshotTaker.jar", path =>
            {
                var resourceFinder = new EmbeddedResourceLoader();
                var jarBytes = resourceFinder.GetEmbeddedResourceBytes(typeof(AndroidApp).Assembly, "screenshotTaker.jar");
                File.WriteAllBytes(path, jarBytes);
            });
        }

        public FileInfo Screenshot(string title)
        {
            var screenshotName = string.Format("screenshot-{0}.png", _counter);

            var screenshotFile = new FileInfo(screenshotName);

            var screenshotPath = screenshotFile.FullName;

            _executor.Execute(
                new CommandJavaRunJar(_screenshotTakerJarPath, _deviceSerial + " \"" + screenshotPath + "\""));

            if (!screenshotFile.Exists)
            {
                var testDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                screenshotPath = Path.Combine(testDir, screenshotName);
                screenshotFile = new FileInfo(screenshotPath);

                _executor.Execute(
                    new CommandJavaRunJar(_screenshotTakerJarPath, _deviceSerial + " \"" + screenshotPath + "\""));

                if (!screenshotFile.Exists)
                {
                    var msg = $"Failed to create {screenshotPath}. You may need to set the "
                        + "working directory, which can be done using System.IO.Directory.SetCurrentDirectory().";

                    throw new Exception(msg);
                }
            }

            Log.Info("Took screenshot.", new { Path = screenshotPath, Title = title });

            _counter += 1;
            return new FileInfo(screenshotPath);
        }
    }
}