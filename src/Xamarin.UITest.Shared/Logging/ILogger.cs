using System;

namespace Xamarin.UITest.Shared.Logging
{
    public interface ILogger
    {
        IDisposable OpenScope(string message);
		void Info (string message, object info = null);
		void Debug (string message, object info = null);
    }
}