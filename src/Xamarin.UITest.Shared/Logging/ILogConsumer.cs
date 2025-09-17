namespace Xamarin.UITest.Shared.Logging
{

    public interface ILogConsumer
    {
        void Consume(LogEntry logEntry);
    }
}