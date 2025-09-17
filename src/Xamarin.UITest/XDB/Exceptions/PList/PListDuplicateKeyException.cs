using System;

namespace Xamarin.UITest.XDB.Exceptions.PList
{
    class PListDuplicateKeyException : PListParsingException
    {
        public PListDuplicateKeyException() {}

        public PListDuplicateKeyException(string message) : base(message) { }

        public PListDuplicateKeyException(string message, Exception inner) : base(message, inner) { }
    }
}