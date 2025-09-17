using System;

namespace Xamarin.UITest.Shared.Dependencies
{
    public static class NUnitVersions
    {
        public static string RecommendedNUnitString
        {
            get { return "3.7+"; }
        }

        public static bool IsSupported(Version nunitVersion)
        {
            if (nunitVersion.Major == 3 && nunitVersion.Minor >= 7)
            {
                return true;
            }

            return nunitVersion.Major == 2 && nunitVersion > new Version(2, 6, 3);
        }
    }
}
