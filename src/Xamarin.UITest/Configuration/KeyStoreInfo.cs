using System.IO;

namespace Xamarin.UITest.Configuration
{
    internal class KeyStoreInfo
    {
        public FileInfo Path { get; private set; }
        public string StorePassword { get; private set; }
        public string KeyPassword { get; private set; }
        public string KeyAlias { get; private set; }

        public KeyStoreInfo(FileInfo path, string storePassword, string keyPassword, string keyAlias)
        {
            Path = path;
            StorePassword = storePassword;
            KeyPassword = keyPassword;
            KeyAlias = keyAlias;
        }
    }
}