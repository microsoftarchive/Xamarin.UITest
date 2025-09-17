using Xamarin.UITest.XDB.Enums;

namespace Xamarin.UITest.XDB.Services
{
    interface ILoggerService
    {
        int GetEventId();

        void Log(string message, LogLevel level, int? eventId = null, object info = null);

        void LogCritical(string message, int? eventId = null, object info = null);
        
        void LogDebug(string message, int? eventId = null, object info = null);
        
        void LogError(string message, int? eventId = null, object info = null);
        
        void LogInfo(string message, int? eventId = null, object info = null);
        
        void LogTrace(string message, int? eventId = null, object info = null);
        
        void LogWarn(string message, int? eventId = null, object info = null);
    }
}