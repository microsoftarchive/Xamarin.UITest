using System;

namespace Xamarin.UITest.Shared.Logging
{
    public class ConsoleLogConsumer : ILogConsumer
    {
        readonly bool _debug;
        readonly bool _includeTimestamp;

        public ConsoleLogConsumer(bool debug, bool includeTimestamp)
        {
            _includeTimestamp = includeTimestamp;
            _debug = debug;
        }

        public void Consume(LogEntry logEntry)
        {
            if (!_debug && logEntry.LogLevel == LogLevel.Debug)
            {
                return;
            }

            if (_includeTimestamp)
            {
                Console.WriteLine(string.Format("{0} - {1} - {2}", logEntry.LogTimestamp, logEntry.Offset, logEntry.Message));
            }
            else
            {
                Console.WriteLine(logEntry.Message);
            }
        }
    }
}