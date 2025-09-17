using System;

namespace Xamarin.UITest.Shared.Logging
{
    public static class Log
    {
        static ILogger _logger = new LoggerFacade();
        static bool _initialized;

        public static void Initialize(ILogger logger)
        {
            _logger = logger;
            _initialized = true;
        }


        public static IDisposable ReplaceLoggerTemporarily(ILogger logger)
        {
            var currentLogger = _logger;
            _logger = logger;

            return new LambdaDisposable(() =>
            {
                _logger = currentLogger;
            });
        }

        public static void VerifyInitialized()
        {
#if DEBUG
            if (!_initialized)
            {
                throw new Exception("Logger was not initialized.");
            }
#endif  
        }

        public static void Info(string message, object info = null)
        {
            _logger.Info(message, info);
        }

        public static void Debug(string message, object info = null)
        {
            _logger.Debug(message, info);
        }
    }
}