using System.IO;
using Xamarin.UITest.XDB.Entities;

namespace Xamarin.UITest.XDB.Services.OSX
{
    class iOSBundleService : IiOSBundleService
    {
        readonly IPListService _plistService;

        public iOSBundleService(IPListService plistService)
        { 
            _plistService = plistService;
        }

        public IiOSAppBundle LoadBundle(string appBundlePath)
        {
            if (!Directory.Exists(appBundlePath))
            {
                throw new FileNotFoundException($"Bundle {appBundlePath} does not exist");
            }

            return new iOSAppBundle(appBundlePath, _plistService);
        }
    }
}