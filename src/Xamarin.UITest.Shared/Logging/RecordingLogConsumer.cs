using System.Collections.Generic;

namespace Xamarin.UITest.Shared.Logging
{
    public class RecordingLogConsumer : IScopedLogConsumer
    {
        readonly Stack<LogScope> _scopeStack = new Stack<LogScope>();
        LogScope _currentScope;

        public RecordingLogConsumer()
        {
            _currentScope = new LogScope("Global scope", 0);
            _scopeStack.Push(_currentScope);
        }

        public void ScopeOpened(LogScope scope)
        {
            _currentScope.AddScope(scope);
            _scopeStack.Push(scope);
            _currentScope = scope;
        }

        public void ScopeClosed()
        {
            _scopeStack.Pop();
            _currentScope = _scopeStack.Peek();
        }

        public void Consume(LogEntry entry)
        {
            _currentScope.AddEntry(entry);
        }
    }
}