using System;
using System.Collections.Generic;

namespace Xamarin.UITest.Shared.Android.Adb
{
    internal class Parameters
    {
        protected readonly IList<string> _arguments = new List<string>();

        public override string ToString()
        {
            return string.Join(" ", _arguments);
        }
    }
}
