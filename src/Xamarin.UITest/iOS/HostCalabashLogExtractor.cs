using System.Text.RegularExpressions;
using System.Linq;
using System;

namespace Xamarin.UITest.iOS
{
    internal static class HostCalabashLogExtractor
    {
        static Regex _beginEnd = new Regex("OUTPUT_JSON:(.+?)END_OUTPUT", RegexOptions.Singleline); 


        // Try to extract reply to command with index commandIndex. Returns null if reply is not found. Throws exception if last command was not recived by run loop
        public static string Extract(int commandIndex, string output)
        {
            var indexString = string.Format("\"index\":{0}", commandIndex);
            var matches = _beginEnd.Matches(output);


            foreach (Match match in matches)
            {
                var potentialResult = match.Groups[1].Value;
                if (potentialResult.Contains(indexString))
                {
                    var newline = new char[] {
                        '\n',
                        '\r'
                    };
                    return potentialResult.TrimStart(newline).TrimEnd(newline);
                } 
            }

            var matchesInAUsableForm = new Match[matches.Count];
            matches.CopyTo(matchesInAUsableForm, 0);

            Match lastOutput = matchesInAUsableForm.LastOrDefault();
            if (lastOutput != null && lastOutput.Groups[1].Value.Contains("unable to execute:"))
            {
                throw new Exception("Last command was not read by run loop");
            }
            return null;

        }
    }
}