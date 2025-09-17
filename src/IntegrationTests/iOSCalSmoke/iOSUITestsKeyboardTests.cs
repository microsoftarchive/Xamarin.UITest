using IntegrationTests.Shared;
using NUnit.Framework;
using System;
using System.Linq;
using Xamarin.UITest.Queries;

namespace IOSUITests
{
    public class iOSUITestsKeyboardTests : IOSTestBase
    {
        protected override AppInformation _appInformation => TestApps.iOSTestApp;

        private void OpenMiscTab()
        {
            _app.Tap(c => c.Marked("Misc"));
            _app.WaitForElement(c => c.Marked("Misc Menu"));
        }

        private void OpenTextInputViewsView()
        {
            _app.Tap(c => c.Marked("text input row"));
            _app.WaitForElement(c => c.Marked("Schreib!"));
        }

        [Test]
        public void CanEnterAndClearTextTest()
        {
            OpenMiscTab();
            OpenTextInputViewsView();
            _app.EnterText("Schreib!", "Test text");
            _app.ClearText();
        }

        [Test]
        public void CanDismissKeyboardTest()
        {
            OpenMiscTab();
            OpenTextInputViewsView();
            _app.EnterText("Schreib!", "Test");
            _app.DismissKeyboard();
        }
    }
}