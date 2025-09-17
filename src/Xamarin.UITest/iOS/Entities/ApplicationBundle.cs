using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Xamarin.UITest.iOS.ApplicationSigning.Entities;
using Xamarin.UITest.XDB;
using Xamarin.UITest.XDB.Services;

[assembly: InternalsVisibleTo("Xamarin.UITest.Tests.Shared")]
namespace Xamarin.UITest.iOS.Entities
{
    internal class ApplicationBundle
    {
        /// <summary>
        /// <see cref="DirectoryInfo"/> of application bundle.
        /// </summary>
        public readonly DirectoryInfo AppBundle;

        /// <summary>
        /// Name of application bundle.
        /// </summary>
        public string Name => AppBundle.Name;

        /// <summary>
        /// Application bundle's info.plist file name.
        /// </summary>
        private static readonly string InfoPlistFileName = "info.plist";

        /// <summary>
        /// Path to application bundle's info.plist file.
        /// </summary>
        private string InfoPlistFilePath => Path.Combine(path1: AppBundle.FullName, path2: InfoPlistFileName);

        /// <summary>
        /// Application bundle's .DS_STORE file name.
		/// </summary>
		private static readonly string DSStoreFileName = ".DS_STORE";

		/// <summary>
		/// Path to application bundle's .DS_STORE file.
		/// </summary>
		private string DSStoreFilePath => Path.Combine(path1: AppBundle.FullName, path2: DSStoreFileName);

		/// <summary>
		/// .DS_STORE file info.
		/// </summary>
		public FileInfo DSStoreFile => new(fileName: DSStoreFilePath);

		/// <summary>
		/// Application bundle's embedded provisioning profile file name.
		/// </summary>
		public static readonly string EmbeddedProvisioningProfileFileName = "embedded.mobileprovision";

		/// <summary>
		/// Path to application bundle's embedded provisioning profile file.
		/// </summary>
		private string EmbeddedProvisioningProfileFilePath => Path.Combine(path1: AppBundle.FullName, path2: EmbeddedProvisioningProfileFileName);

		/// <summary>
		/// embedded.mobileprovision file info.
		/// </summary>
		public FileInfo EmbeddedProvisioningProfileFile => new(fileName: EmbeddedProvisioningProfileFilePath);

		/// <summary>
		/// info.plist file info.
		/// </summary>
		public readonly FileInfo InfoPlistFile;

		/// <summary>
		/// Application bundle's identifier.
		/// </summary>
		public readonly string BundleIdentifier;

		/// <summary>
		/// Main executable of application bundle.
		/// </summary>
		public readonly string BundleExecutable;

        private readonly ILoggerService LoggerService;

		public ApplicationBundle (string appBundlePath)
		{
			AppBundle = new(path: appBundlePath);

			if (!AppBundle.Exists)
			{
				throw new ArgumentException(message: "Provided application bundle path does not exist or have incorrect format.");
			}

			try
			{
				InfoPlistFile = new(fileName: InfoPlistFilePath);
			}
			catch (ArgumentException argumentException)
			{
				throw new Exception(message: $"Info.plist couldn't be found inside application bundle {appBundlePath}", innerException: argumentException);
			}

			LoggerService = XdbServices.GetRequiredService<ILoggerService>();
		}

		public FileInfo ExtractEmbeddedProvisioningProfile(string extractionPath)
		{
			if (EmbeddedProvisioningProfileFile.Exists)
			{
				DirectoryInfo extractionDirectory = new(path: extractionPath);
				if (!extractionDirectory.Exists)
				{
					extractionDirectory.Create();
				}

				string extractedProvisioningProfilePath = Path.Combine(path1: extractionDirectory.FullName, path2: EmbeddedProvisioningProfileFile.Name);
				EmbeddedProvisioningProfileFile.CopyTo(extractedProvisioningProfilePath, overwrite: true);
				return new FileInfo(fileName: extractedProvisioningProfilePath);
			}
			return null;
		}

        public void ReplaceEmbeddedProvisioningProfile(ProvisioningProfile newProvisioningProfile)
        {
            LoggerService.LogInfo(message: $"Copying {newProvisioningProfile.ProvisioningProfileFile.FullName} to {AppBundle}");
            newProvisioningProfile.ProvisioningProfileFile.CopyTo(destFileName: Path.Combine(path1: AppBundle.FullName, path2: EmbeddedProvisioningProfileFileName), overwrite: true);
        }

		/// <summary>
		/// Returns .framework bundles from Frameworks directory.
		/// </summary>
		/// <returns>List of .framework bundles.</returns>
		public List<DirectoryInfo> GetEmbeddedFrameworks()
		{
			DirectoryInfo frameworksDirectory = new(path: Path.Combine(AppBundle.FullName, "Frameworks"));
			if (frameworksDirectory.Exists)
			{
				return frameworksDirectory.GetDirectories().ToList();
			}
			return new List<DirectoryInfo>();
		}

		public List<DirectoryInfo> GetXCTestBundles()
		{
			DirectoryInfo plugInsDirectory = new(path: Path.Combine(path1: AppBundle.FullName, path2: "PlugIns"));
			if (plugInsDirectory.Exists)
			{
				return plugInsDirectory.GetDirectories(searchPattern: "*.xctest").ToList();
			}
			return new List<DirectoryInfo>();
		}

		/// <summary>
		/// Return .dylib files from Frameworks directory of bundle.
		/// </summary>
		/// <returns></returns>
		public List<FileInfo> GetDylibsFromFrameworksDirectory()
		{
			DirectoryInfo frameworksDirectory = new(path: Path.Combine(AppBundle.FullName, "Frameworks"));
			if (frameworksDirectory.Exists)
			{
				return frameworksDirectory.GetFiles(searchPattern: "*.dylib", searchOption: SearchOption.TopDirectoryOnly).ToList();
			}
			return new List<FileInfo>();
		}
	}
}