using System;
using Xamarin.UITest.Shared.Execution;

namespace Xamarin.UITest.Shared.Android.Queries
{
    public class QueryInstalledTokenMatchPrevious : IQuery<bool>
    {
        readonly string _token;

        public QueryInstalledTokenMatchPrevious(string token)
        {
            _token = token;
        }

        public bool Execute()
        {
            if (_token == null)
            {
                return false;
            }

            return true;
        }
    }
}

