namespace Xamarin.UITest.Shared.iOS
{
	public class SimAppInfo
	{
        readonly string _appIdentifier;
        readonly string _appBundlePath;
        readonly string _appPath;
        readonly string _dataPath;

        public SimAppInfo(string appIdentifier, string appBundlePath, string appPath, string dataPath)
        {
            _appBundlePath = appBundlePath;
            _appIdentifier = appIdentifier;
            _appPath = appPath;
            _dataPath = dataPath;
        }

        public string AppPath
        {
            get { return _appPath; }
        }

        public string AppIdentifier
        {
            get { return _appIdentifier; }
        }

        public string AppBundlePath
        {
            get { return _appBundlePath; }
        }

        public string DataPath
        {
            get { return _dataPath; }
        }
	}
}
