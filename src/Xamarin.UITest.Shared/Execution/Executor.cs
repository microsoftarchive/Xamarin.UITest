using System;

namespace Xamarin.UITest.Shared.Execution
{
    public class Executor : IExecutor
    {
        readonly IResolver _resolver;

        public Executor(IResolver resolver)
        {
            if (resolver == null)
            {
                throw new ArgumentNullException("resolver");
            }

            _resolver = resolver;
        }

        public TResult Execute<TResult>(IQuery<TResult> query)
        {
            return WrapQuery(() => query.Execute());
        }

        public TResult Execute<TResult, TDep1>(IQuery<TResult, TDep1> query) where TDep1 : class
        {
            return WrapQuery(() => query.Execute(_resolver.Resolve<TDep1>()));
        }

        public TResult Execute<TResult, TDep1, TDep2>(IQuery<TResult, TDep1, TDep2> query) where TDep1 : class where TDep2 : class
        {
            return WrapQuery(() => query.Execute(_resolver.Resolve<TDep1>(), _resolver.Resolve<TDep2>()));
        }

        public TResult Execute<TResult, TDep1, TDep2, TDep3>(IQuery<TResult, TDep1, TDep2, TDep3> query) where TDep1 : class where TDep2 : class where TDep3 : class
        {
            return WrapQuery(() => query.Execute(_resolver.Resolve<TDep1>(), _resolver.Resolve<TDep2>(), _resolver.Resolve<TDep3>()));
        }

        public TResult Execute<TResult, TDep1, TDep2, TDep3, TDep4>(IQuery<TResult, TDep1, TDep2, TDep3, TDep4> query) where TDep1 : class where TDep2 : class where TDep3 : class where TDep4 : class
        {
            return WrapQuery(() => query.Execute(_resolver.Resolve<TDep1>(), _resolver.Resolve<TDep2>(), _resolver.Resolve<TDep3>(), _resolver.Resolve<TDep4>()));
        }

        TResult WrapQuery<TResult>(Func<TResult> queryFunc)
        {
            return queryFunc();
        }

        public void Execute(ICommand command)
        {
            command.Execute();
        }

        public void Execute<TDep1>(ICommand<TDep1> command) where TDep1 : class
        {
            command.Execute(_resolver.Resolve<TDep1>());
        }

        public void Execute<TDep1, TDep2>(ICommand<TDep1, TDep2> command) where TDep1 : class where TDep2 : class
        {
            command.Execute(_resolver.Resolve<TDep1>(), _resolver.Resolve<TDep2>());
        }

        public void Execute<TDep1, TDep2, TDep3>(ICommand<TDep1, TDep2, TDep3> command) where TDep1 : class where TDep2 : class where TDep3 : class
        {
            command.Execute(_resolver.Resolve<TDep1>(), _resolver.Resolve<TDep2>(), _resolver.Resolve<TDep3>());
        }

        public void Execute<TDep1, TDep2, TDep3, TDep4>(ICommand<TDep1, TDep2, TDep3, TDep4> command) where TDep1 : class where TDep2 : class where TDep3 : class where TDep4 : class
        {
            command.Execute(_resolver.Resolve<TDep1>(), _resolver.Resolve<TDep2>(), _resolver.Resolve<TDep3>(), _resolver.Resolve<TDep4>());
        }
    }
}