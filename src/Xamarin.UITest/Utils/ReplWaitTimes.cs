using System;

namespace Xamarin.UITest.Utils
{
    /// <summary>
    /// Default wait times when using the REPL.
    /// </summary>
    public class ReplWaitTimes : IWaitTimes
    {
        /// <summary>
        /// Time for the framework to wait when using the WaitFor methods.
        /// </summary>
        public TimeSpan WaitForTimeout
        {
            get { return new DefaultWaitTimes().WaitForTimeout; }
        }

        /// <summary>
        /// Time for the framework to wait for elements when performing gestures.
        /// </summary>
        public TimeSpan GestureWaitTimeout 
        {
            get { return TimeSpan.FromMilliseconds(100); } 
        }

        /// <summary>
        /// Time for the framework to wait for gestures to complete.
        /// </summary>
        public TimeSpan GestureCompletionTimeout
        {
            get { return new DefaultWaitTimes().GestureCompletionTimeout; }
        }
    }
}