using System;

namespace Xamarin.UITest.XDB.Exceptions
{
    class DeviceAgentException : XdbException
    {
        public DeviceAgentException() {}

        public DeviceAgentException(string message) : base(message) { }

        public DeviceAgentException(string message, Exception inner) : base(message, inner) { }
    }
}