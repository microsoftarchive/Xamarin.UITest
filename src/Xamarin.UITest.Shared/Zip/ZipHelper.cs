using ICSharpCode.SharpZipLib.Zip;
using System.IO;

namespace Xamarin.UITest.Shared.Zip
{
    /// <summary>
    /// Performs operations with Zip Archives.
    /// </summary>
    internal static class ZipHelper
    {
        public static void Unzip(string zipFile, string target)
        {
            using ZipInputStream zipInputStream = new(File.OpenRead(zipFile));
            ZipEntry entry;
            while ((entry = zipInputStream.GetNextEntry()) != null)
            {
                // Create directory.
                if (entry.IsDirectory)
                {
                    Directory.CreateDirectory(Path.Combine(target, entry.Name));
                }

                if (entry.IsFile)
                {
                    using FileStream streamWriter = File.Create(Path.Combine(target, entry.Name));
                    int size;
                    byte[] data = new byte[2048];
                    while (true)
                    {
                        size = zipInputStream.Read(data, 0, data.Length);
                        if (size > 0)
                        {
                            streamWriter.Write(data, 0, size);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }

        public static void Unzip(Stream zipArchiveStream, string unzipPath)
        {
            using var archive = new ZipFile(zipArchiveStream);
            foreach (ZipEntry entry in archive)
            {
                var entryPath = Path.Combine(unzipPath, entry.Name);
                if (entry.IsDirectory)
                {
                    Directory.CreateDirectory(entryPath);
                    continue;
                }
                using var inputStream = archive.GetInputStream(entry);
                using var outputStream = File.OpenWrite(entryPath);
                inputStream.CopyTo(outputStream);
            }
        }
    }
}