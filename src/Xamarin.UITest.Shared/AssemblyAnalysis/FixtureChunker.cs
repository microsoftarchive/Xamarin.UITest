using System.Collections.Generic;
using System.Linq;

namespace Xamarin.UITest.Shared.AssemblyAnalysis
{
    public class FixtureChunker : ITestChunker
    {
        public string Name => "fixture";

        public List<TestChunk> GetChunks(List<TestMethod> testMethods)
        {
            return testMethods.GroupBy(x => new { an = x.AssemblyFileName, tn = x.TypeName }).Select(
                g => new TestChunk(g.Key.an, g.Key.tn, GetExcludeReason(g.ToArray()))).ToList();
        }

        static string GetExcludeReason(TestMethod[] testMethods)
        {
            var testsLeft = testMethods.Any(tm => !tm.IsExcluded);
            if (!testsLeft)
            {
                return "No tests in Fixture would be run";
            }
            return null;
        }
    }
}