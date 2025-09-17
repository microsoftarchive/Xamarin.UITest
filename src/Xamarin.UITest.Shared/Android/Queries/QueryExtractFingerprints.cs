using System.Linq;
using System.Text.RegularExpressions;
using Xamarin.UITest.Shared.Execution;

namespace Xamarin.UITest.Shared.Android.Queries
{    public class QueryExtractFingerprints : IQuery<string[]>
    {
        // Prioritize SHA256 fingerprints while maintaining backward compatibility.
        // SHA256 is the preferred format for SDL compliance.
        static readonly Regex FingerprintsRegex = new Regex( @"(?:SHA ?256.*:\s*(?<fp>([a-f\d]{2}:){31}[a-f\d]{2})|MD5.*:\s*(?<fp>([a-f\d]{2}:){15}[a-f\d]{2}))",
                                                            RegexOptions.Compiled | RegexOptions.IgnoreCase );

        readonly string _content;

        public QueryExtractFingerprints( string content )
        {
            _content = content;
        }

        public string[] Execute()
        {
            return FingerprintsRegex.Matches( _content )
                                    .OfType<Match>()
                                    .Select( x => x.Groups["fp"].Value )
                                    .ToArray();
        }
    }
}
