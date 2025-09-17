using System;

namespace Xamarin.UITest.Utils
{
    /// <summary>
    /// Contains default wait times for the framework to use.
    /// </summary>
    public interface IWaitTimes
    {
        /// <summary>
        /// Time for the framework to wait when using the WaitFor methods.
        /// </summary>
        TimeSpan WaitForTimeout { get; }

        /// <summary>
        /// Time for the framework to wait for elements when performing gestures.
        /// </summary>
        TimeSpan GestureWaitTimeout { get; }

        /// <summary>
        /// Time for the framework to wait for gestures to complete.
        /// </summary>
        TimeSpan GestureCompletionTimeout { get; }
    }
}