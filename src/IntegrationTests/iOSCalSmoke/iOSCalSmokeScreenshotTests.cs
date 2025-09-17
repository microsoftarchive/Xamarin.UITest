using NUnit.Framework;
using System.Drawing;
using Should;
using System.Diagnostics;
using IntegrationTests.Shared;
using System;
using Xamarin.UITest;
using System.IO;

public class iOSCalSmokeScreenshotTests : IOSTestBase
{
    protected override AppInformation _appInformation => TestApps.iOSCalSmoke;

    public override void BeforeEach()
    {
        _appConfiguration.EnableLocalScreenshots();
    }

    [Test]
    public void ScreenshotTest_LocalUnwriteableDirectory()
    {
        base.BeforeEach();

        var tmpDir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
        var startInfo = new ProcessStartInfo
        {
            RedirectStandardOutput = true,
            FileName = "chmod",
            Arguments = $"a-w {tmpDir.FullName}",
            UseShellExecute = false
        };

        var process = Process.Start(startInfo);
        process.WaitForExit();

        process.ExitCode.ShouldEqual(
            0,
            $"Unexpected process exit code: {process.ExitCode}, output: {process.StandardOutput.ReadToEnd()}");

        var oldCurrentDirectory = Directory.GetCurrentDirectory();

        try
        {
            Directory.SetCurrentDirectory(tmpDir.FullName);

            var screenshot = TestScreenshot();
            var screenshotDirectory = new FileInfo(screenshot.FullName).Directory.FullName;
            var testDirectory = TestContext.CurrentContext.TestDirectory;

            if (!screenshotDirectory.Equals(testDirectory, StringComparison.Ordinal))
            {
                Assert.Fail($"Screenshot '{screenshot.FullName}' not in TestDirectory '{testDirectory}'");
            }
        }
        finally
        {
            Directory.SetCurrentDirectory(oldCurrentDirectory);
        }
    }

    [Test]
    public void ScreenshotTest_LocalWriteableDirectory()
    {
        base.BeforeEach();

        var tmpDir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));

        var oldCurrentDirectory = Directory.GetCurrentDirectory();

        try
        {
            Directory.SetCurrentDirectory(tmpDir.FullName);

            // We can't use tmpDir.FullName due to the var -> /private/var symlink on Mac
            var currentDirPath = Directory.GetCurrentDirectory();

            var screenshot = TestScreenshot();
            var screenshotDirectory = new FileInfo(screenshot.FullName).Directory.FullName;

            if (!screenshotDirectory.Equals(currentDirPath, StringComparison.Ordinal))
            {
                Assert.Fail($"Screenshot '{screenshot.FullName}' not in CurrentDirectory '{currentDirPath}'");
            }
        }
        finally
        {
            Directory.SetCurrentDirectory(oldCurrentDirectory);
        }
    }

    public FileInfo TestScreenshot()
    {
        var screenshot = _app.Screenshot("app launched");

        Assert.IsTrue(screenshot.Exists, $"screenshot {screenshot.FullName} does not exist!");

        if (_app.Device.OSVersion.Major >= 10)
        {
            var startInfo = new ProcessStartInfo
            {
                RedirectStandardOutput = true,
                FileName = "file",
                Arguments = screenshot.FullName,
                UseShellExecute = false
            };

            var process = Process.Start(startInfo);
            process.WaitForExit();
            var output = process.StandardOutput.ReadToEnd();
            if (!output.Contains("PNG image data"))
            {
                Assert.Fail("Screenshot doesn't look like a PNG file");
            }
        }
        else
        {
            if (!(Image.FromFile(screenshot.FullName).Width > 0))
            {
                Assert.Fail("Screenshot is not of expected size");
            }
        }

        return screenshot;
    }
}

