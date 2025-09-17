using Microsoft.CodeAnalysis.Completion;

namespace Xamarin.UITest.Repl.Repl
{
    /// <summary>
    /// ContextHelp class with data that could be used for help messages on the current REPL state.
    /// </summary>
    public class ContextHelp
    {
        /// <summary>
        /// Initializes new <see cref="ContextHelp"/> instance.
        /// </summary>
        public ContextHelp()
        {
            Completions = new CompletionItem[0];
        }

        public int CompletionStart { get; set; }
        public CompletionItem[] Completions { get; set; }
        public int CompletionIndex { get; set; }
        public string Code { get; set; }
        public int Index { get; set; }
    }
}