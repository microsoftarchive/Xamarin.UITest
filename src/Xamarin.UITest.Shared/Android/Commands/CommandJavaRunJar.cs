using Xamarin.UITest.Shared.Dependencies;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Processes;

namespace Xamarin.UITest.Shared.Android.Commands
{
    internal class CommandJavaRunJar : ICommand<IProcessRunner, IJdkTools>
    {
        readonly string _jarPath;
        readonly string _args;

        public CommandJavaRunJar(string jarPath, string args)
        {
            _jarPath = jarPath;
            _args = args;
        }

        public void Execute(IProcessRunner processRunner, IJdkTools jdkTools)
        {
            var arguments = "-jar \"" + _jarPath + "\" " + _args;
            processRunner.Run(jdkTools.GetJavaPath(), arguments);
        }
    }
}