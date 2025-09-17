using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.UITest.Repl.Repl;
using Xamarin.UITest.Shared.Json;

namespace Xamarin.UITest.Repl
{
    public class PromptHandler
    {
        readonly ConsoleString _prompt;
        readonly ReplFacade _replFacade;
        readonly List<char> _inputBuffer = new List<char>();
        readonly List<HistoryEntry> _history = new List<HistoryEntry>();
        readonly ReplCommand[] _commands;

        PromptState _prevState;

        public PromptHandler(ConsoleString prompt, ReplFacade replFacade)
        {
            _prompt = prompt;
            _replFacade = replFacade;

            _commands = new[]
            {
                new ReplCommand("help", "Prints the help screen.", PrintHelp), 
                new ReplCommand("copy", "Copies the command history to the clipboard.", CopyClipboard), 
                new ReplCommand("tree", "Prints a tree of the app view elements.", PrintTree), 
                new ReplCommand(new []{ "quit", "exit" }, "Ends the REPL.", ExitRepl), 
            };
        }

        void PrintHelp()
        {
            WriteLine();

            foreach (var command in _commands)
            {
                const string separator = " / ";
                var commandLength = string.Join(separator, command.Commands).Length;

                for (int i = 0; i < command.Commands.Length; i++)
                {
                    if (i > 0)
                    {
                        Write(separator);
                    }

                    Write(command.Commands[i], ConsoleColor.Yellow);
                }

                var padding = new string(' ', 15 - commandLength);

                Write(padding);
                Write(" - ");
                WriteLine(command.HelpText);
            }

            WriteLine();
        }

        void PrintTree()
        {
            _replFacade.RunCode("app.Print.Tree(true)");
        }

        void ExitRepl()
        {
            Environment.Exit(0);
        }

        public void Start()
        {
            _prevState = GetNewPromptState(Console.CursorTop, 0, 0, Console.BufferHeight);
            RenderPrompt(_prevState, _prevState);
        }

        public void HandleInput(ConsoleKeyInfo key)
        {
            var inputPosition = _prevState.InputPosition;
            var historyBackCount = _prevState.HistoryBackCount;

            if (key.Key == ConsoleKey.Enter)
            {
                _prevState = CleanUpInput(_prevState);

                var line = string.Concat(_inputBuffer);

                var matchedReplCommand = _commands.FirstOrDefault(x => x.Commands.Any(c => string.Equals(line, c)));

                if (matchedReplCommand != null)
                {
                    matchedReplCommand.Action();
                }
                else if (!string.IsNullOrWhiteSpace(line))
                {
                    try
                    {
                        var result = _replFacade.RunCode(line);

                        if (result.HasError)
                        {
                            WriteLine(result.Error.Trim(), ConsoleColor.Red);
                        }

                        if (result.HasValue)
                        {
                            JsonPrettyPrinter.PrettyPrintObj(result.Value, true, IndentFormat.Normal);
                            WriteLine();
                        }

                        _history.Add(new HistoryEntry(line, !result.HasError));
                    }
                    catch (Exception ex)
                    {
                        _history.Add(new HistoryEntry(line, false));

                        Write("Exception: ");
                        WriteLine(ex.Message, ConsoleColor.Red);
                    }
                }

                _inputBuffer.Clear();
                inputPosition = 0;
                historyBackCount = 0;

                _prevState = GetNewPromptState(Console.CursorTop, inputPosition, 0, Console.BufferHeight);
            }
            else if (key.Key == ConsoleKey.Backspace)
            {
                if (inputPosition > 0)
                {
                    inputPosition -= 1;
                    _inputBuffer.RemoveAt(inputPosition);
                }
            }
            else if (key.Key == ConsoleKey.Tab)
            {
                var result = _replFacade.GetContextHelp(string.Join(string.Empty, _inputBuffer), inputPosition, true);

                if (result.Completions.Any())
                {
                    var inputState = _replFacade.TabComplete();
                    _inputBuffer.Clear();
                    _inputBuffer.AddRange(inputState.code);
                    inputPosition = inputState.index;
                }
            }
            else if (key.Key == ConsoleKey.LeftArrow)
            {
                if (inputPosition > 0)
                {
                    inputPosition -= 1;
                }
            }
            else if (key.Key == ConsoleKey.RightArrow)
            {
                if (inputPosition < _inputBuffer.Count)
                {
                    inputPosition += 1;
                }
            }
            else if (key.Key == ConsoleKey.UpArrow)
            {
                if (_history.Count > historyBackCount)
                {
                    historyBackCount += 1;

                    _inputBuffer.Clear();
                    _inputBuffer.AddRange(_history[_history.Count - historyBackCount].Entry);
                    inputPosition = _inputBuffer.Count;
                }
            }
            else if (key.Key == ConsoleKey.DownArrow)
            {
                historyBackCount -= 1;

                if (historyBackCount <= 0)
                {
                    historyBackCount = 0;
                    _inputBuffer.Clear();
                    inputPosition = _inputBuffer.Count;
                }
                else
                {
                    _inputBuffer.Clear();
                    _inputBuffer.AddRange(_history[_history.Count - historyBackCount].Entry);
                    inputPosition = _inputBuffer.Count;
                }
            }
            else if (key.KeyChar.ToString().Length > 0)
            {
                _inputBuffer.Insert(inputPosition, key.KeyChar);
                inputPosition += 1;
            }

            var newState = GetNewPromptState(_prevState.InputTop, inputPosition, historyBackCount, Console.BufferHeight);
            _prevState = RenderPrompt(_prevState, newState);
        }

