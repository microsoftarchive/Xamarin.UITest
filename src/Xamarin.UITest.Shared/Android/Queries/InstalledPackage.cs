namespace Xamarin.UITest.Shared.Android.Queries
{
    public class InstalledPackage
    {
        readonly string _package;
        readonly string _apkPath;

        public InstalledPackage(string package, string apkPath)
        {
            _apkPath = apkPath;
            _package = package;
        }

        public string Package
        {
            get { return _package; }
        }

        public string ApkPath
        {
            get { return _apkPath; }
        }
    }
}