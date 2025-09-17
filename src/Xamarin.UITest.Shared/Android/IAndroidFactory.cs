using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Processes;

namespace Xamarin.UITest.Shared.Android
{
    internal interface IAndroidFactory
    {
        IProcessRunner BuildProcessRunner();
        IExecutor BuildExecutor(IProcessRunner processRunner);
    }
}