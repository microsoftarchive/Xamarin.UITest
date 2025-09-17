using System;

namespace Xamarin.UITest.Shared.Android.Adb
{
    /// <summary>
    /// see https://developer.android.com/studio/command-line/shell.html#pm
    /// </summary>
    internal class PackageManagerCommandOptions : Parameters
    {
        public PackageManagerCommandOptions Packages(params PackagesOption[] options)
        {
            _arguments.Clear();
            _arguments.Add("packages");

            foreach (var option in options)
            {
                _arguments.Add(option.ToParameter());
            }

            return this;
        }
    }
}
