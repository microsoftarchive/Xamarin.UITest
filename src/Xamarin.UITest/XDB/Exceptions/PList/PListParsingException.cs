using System;

namespace Xamarin.UITest.XDB.Exceptions.PList
{
    abstract class PListParsingException : XdbException
    {
        public PListParsingException() {}

        public PListParsingException(string message) : base(message) { }

        public PListParsingException(string message, Exception inner) : base(message, inner) { }
    }
}