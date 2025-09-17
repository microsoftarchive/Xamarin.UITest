
using Xamarin.UITest.XDB.Services;

namespace Xamarin.UITest.XDB
{
    class XdbLoggerService : LoggerService
    {
        public override void Log(
            string message, Enums.LogLevel logLevel, int? eventId = null, object info = null)
        {
            if (eventId.HasValue)
            {
                message = $"{eventId} - {message}";
            }

            if (logLevel >= Enums.LogLevel.Information)
            {
                Shared.Logging.Log.Info(message, info);
            }
            else if (logLevel >= Enums.LogLevel.Debug)
            {
                Shared.Logging.Log.Debug(message, info);
            }
        }
    }
}
