using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Processes;

namespace Xamarin.UITest.Shared.iOS
{
    internal interface IiOSFactory
    {
        IProcessRunner BuildProcessRunner();
        IExecutor BuildExecutor(IProcessRunner processRunner);
    }
}