using System;
using System.Threading;
using Xamarin.UITest.Shared.Extensions;
using System.Linq;
using System.Collections.Generic;
using Xamarin.UITest.Shared.Logging;

namespace Xamarin.UITest.Utils
{
    internal class UnhandledExceptionWorkaround
    {
        static readonly object Lock = new object();
        static UnhandledExceptionWorkaround _workaround;
        static readonly List<Exception> Exceptions = new List<Exception>();

        readonly int _maxStaleThreads;
        int _staleThreads;

        static void InstallWorkaround(int maxStaleThreads)
        {
            lock (Lock)
            {
                if (_workaround == null)
                {
                    _workaround = new UnhandledExceptionWorkaround(maxStaleThreads);
                }
                ClearUncaughtExceptionsFromOtherThreads();
            }
        }

        private UnhandledExceptionWorkaround(int maxStaleThreads)
        {
            _maxStaleThreads = maxStaleThreads;
            AppDomain.CurrentDomain.UnhandledException += UncaughtExceptionHandler;
        }

        void UncaughtExceptionHandler(object s, UnhandledExceptionEventArgs e)
        {
            Log.Debug("Uncaught exception handling");
            var noThreads = Interlocked.Increment(ref _staleThreads);
            if (_maxStaleThreads != 0 && noThreads > _maxStaleThreads)
            {
                //Exceded number of threads and will not handle exception";
            }
            else
            {
                var ec = e.ExceptionObject as Exception;
                // post exception to caller of HandleUncaughtExceptionFromOtherThreads
                lock (Lock)
                {
                    Exceptions.Add(ec);
                }
                try
                {
                    Thread.CurrentThread.IsBackground = true;
                    Thread.Sleep(Timeout.InfiniteTimeSpan);
                }
                catch (Exception)
                {
                    // Ignore
                }
            }
        }

        public static void HandleUncaughtExceptionsFromOtherThreads(string message = null)
        {
            message = message ?? "Detected uncaught exception on other thread";
            AggregateException exception = null;
            lock (Lock)
            {
                if (Exceptions.Count > 0)
                {
                    exception = new AggregateException(message, Exceptions);
                }
                Exceptions.Clear();
            }

            if (exception != null)
            {
                Log.Debug(string.Format("throwing AgregateExcetion with the following exception(s): {0}", string.Join(", " + Environment.NewLine, exception.InnerExceptions)));
                throw exception;
            }
        }

        public static void ClearUncaughtExceptionsFromOtherThreads()
        {
            List<Exception> clearedExceptions = null;
            lock (Lock)
            {
                if (Exceptions.Count > 0)
                {
                    clearedExceptions = new List<Exception>(Exceptions);
                }
                Exceptions.Clear();
            }

            if (clearedExceptions != null)
            {
                Log.Debug(string.Format("Clearing the following uncaught exceptions: {0}", string.Join(", " + Environment.NewLine, clearedExceptions)));
            }
        }
    }
}