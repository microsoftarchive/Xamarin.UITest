using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using Xamarin.UITest.Shared.Logging;

namespace Xamarin.UITest.Shared
{
    internal class WaitForHelper(TimeSpan defaultTimeout)
    {
        private readonly TimeSpan DefaultTimeout = defaultTimeout;

        [NoDelegation]
        public bool WaitForOrElapsed(Func<bool> predicate, TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
        {
            var maxWaitUtc = DateTime.UtcNow + timeout.GetValueOrDefault(DefaultTimeout);

            var exceptions = new List<Exception>();

            while (DateTime.UtcNow < maxWaitUtc)
            {
                try
                {
                    if (predicate())
                    {
                        if (postTimeout.HasValue)
                        {
                            Thread.Sleep(postTimeout.Value);
                        }

                        return true;
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }

                Thread.Sleep(retryFrequency.GetValueOrDefault(TimeSpan.FromMilliseconds(250)));
            }

            var distinctExceptions = exceptions
                .GroupBy(x => x.ToString())
                .Select(x => x.First().ToString())
                .ToArray();

            if (distinctExceptions.Any())
            {
                Log.Info("Exceptions while waiting: " + Environment.NewLine + string.Join(Environment.NewLine, distinctExceptions));
            }

            return false;
        }

        public void WaitFor(Func<bool> predicate,
                        string timeoutMessage = "Timed out waiting...",
                        TimeSpan? timeout = null,
                        TimeSpan? retryFrequency = null,
                        TimeSpan? postTimeout = null)
        {
            postTimeout = postTimeout.GetValueOrDefault(TimeSpan.FromMilliseconds(100));

            if (!WaitForOrElapsed(predicate, timeout, retryFrequency, postTimeout))
            {
                throw new TimeoutException(timeoutMessage);
            }
        }

        [NoDelegation]
        public void ExecuteAndWait(Action action, TimeSpan waitTime)
        {
            var start = DateTime.UtcNow;
            action.Invoke();
            var timeRemaining = waitTime - (DateTime.UtcNow - start);
            if (timeRemaining.TotalMilliseconds > 0)
            {
                Thread.Sleep(timeRemaining);
            }
        }

        [NoDelegation]
        public T[] WaitForAny<T>(Func<T[]> query, string timeoutMessage = "Timed out waiting...", TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
        {
            var result = WaitForAnyOrDefault(query, [], timeout, retryFrequency, postTimeout);
            if (result.Any())
            {
                return result;
            }
            else
            {
                throw new TimeoutException(timeoutMessage);
            }
        }

        [NoDelegation]
        public T[] WaitForAnyOrDefault<T>(Func<T[]> query, T[] defaultValue, TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
        {
            var maxWaitUtc = DateTime.UtcNow + timeout.GetValueOrDefault(DefaultTimeout);

            var exceptions = new List<Exception>();

            while (DateTime.UtcNow < maxWaitUtc)
            {
                try
                {
                    var results = query();

                    if (results != null && results.Any())
                    {
                        var stable = WaitForStableResultOrElapsed(query, results, elapsedTimeout: maxWaitUtc - DateTime.UtcNow);
                        if (stable != null && stable.Any())
                        {
                            Thread.Sleep(postTimeout.GetValueOrDefault(TimeSpan.FromMilliseconds(150)));
                            return stable;
                        }
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }

                Thread.Sleep(retryFrequency.GetValueOrDefault(TimeSpan.FromMilliseconds(250)));
            }

            var distinctExceptions = exceptions
                .GroupBy(x => x.ToString())
                .Select(x => x.First().ToString())
                .ToArray();

            if (distinctExceptions.Any())
            {
                Log.Info("Exceptions while waiting for any: " + Environment.NewLine + string.Join(Environment.NewLine, distinctExceptions));
            }

            return defaultValue;
        }

        [NoDelegation]
        public T[] WaitForStableResultOrElapsed<T>(Func<T[]> query, T[] currentResult = null, TimeSpan? retryTimeout = null, TimeSpan? elapsedTimeout = null)
        {
            if (currentResult == null)
            {
                currentResult = query();
                Thread.Sleep(retryTimeout.GetValueOrDefault(TimeSpan.FromMilliseconds(150)));
            }

            var maxWaitUtc = DateTime.UtcNow + elapsedTimeout.GetValueOrDefault(TimeSpan.FromSeconds(3));

            while (DateTime.UtcNow < maxWaitUtc)
            {
                var newResult = query();

                if (JsonConvert.SerializeObject(currentResult) == JsonConvert.SerializeObject(newResult))
                {
                    return currentResult;
                }

                currentResult = newResult;
                Thread.Sleep(retryTimeout.GetValueOrDefault(TimeSpan.FromMilliseconds(150)));
            }

            return currentResult;
        }
    }
}