using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace Xamarin.UITest.Shared.Zip
{
    /// <summary>
    /// Provides way to obtain data source for byte array.
    /// </summary>
    /// <param name="bytes">Sequence of bytes.</param>
    internal class ByteArrayDataSource(byte[] bytes) : IStaticDataSource, IDisposable
    {
        private readonly MemoryStream MemoryStream = new(bytes);

        public Stream GetSource()
        {
            return MemoryStream;
        }

        public void Dispose()
        {
            MemoryStream?.Dispose();
        }
    }
}