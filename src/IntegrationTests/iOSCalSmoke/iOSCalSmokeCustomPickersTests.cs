using IntegrationTests.Shared;
using NUnit.Framework;
using System;
using System.Linq;
using Xamarin.UITest.Queries;

namespace IOSUITests
{
	public class CustomUIPickersTests : IOSTestBase
	{
        protected override AppInformation _appInformation => TestApps.iOSCalSmoke;

        private void OpenSpecialTab()
        {
            _app.Tap(c => c.Class("tabBarButton").Index(4));
            _app.WaitForElement("date picker page");
        }

        [Test]
        public void CustomDatePickerChangeWheelsValuesTest()
        {
            OpenSpecialTab();
            _app.Tap(c => c.Id("show countdown picker"));
            _app.SetInputViewPickerWheelValue(pickerIndex: 0, wheelIndex: 0, value: "3");
            _app.SetInputViewPickerWheelValue(pickerIndex: 0, wheelIndex: 1, value: "30");
            var picker = _app.GetDatePickers().ElementAt(index: 0);
            var wheels = picker.Descendants(type: "PickerWheel").ToList();

            Assert.AreEqual("3 hours", wheels.ElementAt(index: 0).Value);
            Assert.AreEqual("30 min", wheels.ElementAt(index: 1).Value);
        }
    }
}

