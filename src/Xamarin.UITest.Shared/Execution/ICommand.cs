namespace Xamarin.UITest.Shared.Execution
{
    public interface ICommand
    {
        void Execute();
    }

    public interface ICommand<in TDep1>
    {
        void Execute(TDep1 dep1);
    }

    public interface ICommand<in TDep1, in TDep2>
    {
        void Execute(TDep1 dep1, TDep2 dep2);
    }

    public interface ICommand<in TDep1, in TDep2, in TDep3>
    {
        void Execute(TDep1 dep1, TDep2 dep2, TDep3 dep3);
    }

    public interface ICommand<in TDep1, in TDep2, in TDep3, in TDep4>
    {
        void Execute(TDep1 dep1, TDep2 dep2, TDep3 dep3, TDep4 dep4);
    }
}