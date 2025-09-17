using System.Linq;

namespace Xamarin.UITest.Queries.Tokens
{
    internal static class TokenCodePrinter
    {
        public static string ToCodeString(ITokenContainer tokenContainer)
        {
            if (!tokenContainer.Tokens.Any())
            {
                return "*";
            }

            var codeStrings = tokenContainer
                .Tokens
                .Select(x => x.ToCodeString())
                .Where(x => !string.IsNullOrWhiteSpace(x));

            return string.Join(".", codeStrings);
        }
    }
}