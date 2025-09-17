using System.Collections.Generic;

namespace Xamarin.UITest.Shared.Logging
{
    public class LogScope
    {
        readonly string _message;
        readonly long _startOffset;
        long _endOffset;
        readonly List<LogEntry> _entries = new List<LogEntry>();
        readonly List<LogScope> _scopes = new List<LogScope>();

        public LogScope(string message, long offset)
        {
            _message = message;
            _startOffset = offset;
        }

        public void AddScope(LogScope scope)
        {
            _scopes.Add(scope);
        }

        public void AddEntry(LogEntry logEntry)
        {
            _entries.Add(logEntry);
        }

        public void Complete(long offset)
        {
            _endOffset = offset;
        }

        public string Message
        {
            get { return _message; }
        }

        public long StartOffset
        {
            get { return _startOffset; }
        }    

        public long EndOffset
        {
            get { return _endOffset; }
        }
    }
}