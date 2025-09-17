using System;
using System.IO;
using NUnit.Framework;
using Should;
using Xamarin.UITest.Shared.Android;
using Xamarin.UITest.Configuration;

namespace Xamarin.UITest.Tests
{
    [TestFixture]
    public class KeyStoreTests
    {
        [Test]
        public void FailsIfNoFile()
        {
            Assert.Throws<ArgumentException>(() =>
            {
				new KeyStore(ExecutorHelper.GetDefault(),  new FileInfo("non-existant-file"),
                    DefaultKeyStoreSecrets.KeyAlias,
                    DefaultKeyStoreSecrets.StorePassword,
                    DefaultKeyStoreSecrets.KeyPassword);
            });
        }

        [Test, Ignore("Test fails in case of using defferent keystore locally")]
        public void WorksForLocalFile()
        {
            var keyStore = new KeyStore(ExecutorHelper.GetDefault(),
                new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, "Files", DefaultKeyStoreSecrets.StoreName)),
                DefaultKeyStoreSecrets.KeyAlias,
                DefaultKeyStoreSecrets.StorePassword,
                DefaultKeyStoreSecrets.KeyPassword);

            keyStore.Fingerprints.ShouldContain("0B:06:A7:41:89:21:D3:FB:59:9B:40:A3:73:A8:28:E6:FC:2B:15:CB:EB:98:B9:E3:9B:3B:C2:57:F8:3E:C1:0A");
        }

        [Test]
        public void CanHandleDSA()
        {
            var keyStore = new KeyStore(ExecutorHelper.GetDefault(),
                new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, "Files", "debug2.keystore")),
                "dsa",
                "skodklam",
                "skodklam");
            Assert.AreEqual(keyStore.KeyType, KeyType.DSA);
        }

        [Test]
        public void CanHandleRSA()
        {
            var keyStore = new KeyStore(ExecutorHelper.GetDefault(),
                new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, "Files", "debug2.keystore")),
                "rsa",
                "skodklam",
                "skodklam");
            Assert.AreEqual(keyStore.KeyType, KeyType.RSA);
        }

        [Test]
        public void CanHandleEC()
        {
            var keyStore = new KeyStore(ExecutorHelper.GetDefault(),
                new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, "Files", "debug2.keystore")),
                "ec",
                "skodklam",
                "skodklam");
            Assert.AreEqual(keyStore.KeyType, KeyType.EC);
        }

    }
}
