using System;

namespace Xamarin.UITest.XDB.Exceptions
{
    abstract class XdbException : Exception
    {
        public XdbException() {}

        public XdbException(string message) : base(message) { }

        public XdbException(string message, Exception inner) : base(message, inner) { }
    }
}