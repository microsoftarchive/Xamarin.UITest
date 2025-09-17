using Xamarin.UITest.Shared.Android.Adb;
using Xamarin.UITest.Shared.Dependencies;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Processes;

namespace Xamarin.UITest.Shared.Android
{
    internal class DefaultAndroidFactory : IAndroidFactory
    {
        public IProcessRunner BuildProcessRunner()
        {
            return new ProcessRunner();   
        }

        public IExecutor BuildExecutor(IProcessRunner processRunner)
        {
            var container = new SimpleContainer();

            var androidSdkTools = new AndroidSdkFinder().GetTools();
            var jdkTools = new JdkFinder().GetTools();

            container.Register(new AdbProcessRunner(processRunner, androidSdkTools));
            container.Register<IAndroidSdkTools>(androidSdkTools);
            container.Register<IJdkTools>(jdkTools);
            container.Register(processRunner);

            var executor = new Executor(container);
            container.Register<IExecutor>(executor);

            return executor;
        }
    }
}