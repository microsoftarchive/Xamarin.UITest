namespace Xamarin.UITest.Shared.Execution
{
    public interface IExecutor
    {
        TResult Execute<TResult>(IQuery<TResult> query);
        TResult Execute<TResult, TDep1>(IQuery<TResult, TDep1> query) where TDep1 : class;
        TResult Execute<TResult, TDep1, TDep2>(IQuery<TResult, TDep1, TDep2> query) where TDep1 : class where TDep2 : class;
        TResult Execute<TResult, TDep1, TDep2, TDep3>(IQuery<TResult, TDep1, TDep2, TDep3> query) where TDep1 : class where TDep2 : class where TDep3 : class;
        TResult Execute<TResult, TDep1, TDep2, TDep3, TDep4>(IQuery<TResult, TDep1, TDep2, TDep3, TDep4> query) where TDep1 : class where TDep2 : class where TDep3 : class where TDep4 : class;

        void Execute(ICommand command);
        void Execute<TDep1>(ICommand<TDep1> command) where TDep1 : class;
        void Execute<TDep1, TDep2>(ICommand<TDep1, TDep2> command) where TDep1 : class where TDep2 : class;
        void Execute<TDep1, TDep2, TDep3>(ICommand<TDep1, TDep2, TDep3> command) where TDep1 : class where TDep2 : class where TDep3 : class;
        void Execute<TDep1, TDep2, TDep3, TDep4>(ICommand<TDep1, TDep2, TDep3, TDep4> command) where TDep1 : class where TDep2 : class where TDep3 : class where TDep4 : class;
    }
}