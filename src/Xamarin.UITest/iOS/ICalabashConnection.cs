using Xamarin.UITest.Shared.Http;

namespace Xamarin.UITest.iOS
{
    internal interface ICalabashConnection
    {
        HttpResult Map(object arguments);
        HttpResult Location(object arguments);
        HttpResult UIA(string command);
        HttpResult Condition(object condition);
        HttpResult Backdoor(object condition);
        HttpResult Version();
        HttpResult Dump();
        HttpResult Suspend(double seconds);
        HttpResult Exit();
        HttpResult ClearText();
    }
}