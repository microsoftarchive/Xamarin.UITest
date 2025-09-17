using System;

namespace Xamarin.UITest.Repl
{
    public class ReplCommand
    {
        readonly string[] _commands;
        readonly string _helpText;
        readonly Action _action;

        public ReplCommand(string command, string helpText, Action action)
        {
            _commands = new[] { command };
            _helpText = helpText;
            _action = action;
        }

        public ReplCommand(string[] commands, string helpText, Action action)
        {
            _commands = commands;
            _helpText = helpText;
            _action = action;
        }

        public string[] Commands
        {
            get { return _commands; }
        }

        public string HelpText
        {
            get { return _helpText; }
        }

        public Action Action
        {
            get { return _action; }
        }
    }
}