using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Xamarin.UITest.Tests.Helpers
{
    public static class Timed<TResult> where TResult : class
    {
        public async static Task<TimedResult<TResult>> TimeExecutionOfAsync(Func<Task<TResult>> function)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = await function.Invoke();
            stopwatch.Stop();
            return new TimedResult<TResult> { Elapsed = stopwatch.Elapsed, Result = result };
        }
    }
}
