using System.Collections.Generic;
using System.IO;
using Xamarin.UITest.XDB.Services.OSX;

namespace Xamarin.UITest.XDB.Entities
{
    class iOSAppBundle : IiOSAppBundle
    {
        readonly IPListService _plistService;

        readonly Dictionary<string, string> _infoPLlistValues = new Dictionary<string, string>();

        string _infoPListXml;

        public string BundleId 
        {
            get 
            {
                return GetPListValue("CFBundleIdentifier");
            }
        }

        public string DTPlatform
        {
            get
            {
                return GetPListValue("DTPlatformName");
            }
        }

        public string Path { get; }

        public iOSAppBundle(string path, IPListService plistService)
        {
            var infoPListPath = System.IO.Path.Combine(path, "Info.plist");

            if (!File.Exists(infoPListPath))
            {
                throw new FileNotFoundException($"Info.plist not found within bundle folder: {path}");
            }

            Path = path;
            _plistService = plistService;
            _infoPListXml = _plistService.ReadPListAsXml(infoPListPath);
        }

        string GetPListValue(string key)
        {
            if (!_infoPLlistValues.ContainsKey(key))
            {
                _infoPLlistValues.Add(key, _plistService.ReadPListValueFromString(_infoPListXml, key));
            }

            return _infoPLlistValues[key]; 
        }
    }
}