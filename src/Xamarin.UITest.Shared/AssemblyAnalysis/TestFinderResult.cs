using System;
using System.Collections.Generic;
using System.Linq;

namespace Xamarin.UITest.Shared.AssemblyAnalysis
{
    public class TestFinderResult
    {
        readonly Dictionary<string, Dictionary<string, TestChunk[]>> _result;

        public TestFinderResult(Dictionary<string, Dictionary<string, TestChunk[]>> result)
        {
            _result = result;
        }

        private TestChunk[] Chunks(string chunker)
        {
            if (_result.TryGetValue(chunker, out Dictionary<string, TestChunk[]> testPerAssembly))
            {
                return testPerAssembly.SelectMany(t => t.Value).ToArray();
            }
            return Array.Empty<TestChunk>();
        }

        public bool AllExcluded(string chunker)
        {
            return Chunks(chunker).All(t => t.IsExcluded());
        }

        public  Dictionary<string, TestChunk[]> ExcludeTestByExcludeReason(string chunker)
        {
            return Chunks(chunker).Where(tc=>tc.IsExcluded()).GroupBy(tc => tc.ExcludeReason).ToDictionary(g=>g.Key, g=>g.ToArray());
        }

        public Dictionary<string, Dictionary<string, List<string>>> AsTestTargets()
        {
            return _result.ToDictionary(p => p.Key, p1 => p1.Value.ToDictionary(p => p.Key, p => p.Value.Select(t => t.TestTarget).ToList()));
        }

        public Dictionary<string, Dictionary<string, string[]>> AsExcludedTestTargets()
        {
            return _result.ToDictionary(p => p.Key, p1 => p1.Value.ToDictionary(p => p.Key, p => p.Value.Where(t=>t.IsExcluded()).Select(t => t.TestTarget).ToArray()));
        }

        public Dictionary<string, Dictionary<string, List<string>>> AsExcludedTestTargetsWithReason()
        {
            return _result.ToDictionary(p => p.Key, p1 => p1.Value.ToDictionary(p => p.Key, p => p.Value.Where(t => t.IsExcluded()).Select(t=> string.Join("++", t.TestTarget, t.ExcludeReason)).ToList()));
        }

        public string[] IncludedTestTargets(List<string> requestedFixtures)
        {
            var validMethods = FixtureNames("method");
            var validFixtures = FixtureNames("fixture");

            var atLeastOneMethodRequested = validMethods.Any(requestedFixtures.Contains);
            var onlyMethodsRequested = requestedFixtures.Count() == validMethods.Count() && 
                    validMethods.Union(requestedFixtures).Count() == requestedFixtures.Count();

            if (atLeastOneMethodRequested && !onlyMethodsRequested)
            {
                validFixtures = requestedFixtures.Where(validFixtures.Contains).ToList();
                var explicitlySelectedMethods = requestedFixtures.Where(validMethods.Contains);
                var implicitlySelectedMethods = validFixtures.SelectMany(sf => validMethods.Where(vm => vm.StartsWith($"{sf}.")));
                validMethods = explicitlySelectedMethods.Concat(implicitlySelectedMethods).ToList();
            }
            
            return (onlyMethodsRequested ? validMethods : validMethods.Union(validFixtures)).ToArray();
        }

        List<string> FixtureNames(string key)
        {
            return _result[key].Select(e => e.Value).SelectMany(e => e.Where(tc => !tc.IsExcluded())).Select(e => e.TestTarget).ToList();
        }
    }
}