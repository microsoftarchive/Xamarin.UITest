using System;
using NUnit.Framework;
using Should;
using Xamarin.UITest.Shared.Dependencies;
using Xamarin.UITest.Shared.Processes;
using System.Threading;

namespace Xamarin.UITest.Tests.Utils
{
    [TestFixture]
    public class ProcessRunnerTests
    {
        readonly ProcessRunner _processRunner = new ProcessRunner();
        readonly AndroidSdkFinder _androidSdkFinder = new AndroidSdkFinder();
        string _adbPath;

        [OneTimeSetUp]
        public void BeforeAllTests()
        {
            _adbPath = _androidSdkFinder.GetTools().GetAdbPath();
        }

        [Test]
        public void ProcessWithOnlyErrorOutput()
        {
            Assert.Throws<Exception>(() => _processRunner.Run(_adbPath));
        }

        [Test]
        public void ProcessWithNoErrorOutput()
        {
            ProcessResult result;

            try
            {
                result = _processRunner.Run(_adbPath, "devices");
            }
            catch (Exception ex)
            {
                // Ignore occasional failures that we get in Jenkins
                if (ex.Message.Contains("daemon not running. starting it now on port"))
                {
                    Thread.Sleep(2000);
                    result = _processRunner.Run(_adbPath, "devices");
                }
                else
                {
                    throw;
                }
            }

            Console.WriteLine(result);

            result.Output.ShouldNotBeEmpty();
        }

        [Test]
        public void ExitCodeZeroReturnFalse()
        {
            var result = ProcessRunner.ExitCodeIsUnexpected(0);
            Assert.False(result);
        }

        [Test]
        public void ExitCodeOtherThanZeroReturnsTrue()
        {
            var result = ProcessRunner.ExitCodeIsUnexpected(1);
            Assert.True(result);
        }

        [Test]
        public void ExitCodeIsNotZeroButIsInNoExceptionListReturnsFalse()
        {
            var result = ProcessRunner.ExitCodeIsUnexpected(1, new[] { 1 });
            Assert.False(result);
        }

        [Test]
        public void ExitCodeIsNotZeroAndNotInNoExceptionListReturnsTrue()
        {
            var result = ProcessRunner.ExitCodeIsUnexpected(2, new[] { 1 });
            Assert.True(result);
        }
    }
}
