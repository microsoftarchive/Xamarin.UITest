using System.IO;
using NUnit.Framework;
using Xamarin.UITest.Shared.Android;
using Xamarin.UITest.Shared.Resources;

namespace Xamarin.UITest.Tests.Utils
{
    [TestFixture]
    public class ResourceLoaderTests
    {
        [Test]
        public void CanFindResourceAsPostfix()
        {
            var resourceFinder = new EmbeddedResourceLoader();

            var stream = resourceFinder
                .GetEmbeddedResourceStream(typeof(ApkFile).Assembly, 
                "TestServer.apk");

            byte[] contents;

            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                contents = memoryStream.ToArray();
            }

            Assert.AreNotEqual(0, contents.Length);
        }

        [Test]
        public void CanGetResourceBytes()
        {
            var resourceFinder = new EmbeddedResourceLoader();

            var contents = resourceFinder
                .GetEmbeddedResourceBytes(typeof(ApkFile).Assembly, 
                "TestServer.apk");

            Assert.AreNotEqual(0, contents.Length);
        }
        
        [Test]
        public void CanGetResourceString()
        {
            var resourceFinder = new EmbeddedResourceLoader();

            var contents = resourceFinder
                .GetEmbeddedResourceString(typeof(ApkFile).Assembly, 
                "AndroidManifest.xml");

            Assert.AreNotEqual(0, contents.Length);
        }
    }
}
