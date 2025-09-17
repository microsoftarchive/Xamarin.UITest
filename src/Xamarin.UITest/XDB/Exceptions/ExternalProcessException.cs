using System;

namespace Xamarin.UITest.XDB.Exceptions
{
    class ExternalProcessException : XdbException
    {
        public ExternalProcessException() {}

        public ExternalProcessException(string message) : base(message) { }

        public ExternalProcessException(string message, Exception inner) : base(message, inner) { }
    }
}