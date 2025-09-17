using NSubstitute;
using NUnit.Framework;
using System;
using System.Diagnostics;
using Xamarin.UITest.Shared.Logging;
using System.Collections.Generic;

namespace Xamarin.UITest.Tests
{
    [TestFixture]
    public class TraceSourceLogConsumerTest
    {
        [Test]
        public void CanConfigureExistingTraceSourceForAppDomain()
        {
            var consumer = new TraceSourceLogConsumer();

            consumer.Consume(new LogEntry("Foo", LogLevel.Info, 0));

            var traceSource = (TraceSource)AppDomain.CurrentDomain.GetData(TraceSourceLogConsumer.AppDomainTraceSourceKey);
            var listener = Substitute.For<TraceListener>();

            var oldListeners = new List<TraceListener>();
            foreach (var oldListener in traceSource.Listeners)
            {
                oldListeners.Add((TraceListener)oldListener);
            }

            traceSource.Listeners.Clear();
            try
            {
                traceSource.Listeners.Add(listener);

                listener.Received(0);

                consumer.Consume(new LogEntry("Bar", LogLevel.Info, 0));

                listener.Received(1);
            }
            finally
            {
                traceSource.Listeners.Clear();
                oldListeners.ForEach(l => traceSource.Listeners.Add(l));
            }
        }

        [Test]
        public void CanOverrideTraceSourceForAppDomain()
        {
            var traceSource = new TraceSource("Xamarin.UITest", SourceLevels.All);

            var listener = Substitute.For<TraceListener>();
            traceSource.Listeners.Add(listener);

            var oldTraceSource = (TraceSource)AppDomain.CurrentDomain.GetData(TraceSourceLogConsumer.AppDomainTraceSourceKey);

            try
            {
                AppDomain.CurrentDomain.SetData(TraceSourceLogConsumer.AppDomainTraceSourceKey, traceSource);

                var consumer = new TraceSourceLogConsumer();

                consumer.Consume(new LogEntry("Foo", LogLevel.Info, 0));

                listener.Received(1);
            }
            finally
            {
                AppDomain.CurrentDomain.SetData(TraceSourceLogConsumer.AppDomainTraceSourceKey, oldTraceSource);
            }
        }
    }
}
