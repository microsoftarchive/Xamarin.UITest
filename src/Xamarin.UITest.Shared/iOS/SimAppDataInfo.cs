namespace Xamarin.UITest.Shared.iOS
{
    public class SimAppDataInfo
    {
        readonly string _appIdentifier;
        readonly string _path;

        public SimAppDataInfo(string appIdentifier, string path)
        {
            _path = path;
            _appIdentifier = appIdentifier;
        }

        public string AppIdentifier
        {
            get { return _appIdentifier; }
        }

        public string DataPath
        {
            get { return _path; }
        }
    }
}