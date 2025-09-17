using System;
using NUnit.Framework;

namespace Xamarin.UITest.Tests.Integration
{
    public class iOSMultiArchIncompleteBundleTests
    {
        [Test]
        public void RaisesExceptionBecauseAppBundleIsIncomplete()
        {
            if (!UITest.Shared.Processes.Platform.Instance.IsOSX)
            {
                Assert.Ignore();
            }

            var ex = Assert.Throws<Exception>(delegate {
                ConfigureApp
                    .iOS
                    .AppBundleZip("../../../../binaries/TestApps/iOSMultiArchIncompleteBundle.app.zip")
                    .StartApp();
            });
            StringAssert.Contains("This app is not compatible with UITest because it was built for multiple simulator architectures.", ex.Message);
        }
    }
}

