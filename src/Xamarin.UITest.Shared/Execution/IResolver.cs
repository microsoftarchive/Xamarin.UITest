namespace Xamarin.UITest.Shared.Execution
{
    public interface IResolver
    {
        T Resolve<T>() where T : class;
    }
}