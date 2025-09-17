using System;
namespace Xamarin.UITest.Shared.Android.Adb
{
    internal enum PackagesOption
    {
        SeeAssociatedFiles,
        ShowEnabledOnly
    }

    internal static class PackagesOptionExtensions
    {
        public static string ToParameter(this PackagesOption option)
        { 
            switch (option)
            {
                case PackagesOption.SeeAssociatedFiles:
                    return "-f";
                case PackagesOption.ShowEnabledOnly:
                    return "-e";    
                default:
                    throw new ArgumentException($"{nameof(PackagesOption)} '{option}' is not valid");
            }
        }
    }
}
