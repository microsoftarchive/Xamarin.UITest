using System;
using NUnit.Framework;
using Xamarin.UITest.iOS;
using Should;

namespace Xamarin.UITest.Tests.iOS
{
    [TestFixture]
    public class iAppDataPatternTests
    {
        [Test]
        public void PatternStandardOutput()
        {
            var text = @"NAME         TYPE MTIME                SIZE
Caches       DIR  2015-05-22 15:00:46  102
Preferences  DIR  2015-05-22 15:10:46  102
iproxy            2015-05-22 15:14:24  19904";


            var matches = iAppData.Pattern.Matches(text);


            matches.Count.ShouldEqual(3);

            matches[0].Groups["name"].Value.ShouldEqual("Caches");
            matches[1].Groups["name"].Value.ShouldEqual("Preferences");
            matches[2].Groups["name"].Value.ShouldEqual("iproxy");
        }
    
    }
}

