using IntegrationTests.Shared;
using NUnit.Framework;
using System;
using System.Linq;
using Xamarin.UITest.Queries;

namespace IOSUITests
{
	public class iOSUITestsCustomPickersTests : IOSTestBase
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
        public void CustomPickerViewTapTest()
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
            _app.Print.AllElements();
            _app.SetInputViewPickerWheelValue(pickerIndex: 0, wheelIndex: 0, value: "25");
        }
    }
}

