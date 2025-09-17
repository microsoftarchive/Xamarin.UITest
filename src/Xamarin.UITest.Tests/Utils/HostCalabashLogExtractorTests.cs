using NUnit.Framework;
using Should;
using Xamarin.UITest.iOS;
using System;

namespace Xamarin.UITest.Tests.Utils
{
    [TestFixture]
    class HostCalabashLogExtractorTests
    {
        string sample1 = @"2014-09-24 10:54:56.727 instruments[22396:5d03] WebKit Threading Violation - initial use of WebKit from a secondary thread.
2014-09-24 08:54:58 +0000 Default: OUTPUT_JSON:
{""output"":""Starting loop""}
END_OUTPUT
2014-09-24 08:54:58 +0000 Default: *
2014-09-24 08:55:01 +0000 Default: OUTPUT_JSON:
{""output"":""Execute: uia.tapOffset('{:x 145.5 :y 233.5}')\n"",""last_index\"":1}
END_OUTPUT
2014-09-24 08:55:01 +0000 Default: *
2014-09-24 08:55:01 +0000 Debug: target.tapWithOptions({x:""145.5"", y:""233.5""}, )
2014-09-24 08:55:01 +0000 Default: OUTPUT_JSON:
{""status"":""success"",""value"":true,""index"":1}
END_OUTPUT
2014-09-24 08:55:01 +0000 Default: *";

        string sampleWithErrorInMiddel = @"2014-09-24 10:54:56.727 instruments[22396:5d03] WebKit Threading Violation - initial use of WebKit from a secondary thread.
2014-09-24 08:54:58 +0000 Default: OUTPUT_JSON:
{""output"":""Starting loop""}
END_OUTPUT
2014-09-24 08:54:58 +0000 Default: *
2014-09-24 08:55:01 +0000 Default: OUTPUT_JSON:
{""output"":""unable to execute: fii""}
END_OUTPUT
2014-09-24 08:55:01 +0000 Default: *
2014-09-24 08:55:01 +0000 Debug: target.tapWithOptions({x:""145.5"", y:""233.5""}, )
2014-09-24 08:55:01 +0000 Default: OUTPUT_JSON:
{""status"":""success"",""value"":true,""index"":1}
END_OUTPUT
2014-09-24 08:55:01 +0000 Default: *";

        string sampleWithErrorInLast = @"2014-09-24 10:54:56.727 instruments[22396:5d03] WebKit Threading Violation - initial use of WebKit from a secondary thread.
2014-09-24 08:54:58 +0000 Default: OUTPUT_JSON:
{""output"":""Starting loop""}
END_OUTPUT
2014-09-24 08:54:58 +0000 Default: *
2014-09-24 08:55:01 +0000 Default: OUTPUT_JSON:
{""output"":""Execute: uia.tapOffset('{:x 145.5 :y 233.5}')\n"",""last_index\"":1}
END_OUTPUT
2014-09-24 08:55:01 +0000 Default: *
2014-09-24 08:55:01 +0000 Debug: target.tapWithOptions({x:""145.5"", y:""233.5""}, )
2014-09-24 08:55:01 +0000 Default: OUTPUT_JSON:
{""output"":""unable to execute: fii""}
END_OUTPUT
2014-09-24 08:55:01 +0000 Default: *";



        [Test]
        public void CanExtractFromSample()
        {
            var result = HostCalabashLogExtractor.Extract(1, sample1);
            result.ShouldEqual("{\"status\":\"success\",\"value\":true,\"index\":1}");

        }

        [Test]
        public void CanExtractFromSampleIndexToHigh()
        {
            var result = HostCalabashLogExtractor.Extract(2, sample1);
            result.ShouldEqual(null);

        }

        [Test]
        public void WillThrowIfLastCommandWasNotRecived()
        {
            Assert.Throws<Exception>(delegate
            {
                HostCalabashLogExtractor.Extract(2, sampleWithErrorInLast);
            });
        }

        [Test]
        public void WillNoThrowUnlessErrorIsFromLastCommandRecived()
        {
            var result = HostCalabashLogExtractor.Extract(2, sampleWithErrorInMiddel);
            result.ShouldEqual(null);
        }


    }
}
