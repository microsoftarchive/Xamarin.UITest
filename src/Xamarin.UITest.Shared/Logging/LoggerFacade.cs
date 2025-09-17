using System;
using System.Linq;
using System.Diagnostics;
using Xamarin.UITest.Shared.Extensions;

namespace Xamarin.UITest.Shared.Logging
{
    public class LoggerFacade : ILogger
    {
        readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        readonly ILogConsumer[] _logConsumers;

        public LoggerFacade(params ILogConsumer[] logConsumers)
        {
            if (logConsumers == null || !logConsumers.Any())
            {
                _logConsumers = new ILogConsumer[] { new ConsoleLogConsumer(false, false) };
            }
            else
            {
                _logConsumers = logConsumers;
            }
        }

        public IDisposable OpenScope(string message)
        {
            var scope = new LogScope(message, _stopwatch.ElapsedMilliseconds);

            foreach (var logConsumer in _logConsumers.OfType<IScopedLogConsumer>())
            {
                logConsumer.ScopeOpened(scope);
            }

            return new LambdaDisposable(() =>
            {
                scope.Complete(_stopwatch.ElapsedMilliseconds);

                foreach (var logConsumer in _logConsumers.OfType<IScopedLogConsumer>())
                {
                    logConsumer.ScopeClosed();
                }
            });
       }

        public void Info(string message, object info = null)
        {
            var msg = BuildMessage(message, info);
            RegisterEntry(new LogEntry(msg, LogLevel.Info, _stopwatch.ElapsedMilliseconds));
        }

        public void Debug(string message, object info = null)
        {
            var msg = BuildMessage(message, info);
            RegisterEntry(new LogEntry(msg, LogLevel.Debug, _stopwatch.ElapsedMilliseconds));
        }

        void RegisterEntry(LogEntry logEntry)
        {
            foreach (var logConsumer in _logConsumers)
            {
                logConsumer.Consume(logEntry);
            }
        }

        string BuildMessage(string message, object info)
        {
            var msg = message;
            var stringify = info.Stringify();

            if (!stringify.IsNullOrWhiteSpace())
            {
                msg = string.Format("{0} {1}", message, stringify);
            }

            if (info is Exception)
            {
                msg += string.Format("{0}Exception: {1}", Environment.NewLine, info);
            }

            return msg;
        }
    }
}