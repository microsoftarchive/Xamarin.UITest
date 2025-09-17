using System;
using System.Linq;
using Xamarin.UITest.Repl.Repl;

namespace Xamarin.UITest.Repl
{
    public class PromptState
    {
        readonly string _input;
        readonly ConsoleString _prompt;
        readonly ContextHelp _contextHelp;
        readonly int _inputPosition;
        readonly int _inputTop;
        readonly int _historyBackCount;
        readonly int _bufferHeight;

        public PromptState(string input, ConsoleString prompt, ContextHelp contextHelp, int inputPosition, int inputTop, int historyBackCount, int bufferHeight)
        {
            _input = input;
            _prompt = prompt;
            _contextHelp = contextHelp;
            _inputPosition = inputPosition;
            _inputTop = inputTop;
            _historyBackCount = historyBackCount;
            _bufferHeight = bufferHeight;
        }

        public int HistoryBackCount
        {
            get { return _historyBackCount; }
        }

        public int BufferHeight
        {
            get { return _bufferHeight; }
        }

        public int InputTop
        {
            get { return _inputTop; }
        }

        public string Input
        {
            get { return _input; }
        }

        public ConsoleString Prompt
        {
            get { return _prompt; }
        }

        public ContextHelp ContextHelp
        {
            get { return _contextHelp; }
        }

        public int InputPosition
        {
            get { return _inputPosition; }
        }

        public int TotalNumberOfLines
        {
            get
            {
                //var inputStr = _prompt.Message + _input + _contextHelp.CompletionPostfix;
                var inputStr = _prompt.Message + _input;
                return GetNumberOfLines(inputStr) + GetNumberOfLines(string.Join(", ", _contextHelp.Completions.Select(x => x.DisplayText)));
            }
        }

        public int NumberOfPromptAndInputLines
        {
            get
            {
                var inputStr = _prompt.Message + _input;
                return GetNumberOfLines(inputStr);
            }
        }

        public PromptState PushTopUp()
        {
            return new PromptState(_input, _prompt, _contextHelp, _inputPosition, _inputTop - 1, _historyBackCount, _bufferHeight);
        }

        public PromptState OverwriteInputTop(int inputTop)
        { 
            return new PromptState(
                Input,
                Prompt,
                ContextHelp,
                InputPosition,
                inputTop,
                HistoryBackCount,
                BufferHeight
            );
        }

        static int GetNumberOfLines(string str)
        {
            if (String.IsNullOrEmpty(str))
            {
                return 0;
            }

            var numberOfLines = str.Length / Console.BufferWidth;

            if (str.Length % Console.BufferWidth > 0)
            {
                numberOfLines += 1;
            }

            return numberOfLines;
        }
    }
}