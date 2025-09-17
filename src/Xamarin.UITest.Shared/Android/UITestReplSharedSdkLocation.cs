using System;

namespace Xamarin.UITest.Shared.Android
{
    public static class UITestReplSharedSdkLocation
    {
        const string UITestAdbPathEnvVar = "UITEST_REPL_ANDROID_SDK_PATH";

        public static bool SharedSdkPathIsSet()
        {
            var currentSdkPath = Environment.GetEnvironmentVariable(UITestAdbPathEnvVar);
            return !string.IsNullOrEmpty(currentSdkPath);
        }

        public static string GetSharedSdkPathAndReset()
        { 
            var path = Environment.GetEnvironmentVariable(UITestAdbPathEnvVar);
            SetSharedSdkPath(null);
            return path;
        }

        public static void SetSharedSdkPath(string path)
        { 
            Environment.SetEnvironmentVariable(UITestAdbPathEnvVar, path);
        }
    }
}
