using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Processes;

namespace Xamarin.UITest.Shared.iOS
{
    internal class DefaultiOSFactory : IiOSFactory
    {
        public IProcessRunner BuildProcessRunner()
        {
            return new ProcessRunner();
        }

        public IExecutor BuildExecutor(IProcessRunner processRunner)
        {
            var container = new SimpleContainer();

            container.Register(processRunner);

            var executor = new Executor(container);
            container.Register<IExecutor>(executor);

            return executor;
        }
    }
}