        PromptState GetNewPromptState(int inputTop, int inputPosition, int historyBackCount, int bufferHeight)
        {
            var input = string.Concat(_inputBuffer);
            var contextHelp = _replFacade.GetContextHelp(input, inputPosition, true);
            return new PromptState(input, _prompt, contextHelp, inputPosition, inputTop, historyBackCount, bufferHeight);
        }

        static PromptState RenderPrompt(PromptState prevState, PromptState newState)
        {
            if (prevState.InputTop <= 0 || prevState.InputTop >= Console.BufferHeight)
            {
                prevState = prevState.OverwriteInputTop(Console.BufferHeight - 1);
            }

            if (Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix)
            {
                // Adjust for term resizing
                if (prevState.BufferHeight != newState.BufferHeight)
                {
                    var adjustedTop = newState.InputTop + (newState.BufferHeight - prevState.BufferHeight);
                    newState = newState.OverwriteInputTop(adjustedTop);
                }
            }

            Console.CursorTop = prevState.InputTop;
            Console.CursorLeft = 0;

            for (var i = 0; i < prevState.TotalNumberOfLines; i++)
            {
                ClearConsoleLine(prevState.InputTop + i);
            }

            Console.CursorTop = prevState.InputTop;
            Console.CursorLeft = 0;

            var currentLine = (newState.Prompt.Message.Length + newState.InputPosition) / Console.BufferWidth;
            var currentCol = (newState.Prompt.Message.Length + newState.InputPosition) % Console.BufferWidth;

            while (newState.InputTop + newState.TotalNumberOfLines > Console.BufferHeight || newState.InputTop + currentLine >= Console.BufferHeight)
            {
                newState = newState.PushTopUp();
                Console.CursorTop = Console.BufferHeight - 1;
                Console.CursorLeft = 0;
                Console.WriteLine();
            }

            Console.CursorTop = newState.InputTop;
            Console.CursorLeft = 0;

            Write(newState.Prompt);
            Write(newState.Input);
            //Write(newState.ContextHelp.CompletionPostfix, ConsoleColor.Magenta);

            if (newState.ContextHelp.Completions.Any())
            {
                Console.CursorTop = newState.InputTop + newState.NumberOfPromptAndInputLines;
                Console.CursorLeft = 0;

                Write(string.Join(", ", newState.ContextHelp.Completions.Select(x => x.DisplayText)));
            }

            Console.CursorTop = newState.InputTop + currentLine;
            Console.CursorLeft = currentCol;

            return newState;
        }

        static PromptState CleanUpInput(PromptState promptState)
        {
            while (promptState.InputTop + promptState.TotalNumberOfLines > Console.BufferHeight)
            {
                promptState = promptState.PushTopUp();
                Console.CursorTop = Console.BufferHeight - 1;
                Console.CursorLeft = 0;
                Console.WriteLine();
            }

            for (var i = 0; i < promptState.TotalNumberOfLines; i++)
            {
                ClearConsoleLine(promptState.InputTop + i);
            }

            Console.CursorTop = promptState.InputTop;
            Console.CursorLeft = 0;

            Write(promptState.Prompt);
            Write(promptState.Input);

            Console.WriteLine();

            return promptState;
        }

        private static void ClearConsoleLine(int consoleLine)
        {
            Console.CursorTop = consoleLine < Console.BufferHeight && consoleLine > 0 ? 
                consoleLine : 
                Console.BufferHeight - 1;
            
            Console.CursorLeft = 0;

            if (Console.CursorTop + 1 == Console.BufferHeight)
            {
                Console.Write(new string(' ', Console.BufferWidth - 1));
            }
            else
            {
                Console.Write(new string(' ', Console.BufferWidth));
            }
        }

        static void Write(string str = null, ConsoleColor? color = null)
        {
            if (color != null)
            {
                Console.ForegroundColor = color.Value;
            }

            Console.Write(str);
            Console.ResetColor();
        }

        static void Write(ConsoleString consoleString)
        {
            if (consoleString.Color != null)
            {
                Console.ForegroundColor = consoleString.Color.Value;
            }

            Console.Write(consoleString.Message);
            Console.ResetColor();
        }

        static void WriteLine(string str = null, ConsoleColor? color = null)
        {
            if (color != null)
            {
                Console.ForegroundColor = color.Value;
            }

            if (str != null)
            {
                Console.WriteLine(str);
            }
            else
            {
                Console.WriteLine();
            }

            Console.ResetColor();
        }

        static void WriteLine(ConsoleString consoleString)
        {
            WriteLine(consoleString.Message, consoleString.Color);
        }

        void CopyClipboard()
        {
            Console.WriteLine("Copying history to clipboard.");
            var successfulEntries = _history.Where(x => x.Success).Select(x => string.Format("{0};", x.Entry));
            ClipboardHelper.SetClipboardText(string.Join(Environment.NewLine, successfulEntries));
        }
    }
}