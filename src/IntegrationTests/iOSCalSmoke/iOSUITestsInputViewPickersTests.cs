using IntegrationTests.Shared;
using NUnit.Framework;
using System;
using System.Linq;
using Xamarin.UITest.Queries;

namespace IOSUITests
{
	public class InputViewUIPickersTests : IOSTestBase
	{
        protected override AppInformation _appInformation => TestApps.iOSTestApp;

        private void OpenMiscTab()
        {
            _app.Tap(c => c.Marked("Misc"));
            _app.WaitForElement(c => c.Marked("Misc Menu"));
        }

        private void OpenPickersView()
        {
            _app.Tap(c => c.Marked("UIPickerView"));
            _app.WaitForElement(c => c.Marked("Pickers"));
        }

        [Test]
        public void InputViewPickerChangesWheelsValuesTest()
        {
            _app.Tap(c => c.Marked("Misc"));
            _app.WaitForElement(c => c.Marked("Misc Menu"));
            _app.Tap(c => c.Marked("UIPickerView"));
            _app.WaitForElement(c => c.Marked("Pickers"));
            _app.Query(c => c.Class("UIPickerView").Invoke("selectRow", 3, "inComponent", 0, "animated", true));
            _app.Print.Tree();
        }

        [Test]
        public void DatePickerAppearAfterTapOnRelatedTextFieldTest()
        {
            OpenMiscTab();
            OpenPickersView();
            _app.Tap(c => c.Class("UITextField"));
            _app.SetInputViewPickerWheelValue(pickerIndex: 0, wheelIndex: 0, value: "25");
            _app.SetInputViewPickerWheelValue(pickerIndex: 0, wheelIndex: 0, value: "December");
            _app.SetInputViewPickerWheelValue(pickerIndex: 0, wheelIndex: 0, value: "2022");
        }
    }
}