using System.Collections.Generic;

namespace Xamarin.UITest.Shared.AssemblyAnalysis
{
    public interface ITestChunker
    {
        public string Name { get; }

        public List<TestChunk> GetChunks(List<TestMethod> testMethods);
    }
}