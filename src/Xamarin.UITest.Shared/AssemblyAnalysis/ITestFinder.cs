namespace Xamarin.UITest.Shared.AssemblyAnalysis
{
    public interface ITestFinder
    {
        TestMethod[] Find(string assemblyFile, string[] fixtures, string[] includedCategories, string[] excludedCategories);
    }
}