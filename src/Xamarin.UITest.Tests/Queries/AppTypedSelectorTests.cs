using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.UITest.Android;
using Xamarin.UITest.Queries;

namespace Xamarin.UITest.Tests.Queries
{
    [TestFixture]
    class AppTypedSelectorTests
    {
        [TestCase(QueryPlatform.Android)]
        [TestCase(QueryPlatform.iOS)]
        public void ValueRequestIsRemembered(QueryPlatform platform)
        {
            Func<AppQuery, AppTypedSelector<object>> query = q => q.All().Invoke("foo");
            Func<AppQuery, AppTypedSelector<int>> queryValue = q => q.All().Invoke("foo").Value<int>();

            Assert.False(((IAppTypedSelector)query(new AppQuery(platform))).ExplicitlyRequestedValue);
            Assert.True(((IAppTypedSelector)queryValue(new AppQuery(platform))).ExplicitlyRequestedValue);
        }

        [TestCase(QueryPlatform.Android)]
        [TestCase(QueryPlatform.iOS)]
        public void ValueOnlyValueOnce(QueryPlatform platform)
        {
            Func<AppQuery, AppTypedSelector<int>> invalidQuery = q => q.All().Invoke("foo").Value<object>().Value<int>();

            Assert.Throws<Exception>(() => invalidQuery(new AppQuery(platform)));
        }
    }
}
