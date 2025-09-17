using System;
using System.IO;
using Xamarin.UITest.Shared.Android.Queries;
using Xamarin.UITest.Shared.Artifacts;
using Xamarin.UITest.Shared.Execution;

namespace Xamarin.UITest.Shared.Android
{
    public class KeyStore
    {
        readonly IExecutor _executor;
        readonly ISigner _signer;
        readonly Credentials _credentials;

        public class Credentials
        {
            public string KeyStoreFile { get; private set; }
            public string StorePassword { get; private set; }
            public string KeyAlias { get; private set; }
            public string KeyPassword { get; private set; }

            public Credentials(string keystoreFile, string storePassword, string keyAlias, string keyPassword)
            {
                KeyStoreFile = keystoreFile;
                StorePassword = storePassword;
                KeyAlias = keyAlias;
                KeyPassword = keyPassword;
            }
        }

        public KeyStore(IExecutor executor, FileInfo keyStoreFile, string keyAlias, string storePassword, string keyPassword)
        {
            if (keyStoreFile == null)
            {
                throw new ArgumentNullException("keyStoreFile");
            }

            if (!keyStoreFile.Exists)
            {
                throw new ArgumentException("Keystore file does not exist: " + keyStoreFile.FullName, "keyStoreFile");
            }

            _executor = executor;
            _credentials = new Credentials(keyStoreFile.FullName, storePassword, keyAlias, keyPassword);

            KeyType = executor.Execute(new QueryKeyStoreKeyType(keyStoreFile.FullName, keyAlias, storePassword));
            _signer = new ApkSigner(executor); //Probably should inject it via constructor

            Fingerprints = _executor.Execute(new QueryKeyStoreFingerprints(keyStoreFile.FullName, keyAlias, storePassword));
        }

        public string[] Fingerprints { get; set; }

        public KeyType KeyType { get; }

        public override string ToString()
        {
            return "KeyStore - " + _credentials.KeyStoreFile;
        }

        public void SignApk(ApkFile sourceApkFile, string targetApkFilePath)
        {
            _signer.SignApk(sourceApkFile, targetApkFilePath, _credentials);
        }

        public ApkFile ResignApk(ArtifactFolder artifactFolder, string apkFilePath)
        {
            return _signer.ResignApk(artifactFolder, apkFilePath, _credentials); 
        }
    }
}