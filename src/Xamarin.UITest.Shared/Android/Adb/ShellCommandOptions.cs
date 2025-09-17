using System;

namespace Xamarin.UITest.Shared.Android.Adb
{
    internal class ShellCommandOptions : Parameters
    {
        public ShellCommandOptions DumpSystem(params string[] filters)
        {
            _arguments.Clear();
            _arguments.Add("dumpsys");

            foreach (var filter in filters)
            {
                _arguments.Add(filter);
            }

            return this;
        }

        public ShellCommandOptions PropertByName(string propName)
        {
            _arguments.Clear();
            _arguments.Add($"getprop {propName}");
            return this;
        }
    }
}
