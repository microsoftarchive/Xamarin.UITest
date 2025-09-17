using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Should;
using Xamarin.UITest.Shared.iOS.Queries;

namespace Xamarin.UITest.Tests.Utils
{
    [TestFixture]
    class DeviceInfoVersionTests
    {
        [TestCase(3, "7E18")]	
        [TestCase(4, "8C148")]	
        [TestCase(5, "9B206")]	
        [TestCase(6,"10B500")]
        [TestCase(7, "11D257")]
        [TestCase(8, "12A365")]
        [TestCase(-1, "Unknown")]
        public void BuildToMajorVersionsAreCorrect(int iosVesion, string buildnumber)
        {
            var deviceInfo = new DeviceInfo("notused", buildnumber);
            deviceInfo.GetiOSMajorVersion().ShouldEqual(iosVesion);
        }
    }
}
