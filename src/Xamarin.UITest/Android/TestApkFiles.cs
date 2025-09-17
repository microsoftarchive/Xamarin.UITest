using Xamarin.UITest.Shared.Android;

namespace Xamarin.UITest.Android
{
    internal class TestApkFiles
    {
        readonly ApkFile _appApkFile;
        readonly ApkFile _testServerApkFile;

        public TestApkFiles(ApkFile appApkFile, ApkFile testServerApkFile)
        {
            _appApkFile = appApkFile;
            _testServerApkFile = testServerApkFile;
        }

        public ApkFile AppApkFile
        {
            get { return _appApkFile; }
        }

        public ApkFile TestServerApkFile
        {
            get { return _testServerApkFile; }
        }
    }
}