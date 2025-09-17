


using System;

namespace Xamarin.UITest.XDB.Exceptions.PList
{
    class PListMissingKeyException : PListParsingException
    {
        public PListMissingKeyException() {}

        public PListMissingKeyException(string message) : base(message) { }

        public PListMissingKeyException(string message, Exception inner) : base(message, inner) { }
    }
}