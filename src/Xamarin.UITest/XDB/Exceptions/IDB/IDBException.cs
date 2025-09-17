using System;

namespace Xamarin.UITest.XDB.Exceptions
{
    class IDBException : XdbException
    {
        public IDBException() { }

        public IDBException(string message) : base(message) { }

        public IDBException(string message, Exception inner) : base(message, inner) { }
    }
}

