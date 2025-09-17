using System;
using System.Collections.Generic;
using System.Linq;

namespace Xamarin.UITest.Shared.Logging
{
    public class MemoryLogConsumer : ILogConsumer
    {
        readonly List<LogEntry> _entries = new List<LogEntry>();

        public MemoryLogConsumer()
        {
        }

        #region ILogConsumer implementation

        public void Consume(LogEntry logEntry)
        {
            _entries.Add(logEntry);            
        }

        #endregion

        public string[] Messages {
            get {
                return _entries
                    .Where(entry => entry.LogLevel == LogLevel.Info)
                    .Select(entry => entry.Message).ToArray();
            }
        }

    }
}

