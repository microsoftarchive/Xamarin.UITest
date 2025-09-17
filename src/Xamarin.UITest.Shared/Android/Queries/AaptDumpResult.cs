using Xamarin.UITest.Shared.Extensions;
using System.Collections.Generic;

namespace Xamarin.UITest.Shared.Android.Queries
{
    public class AaptDumpResult
    {
        public AaptDumpResult(string packageName, List<string> permissions)
        {
            PackageName = packageName;
            Permissions = permissions;
        }

        public string PackageName { get; }

        public List<string> Permissions { get; }

        public bool IsValid => !PackageName.IsNullOrWhiteSpace();
    }
}