using System;

namespace Xamarin.UITest.Shared.Logging
{
    public class LogEntry
    {
        readonly string _message;
        readonly long _offset;
        readonly LogLevel _level;
        readonly DateTimeOffset _when;

        public LogEntry(string message, LogLevel level, long offset)
        {
            _message = message;
            _offset = offset;
            _level = level;
            _when = DateTimeOffset.Now;
        }

        public string Message
        {
            get { return _message; }
        }

        public LogLevel LogLevel
        {
            get { return _level; }
        }

        public long Offset
        {
            get { return _offset; }
        }

        public DateTimeOffset When
        {
            get { return _when; }
        }

        public string LogTimestamp
        {
            get { return _when.ToString("dd-MM-yyyy HH:mm:ss.fff K"); }
        }
    }
}