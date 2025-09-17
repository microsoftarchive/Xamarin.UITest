using System;
using System.IO;
using Xamarin.UITest.Shared.Logging;
using Xamarin.UITest.Shared.Processes;

namespace Xamarin.UITest.Shared.Dependencies
{
    public interface IJdkTools
    {
        string GetJarSignerPath();
        string GetKeyToolPath();
        string GetJavaPath();
        bool AreValid();
    }    public class JdkTools : IJdkTools
    {
        readonly DirectoryInfo _jdkDirectory;
        private readonly string _executableExtension;

		public JdkTools(DirectoryInfo jdkDirectory)
        {
            _jdkDirectory = jdkDirectory;
            // Determine if we're on Windows or non-Windows OS
            _executableExtension = Environment.OSVersion.Platform == PlatformID.Win32NT ? ".exe" : "";
        }

        public string GetJarSignerPath()
        {
            return Path.Combine(_jdkDirectory.FullName, "bin", "jarsigner" + _executableExtension);
        }

        public string GetKeyToolPath()
        {
            return Path.Combine(_jdkDirectory.FullName, "bin", "keytool" + _executableExtension);
        }

        public string GetJavaPath()
        {
            return Path.Combine(_jdkDirectory.FullName, "bin", "java" + _executableExtension);
        }

        public bool AreValid()
        {
            try
            {
                // FindCommand throws an argument exeption if a file doesn't exist
                ProcessRunner.FindCommand(GetJarSignerPath());
                ProcessRunner.FindCommand(GetKeyToolPath());
                ProcessRunner.FindCommand(GetJavaPath());
                return true;
            }
            catch (ArgumentException e)
            {
                Log.Debug(e.Message);
                return false;
            }
        }
    }
}