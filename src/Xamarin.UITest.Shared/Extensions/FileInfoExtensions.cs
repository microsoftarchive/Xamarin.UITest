using System;
using System.IO;

namespace Xamarin.UITest.Shared.Extensions
{
    public static class FileInfoExtensions
    {
        public static bool IsInDirectory(this FileInfo file, DirectoryInfo directory)
        {
            return new FileInfo(Path.Combine(directory.FullName,file.Name)).FullName == file.FullName;
        }
    }
}