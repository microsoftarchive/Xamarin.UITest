using System;
using System.Diagnostics;
using System.Threading;
using Xamarin.UITest.XDB.Enums;

namespace Xamarin.UITest.XDB.Services
{
    class LoggerService : ILoggerService
    {
        int _currentLogId;
        bool _logToConsole;
        LogLevel _logLevel;

        public LoggerService() : this(false, LogLevel.Error)
        { }

        public LoggerService(bool logToConsole, LogLevel logLevel)
        {
            _logToConsole = logToConsole;
            _logLevel = logLevel;
        }

        public virtual int GetEventId()
        {
            return Interlocked.Increment(ref _currentLogId);
        }

        public virtual void Log(string message, LogLevel logLevel, int? eventId = null, object info = null)
        {
            if (logLevel >= _logLevel)
            {
                var logString = info == null ? message : $"{message}: {info}";

                if (eventId.HasValue)
                {
                    logString = $"{eventId} - {logString}";
                }

                if (_logToConsole)
                {
                    Console.WriteLine(logString);
                }

                Debug.WriteLine(logString);
            }
        }

        public void LogCritical(string message, int? eventId = null, object info = null)
        {
            Log(message, LogLevel.Critical, eventId, info);
        }

        public void LogDebug(string message, int? eventId = null, object info = null)
        {
            Log(message, LogLevel.Debug, eventId, info);
        }

        public void LogError(string message, int? eventId = null, object info = null)
        {
            Log(message, LogLevel.Error, eventId, info);
        }

        public void LogInfo(string message, int? eventId = null, object info = null)
        {
            Log(message,  LogLevel.Information, eventId, info);
        }

        public void LogTrace(string message, int? eventId = null, object info = null)
        {
            Log(message, LogLevel.Trace, eventId, info);
        }

        public void LogWarn(string message, int? eventId = null, object info = null)
        {
            Log(message, LogLevel.Warning, eventId, info);
        }
    }
}