using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xamarin.UITest.Shared.Hashes;

namespace Xamarin.UITest.Shared.Resources
{
    /// <summary>
    /// Provides operations for loading embedded resources.
    /// </summary>
    internal class EmbeddedResourceLoader
    {
        public Stream GetEmbeddedResourceStream(Assembly assembly, string resourcePostfix)
        {
            var resourceNames = assembly.GetManifestResourceNames();

            var resourcePaths = resourceNames
                .Where(x => x.EndsWith(resourcePostfix, StringComparison.InvariantCultureIgnoreCase))
                .ToArray();

            if (!resourcePaths.Any())
            {
                throw new Exception(string.Format("Resource ending with {0} not found.", resourcePostfix));
            }

            if (resourcePaths.Count() > 1)
            {
                throw new Exception(string.Format("Multiple resources ending with {0} found: {1}{2}", resourcePostfix, Environment.NewLine, string.Join(Environment.NewLine, resourcePaths)));
            }

            return assembly.GetManifestResourceStream(resourcePaths.Single());
        }

        public byte[] GetEmbeddedResourceBytes(Assembly assembly, string resourcePostfix)
        {
            var stream = GetEmbeddedResourceStream(assembly, resourcePostfix);

            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }

        public string GetEmbeddedResourceSha1Hash(Assembly assembly, string resourcePostfix)
        {
            var stream = GetEmbeddedResourceStream(assembly, resourcePostfix);

            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            byte[] bytes = memoryStream.ToArray();

            var hashHelper = new HashHelper();
            return hashHelper.GetSha256Hash(bytes);
        }

        public string GetEmbeddedResourceString(Assembly assembly, string resourcePostfix)
        {
            var stream = GetEmbeddedResourceStream(assembly, resourcePostfix);

            using var streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }
    }
}