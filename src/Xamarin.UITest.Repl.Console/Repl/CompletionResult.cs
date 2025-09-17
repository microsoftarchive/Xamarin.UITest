using Microsoft.CodeAnalysis.Completion;

namespace Xamarin.UITest.Repl.Repl
{
    /// <summary>
    /// Class for storing completions for current statement in REPL.
    /// </summary>
    public class CompletionResult
    {
        /// <summary>
        /// List of completions.
        /// </summary>
        public CompletionItem[] Completions { get; private set; }

        /// <summary>
        /// Completion prefix.
        /// </summary>
        public string CompletionPrefix { get; private set; }

        /// <summary>
        /// Initializes <see cref="CompletionResult"/> class with given parameters.
        /// </summary>
        /// <param name="completions">Possible completions list.</param>
        /// <param name="completionPrefix">Completion prefix.</param>
        public CompletionResult(CompletionItem[] completions, string completionPrefix)
        {
            Completions = completions;
            CompletionPrefix = completionPrefix;
        }
    }
}