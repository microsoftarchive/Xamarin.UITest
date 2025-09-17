using System;
namespace Xamarin.UITest.Tests.Helpers
{
    public struct TimedResult<TResult> where TResult : class
    {
        public TimeSpan Elapsed { get; set; }
        public TResult Result { get; set; }
    }
}
