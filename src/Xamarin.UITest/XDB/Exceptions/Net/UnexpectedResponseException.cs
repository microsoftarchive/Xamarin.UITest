using System;

namespace Xamarin.UITest.XDB.Exceptions.Net
{
    class UnexpectedResponseException : XdbException
    {
        public string Content { get; }
        public UnexpectedResponseException(string content) 
        {
            Content = content;
        }

        public UnexpectedResponseException(string message, string content) : base(message) 
        {
            Content = content;
        }

        public UnexpectedResponseException(string message, Exception inner, string content) : base(message, inner) 
        {
            Content = content;
        }
    }
}