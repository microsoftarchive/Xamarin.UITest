using System;

namespace Xamarin.UITest.XDB.Exceptions.IDB
{
    internal class IDBLocatorException : XdbException
    {
        public IDBLocatorException() { }

        public IDBLocatorException(string message) : base(message) { }

        public IDBLocatorException(string message, Exception inner) : base(message, inner) { }
    }
}