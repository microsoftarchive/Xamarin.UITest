using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Xamarin.UITest.Repl.Evaluation;

namespace Xamarin.UITest.Repl.Repl
{
    public class ReplFacade
    {
        private readonly CSharpReplEngine _replEngine;
        private readonly List<string> _commandHistory;

        private int _historyBackCount;
        private ContextHelp _contextHelp;
        private readonly CompletionEngine _completionEngine;

        public ReplFacade()
        {
            _replEngine = new CSharpReplEngine();

            _completionEngine = new CompletionEngine();

            _commandHistory = new List<string>();
        }

        public Evaluation.ReplResult RunCode(string code)
        {
            if (code != _commandHistory.LastOrDefault())
            {
                _commandHistory.Add(code);
            }

            _historyBackCount = 0;

            return _replEngine.Evaluate(code);
        }

        public string PreviousCommand()
        {
            if (!_commandHistory.Any())
            {
                return null;
            }

            _historyBackCount = Math.Min(_historyBackCount + 1, _commandHistory.Count);
            return _commandHistory[_commandHistory.Count - _historyBackCount];
        }

        public string NextCommand()
        {
            _historyBackCount = Math.Max(_historyBackCount - 1, 0);

            if (_historyBackCount <= 0)
            {
                return null;
            }

            return _commandHistory[_commandHistory.Count - _historyBackCount];
        }

        public ContextHelp GetContextHelp(string code, int? contextIndex = null, bool ctrlSpace = false)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return new ContextHelp();
            }

            var index = contextIndex.GetValueOrDefault(code.Length);

            var currentChar = contextIndex.HasValue && contextIndex > 0 ? code[index - 1] : code.Last();

            if (!char.IsLetterOrDigit(currentChar) && currentChar != '.')
            {
                return new ContextHelp();
            }

            var completionInfo = _completionEngine.Evaluate(code, _replEngine.scriptState);

            Debug.WriteLine("Evaluated completions:");

            string prevSelected = null;

            if (_contextHelp != null && _contextHelp.Completions.Any() && _contextHelp.CompletionIndex != 0)
            {
                prevSelected = _contextHelp.Completions[_contextHelp.CompletionIndex].DisplayText;
            }

            var completions = completionInfo.Completions.Distinct().ToArray();

            _contextHelp = new ContextHelp
            {
                CompletionStart = index - completionInfo.CompletionPrefix.Length,
                Completions = completions,
                Code = code,
                Index = index,
            };

            if (!string.IsNullOrWhiteSpace(prevSelected))
            {
                var prevIndex = Array.FindIndex(_contextHelp.Completions, x => string.Equals(x.DisplayText, prevSelected));

                if (prevIndex != -1)
                {
                    _contextHelp.CompletionIndex = prevIndex;
                }
            }

            return _contextHelp;
        }

        public ContextHelp NextCompletion()
        {
            _contextHelp.CompletionIndex = Math.Min(_contextHelp.CompletionIndex + 1, _contextHelp.Completions.Length - 1);
            return _contextHelp;
        }

        public ContextHelp PreviousCompletion()
        {
            _contextHelp.CompletionIndex = Math.Max(_contextHelp.CompletionIndex - 1, 0);
            return _contextHelp;
        }

        public InputState TabComplete()
        {
            var completion = _contextHelp.Completions[_contextHelp.CompletionIndex];

            var prefixLength = _contextHelp.Index - _contextHelp.CompletionStart;

            var code = _contextHelp.Code.Remove(_contextHelp.CompletionStart, prefixLength);

            code = code.Insert(_contextHelp.CompletionStart, completion.DisplayText);
            var index = _contextHelp.CompletionStart + completion.DisplayText.Length;

            return new InputState(code, index);
        }

        public void LoadAssembly(string assemblyPath)
        {
            _replEngine.LoadAssembly(assemblyPath);
            _completionEngine.LoadAssembly(assemblyPath);
        }

        public void AddUsing(string usingEntry)
        {
            _replEngine.AddUsing(usingEntry);
            _completionEngine.AddUsing(usingEntry);
        }
    }
}