using System;
using Xamarin.UITest.Shared;

namespace Xamarin.UITest.Utils
{
    internal class DefaultWaitTimes : IWaitTimes
    {
        public TimeSpan WaitForTimeout
        {
            get { return TimeSpan.FromSeconds(15); }
        }

        public TimeSpan GestureWaitTimeout
        {
            get { return TimeSpan.FromSeconds(15); }
        }

        public TimeSpan GestureCompletionTimeout
        {
            get { return TimeSpan.FromSeconds(15); }
        }
    }
}
