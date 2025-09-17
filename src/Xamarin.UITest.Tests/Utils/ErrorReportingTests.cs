using System;
using NSubstitute;
using NUnit.Framework;
using Should;
using Xamarin.UITest.Queries;
using Xamarin.UITest.Shared.Logging;
using Xamarin.UITest.Utils;

namespace Xamarin.UITest.Tests.Utils
{
    [TestFixture]
    public class ErrorReportingTests
    {
        private static readonly Func<AppQuery, AppQuery> FluentFunc =  c => c.Button("foo");
        private static readonly AppQuery AppQuery = FluentFunc(new AppQuery(QueryPlatform.iOS));
        private static readonly Func<int> FuncWithReturnValue = () => 1;
        private static readonly Action FuncWithNoReturnValue = () => { };
        private static readonly Func<int> FuncWithReturnValueEx = () => { throw new Exception("Inner");};
        private static readonly Action FuncWithNoReturnValueEx = () => { throw new Exception("Inner");};

        [Test]
        public void NoErrorNoReport()
        {
            var value = new ErrorReporting(QueryPlatform.Android).With(FuncWithReturnValue);
            value.ShouldEqual(1);
        }

        [Test]
        public void NoErrorNoReportVoid()
        {
            new ErrorReporting(QueryPlatform.Android).With(FuncWithNoReturnValue);
        }


        [Test]
        public void ErrorsShouldBeWrapped()
        {
            try
            {
                new ErrorReporting(QueryPlatform.Android).With(FuncWithReturnValueEx);
                Assert.Fail("No exception was thrown");

            }
            catch (Exception e)
            {
                e.InnerException.Message.ShouldBeSameAs("Inner");
                e.Message.ShouldContain("ErrorsShouldBeWrapped");
            }
        }

        [Test]
        public void ErrorsShouldPPrintQuerys()
        {
            try
            {
                new ErrorReporting(QueryPlatform.Android).With(FuncWithReturnValueEx, new[] { AppQuery });
                Assert.Fail("No exception was thrown");

            }
            catch (Exception e)
            {
                e.InnerException.Message.ShouldBeSameAs("Inner");
                e.Message.ShouldContain("ErrorsShouldPPrintQuerys");
                e.Message.ShouldContain("Button(\"foo\")");
            }
        }

        [Test]
        public void ErrorsShouldPPrintFluentQueryFunc()
        {
            try
            {
                new ErrorReporting(QueryPlatform.Android).With(FuncWithReturnValueEx, new[] {FluentFunc});
                Assert.Fail("No exception was thrown");

            }
            catch (Exception e)
            {
                e.InnerException.Message.ShouldBeSameAs("Inner");
                e.Message.ShouldContain("ErrorsShouldPPrintFluentQueryFunc");
                e.Message.ShouldContain("Button(\"foo\")");

            }
        }

        [Test]
        public void ErrorsShouldBeWrappedVoid()
        {
            try
            {
                new ErrorReporting(QueryPlatform.Android).With(FuncWithNoReturnValueEx);
                Assert.Fail("No exception was thrown");
            }
            catch (Exception e)
            {
                e.InnerException.Message.ShouldBeSameAs("Inner");
                e.Message.ShouldContain("ErrorsShouldBeWrappedVoid");
            }
        }

        [Test]
        public void ErrorsShouldPPrintQuerysVoid()
        {
            try
            {
                new ErrorReporting(QueryPlatform.Android).With(FuncWithNoReturnValueEx, new[] {AppQuery});
                Assert.Fail("No exception was thrown");
            }
            catch (Exception e)
            {
                e.InnerException.Message.ShouldBeSameAs("Inner");
                e.Message.ShouldContain("ErrorsShouldPPrintQuerysVoid");
                e.Message.ShouldContain("Button(\"foo\")");
            }
        }

        [Test]
        public void ErrorsShouldPPrintSimpleArgs()
        {
            try
            {
                new ErrorReporting(QueryPlatform.Android).With(FuncWithNoReturnValueEx, new object[] {1,2,4});
                Assert.Fail("No exception was thrown");
            }
            catch (Exception e)
            {
                e.InnerException.Message.ShouldBeSameAs("Inner");
                e.Message.ShouldContain("ErrorsShouldPPrintSimpleArgs");
                e.Message.ShouldContain("1, 2, 4");
            }
        }

        [Test]
        public void ErrorsShouldBeLoggedToo()
        {
            var logger = Substitute.For<ILogger>();
            var errorReporting = new ErrorReporting(QueryPlatform.Android);

            using (Log.ReplaceLoggerTemporarily(logger))
            {
                try
                {
                    errorReporting.With(FuncWithNoReturnValueEx);
                    Assert.Fail("No exception was thrown");
                }
                catch (Exception e)
                {
                    logger.Received().Info(e.Message, e.InnerException);
                }
            }
        }
    }
}
