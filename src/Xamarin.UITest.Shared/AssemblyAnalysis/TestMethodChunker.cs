using System.Collections.Generic;
using System.Linq;

namespace Xamarin.UITest.Shared.AssemblyAnalysis
{
    /// <summary>
    /// <see cref="ITestChunker"/> implementation that chunks tests with test methods.
    /// </summary>
    public class TestMethodChunker : ITestChunker
    {
        public string Name
        {
            get
            {
                return "method";
            }
        }

        /// <summary>
        /// Gets chunks for each test method.
        /// For each test method will be created separate chunk.
        /// </summary>
        /// <param name="testMethods">Test methods chunking to be performed for.</param>
        /// <returns><see cref="List{T}"/> of <see cref="TestChunk"/>.</returns>
        public List<TestChunk> GetChunks(List<TestMethod> testMethods)
        {
            return testMethods
                .Select(testMethod => new TestChunk(
                    testMethod.AssemblyFileName,
                    testMethod.FullName,
                    testMethod.ExcludeReason))
                .ToList();
        }
    }
}
