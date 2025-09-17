using System.IO;

namespace Xamarin.UITest.Tests.Helpers
{
	internal static class FileSystemHelper
	{
		public static void CopyDirectory(DirectoryInfo sourceDirectory, string destinationDirectoryPath, bool recursive)
		{
			if (!sourceDirectory.Exists)
				throw new DirectoryNotFoundException($"Source directory not found: {sourceDirectory.FullName}");

			DirectoryInfo[] dirs = sourceDirectory.GetDirectories();

			Directory.CreateDirectory(destinationDirectoryPath);

			foreach (FileInfo file in sourceDirectory.GetFiles())
			{
				string targetFilePath = Path.Combine(destinationDirectoryPath, file.Name);
				file.CopyTo(targetFilePath, overwrite: true);
			}

			if (recursive)
			{
				foreach (DirectoryInfo subDir in dirs)
				{
					string newDestinationDir = Path.Combine(destinationDirectoryPath, subDir.Name);
					CopyDirectory(subDir, newDestinationDir, true);
				}
			}
		}
	}
}

