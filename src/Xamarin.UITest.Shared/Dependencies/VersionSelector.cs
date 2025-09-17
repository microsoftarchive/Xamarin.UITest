using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Xamarin.UITest.Shared.Dependencies
{
    public class VersionSelector
    {
        public string PickLatest(string pattern, string[] inputs)
        {
            var regex = new Regex(pattern);

            var matches = (from input in inputs
                           let match = regex.Match(input)
                           where match.Success
                           let ints = ExtractDigitGroups(match.Groups.Cast<Group>())
                           select new { Input = input, Ints = ints })
                          .ToArray();

            return matches
                .OrderByDescending(x => x.Ints, new IntArrayComparer())
                .Select(x => x.Input)
                .FirstOrDefault();
        }

        public string PickLatest(string[] inputs)
        {
            var regex = new Regex(@"(\d+)");

            var matches = (from input in inputs
                           let m = regex.Matches(input)
                           let ints = ExtractDigitGroups(m.Cast<Match>().SelectMany(x => x.Groups.Cast<Group>()))
                           select new { Input = input, Ints = ints })
                          .ToArray();

            return matches
                .OrderByDescending(x => x.Ints, new IntArrayComparer())
                .Select(x => x.Input)
                .FirstOrDefault();
        }

        int[] ExtractDigitGroups(IEnumerable<Group> groups)
        {
            int notUsed;

            return groups
                .Where(g => int.TryParse(g.Value, out notUsed))
                .Select(g => int.Parse(g.Value))
                .ToArray();
        }

        class IntArrayComparer : Comparer<int[]>
        {
            public override int Compare(int[] x, int[] y)
            {
                x = x ?? new int[0];
                y = y ?? new int[0];

                var maxLength = Math.Max(x.Length, y.Length);

                for (var i = 0; i < maxLength; i++)
                {
                    var xVal = x.Length > i ? x[i] : 0;
                    var yVal = y.Length > i ? y[i] : 0;

                    if (xVal != yVal)
                    {
                        return xVal - yVal;
                    }
                }

                return 0;
            }
        }
    }
}