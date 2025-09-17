using Xamarin.UITest.Shared.Android.Adb;
using Xamarin.UITest.Shared.Dependencies;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Processes;

namespace Xamarin.UITest.Tests
{
    internal static class ExecutorHelper
    {
        public static Executor GetDefault(IJdkTools jdkTools = null, IProcessRunner processRunner = null, IAndroidSdkTools androidSdkTools = null)
        {
            var container = new SimpleContainer();

            processRunner = processRunner ?? new ProcessRunner();
            androidSdkTools = androidSdkTools ?? new AndroidSdkFinder().GetTools();

            container.Register(androidSdkTools);
            container.Register(jdkTools ?? new JdkFinder().GetTools());
            container.Register(processRunner);
            container.Register(new AdbProcessRunner(processRunner, androidSdkTools));

            var executor = new Executor( container );
            container.Register<IExecutor>( executor );

            return executor;
        }
    }
}