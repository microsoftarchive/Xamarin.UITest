using TinyIoC;
using Xamarin.UITest.XDB.Services;

namespace Xamarin.UITest.XDB
{
    static class XdbServices
    {
        static readonly TinyIoCContainer _container;

        static XdbServices()
        {
            _container = new TinyIoCContainer();

            ServiceHelper.RegisterServices((i, t) => _container.Register(i, t).AsSingleton());

            _container.Register<ILoggerService, XdbLoggerService>();
        }

        public static T GetRequiredService<T>() where T : class
        {
            return _container.Resolve<T>();
        }
    }
}