using System;
using System.Linq;

namespace Xamarin.UITest.Shared.Android.Adb
{   
    /// <summary>
    /// see https://developer.android.com/studio/command-line/shell.html#IntentSpec
    /// </summary>
    internal class ActivityManagerIntentArguments : Parameters
    {
        public ActivityManagerIntentArguments AddAction(string action)
        {
            if (_arguments.Any(e => e.StartsWith($"-a")))
            {
                throw new Exception("'-a' parameter can be declared only once");
            }

            return Add($"-a {action}");
        }

        public ActivityManagerIntentArguments AddComponent(string component)
        {
            return Add($"-n {component}");
        }

        public ActivityManagerIntentArguments AddData(string key, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                Add($"-e \"{key}\" \"{value}\"");
            }
            return this;
        }

        ActivityManagerIntentArguments Add(string arg)
        {
            _arguments.Add(arg);
            return this;
        }
    }
}
