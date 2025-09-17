using System;
using NUnit.Framework;
using Should;
using Xamarin.UITest.Shared.Extensions;
using System.IO;

namespace Xamarin.UITest.Tests.Utils
{
	[TestFixture]
	public class ExtensionTests
	{
		[Test]
		public void CombineAbsoluteUrlContainingPathWithRelative()
		{
			var baseUri = new Uri ("http://localhost:12345/endpoint");

			var newUri = baseUri.Combine("test");

			newUri.ShouldEqual (new Uri ("http://localhost:12345/endpoint/test"));
		}

		[Test]
		public void CombineAbsoluteUrlNoTrailingSlashWithRelative()
		{
			var baseUri = new Uri ("http://localhost:12345");

			var newUri = baseUri.Combine("test");

			newUri.ShouldEqual (new Uri ("http://localhost:12345/test"));
		}

		[Test]
		public void CombineAbsoluteUrlTrailingSlashWithRelative()
		{
			var baseUri = new Uri ("http://localhost:12345/");

			var newUri = baseUri.Combine("test");

			newUri.ShouldEqual (new Uri ("http://localhost:12345/test"));
		}

		[Test]
		public void CombineAbsoluteUrlContainingPathWithRelativeWithSlash()
		{
			var baseUri = new Uri ("http://localhost:12345/endpoint");

			var newUri = baseUri.Combine("/test");

			newUri.ShouldEqual (new Uri ("http://localhost:12345/endpoint/test"));
		}

		[Test]
		public void CombineAbsoluteUrlNoTrailingSlashWithRelativeWithSlash()
		{
			var baseUri = new Uri ("http://localhost:12345");

			var newUri = baseUri.Combine("/test");

			newUri.ShouldEqual (new Uri ("http://localhost:12345/test"));
		}

		[Test]
		public void CombineAbsoluteUrlTrailingSlashWithRelativeWithSlash()
		{
			var baseUri = new Uri ("http://localhost:12345/");

			var newUri = baseUri.Combine("/test");

			newUri.ShouldEqual (new Uri ("http://localhost:12345/test"));
		}

        [Test]
        public void FileIsInDirectoryNormalBackslash()
        {
            DirectoryInfo directory = new DirectoryInfo("..\\assemblies");

            FileInfo file = new FileInfo(Path.Combine(directory.FullName,"myfile.dll"));

            file.IsInDirectory(directory).ShouldEqual(true, string.Format("File {0} should be in Directory {1}", file.FullName, directory.FullName));
        }

        [Test]
        public void FileIsInDirectoryTrailingBackslash()
        {
            DirectoryInfo directory = new DirectoryInfo("..\\assemblies\\");

            FileInfo file = new FileInfo(Path.Combine(directory.FullName,"myfile.dll"));

            file.IsInDirectory(directory).ShouldEqual(true, string.Format("File {0} should be in Directory {1}", file.FullName, directory.FullName));
        }

        [Test]
        public void FileIsInDirectoryNormalSlash()
        {
            DirectoryInfo directory = new DirectoryInfo("../assemblies");

            FileInfo file = new FileInfo(Path.Combine(directory.FullName,"myfile.dll"));

            file.IsInDirectory(directory).ShouldEqual(true, string.Format("File {0} should be in Directory {1}", file.FullName, directory.FullName));
        }

        [Test]
        public void FileIsInDirectoryTrailingSlash()
        {
            DirectoryInfo directory = new DirectoryInfo("../assemblies/");

            FileInfo file = new FileInfo(Path.Combine(directory.FullName,"myfile.dll"));

            file.IsInDirectory(directory).ShouldEqual(true, string.Format("File {0} should be in Directory {1}", file.FullName, directory.FullName));
        }

        [Test]
        public void FileIsNotInDirectoryNormalBackslash()
        {
            DirectoryInfo directory = new DirectoryInfo("..\\assemblies");

            FileInfo file = new FileInfo(Path.Combine("..\\elsewhere","myfile.dll"));

            file.IsInDirectory(directory).ShouldEqual(false, string.Format("File {0} should not be in Directory {1}", file.FullName, directory.FullName));
        }

        [Test]
        public void FileIsNotInDirectoryTrailingBackslash()
        {
            DirectoryInfo directory = new DirectoryInfo("..\\assemblies\\");

            FileInfo file = new FileInfo(Path.Combine("..\\elsewhere\\","myfile.dll"));

            file.IsInDirectory(directory).ShouldEqual(false, string.Format("File {0} should not be in Directory {1}", file.FullName, directory.FullName));
        }

        [Test]
        public void FileIsNotInDirectoryNormalSlash()
        {
            DirectoryInfo directory = new DirectoryInfo("../assemblies");

            FileInfo file = new FileInfo(Path.Combine("../elsewhere","myfile.dll"));

            file.IsInDirectory(directory).ShouldEqual(false, string.Format("File {0} should not be in Directory {1}", file.FullName, directory.FullName));
        }

        [Test]
        public void FileIsNotInDirectoryTrailingSlash()
        {
            DirectoryInfo directory = new DirectoryInfo("../assemblies/");

            FileInfo file = new FileInfo(Path.Combine("../elsewhere/","myfile.dll"));

            file.IsInDirectory(directory).ShouldEqual(false, string.Format("File {0} should not be in Directory {1}", file.FullName, directory.FullName));
        }
    }
}

